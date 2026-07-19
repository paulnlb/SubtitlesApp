using SubtitlesApp.ClientModels.Enums;

namespace SubtitlesApp.Interfaces;

public interface IFileResource : IDisposable
{
    FileResourceType Type { get; }

    string Id { get; }

    string Name { get; }

    string Uri { get; }

    public string? AbsolutePath { get; }
}
