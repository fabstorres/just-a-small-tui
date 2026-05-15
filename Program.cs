using System.Text;
using JustASmallTui.Tui;

Console.CancelKeyPress += (_, _) => Terminal.Restore();

Terminal.EnableRawMode();
// Console.Write("\u001b[>9u");
Console.Write("\u001b[<u");
Console.Write("Press keys to inspect input. Press q or Ctrl-C to quit.\r\n");

var stdin = Console.OpenStandardInput();
var buffer = new byte[64];

try
{
    while (true)
    {
        var n = await stdin.ReadAsync(buffer);
        if (n > 0)
        {
            var input = Encoding.UTF8.GetString(buffer, 0, n);
            Console.Write($"hex={Convert.ToHexString(buffer.AsSpan(0, n))} text={EscapeForDisplay(input)}\r\n");

            if (ShouldQuit(input))
            {
                break;
            }
        }

    }
}
finally
{
    //Console.Write("\u001b[<u");
    Terminal.Restore();
}

static string EscapeForDisplay(string value)
{
    var builder = new StringBuilder(value.Length);

    foreach (var c in value)
    {
        builder.Append(c switch
        {
            '\u001b' => "\\e",
            '\r' => "\\r",
            '\n' => "\\n",
            '\t' => "\\t",
            < ' ' or '\u007f' => $"\\x{(int)c:X2}",
            _ => c
        });
    }

    return builder.ToString();
}

static bool ShouldQuit(string input) =>
    input.Contains('q')
    || input.Contains('\u0003')
    || input.Contains("\u001b[113")
    || input.Contains("\u001b[99;5u");
