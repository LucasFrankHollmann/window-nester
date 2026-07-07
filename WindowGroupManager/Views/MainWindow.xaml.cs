using System.Windows;
using System.Windows.Controls;
using WindowGroupManager.Services;
using WindowGroupManager.Models;
namespace WindowGroupManager.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    /*private void WindowsList_SelectionChanged(
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
    }*/
}