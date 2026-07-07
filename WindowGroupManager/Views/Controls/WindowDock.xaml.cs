using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
    private readonly WindowManager _windowManager =
        new();

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
        // Open a small popup window (dropdown-like) under the Attach button
        var owner = Window.GetWindow(this);

        var popup = new Window
        {
            Owner = owner,
            WindowStyle = WindowStyle.ToolWindow,
            ShowInTaskbar = false,
            ResizeMode = ResizeMode.NoResize,
            Width = 320,
            Height = 300,
            Topmost = true
        };

        // Build list of windows
        var list = new ListBox();
        list.DisplayMemberPath = "ToString";
        list.ItemsSource = _windowManager.GetWindows();
        list.MouseDoubleClick += (s, ev) =>
        {
            if (list.SelectedItem is WindowInfo win)
            {
                selectedWindow = win;
                popup.Close();
                AttachWindow(win.Handle);
            }
        };

        // Optional select button
        var selectButton = new Button { Content = "Select", Margin = new Thickness(4), HorizontalAlignment = HorizontalAlignment.Right };
        selectButton.Click += (s, ev) =>
        {
            if (list.SelectedItem is WindowInfo win)
            {
                selectedWindow = win;
                popup.Close();
                AttachWindow(win.Handle);
            }
        };

        var panel = new DockPanel();
        DockPanel.SetDock(selectButton, Dock.Bottom);
        panel.Children.Add(selectButton);
        panel.Children.Add(list);

        popup.Content = panel;

        // Position popup under the Attach button if possible
        if (sender is FrameworkElement fe)
        {
            var pt = fe.PointToScreen(new System.Windows.Point(0, fe.ActualHeight));
            // Convert screen to owner window coordinates
            var presentationSource = System.Windows.PresentationSource.FromVisual(owner);
            if (presentationSource != null)
            {
                var transform = presentationSource.CompositionTarget.TransformFromDevice;
                var target = transform.Transform(pt);
                popup.Left = target.X;
                popup.Top = target.Y;
            }
            else
            {
                popup.Left = pt.X;
                popup.Top = pt.Y;
            }
        }

        popup.Show();
    }

    private void LoadWindows()
    {
        //ItemsSource = _windowManager.GetWindows();
    }
}