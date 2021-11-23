using System;
using System.Net;
using System.Reflection;

namespace HTTPConsole
{
    public static class PropertyHandler
    {
        public static void Handle(HttpWebRequest request, string input, bool verbose, bool loop, int counter)
        {
            try
            {
                if (loop)
                {
                    input = input.Replace("$i", $"{counter}");
                }

                string[] split = input.Split();
                split[0] = char.ToUpper(split[0][0]) + split[0][1..];

                object obj;

                string value = split[1];
                for (int i = 2; i < split.Length; i++) value += $" {split[i]}";

                PropertyInfo pInfo = typeof(WebRequest).GetProperty(split[0]);
                obj = pInfo.PropertyType.IsEnum 
                    ? Enum.Parse(pInfo.PropertyType, value) 
                    : Convert.ChangeType(value, pInfo.PropertyType);

                pInfo.SetValue(request, obj);
            }
            catch (Exception e)
            {
                WriteLine($"{e.Message}... Please try again.");
            }

            void WriteLine(object x)
            {
                if (verbose) Console.WriteLine(x);
            }
        }
    }
}
