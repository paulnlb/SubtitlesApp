﻿using Whisper.net.Ggml;

namespace SubtitlesServer.Infrastructure.Configs;

public class WhisperConfigs
{
    public GgmlType ModelSize { get; set; }

    public QuantizationType QuantizationType { get; set; }

    public string BinaryModelFolder { get; set; }
}
