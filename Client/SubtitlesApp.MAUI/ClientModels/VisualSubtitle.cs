using System.ComponentModel;
using SubtitlesApp.Core.Models;

namespace SubtitlesApp.ClientModels;

public class VisualSubtitle : Subtitle, INotifyPropertyChanged
{
    private bool _isHighlighted;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsHighlighted
    {
        get => _isHighlighted;
        set
        {
            if (_isHighlighted != value)
            {
                _isHighlighted = value;
                OnPropertyChanged(nameof(IsHighlighted));
            }
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
