using System.Threading;
using System.Text;

namespace JustASmallTui.Native;

public sealed class StdinBuffer(int timeoutMs = 10) : IDisposable
{
    private readonly StringBuilder _buffer = new();
    private readonly CancellationTokenSource _cts = new();
    private Timer? _flushTimer;
    private readonly Lock _lock = new();

    public event Action<string>? OnData;
    public event Action<string>? OnPaste;

    public void Process(string data)
    {
        lock (_lock)
        {
            // Bracketed paste — entire paste arrives as one chunk
            // \x1b[200~ ... \x1b[201~
            var pasteStart = data.IndexOf("\x1b[200~", StringComparison.Ordinal);
            if (pasteStart >= 0)
            {
                var pasteEnd = data.IndexOf("\x1b[201~", pasteStart, StringComparison.Ordinal);
                if (pasteEnd >= 0)
                {
                    // Flush anything before the paste
                    if (pasteStart > 0)
                        FlushBuffer();

                    var content = data[(pasteStart + 6)..pasteEnd];
                    OnPaste?.Invoke(content);

                    // Process anything after the paste end marker
                    var after = data[(pasteEnd + 6)..];
                    if (after.Length > 0)
                        Process(after);

                    return;
                }
            }

            _buffer.Append(data);
            ResetTimer();
        }
    }

    private void ResetTimer()
    {
        _flushTimer?.Dispose();
        _flushTimer = new Timer(_ =>
        {
            lock (_lock)
                FlushBuffer();
        }, null, timeoutMs, Timeout.Infinite);
    }

    private void FlushBuffer()
    {
        if (_buffer.Length == 0) return;

        // Split buffer into individual sequences and emit each
        var sequences = SplitSequences(_buffer.ToString());
        _buffer.Clear();
        _flushTimer?.Dispose();
        _flushTimer = null;

        foreach (var seq in sequences)
            OnData?.Invoke(seq);
    }

    // Split a string of potentially multiple escape sequences into individual ones
    private static List<string> SplitSequences(string input)
    {
        var result = new List<string>();
        var i = 0;

        while (i < input.Length)
        {
            if (input[i] == '\x1b' && i + 1 < input.Length)
            {
                // ESC [ ... — CSI sequence, ends at first letter
                if (input[i + 1] == '[')
                {
                    var end = i + 2;
                    while (end < input.Length && !char.IsLetter(input[end]))
                        end++;
                    if (end < input.Length) end++; // include terminating letter
                    result.Add(input[i..end]);
                    i = end;
                }
                // ESC ] ... BEL or ST — OSC sequence
                else if (input[i + 1] == ']')
                {
                    var end = input.IndexOf('\x07', i + 2);
                    if (end < 0) end = input.Length - 1;
                    result.Add(input[i..(end + 1)]);
                    i = end + 1;
                }
                // Bare ESC or ESC + single char
                else
                {
                    result.Add(input[i..(i + 2)]);
                    i += 2;
                }
            }
            else
            {
                // Regular character(s) — group consecutive non-ESC chars
                var end = input.IndexOf('\x1b', i + 1);
                if (end < 0) end = input.Length;
                result.Add(input[i..end]);
                i = end;
            }
        }

        return result;
    }

    public void Dispose()
    {
        _flushTimer?.Dispose();
        _cts.Dispose();
    }
}
