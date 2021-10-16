using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTTPConsole
{
    public static class ConditionHandler
    {
        public static List<Condition> Handle(Func<string> getString)
        {
            List<Condition> conditions = new();

            string s = getString();
            while (!string.IsNullOrWhiteSpace(s))
            {
                conditions.Add(new Condition(s));
                s = getString();
            }

            return conditions;
        }
    }
}
