namespace SubtitlesApp.Interfaces;

public interface IVideoPicker
{
    Task<string?> PickAsync();
}
