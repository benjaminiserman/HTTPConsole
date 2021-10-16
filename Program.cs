using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

namespace HTTPConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri uri = null;
            InputHandler.Input(Console.ReadLine, s => uri = new UriBuilder(s).Uri);

            Console.Write("Enter URL: ");

            Command(Console.ReadLine, true, uri);
        }

        private static void Command(Func<string> getString, bool verbose, Uri uri)
        {
            bool pipe = false, write = true;
            string pipePath = string.Empty;

            while (true)
            {
                HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;

                WriteLine("METHOD");
                InputHandler.Input(() => getString().ToUpper(), s => request.Method = s);

                switch (request.Method)
                {
                    case "LOOP":
                        Loop(uri);
                        continue;
                    case "URL":
                        WriteLine("Enter URL: ");
                        InputHandler.Input(getString, s => uri = new UriBuilder(s).Uri);
                        continue;
                    case "VERBATIM":
                        WriteLine("METHOD (verbatim)");
                        InputHandler.Input(() => getString().ToUpper(), s => request.Method = s);
                        continue;
                    case "RUN":
                        WriteLine("Display prompts while running? (y/n)");
                        bool runVerbose = InputHandler.InputYN(getString);

                        WriteLine("Enter path of file to run.");

                        string path = string.Empty;
                        InputHandler.Input(() => getString().Trim(), delegate (string s)
                        {
                            new Uri(s, UriKind.RelativeOrAbsolute); // this will error and trip InputHandler try/catch if bad path
                            path = s;
                        });

                        using (StreamReader reader = new(path))
                        {
                            Command(reader.ReadLine, runVerbose, uri);
                        }

                        WriteLine("RUN complete.");
                        continue;
                    case "PIPE":
                        WriteLine("Display output? (y/n)");
                        write = InputHandler.InputYN(getString);

                        WriteLine("Pipe to file: (enter nothing to disable)");
                        InputHandler.Input(() => getString().Trim(), delegate (string s)
                        {
                            if (string.IsNullOrEmpty(s))
                            {
                                pipe = false;
                            }
                            else
                            {
                                _ = new Uri(s, UriKind.RelativeOrAbsolute); // this will error and trip InputHandler try/catch if bad path
                                pipePath = s;
                                pipe = true;
                            }
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
                    if (check == "cookie") CookieHandler.Handle(request, uri, getString, verbose);
                    else if (check == "content") ContentHandler.Handle(request, getString, verbose);
                    else try
                        {
                            string[] split = input.Split();
                            split[0] = char.ToUpper(split[0][0]) + split[0].Substring(1);

                            object obj;

                            string value = split[1];
                            for (int i = 2; i < split.Length; i++) value += $" {split[i]}";

                            PropertyInfo pInfo = typeof(WebRequest).GetProperty(split[0]);
                            if (pInfo.PropertyType.IsEnum) obj = Enum.Parse(pInfo.PropertyType, value);
                            else obj = Convert.ChangeType(value, pInfo.PropertyType);

                            pInfo.SetValue(request, obj);
                        }
                        catch (Exception e)
                        {
                            WriteLine($"{e.Message}... Please try again.");
                        }
                }

                WriteLine("Sending...");

                DisplayResponse(GetResponse(request), write, pipePath);
            }

            void WriteLine(object x)
            {
                if (verbose) Console.WriteLine(x);
            }
        }

        private static void Loop(Uri uri)
        {
            for (int i = 0; i < 100; i++)
            {
                HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;

                request.Method = "GET";

                if (true) // COOKIES
                {
                    request.CookieContainer ??= new CookieContainer();
                    request.CookieContainer.Add(new Cookie("name", $"{i}", null, uri.Host));
                }

                Console.WriteLine($"\nTRIAL {i}\n");
                
                WebResponse response = GetResponse(request);

                var s = new StreamReader(response.GetResponseStream());

                string output = s.ReadToEnd();

                s.Close();

                if (output.Contains("appear to be a valid cookie")) break;
                if (!output.Contains("Not very special though"))
                {
                    Console.WriteLine(output);
                }
            }
        }

        private static WebResponse GetResponse(WebRequest request)
        {
            WebResponse response = null;
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

        private static void DisplayResponse(WebResponse response, bool display, string pipePath)
        {
            StreamWriter writer = null;
            if (!string.IsNullOrWhiteSpace(pipePath)) writer = File.AppendText(pipePath);

            Write($"Response with code ({(int)(response as HttpWebResponse).StatusCode}) from {response.ResponseUri}:");
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
    }
}
