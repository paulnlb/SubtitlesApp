﻿using SubtitlesApp.Core.Models;

namespace SubtitlesServer.Application.Interfaces;

public interface IWhisperService
{
    IAsyncEnumerable<Subtitle> TranscribeAudioAsync(
        MemoryStream audioStream,
        CancellationToken cancellationToken = default);
}
