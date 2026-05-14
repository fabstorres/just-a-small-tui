var textInput = new JustASmallTui.TextInput();
while (true)
{
    var key = Console.ReadKey(intercept: true);
    if (key.Key == ConsoleKey.C && key.Modifiers == ConsoleModifiers.Control)
    {
        break;
    }
    textInput.HandleInput(key);
    Console.Clear();
    foreach (var line in textInput.Render(Console.WindowWidth))
    {
        Console.WriteLine(line);
    }
}

namespace JustASmallTui
{
    public interface IComponent
    {
        bool Focused { get; set; }
        string[] Render(int width);
        void HandleInput(ConsoleKeyInfo keyInfo);
        bool WantsKeyRelease { get; }
        void Invalidate();
    }

    public class TextInput : IComponent
    {
        private string[] lines = [""];

        public string[] Render(int width)
        {
            return lines;
        }

        public void Invalidate()
        {

        }

        public bool Focused { get; set; }

        public bool WantsKeyRelease => false;

        public void HandleInput(ConsoleKeyInfo keyInfo)
        {
            //TODO: ConsoleKeyInfo is unreliable and may not always reflect the actual key pressed
            if (keyInfo.Key == ConsoleKey.Enter && keyInfo.Modifiers == ConsoleModifiers.Shift)
            {
                lines = [.. lines, ""];
                Invalidate();
            }
            else if (keyInfo.Key == ConsoleKey.Enter)
            {
                lines = [""];
                Invalidate();
            }
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (lines[^1].Length > 0)
                {
                    lines[^1] = lines[^1][..^1];
                    Invalidate();
                }
            }
            else
            {
                lines[^1] += keyInfo.KeyChar;
                Invalidate();
            }
        }
    }
}
