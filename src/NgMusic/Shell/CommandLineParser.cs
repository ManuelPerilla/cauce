using System.Text;

namespace NgMusic.Shell;

public static class CommandLineParser
{
    public static IReadOnlyList<string> Parse(string input)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        char? quote = null;

        for (var i = 0; i < input.Length; i++)
        {
            var ch = input[i];

            if (quote is not null)
            {
                if (ch == quote)
                {
                    quote = null;
                    continue;
                }

                if (ch == '\\' && i + 1 < input.Length && input[i + 1] == quote)
                {
                    current.Append(input[++i]);
                    continue;
                }

                current.Append(ch);
                continue;
            }

            if (ch is '\'' or '"')
            {
                quote = ch;
                continue;
            }

            if (char.IsWhiteSpace(ch))
            {
                if (current.Length > 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                continue;
            }

            current.Append(ch);
        }

        if (current.Length > 0)
            result.Add(current.ToString());

        return result;
    }
}
