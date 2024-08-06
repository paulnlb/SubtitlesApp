namespace SubtitlesApp.Application.Interfaces;

public interface IVideoPicker
{
    Task<string?> PickAsync();
}
