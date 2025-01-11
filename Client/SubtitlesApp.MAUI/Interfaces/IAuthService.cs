using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Interfaces;

public interface IAuthService
{
    Task<Result> LogInAsync();

    Task<Result> LogOutAsync();

    Task<string> GetAccessTokenAsync();

    Task<Result> RefreshAccessTokenAsync();
}
