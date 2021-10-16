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

        public Condition(string s)
        {
            string[] split = s.Split();

            bool not = s[0] == '!';

            string method;
            if (not) method = split[0][1..].ToLower();
            else method = split[0].ToLower();

            string query = string.Empty;
            for (int i = 1; i < split.Length; i++) query += $"{split[i]} ";
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