// using JustASmallTui.Native;

//var termios = new Termios();
//TermiosHandler.EnableRawMode(ref termios);


Console.Write("\x1b[?25l");


while (true)
{
    var key = Console.ReadKey(intercept: true);
    if (key.Key == ConsoleKey.Q)
        break;
}


Console.Write("\x1b[?25h");
