using CommunityToolkit.Mvvm.ComponentModel;
using SubtitlesApp.Core.Models;

namespace SubtitlesApp.ClientModels;

public partial class TranslationSettings : ObservableObject
{
    public required Language TargetLanguage { get; set; }

    public TimeSpan FromTime { get; set; }

    public TimeSpan ToTime { get; set; }
}
