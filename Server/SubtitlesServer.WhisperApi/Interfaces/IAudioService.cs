using SubtitlesApp.Core.Result;

namespace SubtitlesServer.WhisperApi.Interfaces;

public interface IAudioService
{
    Result ValidateAudio(IFormFile audio);
}
