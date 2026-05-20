
using System.Runtime.InteropServices;

namespace JustASmallTui.Native;

#if MACOS
using cc_t = byte;
using tc_flagt = ulong;
using tc_speedt = ulong;
#else
using cc_t = uint;
using tc_flagt = uint;
using tc_speedt = uint;
#endif

[StructLayout(LayoutKind.Sequential)]
public unsafe struct Termios
{
    public tc_flagt c_iflag;
    public tc_flagt c_oflag;
    public tc_flagt c_cflag;
    public tc_flagt c_lflag;
#if MACOS
    public fixed cc_t c_cc[20];
#else
    public fixed cc_t c_cc[32];
#endif
    public tc_speedt c_ispeed;
    public tc_speedt c_ospeed;
}

public static partial class TermiosHandler
{
    private const tc_flagt ECHO = 0x8;
    private const int TCSAFLUSH = 2;
    private const int STDIN_FILENO = 0;
    private const tc_flagt OPOST = 0x1;

    private const tc_flagt BRKINT = 0x2;
    private const tc_flagt INPCK = 0x10;
    private const tc_flagt ISTRIP = 0x20;

#if MACOS
    private const int VTIME = 17;
    private const int VMIN = 16;
    private const tc_flagt ICANON = 0x100;
    private const tc_flagt ISIG = 0x80;
    private const tc_flagt IEXTEN = 0x400;
    private const tc_flagt ECHONL = 0x10;
    private const tc_flagt IXON = 0x200;
    private const tc_flagt ICRNL = 0x100;

    private const tc_flagt CS8 = 0x300;
    private const tc_flagt CSIZE = 0x300;

#else
    private const int VTIME = 5;
    private const int VMIN = 6;
    private const tc_flagt ICANON = 0x2;
    private const tc_flagt ISIG = 0x1;
    private const tc_flagt IEXTEN = 0x10000;
    private const tc_flagt ECHONL = 0x40;
    private const tc_flagt IXON = 0x400;
    private const tc_flagt ICRNL = 0x80;

    private const tc_flagt CS8   = 0x30;
    private const tc_flagt CSIZE = 0x30;
#endif
    [LibraryImport("libc")]
    private static partial int tcgetattr(int fd, out Termios termios);

    [LibraryImport("libc")]
    private static partial int tcsetattr(int fd, int optional_actions, in Termios termios);

    public static void GetCurrentSettings(out Termios termios)
    {
        tcgetattr(STDIN_FILENO, out termios);
    }

    public static unsafe void EnableRawMode(ref Termios raw)
    {
        tcgetattr(STDIN_FILENO, out raw);
        raw.c_iflag &= ~(BRKINT | ICRNL | INPCK | ISTRIP | IXON);
        raw.c_oflag &= ~OPOST;
        raw.c_cflag |= CS8;
        raw.c_lflag &= ~(ECHO | ICANON | IEXTEN | ISIG);

        raw.c_cc[VMIN] = 0;
        raw.c_cc[VTIME] = 1;
        tcsetattr(STDIN_FILENO, TCSAFLUSH, raw);
    }

    public static void DisableRawMode(ref Termios raw)
    {
        tcsetattr(STDIN_FILENO, TCSAFLUSH, raw);
    }
}
