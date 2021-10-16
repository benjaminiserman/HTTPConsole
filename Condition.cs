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

        public Condition(string s)
        {
            string[] split = s.Split();

            Expected = split[0];

            bool not = s[1] == '!';

            string method;
            if (not) method = split[1][1..].ToLower();
            else method = split[1].ToLower();

            string query = string.Empty;
            for (int i = 2; i < split.Length; i++) query += $"{split[i]} ";
            query = query[0..^1];

            switch (method)
            {
                case "contains":
                case "contain":
                {
                    condition = s => s.Contains(query);
                    break;
                }
                case "is":
                case "equals":
                {
                    condition = s => s == query;
                    break;
                }
            }
        }

        public bool Eval(string x) => condition(x);
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