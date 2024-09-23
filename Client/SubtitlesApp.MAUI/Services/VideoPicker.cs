using SubtitlesApp.Maui.Interfaces;

namespace SubtitlesApp.Services;

public partial class VideoPicker : IVideoPicker
{
    public partial Task<string?> PickAsync();
}
