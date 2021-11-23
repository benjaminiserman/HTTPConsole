namespace HTTPConsole;
using System;
using System.Collections.Generic;

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
