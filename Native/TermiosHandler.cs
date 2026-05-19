
using System.Runtime.InteropServices;

namespace JustASmallTui.Native;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct Termios
{
    public UIntPtr c_iflag;
    public UIntPtr c_oflag;
    public UIntPtr c_cflag;
    public UIntPtr c_lflag;
#if MACOS
    public fixed ulong c_cc[20];
#else
    public fixed ulong c_cc[32];
#endif
    public UIntPtr c_ispeed;
    public UIntPtr c_ospeed;
}

public static partial class TermiosHandler
{
    [LibraryImport("libc")]
    private static partial int tcgetattr(int fd, out Termios termios);

    [LibraryImport("libc")]
    private static partial int tcsetattr(int fd, int optional_actions, in Termios termios);

    public static unsafe void EnableRawMode(ref Termios raw)
    {
        tcgetattr(0, out raw);
        raw.c_lflag &= ~(0x8u | 0x100u | 0x8000u);
        raw.c_iflag &= ~(0x100u | 0x2u);
        raw.c_oflag &= ~0x1u;
        raw.c_cflag |= ~0x30u;
        raw.c_cc[6] = 1;
        raw.c_cc[5] = 0;
        tcsetattr(0, 2, raw);
    }

    public static void Flush(ref Termios raw)
    {
        tcsetattr(0, 2, raw);
    }
}
