using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SubtitlesApp.Core.Models;

public class Subtitle : INotifyPropertyChanged
{
    private string _text;
    private string _languageCode;
    private Translation? _translation;

    private bool _isTranslated;

    public TimeInterval TimeInterval { get; set; }

    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }
    }
    public string LanguageCode
    {
        get => _languageCode;
        set
        {
            if (_languageCode != value)
            {
                _languageCode = value;
                OnPropertyChanged(nameof(LanguageCode));
            }
        }
    }

    public bool IsTranslated
    {
        get => _isTranslated;
        private set
        {
            if (_isTranslated != value)
            {
                _isTranslated = value;
                OnPropertyChanged(nameof(IsTranslated));
            }
        }
    }

    public Translation? Translation
    {
        get => _translation;
        set
        {
            if (_translation != value)
            {
                _translation = value;
                OnPropertyChanged(nameof(Translation));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void SwitchToTranslation()
    {
        // Do nothing if already translated
        if (!IsTranslated)
        {
            ToggleTranslation();
        }
    }

    public void RestoreOriginalLanguage()
    {
        // Do nothing if not translated
        if (IsTranslated)
        {
            ToggleTranslation();
        }
    }

    public bool SupportsLanguage(string languageCode)
    {
        return LanguageCode == languageCode || Translation?.LanguageCode == languageCode;
    }

    public void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void ToggleTranslation()
    {
        if (Translation == null)
        {
            return;
        }

        var translatedText = Translation.Text;
        var translateToLanguageCode = Translation.LanguageCode;

        Translation = new Translation
        {
            LanguageCode = LanguageCode,
            Text = Text,
        };

        LanguageCode = translateToLanguageCode;
        Text = translatedText;
        IsTranslated = !IsTranslated;
    }
}
