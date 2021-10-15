using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Reflection;
using System.IO;

namespace HTTPConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter URL: ");
            Uri uri = null; 
            Input(s => uri = new UriBuilder(s).Uri);

            while (true)
            {
                HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
                
                Console.WriteLine("Method?");
                Input(s => request.Method = s, s => s.ToUpper());

                switch (request.Method)
                {
                    case "LOOP":
                        Loop(uri);
                        continue;
                    case "URL":
                    case "URI":
                        Console.Write("Enter URL: ");
                        Input(s => uri = new UriBuilder(s).Uri);
                        continue;
                }

                Console.WriteLine("PROPERTY VALUE (press enter twice to continue)");
                while (true)
                {
                    string input = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(input)) break;

                    string check = input.ToLower();
                    if (check == "cookie") HandleCookies(request, uri);
                    else if (check == "content") HandleContent(request);
                    else try
                    {
                        string[] split = input.Split();
                        split[0] = Alias(char.ToUpper(split[0][0]) + split[0].Substring(1));

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
                        Console.WriteLine($"{e.Message}... Please try again.");
                    }
                }

                Console.WriteLine("Sending...");

                DisplayResponse(GetResponse(request));
            }
        }

        private static void HandleCookies(HttpWebRequest request, Uri uri)
        {
            if (request.SupportsCookieContainer)
            {
                Console.WriteLine("COOKIE=VALUE");

                request.CookieContainer ??= new CookieContainer();
                while (true)
                {
                    string cookieString = Input(s => s.Contains('=') || string.IsNullOrWhiteSpace(s));

                    if (string.IsNullOrWhiteSpace(cookieString)) break;

                    try
                    {
                        int equalsIndex = cookieString.IndexOf('=');
                        string key = cookieString.Substring(0, equalsIndex);
                        string value = cookieString.Substring(equalsIndex + 1);

                        request.CookieContainer.Add(new Cookie(key, value, null, uri.Host));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{e.Message}... Please try again.");
                    }
                }
            }
            else Console.WriteLine("Cookies are not supported for this type of request.");

            Console.WriteLine("PROPERTY VALUE (press enter twice to continue)");
        }

        private static void HandleContent(HttpWebRequest request)
        {
            Console.WriteLine("ContentType: (blank to skip, 0 for application/x-www-form-urlencoded)");
            string s = Console.ReadLine().Trim();
            if (!string.IsNullOrEmpty(s))
            {
                if (s == "0") s = "application/x-www-form-urlencoded";
                request.ContentType = s;
            }
            string contentString = string.Empty;
            Console.WriteLine("Content:");
            while (true)
            {
                s = Console.ReadLine();
                if (string.IsNullOrEmpty(s)) break;
                else contentString += s;
            }

            byte[] byteArray = Encoding.ASCII.GetBytes(contentString);

            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            Console.WriteLine("PROPERTY VALUE (press enter twice to continue)");
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

        private static void DisplayResponse(WebResponse response)
        {
            Console.WriteLine($"Response with code ({(int)(response as HttpWebResponse).StatusCode}) from {response.ResponseUri}:");
            Console.WriteLine($"\nHEADERS:\n");

            foreach (string key in response.Headers.AllKeys)
            {
                Console.WriteLine($"{key}, {response.Headers[key]}");
            }

            Console.WriteLine("\nCONTENT:");

            var s = new StreamReader(response.GetResponseStream());

            Console.WriteLine(s.ReadToEnd());

            s.Close();
        }

        private static void Template(Uri uri)
        {
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;

            request.Method = "GET";

            if (false) // COOKIES
            {
                request.CookieContainer ??= new CookieContainer();
                request.CookieContainer.Add(new Cookie("key", "value", null, uri.Host));
            }

            if (false) // CONTENT
            {
                request.ContentType = "application/x-www-form-urlencoded";

                string contentString = "";

                byte[] byteArray = Encoding.ASCII.GetBytes(contentString);

                request.ContentLength = byteArray.Length;

                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }
            
            DisplayResponse(GetResponse(request));
        }

        private static string Alias(string s)
        {
            return s;
        }

        private static void Input(Action<string> action, Func<string, string> modify = null)
        {
            modify ??= s => s;

            while (true)
            {
                try
                {
                    action(modify(Console.ReadLine().Trim()));
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message}... Please try again.");
                }
            }
        }

        private static string Input(Func<string, bool> check, Func<string, string> modify = null)
        {
            modify ??= x => x;
            string s;
            while (!check(s = modify(Console.ReadLine().Trim()))) 
            {
                Console.WriteLine("Invalid format... Please try again.");
            }
            return s;
        }
    }
}
