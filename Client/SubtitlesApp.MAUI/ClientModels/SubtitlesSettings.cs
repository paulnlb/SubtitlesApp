using CommunityToolkit.Mvvm.ComponentModel;
using SubtitlesApp.ClientModels.Enums;
using SubtitlesApp.Core.Models;

namespace SubtitlesApp.ClientModels;

public partial class SubtitlesSettings : ObservableObject
{
    [ObservableProperty]
    List<Language> _availableLanguages;

    [ObservableProperty]
    Language _OriginalLanguage;

    [ObservableProperty]
    Language? _translateToLanguage;

    [ObservableProperty]
    bool _showTranslation;

    [ObservableProperty]
    SubtitlesCaptureMode _whichSubtitlesToTranslate;

    public SubtitlesSettings ShallowCopy()
    {
        return (SubtitlesSettings)MemberwiseClone();
    }
}
