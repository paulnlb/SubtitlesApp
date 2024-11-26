﻿namespace SubtitlesApp.Interfaces;

/// <summary>
///     App settings
/// </summary>
public interface ISettingsService
{
    string BackendBaseUrl { get; set; }

    string WhisperAddress { get; set; }

    string UnixSocketPath { get; set; }

    int TranscribeBufferLength { get; set; }

    string CallBackUrl { get; set; }
}
