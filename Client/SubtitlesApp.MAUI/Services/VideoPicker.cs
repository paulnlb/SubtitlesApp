using SubtitlesApp.Interfaces;

namespace SubtitlesApp.Services;

public partial class VideoPicker : IVideoPicker
{
    public partial Task<string?> PickAsync();
}
