using System.Text;
using WindowGroupManager.Models;

namespace WindowGroupManager.Services;

public class WindowManager
{
    public List<WindowInfo> GetWindows()
    {
        var windows = new List<WindowInfo>();

        Win32.EnumWindows((hWnd, lParam) =>
        {
            if (!Win32.IsWindowVisible(hWnd))
                return true;

            int length = Win32.GetWindowTextLength(hWnd);

            if (length == 0)
                return true;

            var builder = new StringBuilder(length + 1);

            Win32.GetWindowText(
                hWnd,
                builder,
                builder.Capacity);

            Win32.GetWindowThreadProcessId(
                hWnd,
                out uint pid);

            string processName = "";

            try
            {
                processName =
                    System.Diagnostics.Process
                    .GetProcessById((int)pid)
                    .ProcessName;
            }
            catch
            {
            }

            windows.Add(new WindowInfo
            {
                Handle = hWnd,
                Title = builder.ToString(),
                ProcessId = pid,
                ProcessName = processName
            });

            return true;
        },
        IntPtr.Zero);

        return windows;
    }
}