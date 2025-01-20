using Whisper.net.Ggml;

namespace SubtitlesServer.WhisperApi.Configs;

public class WhisperConfig
{
    public GgmlType ModelSize { get; set; }

    public QuantizationType QuantizationType { get; set; }

    public string BinariesPath { get; set; }
}
