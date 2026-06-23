using System.Windows;
using System.Windows.Controls;
using WindowGroupManager.Services;
using WindowGroupManager.Models;
namespace WindowGroupManager.Views;

public partial class MainWindow : Window
{
    private WindowInfo? selectedWindow;
    private WindowHost? _host;
    private readonly WindowManager _windowManager =
        new();

    public MainWindow()
    {
        InitializeComponent();

        _host = new WindowHost();

        HostArea.Child = _host;


        WindowsList.SelectionChanged +=
            WindowsList_SelectionChanged;


        LoadWindows();
    }

    private void RefreshButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        LoadWindows();
    }

    private void LoadWindows()
    {
        WindowsList.ItemsSource =
            _windowManager.GetWindows();
    }

    private void WindowsList_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        selectedWindow =
            WindowsList.SelectedItem as WindowInfo;


        if (selectedWindow == null)
            return;


        SelectedWindowInfo.Text =
            $"HWND: 0x{selectedWindow.Handle.ToInt64():X}\n" +
            $"PID: {selectedWindow.ProcessId}\n" +
            $"Processo: {selectedWindow.ProcessName}";
    }

    private void AttachButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (selectedWindow == null)
        {
            MessageBox.Show(
                "Selecione uma janela");
            return;
        }


        Win32.SetParent(
            selectedWindow.Handle,
            _host!.Handle);


        Win32.MoveWindow(
            selectedWindow.Handle,
            0,
            0,
            (int)HostArea.ActualWidth,
            (int)HostArea.ActualHeight,
            true);
    }
}