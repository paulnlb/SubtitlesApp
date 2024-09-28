using Microsoft.AspNetCore.Mvc;
using SubtitlesApp.Core.DTOs;
using SubtitlesServer.Application.Interfaces;

namespace SubtitlesServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WhisperMockController(
    ILogger<WhisperController> logger,
    IWaveService waveService) : ControllerBase
{
    [HttpPost("transcription")]
    public async Task<IActionResult> TranscribeAudio(
        [FromBody] TrimmedAudioDto audioMetadata,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Connected");

        var validationResult = waveService.ValidateAudio(audioMetadata);

        if (validationResult.IsFailure)
        {
            logger.LogError("Invalid audio file: {error}", validationResult.Error);

            return BadRequest(validationResult.Error);
        }

        var max = audioMetadata.EndTime - audioMetadata.StartTimeOffset;

        logger.LogInformation("Max: {Max}", max);

        var subs = new List<SubtitleDTO>();

        for (int i = 0; i < max.TotalSeconds; i += 2)
        {
            TimeSpan startTime;
            TimeSpan endTime;

            if (i % 10 == 0)
            {
                startTime = TimeSpan.FromSeconds(audioMetadata.StartTimeOffset.TotalSeconds + i + 1);
                endTime = TimeSpan.FromSeconds(audioMetadata.StartTimeOffset.TotalSeconds + i + 3);
            }
            else
            {
                startTime = TimeSpan.FromSeconds(audioMetadata.StartTimeOffset.TotalSeconds + i);
                endTime = TimeSpan.FromSeconds(audioMetadata.StartTimeOffset.TotalSeconds + i + 2);
            }

            var text = $"Subtitle ({startTime.Minutes}m, {startTime.Seconds}s) \nTest";

            var subtitle = new SubtitleDTO
            {
                TimeInterval = new TimeIntervalDTO() { StartTime = startTime, EndTime = endTime },
                Text = text
            };

            logger.LogInformation("{StartTime}: {Text}", subtitle.TimeInterval.StartTime, subtitle.Text);

            await Task.Delay(0, cancellationToken);

            subs.Add(subtitle);
        }

        logger.LogInformation("Transcribing done.");

        return Ok(subs);
    }
}
