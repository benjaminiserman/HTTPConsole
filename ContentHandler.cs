using System;
using System.Net;
using System.IO;
using System.Text;

namespace HTTPConsole
{
    public static class ContentHandler
    {
        public static void Handle(HttpWebRequest request, Func<string> getString, bool verbose)
        {
            WriteLine("ContentType: (blank to skip, 0 for application/x-www-form-urlencoded)");
            string s = getString().Trim();
            if (!string.IsNullOrEmpty(s))
            {
                if (s == "0") s = "application/x-www-form-urlencoded";
                request.ContentType = s;
            }
            string contentString = string.Empty;
            WriteLine("Content:");
            while (true)
            {
                s = getString();
                if (string.IsNullOrEmpty(s)) break;
                else contentString += s;
            }

            byte[] byteArray = Encoding.ASCII.GetBytes(contentString);

            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WriteLine("PROPERTY VALUE (press enter twice to continue)");

            void WriteLine(object x)
            {
                if (verbose) Console.WriteLine(x);
            }
        }
    }
}
