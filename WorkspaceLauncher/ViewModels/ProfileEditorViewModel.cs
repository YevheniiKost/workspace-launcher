using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;
using WorkspaceLauncher.Models;

namespace WorkspaceLauncher.ViewModels;

public partial class ProfileEditorViewModel : ObservableObject
{
    public Profile Profile { get; }

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _selectedColor;

    [ObservableProperty]
    private string _iconPath;

    [ObservableProperty]
    private ObservableCollection<LaunchItemViewModel> _items;

    public static readonly string[] PaletteColors = new[]
    {
        "#4A90E2", "#E24A4A", "#4AE24A", "#E2A34A",
        "#A34AE2", "#4AE2D6", "#E2E24A", "#E24AA3",
        "#4AA3E2", "#888888"
    };

    public IEnumerable<string> Palette => PaletteColors;

    public ProfileEditorViewModel(Profile profile)
    {
        Profile = profile;
        _name = profile.Name;
        _selectedColor = profile.Color;
        _iconPath = profile.IconPath;
        _items = new ObservableCollection<LaunchItemViewModel>(
            profile.Items.Select(i => new LaunchItemViewModel(i)));
    }

    partial void OnNameChanged(string value) => Profile.Name = value;
    partial void OnSelectedColorChanged(string value) => Profile.Color = value;
    partial void OnIconPathChanged(string value) => Profile.IconPath = value;

    [RelayCommand]
    private void SelectColor(string color)
    {
        SelectedColor = color;
    }

    [RelayCommand]
    private void AddItem()
    {
        var item = new LaunchItem();
        Profile.Items.Add(item);
        Items.Add(new LaunchItemViewModel(item));
    }

    [RelayCommand]
    private void RemoveItem(LaunchItemViewModel item)
    {
        Profile.Items.Remove(item.Model);
        Items.Remove(item);
    }

    [RelayCommand]
    private void BrowseItemPath(LaunchItemViewModel item)
    {
        if (item.Type == ItemType.Folder)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select Folder"
            };
            if (dialog.ShowDialog() == true)
                item.Path = dialog.FolderName;
        }
        else if (item.Type == ItemType.Executable)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Executable",
                Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
                item.Path = dialog.FileName;
        }
    }

    [RelayCommand]
    private void BrowseIcon()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Icon",
            Filter = "Icon files (*.ico)|*.ico|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog() == true)
            IconPath = dialog.FileName;
    }

    public bool Validate(out string error)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            error = "Profile name cannot be empty.";
            return false;
        }

        foreach (var item in Items)
        {
            if (string.IsNullOrWhiteSpace(item.Path))
            {
                error = "All items must have a path.";
                return false;
            }

            if (item.DelayMs < 0)
            {
                error = "Delay cannot be negative.";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }

    public void ApplyOrder()
    {
        Profile.Items.Clear();
        foreach (var item in Items)
            Profile.Items.Add(item.Model);
    }
}
