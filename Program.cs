using System;
using System.IO;
using System.Net;
using System.Text;

namespace HTTPConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri uri = null;
            Console.WriteLine("Enter URL: ");
            InputHandler.Input(Console.ReadLine, s => uri = new UriBuilder(s).Uri);

            Command(Console.ReadLine, true, uri, false, 0, string.Empty);
        }

        public static void Command(Func<string> getString, bool verbose, Uri uri, bool loop, int counter, string pipePath)
        {
            bool display = true;

            while (true)
            {
                HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;

                WriteLine("METHOD");
                InputHandler.Input(() => getString().ToUpper(), s => request.Method = s);

                bool runVerbose = false;

                switch (request.Method)
                {
                    case "LOOP":
                        LoopHandler.Loop(getString, verbose, uri, display, pipePath);
                        continue;
                    case "URL":
                        WriteLine("Enter URL: ");
                        InputHandler.Input(getString, s => uri = new UriBuilder(s).Uri);
                        continue;
                    case "VERBATIM":
                        WriteLine("METHOD (verbatim)");
                        InputHandler.Input(() => getString().ToUpper(), s => request.Method = s);
                        continue;
                    case "RUN_DEBUG":
                        runVerbose = true;
                        goto case "RUN";
                    case "RUN":
                        WriteLine("Enter path of file to run.");

                        string path = string.Empty;
                        InputHandler.Input(() => getString().Trim(), delegate (string s)
                        {
                            new Uri(s, UriKind.RelativeOrAbsolute); // this will error and trip InputHandler try/catch if bad path
                            path = s;
                        });

                        using (StreamReader reader = new(path))
                        {
                            Command(reader.ReadLine, runVerbose, uri, loop, counter, pipePath);
                        }

                        WriteLine("RUN complete.");
                        continue;
                    case "PIPE":
                        WriteLine("Display output? (y/n)");
                        display = InputHandler.InputYN(getString);

                        WriteLine("Pipe to file: (enter nothing to disable)");
                        InputHandler.Input(() => getString().Trim(), delegate (string s)
                        {
                            if (!string.IsNullOrEmpty(s)) _ = new Uri(s, UriKind.RelativeOrAbsolute); // this will error and trip InputHandler try/catch if bad path
                            
                            pipePath = s;
                        });

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
                else DisplayResponse(response, display, pipePath);
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

        private static void DisplayResponse(HttpWebResponse response, bool display, string pipePath)
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

            var s = new StreamReader(response.GetResponseStream());

            Write(s.ReadToEnd());

            s.Close();
            if (writer is not null) writer.Close();

            void Write(object x)
            {
                if (display) Console.WriteLine(x);
                if (writer is not null) writer.WriteLine(x);
            }
        }

        public static void BadLog(object x, string pipePath, bool display) // this is very inefficent
        {
            StreamWriter writer = null;
            if (!string.IsNullOrWhiteSpace(pipePath))
            {
                try
                {
                    writer = File.AppendText(pipePath);
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
}
