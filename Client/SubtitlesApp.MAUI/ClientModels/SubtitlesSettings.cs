using CommunityToolkit.Mvvm.ComponentModel;
using SubtitlesApp.ClientModels.Enums;
using SubtitlesApp.Core.Models;

namespace SubtitlesApp.ClientModels;

public partial class SubtitlesSettings : ObservableObject
{
    [ObservableProperty]
    private List<Language> _availableLanguages;

    [ObservableProperty]
    private Language _OriginalLanguage;

    [ObservableProperty]
    private Language? _translateToLanguage;

    [ObservableProperty]
    private bool _showTranslation;

    [ObservableProperty]
    private SubtitlesCaptureMode _whichSubtitlesToTranslate;

    [ObservableProperty]
    private bool _translationStreamingEnabled;

    [ObservableProperty]
    private bool _autoTranslationEnabled;

    public SubtitlesSettings ShallowCopy()
    {
        return (SubtitlesSettings)MemberwiseClone();
    }
}
