using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SubtitlesApp.Core.Models;

public class Subtitle : INotifyPropertyChanged
{
    private string _text;
    private string _languageCode;
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

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool SupportsLanguage(string languageCode)
    {
        return LanguageCode == languageCode;
    }

    public void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
