using System;
using System.Net;

namespace HTTPConsole
{
    public static class CookieHandler
    {
        public static void Handle(HttpWebRequest request, Uri uri, Func<string> getString, bool verbose, bool loop, int counter)
        {
            if (request.SupportsCookieContainer)
            {
                WriteLine("COOKIE=VALUE");

                request.CookieContainer ??= new CookieContainer();
                while (true)
                {
                    string cookieString = InputHandler.Input.Get(getString, s => s.Contains('=') || string.IsNullOrWhiteSpace(s));

                    if (string.IsNullOrWhiteSpace(cookieString)) break;

                    try
                    {
                        int equalsIndex = cookieString.IndexOf('=');
                        string key = cookieString[0..equalsIndex];
                        string value = cookieString[(equalsIndex + 1)..];

                        if (loop)
                        {
                            key = key.Replace("$i", $"{counter}");
                            value = value.Replace("$i", $"{counter}");
                        }

                        request.CookieContainer.Add(new Cookie(key, value, null, uri.Host));
                    }
                    catch (Exception e)
                    {
                        WriteLine($"{e.Message}... Please try again.");
                    }
                }
            }
            else WriteLine("Cookies are not supported for this type of request.");

            WriteLine("PROPERTY VALUE (press enter twice to continue)");

            void WriteLine(object x)
            {
                if (verbose) Console.WriteLine(x);
            }
        }
    }
}
