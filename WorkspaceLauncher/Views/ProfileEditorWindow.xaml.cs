using System.Windows;
using WorkspaceLauncher.ViewModels;

namespace WorkspaceLauncher.Views;

public partial class ProfileEditorWindow : Window
{
    private readonly ProfileEditorViewModel _viewModel;
    private readonly ProfileViewModel _profileVm;

    public ProfileEditorWindow(ProfileViewModel profileVm)
    {
        InitializeComponent();
        _profileVm = profileVm;
        _viewModel = new ProfileEditorViewModel(profileVm.Model);
        DataContext = _viewModel;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!_viewModel.Validate(out string error))
        {
            MessageBox.Show(error, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _viewModel.ApplyOrder();

        _profileVm.Name = _viewModel.Name;
        _profileVm.Color = _viewModel.SelectedColor;
        _profileVm.IconPath = _viewModel.IconPath;

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
