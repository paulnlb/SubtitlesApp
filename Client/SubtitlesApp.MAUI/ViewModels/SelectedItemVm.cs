using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitlesApp.ViewModels;

public partial class SelectedItemVm<T> : ObservableObject
{
    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private T _value;

    [ObservableProperty]
    private bool _isSelected;
}
