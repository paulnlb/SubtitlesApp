using SubtitlesApp.Core.Models;

namespace SubtitlesApp.ClientModels;

public class VisualSubtitle : Subtitle
{
    private bool _isHighlighted;

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
}
