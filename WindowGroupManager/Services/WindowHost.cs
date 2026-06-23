using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace WindowGroupManager.Services;

public class WindowHost : HwndHost
{
    [DllImport("user32.dll",
        CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateWindowEx(
        int dwExStyle,
        string lpClassName,
        string lpWindowName,
        int dwStyle,
        int x,
        int y,
        int width,
        int height,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);


    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(
        IntPtr hwnd);


    private IntPtr _handle;


    protected override HandleRef BuildWindowCore(
        HandleRef hwndParent)
    {
        _handle = CreateWindowEx(
            0,
            "STATIC",
            "",
            0x40000000 | 0x10000000,
            0,
            0,
            100,
            100,
            hwndParent.Handle,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);


        return new HandleRef(
            this,
            _handle);
    }


    protected override void DestroyWindowCore(
        HandleRef hwnd)
    {
        DestroyWindow(hwnd.Handle);
    }
}