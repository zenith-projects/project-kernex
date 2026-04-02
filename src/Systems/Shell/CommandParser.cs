using System.Collections.Generic;

namespace ProjectKernex.Systems.Shell;

public static class CommandParser
{
    public static ParsedCommand Parse(string rawInput)
    {
        var trimmed = rawInput.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return new ParsedCommand("", [], new Dictionary<string, string>(), rawInput);

        var tokens = Tokenize(trimmed);
        if (tokens.Count == 0)
            return new ParsedCommand("", [], new Dictionary<string, string>(), rawInput);

        var commandName = tokens[0].ToLowerInvariant();
        var arguments = new List<string>();
        var flags = new Dictionary<string, string>();

        for (int i = 1; i < tokens.Count; i++)
        {
            var token = tokens[i];

            if (token.StartsWith("--"))
            {
                var flagName = token[2..];
                if (i + 1 < tokens.Count && !tokens[i + 1].StartsWith("-"))
                {
                    flags[flagName] = tokens[i + 1];
                    i++;
                }
                else
                {
                    flags[flagName] = "true";
                }
            }
            else if (token.StartsWith('-') && token.Length == 2)
            {
                var flagName = token[1..];
                if (i + 1 < tokens.Count && !tokens[i + 1].StartsWith("-"))
                {
                    flags[flagName] = tokens[i + 1];
                    i++;
                }
                else
                {
                    flags[flagName] = "true";
                }
            }
            else
            {
                arguments.Add(token);
            }
        }

        return new ParsedCommand(commandName, arguments.ToArray(), flags, rawInput);
    }

    private static List<string> Tokenize(string input)
    {
        var tokens = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;
        char quoteChar = '"';

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (inQuotes)
            {
                if (c == quoteChar)
                {
                    inQuotes = false;
                }
                else
                {
                    current.Append(c);
                }
            }
            else if (c == '"' || c == '\'')
            {
                inQuotes = true;
                quoteChar = c;
            }
            else if (c == ' ' || c == '\t')
            {
                if (current.Length > 0)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                }
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
            tokens.Add(current.ToString());

        return tokens;
    }
}
