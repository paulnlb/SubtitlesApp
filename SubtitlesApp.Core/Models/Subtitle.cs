using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SubtitlesApp.Core.Models;

public class Subtitle : INotifyPropertyChanged
{
    bool _isHighlighted = false;

    public TimeInterval TimeInterval { get; set; }

    public string Text { get; set; }

    public bool IsHighlighted
    {
        get => _isHighlighted;
        set
        {
            if (_isHighlighted != value)
            {
                _isHighlighted = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
