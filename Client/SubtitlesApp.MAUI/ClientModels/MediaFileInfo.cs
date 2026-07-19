using SubtitlesApp.ClientModels.Enums;

namespace SubtitlesApp.ClientModels;

public record MediaFileInfo(FileResourceType Type, string Id, string Name, string Uri);
