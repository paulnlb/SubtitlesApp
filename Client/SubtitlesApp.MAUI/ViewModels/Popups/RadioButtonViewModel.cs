using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitlesApp.ViewModels.Popups;

public partial class RadioButtonViewModel<T> : ObservableObject
{
    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private T _value;

    [ObservableProperty]
    private bool _isChecked;
}
