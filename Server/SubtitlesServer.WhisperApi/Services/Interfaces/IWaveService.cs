using SubtitlesApp.Core.Result;

namespace SubtitlesServer.WhisperApi.Services.Interfaces;

public interface IWaveService
{
    Result ValidateAudio(IFormFile audio);
}
