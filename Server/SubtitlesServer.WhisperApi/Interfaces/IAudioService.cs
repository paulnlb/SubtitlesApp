using SubtitlesServer.Shared.Result;

namespace SubtitlesServer.WhisperApi.Interfaces;

public interface IAudioService
{
    Result ValidateAudio(IFormFile audio);
}
