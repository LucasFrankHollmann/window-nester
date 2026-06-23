using System.Windows;
using System.Windows.Controls;
using WindowGroupManager.Services;

namespace WindowGroupManager.Views.Controls;


public partial class WindowDock : UserControl
{
    private IntPtr _windowHandle;


    public WindowDock()
    {
        InitializeComponent();

        SizeChanged +=
            WindowDock_SizeChanged;

        Loaded +=
            WindowDock_Loaded;
    }

    private void WindowDock_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        ResizeWindow();
    }

    public void AttachWindow(IntPtr handle)
    {
        _windowHandle = handle;


        if (!Host.IsInitialized)
        {
            Host.Initialized += Host_Initialized;
            return;
        }


        AttachInternal();
    }

    private void Host_Initialized(
        object? sender,
        EventArgs e)
    {
        AttachInternal();
    }

    private void AttachInternal()
    {
        Win32.SetParent(
            _windowHandle,
            Host.Handle);


        var style =
            Win32.GetWindowLongPtr(
                _windowHandle,
                Win32.GWL_STYLE)
            .ToInt64();


        style |= Win32.WS_CHILD;
        style |= Win32.WS_VISIBLE;


        Win32.SetWindowLongPtr(
            _windowHandle,
            Win32.GWL_STYLE,
            new IntPtr(style));


        ResizeWindow();
    }

    private void WindowDock_SizeChanged(
        object sender,
        SizeChangedEventArgs e)
    {
        ResizeWindow();
    }

    private void ResizeWindow()
    {
        if (_windowHandle == IntPtr.Zero)
            return;


        Win32.MoveWindow(
            _windowHandle,
            0,
            0,
            (int)ActualWidth,
            (int)ActualHeight,
            true);
    }
}