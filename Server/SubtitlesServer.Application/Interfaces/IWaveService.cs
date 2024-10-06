using SubtitlesApp.Core.Result;

namespace SubtitlesServer.Application.Interfaces;

public interface IWaveService
{
    Result ValidateAudio(byte[] audio);
}
