using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTTPConsole
{
    public static class ConditionHandler
    {
        public static Dictionary<Condition, string> Handle(Func<string> getString)
        {
            Dictionary<Condition, string> conditions = new();

            string s;
            do
            {
                s = getString();

                string[] split = s.Split(' ', 2);

                conditions.Add(new Condition(split[1]), split[0]);
            }
            while (!string.IsNullOrWhiteSpace(s));

            return conditions;
        }

        public static void EvalMatching(Dictionary<Condition, string> conditions, string command, string type, string value)
        {
            foreach (var kvp in conditions)
            {
                if (kvp.Value == command && kvp.Key.Expected == type)
                {

                }
            }
        }
    }
}
