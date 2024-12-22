using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.DTOs;
using SubtitlesServer.Application.Interfaces;

namespace SubtitlesServer.WhisperApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class WhisperMockController(
    ILogger<WhisperMockController> logger,
    IWaveService waveService) : ControllerBase
{
    [HttpPost("transcription")]
    public IActionResult TranscribeAudio(
        IFormFile audioFile,
        [FromHeader(Name = RequestConstants.SubtitlesLanguageHeader)] string subtitlesLanguageCode)
    {
        logger.LogInformation("Connected");

        using var audioStream = audioFile.OpenReadStream();
        using var binaryReader = new BinaryReader(audioStream);
        var audioBytes = binaryReader.ReadBytes((int)audioStream.Length);

        var validationResult = waveService.ValidateAudio(audioBytes);

        if (validationResult.IsFailure)
        {
            logger.LogError("Invalid audio file: {error}", validationResult.Error);

            return BadRequest(validationResult.Error);
        }

        var max = 60;

        logger.LogInformation("Max: {Max}", max);

        var subs = new List<SubtitleDTO>();

        for (int i = 0; i < max; i += 2)
        {
            TimeSpan startTime;
            TimeSpan endTime;

            if (i % 10 == 0)
            {
                startTime = TimeSpan.FromSeconds(i + 1);
                endTime = TimeSpan.FromSeconds(i + 3);
            }
            else
            {
                startTime = TimeSpan.FromSeconds(i);
                endTime = TimeSpan.FromSeconds(i + 2);
            }

            var text = $"Subtitle ({startTime.Minutes}m, {startTime.Seconds}s) \nTest";

            var subtitle = new SubtitleDTO
            {
                TimeInterval = new TimeIntervalDTO() { StartTime = startTime, EndTime = endTime },
                Text = text,
                LanguageCode = subtitlesLanguageCode,
                IsTranslated = false,
            };

            logger.LogInformation("{StartTime}: {Text}", subtitle.TimeInterval.StartTime, subtitle.Text);

            subs.Add(subtitle);
        }

        logger.LogInformation("Transcribing done.");

        return Ok(subs);
    }
}
