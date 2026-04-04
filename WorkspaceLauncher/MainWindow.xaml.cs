using System.Windows;
using WorkspaceLauncher.ViewModels;

namespace WorkspaceLauncher;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}