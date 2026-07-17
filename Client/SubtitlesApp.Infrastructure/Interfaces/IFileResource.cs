namespace SubtitlesApp.Infrastructure.Interfaces;

public interface IFileResource : IDisposable
{
    string Path { get; }
}
