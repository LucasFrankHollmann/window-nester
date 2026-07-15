using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Interop;
using WindowGroupManager.Services;
using WindowGroupManager.Models;

namespace WindowGroupManager.Views.Controls;

public class InvertedBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool bValue)
        {
            return bValue ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible ? false : true;
        }
        return true;
    }
}


public partial class WindowDock : UserControl, INotifyPropertyChanged
{
    private IntPtr _windowHandle;
    private WindowInfo? selectedWindow;
    private bool _isEmpty = true; // Por padrão, vazio (mostra StackPanel)
    private readonly WindowManager _windowManager = new();
    private Popup? _windowsPopup;

    public bool IsEmpty
    {
        get => _isEmpty;
        set
        {
            if (_isEmpty != value)
            {
                _isEmpty = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public WindowDock()
    {
        InitializeComponent();
        DataContext = this;

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
        IsEmpty = false;

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

    private void RefreshButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        LoadWindows();
    }

    private void AttachWindowButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not FrameworkElement button)
            return;

        // Close if already open
        if (_windowsPopup?.IsOpen == true)
        {
            _windowsPopup.IsOpen = false;
            return;
        }

        // Get main window handle to exclude from list
        var mainWindow = Window.GetWindow(this);
        var mainWindowHandle = new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle;

        // Filter out the main window and the Explorer shell (desktop/taskbar).
        // Allow normal File Explorer windows (CabinetWClass, etc.).
        var windows = _windowManager.GetWindows()
            .Where(w => w.Handle != mainWindowHandle)
            .Where(w => !(string.Equals(w.ProcessName, "explorer", StringComparison.OrdinalIgnoreCase)
                          && (string.Equals(w.ClassName, "Progman", StringComparison.OrdinalIgnoreCase)
                              || string.Equals(w.ClassName, "WorkerW", StringComparison.OrdinalIgnoreCase)
                              || string.Equals(w.Title, "Program Manager", StringComparison.OrdinalIgnoreCase))))
            .ToList();

        // Create popup content (ListBox)
        var list = new ListBox
        {
            ItemsSource = windows,
            MinWidth = 280,
            MaxHeight = 250
        };

        // On double-click or item selected
        list.MouseDoubleClick += (s, ev) =>
        {
            if (list.SelectedItem is WindowInfo win)
            {
                selectedWindow = win;
                _windowsPopup!.IsOpen = false;
                AttachWindow(win.Handle);
            }
        };

        // Create popup
        _windowsPopup = new Popup
        {
            Child = list,
            PlacementTarget = button,
            Placement = PlacementMode.Bottom,
            PopupAnimation = PopupAnimation.Slide,
            AllowsTransparency = false,
            StaysOpen = false,
            IsOpen = true
        };

        // Focus list for keyboard navigation
        list.Focus();
    }

    private void DetachWindowButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_windowHandle == IntPtr.Zero)
            return;

        DetachWindow();
    }

    private void DetachWindow()
    {
        try
        {
            // remove parent
            Win32.SetParent(_windowHandle, IntPtr.Zero);

            // remove WS_CHILD style
            var style = Win32.GetWindowLongPtr(_windowHandle, Win32.GWL_STYLE).ToInt64();
            style &= ~Win32.WS_CHILD;
            Win32.SetWindowLongPtr(_windowHandle, Win32.GWL_STYLE, new IntPtr(style));
        }
        catch
        {
            // ignore errors
        }

        _windowHandle = IntPtr.Zero;
        IsEmpty = true;
    }

    private void LoadWindows()
    {
        //ItemsSource = _windowManager.GetWindows();
    }
}