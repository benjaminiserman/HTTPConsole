namespace HTTPConsole;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using InputHandler;

class Program
{
    static void Main()
    {
        Uri uri = null;
        Console.WriteLine("Enter URL: ");
        Input.Get(Console.ReadLine, s => uri = new UriBuilder(s).Uri);

        Command(Console.ReadLine, true, uri, false, 0, string.Empty, null);
    }

    public static void Command(Func<string> getString, bool verbose, Uri uri, bool loop, int counter, string pipePath, List<Condition> conditions)
    {
        bool display = true;

        while (true)
        {
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;

            WriteLine("METHOD");
            Input.Get(() => getString().ToUpper(), s => request.Method = s);

            bool runVerbose = false;

            switch (request.Method)
            {
                case "LOOP":
                    LoopHandler.Loop(getString, verbose, uri, display, pipePath);
                    continue;
                case "URL":
                    WriteLine("Enter URL: ");
                    Input.Get(getString, s => uri = new UriBuilder(s).Uri);
                    continue;
                case "VERBATIM":
                    WriteLine("METHOD (verbatim)");
                    Input.Get(() => getString().ToUpper(), s => request.Method = s);
                    continue;
                case "RUN_DEBUG":
                    runVerbose = true;
                    goto case "RUN";
                case "RUN":
                    WriteLine("Enter path of file to run.");

                    string path = string.Empty;
                    Input.Get(() => getString().Trim(), delegate (string s)
                    {
                        _ = new Uri(s, UriKind.RelativeOrAbsolute); // this will error and trip InputHandler try/catch if bad path
                            path = s;
                    });

                    using (StreamReader reader = new(path))
                    {
                        Command(reader.ReadLine, runVerbose, uri, loop, counter, pipePath, conditions);
                    }

                    WriteLine("RUN complete.");
                    continue;
                case "PIPE":
                    WriteLine("Display output? (y/n)");
                    display = Input.GetYN(getString);

                    WriteLine("Pipe to file: (enter nothing to disable)");
                    Input.Get(() => getString().Trim(), delegate (string s)
                    {
                        if (!string.IsNullOrEmpty(s)) _ = new Uri(s, UriKind.RelativeOrAbsolute); // this will error and trip InputHandler try/catch if bad path

                            pipePath = s;
                    });

                    continue;
                case "CLEAR":
                    Console.Clear();
                    continue;
                case "END":
                    return;
            }

            WriteLine("PROPERTY VALUE (press enter twice to continue)");
            while (true)
            {
                string input = getString();
                if (string.IsNullOrWhiteSpace(input)) break;

                string check = input.ToLower();
                if (check == "cookie") CookieHandler.Handle(request, uri, getString, verbose, loop, counter);
                else if (check == "content") ContentHandler.Handle(request, getString, verbose, loop, counter);
                else PropertyHandler.Handle(request, input, verbose, loop, counter);
            }

            WriteLine("Sending...");

            if (GetResponse(request) is not HttpWebResponse response) BadLog("RECEIVED NO RESPONSE", pipePath, display);
            else
            {
                var s = new StreamReader(response.GetResponseStream());
                string content = s.ReadToEnd();
                s.Close();

                bool commandDisplay = true;

                if (conditions is not null)
                {
                    Dictionary<string, string> dict = new()
                    {
                        { "content", content },
                    };

                    // key is 

                    commandDisplay = !conditions.Any(x => x.Command == "display");

                    foreach (Condition condition in conditions)
                    {
                        if (condition.Eval(dict))
                        {
                            switch (condition.Command)
                            {
                                case "display":
                                    commandDisplay = true;
                                    break;
                                case "break":
                                    BadLog("BREAK CASE ENCOUNTERED", pipePath, display);
                                    throw new TimeoutException("Interrupt.");
                            }
                        }
                    }
                }

                DisplayResponse(content, response, display && commandDisplay, pipePath);
            }
        }

        void WriteLine(object x)
        {
            if (verbose) Console.WriteLine(x);
        }
    }

    private static WebResponse GetResponse(WebRequest request)
    {
        WebResponse response;
        try
        {
            response = request.GetResponse();
        }
        catch (WebException ex)
        {
            response = ex.Response;
        }

        return response;
    }

    private static void DisplayResponse(string content, HttpWebResponse response, bool display, string pipePath)
    {
        StreamWriter writer = null;
        try
        {
            if (!string.IsNullOrWhiteSpace(pipePath)) writer = File.AppendText(pipePath);
        }
        catch (Exception e)
        {
            Console.WriteLine($"File write failed: {e.Message}");
        }

        Write($"Response with code ({(int)response.StatusCode}) from {response.ResponseUri}:");
        Write($"\nHEADERS:\n");

        foreach (string key in response.Headers.AllKeys)
        {
            Write($"{key}, {response.Headers[key]}");
        }

        Write("\nCONTENT:");

        Write(content);

        if (writer is not null) writer.Close();

        void Write(object x)
        {
            if (display) Console.WriteLine(x);
            if (writer is not null) writer.WriteLine(x);
        }
    }

    public static void BadLog(object x, string pipePath, bool display) // this is very inefficent
    {
        if (!string.IsNullOrWhiteSpace(pipePath))
        {
            try
            {
                StreamWriter writer = File.AppendText(pipePath);
                if (writer is not null)
                {
                    writer.WriteLine(x);
                    writer.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"File write failed: {e.Message}");
            }
        }

        if (display) Console.WriteLine(x);
    }
}
