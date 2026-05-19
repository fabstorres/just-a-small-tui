namespace JustASmallTui.Components;

public interface IComponent
{
    bool Focus { get; set; }
    string[] Render(int width);
    void Invalidate();
    void HandleKeyInput(string data);
}
