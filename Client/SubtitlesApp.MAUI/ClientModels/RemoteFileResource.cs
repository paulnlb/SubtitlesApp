using SubtitlesApp.ClientModels.Enums;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ClientModels;

public class RemoteFileResource : IFileResource
{
    public RemoteFileResource(string uri)
    {
        Name = new Uri(uri).Segments.Last().TrimEnd('/');
        Id = uri;
        Uri = uri;
    }

    public FileResourceType Type => FileResourceType.Remote;

    public string Id { get; private set; }

    public string Name { get; private set; }

    public string Uri { get; private set; }

    public string? AbsolutePath { get; private set; }

    public void Dispose() { }
}
