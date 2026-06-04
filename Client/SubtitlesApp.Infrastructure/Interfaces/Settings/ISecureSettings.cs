namespace SubtitlesApp.Infrastructure.Interfaces.Settings;

public interface ISecureSettings
{
    Task<string> GetSecret();

    Task SetSecret(string value);
}
