using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JustASmallTui.Tui;

static partial class Terminal
{
    [InlineArray(32)]
    struct TermiosCC { private byte _e0; }

    [StructLayout(LayoutKind.Sequential)]
    struct Termios
    {
        public uint c_iflag, c_oflag, c_cflag, c_lflag;
        public byte c_line;
        public TermiosCC c_cc;
        public uint c_ispeed, c_ospeed;
    }

    [LibraryImport("libc", EntryPoint = "tcgetattr", SetLastError = true)]
    private static partial int tcgetattr(int fd, ref Termios t);

    [LibraryImport("libc", EntryPoint = "tcsetattr", SetLastError = true)]
    private static partial int tcsetattr(int fd, int action, ref Termios t);

    const uint BRKINT = 0x2, ICRNL = 0x100, INPCK = 0x10, ISTRIP = 0x20, IXON = 0x400;
    const uint OPOST = 0x1;
    const uint CS8 = 0x30;
    const uint ECHO = 0x8, ICANON = 0x100, IEXTEN = 0x8000;
    const int TCSAFLUSH = 2;
    const int VTIME = 5, VMIN = 6;

    static Termios _saved;
    static bool _enabled;

    public static void EnableRawMode()
    {
        if (tcgetattr(0, ref _saved) == -1)
        {
            throw new InvalidOperationException($"tcgetattr failed: {Marshal.GetLastPInvokeError()}");
        }

        var raw = _saved;
        raw.c_iflag &= ~(BRKINT | ICRNL | INPCK | ISTRIP | IXON);
        raw.c_oflag &= ~OPOST;
        raw.c_cflag |= CS8;
        raw.c_lflag &= ~(ECHO | ICANON | IEXTEN);
        raw.c_cc[VMIN] = 1;
        raw.c_cc[VTIME] = 0;

        if (tcsetattr(0, TCSAFLUSH, ref raw) == -1)
        {
            throw new InvalidOperationException($"tcsetattr failed: {Marshal.GetLastPInvokeError()}");
        }

        _enabled = true;
    }

    public static void Restore()
    {
        if (_enabled)
        {
            tcsetattr(0, TCSAFLUSH, ref _saved);
            _enabled = false;
        }
    }
}
