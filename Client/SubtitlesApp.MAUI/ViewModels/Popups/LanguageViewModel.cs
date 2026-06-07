using CommunityToolkit.Mvvm.ComponentModel;
using SubtitlesApp.Core.Models;

namespace SubtitlesApp.ViewModels.Popups;

public partial class LanguageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private Language _value;

    [ObservableProperty]
    private bool _isChecked;
}
