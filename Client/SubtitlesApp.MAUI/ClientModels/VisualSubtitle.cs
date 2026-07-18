using System.ComponentModel;
using SubtitlesApp.Core.Models;

namespace SubtitlesApp.ClientModels;

public class VisualSubtitle : Subtitle, INotifyPropertyChanged
{
    private bool _isHighlighted;
    private string _additionalInfo = string.Empty;

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

    public string AdditionalInfo
    {
        get => _additionalInfo;
        set
        {
            if (_additionalInfo != value)
            {
                _additionalInfo = value;
                OnPropertyChanged(nameof(AdditionalInfo));
            }
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
