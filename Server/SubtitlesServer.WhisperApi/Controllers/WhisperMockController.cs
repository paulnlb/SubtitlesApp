﻿using Microsoft.AspNetCore.Mvc;
using SubtitlesApp.Core.DTOs;
using SubtitlesServer.WhisperApi.Models;
using SubtitlesServer.WhisperApi.Services.Interfaces;

namespace SubtitlesServer.WhisperApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WhisperMockController(ILogger<WhisperMockController> logger, IWaveService waveService) : ControllerBase
{
    [HttpHead("transcription")]
    public IActionResult PingTranscribeAudio()
    {
        return Ok();
    }

    [HttpPost("transcription")]
    public IActionResult TranscribeAudio(WhisperRequestModel requestModel)
    {
        logger.LogInformation("Connected");

        var validationResult = waveService.ValidateAudio(requestModel.AudioFile);

        if (validationResult.IsFailure)
        {
            logger.LogError("Invalid audio file: {error}", validationResult.Error);

            return BadRequest(validationResult.Error);
        }

        var max = 60;

        logger.LogInformation("Max: {Max}", max);

        var subs = new List<SubtitleDto>();

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

            var subtitle = new SubtitleDto
            {
                StartTime = startTime,
                EndTime = endTime,
                Text = text,
                LanguageCode = requestModel.LanguageCode,
            };

            logger.LogInformation("{StartTime}: {Text}", subtitle.StartTime, subtitle.Text);

            subs.Add(subtitle);
        }

        logger.LogInformation("Transcribing done.");

        return Ok(subs);
    }
}
