using SubtitlesApp.ClientModels.Enums;

namespace SubtitlesApp.ClientModels;

public class TimePreset
{
    public required string Title { get; init; }

    public required TimeSpan Time { get; init; }

    public required TimePresetType Type { get; init; }
}
