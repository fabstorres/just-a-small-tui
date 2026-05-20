using System.Text;

namespace JustASmallTui.Native;

public interface ITerminal
{
    int Columns { get; set; }
    int Rows { get; set; }
    bool IsKittyProtocolActive { get; }
    void Start(Action<string> onInput);
    void Stop();
    void HideCursor();
    void ShowCursor();
}

public sealed class ProcessTerminal : ITerminal
{
    private int _columns;
    private int _rows;
    private Termios _termios;
    private Termios _originalTermios;
    private Action<string>? _onInput;
    private bool _kittyProtocolActive;

    public int Columns
    {
        get
        {
            try
            {
                _columns = Console.WindowWidth;
            }
            catch
            {
                _columns = 80;
            }

            return _columns;
        }
        set => _columns = value;
    }

    public int Rows
    {
        get
        {
            try
            {
                _rows = Console.WindowHeight;
            }
            catch
            {
                _rows = 24;
            }

            return _rows;
        }
        set => _rows = value;
    }

    public bool IsKittyProtocolActive => _kittyProtocolActive;

    public void Start(Action<string> onInput)
    {
        _onInput = onInput;
        TermiosHandler.GetCurrentSettings(out _originalTermios);
        TermiosHandler.EnableRawMode(ref _termios);
        Console.InputEncoding = Encoding.UTF8;
        Console.Write("\x1b[?2004h");
        HideCursor();
    }

    public void Stop()
    {
        ShowCursor();
        Console.Write("\x1b[?2004l");
        TermiosHandler.DisableRawMode(ref _originalTermios);
    }

    public void HideCursor()
    {
        Console.Write("\x1b[?25l");
    }

    public void ShowCursor()
    {
        Console.Write("\x1b[?25h");
    }
}
