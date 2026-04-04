using System.Windows;
using WorkspaceLauncher.ViewModels;

namespace WorkspaceLauncher.Views;

public partial class ProfileEditorWindow : Window
{
    private readonly ProfileEditorViewModel _viewModel;

    public ProfileEditorWindow(ProfileViewModel profileVm)
    {
        InitializeComponent();
        _viewModel = new ProfileEditorViewModel(profileVm.Model);
        DataContext = _viewModel;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!_viewModel.Validate(out var error))
        {
            MessageBox.Show(error, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _viewModel.ApplyOrder();
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
