using JustASmallTui.Native;

var terminal = new ProcessTerminal();

try
{
    terminal.Start(data => { });
    while (true) ;
}
finally
{
    terminal.Stop();
}
