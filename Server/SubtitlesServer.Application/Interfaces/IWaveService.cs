using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;

namespace SubtitlesServer.Application.Interfaces;

public interface IWaveService
{
    Result ValidateAudio(TrimmedAudioDto audioMetadata);
}
