using System.Windows;
using WindowGroupManager.Services;
namespace WindowGroupManager.Views;

public partial class MainWindow : Window
{
    private readonly WindowManager _windowManager =
        new();

    public MainWindow()
    {
        InitializeComponent();

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
}