namespace HTTPConsole;
using System;
using System.Collections.Generic;

public class Condition
{
    readonly Predicate<string> _condition;
    public string Expected { get; private set; }
    public string Command { get; private set; }

    public Condition(string s)
    {
        string[] split = s.Split();

        Command = split[0];
        Expected = split[1];

        bool not = split[2][0] == '!';

        string method = not
            ? split[2][1..].ToLower()
            : split[2].ToLower();
        string query = string.Empty;
        for (int i = 3; i < split.Length; i++) query += $"{split[i]} ";
        query = query[..^1];

        switch (method)
        {
            case "contains":
            case "contain":
            {
                _condition = not
                    ? (s => !s.Contains(query))
                    : (s => s.Contains(query));
                break;
            }
            case "is":
            case "equals":
            {
                _condition = not
                    ? (s => s != query)
                    : (s => s == query);
                break;
            }
        }
    }

    public bool Eval(string x) => _condition(x);

    public bool Eval(Dictionary<string, string> dict) => Eval(dict[Expected]);
}
