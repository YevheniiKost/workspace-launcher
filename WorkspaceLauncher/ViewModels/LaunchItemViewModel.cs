using CommunityToolkit.Mvvm.ComponentModel;
using WorkspaceLauncher.Models;

namespace WorkspaceLauncher.ViewModels;

public partial class LaunchItemViewModel : ObservableObject
{
    public LaunchItem Model { get; }

    [ObservableProperty]
    private ItemType _type;

    [ObservableProperty]
    private string _path;

    [ObservableProperty]
    private int _delayMs;

    [ObservableProperty]
    private bool _closeOnSessionEnd;

    public LaunchItemViewModel(LaunchItem model)
    {
        Model = model;
        _type = model.Type;
        _path = model.Path;
        _delayMs = model.DelayMs;
        _closeOnSessionEnd = model.CloseOnSessionEnd;
    }

    public LaunchItemViewModel() : this(new LaunchItem()) { }

    partial void OnTypeChanged(ItemType value) => Model.Type = value;
    partial void OnPathChanged(string value) => Model.Path = value;
    partial void OnDelayMsChanged(int value) => Model.DelayMs = value;
    partial void OnCloseOnSessionEndChanged(bool value) => Model.CloseOnSessionEnd = value;
}
