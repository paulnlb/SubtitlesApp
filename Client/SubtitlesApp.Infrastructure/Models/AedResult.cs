using SubtitlesApp.Core.Models;

namespace SubtitlesApp.Infrastructure.Models;

public class AedResult
{
    public required List<TimeInterval> VoiceSegments { get; set; }

    public required List<TimeInterval> NonVoiceSegments { get; set; }
}
