using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Interfaces;

public interface IAuthService
{
    Task<bool> IsAccessTokenExpired();

    Task<Result> LogInAsync();

    Task<Result> LogOutAsync();

    Task<string?> GetAccesTokenAsync();

    Task<Result> RefreshAccessTokenAsync();
}
