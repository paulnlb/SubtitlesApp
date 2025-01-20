namespace SubtitlesServer.Shared.Configs;

public class JwtConfig
{
    public string Authority { get; set; } = default!;

    public string ValidIssuer { get; set; } = default!;

    public bool ValidateAudience { get; set; }
}
