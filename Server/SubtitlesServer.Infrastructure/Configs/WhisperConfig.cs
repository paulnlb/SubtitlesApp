using Whisper.net.Ggml;

namespace SubtitlesServer.Infrastructure.Configs;

public class WhisperConfig
{
    public GgmlType ModelSize { get; set; }

    public QuantizationType QuantizationType { get; set; }

    public string BinaryModelFolder { get; set; }
}
