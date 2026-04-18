using CommunityToolkit.Mvvm.ComponentModel;
using SubtitlesApp.Core.Models;

namespace SubtitlesApp.ClientModels;

public partial class TranscriptionSettings : ObservableObject
{
    [ObservableProperty]
    private Language _subtitlesLanguage;

    [ObservableProperty]
    private TimeSpan _fromTime;

    [ObservableProperty]
    private TimeSpan _toTime;
}
