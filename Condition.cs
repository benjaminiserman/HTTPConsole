using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTTPConsole
{
    public class Condition
    {
        Predicate<string> condition;
        public string Expected { get; private set; }
        public string Command { get; private set; }

        public Condition(string s)
        {
            string[] split = s.Split();

            Command = split[0];
            Expected = split[1];

            bool not = split[2][0] == '!';

            string method;
            if (not) method = split[2][1..].ToLower();
            else method = split[2].ToLower();

            string query = string.Empty;
            for (int i = 3; i < split.Length; i++) query += $"{split[i]} ";
            query = query[0..^1];

            switch (method)
            {
                case "contains":
                case "contain":
                {
                    if (not) condition = s => !s.Contains(query);
                    else condition = s => s.Contains(query);
                    break;
                }
                case "is":
                case "equals":
                {
                    if (not) condition = s => s != query;
                    else condition = s => s == query;
                    break;
                }
            }
        }

        public bool Eval(string x) => condition(x);

        public bool Eval(Dictionary<string, string> dict) => Eval(dict[Expected]);
    }
}

/* y
 * for 100
 * break content contains appear to be a valid cookie
 * display content !contains Not very special though
 * 
 * get
 * cookie
 * name=$i
 * 
 * 
 * end
 */