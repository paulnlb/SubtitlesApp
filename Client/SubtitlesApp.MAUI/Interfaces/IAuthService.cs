using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Checks if the access token is expired. If the token is expired, it returns true.
    /// </summary>
    /// <param name="minutesBeforeExpiration">The number of minutes before the token expires.</param>
    /// <returns></returns>
    Task<bool> IsAccessTokenExpired(uint minutesBeforeExpiration = 0);

    Task<Result> LogInAsync();

    Task<Result> LogOutAsync();

    Task<string> GetAccessTokenAsync();

    Task<Result> RefreshAccessTokenAsync();
}
