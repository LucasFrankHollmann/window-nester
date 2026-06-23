namespace WindowGroupManager.Models;

public class WindowInfo
{
    public IntPtr Handle { get; set; }

    public string Title { get; set; } = string.Empty;

    public override string ToString()
    {
        return Title;
    }
}