using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SubtitlesApp.Core.DTOs;
using SubtitlesServer.Application.Interfaces;

namespace SubtitlesServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WhisperController(
    ILogger<WhisperController> logger,
    ITranscriptionService transcriptionService) : ControllerBase
{
    [HttpPost("transcription")]
    public async Task<List<SubtitleDTO>> TranscribeAudio(
        [FromBody] TrimmedAudioDto audioMetadata,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Connected");

        var subtitles = await transcriptionService.TranscribeAudioAsync(
            audioMetadata,
            cancellationToken);

        logger.LogInformation("Transcribing done.");

        return subtitles;
    }
}
