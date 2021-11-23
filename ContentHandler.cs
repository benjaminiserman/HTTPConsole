namespace HTTPConsole;
using System;
using System.IO;
using System.Net;
using System.Text;

public static class ContentHandler
{
    public static void Handle(HttpWebRequest request, Func<string> getString, bool verbose, bool loop, int counter)
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

        if (loop)
        {
            contentString = contentString.Replace("$i", $"{counter}");
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
