using CommunityToolkit.Mvvm.ComponentModel;
using WorkspaceLauncher.Models;

namespace WorkspaceLauncher.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    public Profile Model { get; }

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _color;

    [ObservableProperty]
    private string _iconPath;

    [ObservableProperty]
    private bool _isActive;

    public ProfileViewModel(Profile model)
    {
        Model = model;
        _name = model.Name;
        _color = model.Color;
        _iconPath = model.IconPath;
    }

    partial void OnNameChanged(string value) => Model.Name = value;
    partial void OnColorChanged(string value) => Model.Color = value;
    partial void OnIconPathChanged(string value) => Model.IconPath = value;
}
