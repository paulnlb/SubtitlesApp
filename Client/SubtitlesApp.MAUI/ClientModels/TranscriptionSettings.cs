using CommunityToolkit.Mvvm.ComponentModel;
using SubtitlesApp.Core.Models;

namespace SubtitlesApp.ClientModels;

public partial class TranscriptionSettings : ObservableObject
{
    public Language SubtitlesLanguage { get; set; }

    public TimeSpan FromTime { get; set; }

    public TimeSpan ToTime { get; set; }
}
