using IdentityModel.OidcClient;
using SubtitlesApp.Constants;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Interfaces;
using System.Globalization;
using Result = SubtitlesApp.Core.Result.Result;

namespace SubtitlesApp.Services;

public class AuthService : IAuthService
{
    private readonly OidcClient _oidcClient;

    public AuthService(OidcClient oidcClient)
    {
        _oidcClient = oidcClient;
    }
    public Task<string?> GetAccesTokenAsync()
    {
        return SecureStorage.Default.GetAsync(SecurityConstants.AccessToken);
    }

    public async Task<bool> IsAccessTokenExpired()
    {
        var expiredAtStr = await SecureStorage.Default.GetAsync(SecurityConstants.AccessTokenExpiresAt);
        var expiredAt = expiredAtStr == null ? default : DateTime.Parse(expiredAtStr, CultureInfo.CurrentCulture);

        return expiredAt <= DateTime.Now;
    }

    public async Task<Result> LogInAsync()
    {
        var result = await _oidcClient.LoginAsync();

        if (result.IsError)
        {
            var error = new Error(ErrorCode.AuthenticationError, result.Error);
            return Result.Failure(error);
        }

        await SetAuthDataToStorage(result);

        return Result.Success();
    }

    public async Task<Result> LogOutAsync()
    {
        var logoutRequest = new LogoutRequest
        {
            IdTokenHint = await SecureStorage.Default.GetAsync(SecurityConstants.IdToken),
        };

        var result = await _oidcClient.LogoutAsync(logoutRequest);

        if (result.IsError)
        {
            var error = new Error(ErrorCode.AuthenticationError, result.Error);
            return Result.Failure(error);
        }

        ClearAuthDataFromStorage();

        return Result.Success();
    }

    public async Task<Result> RefreshAccessTokenAsync()
    {
        var refreshToken = await SecureStorage.Default.GetAsync(SecurityConstants.RefreshToken);
        var refreshTokenResult = await _oidcClient.RefreshTokenAsync(refreshToken);
        if (refreshTokenResult.IsError)
        {
            var error = new Error(ErrorCode.AuthenticationError, refreshTokenResult.Error);
            return Result.Failure(error);
        }

        await SecureStorage.Default.SetAsync(SecurityConstants.AccessToken, refreshTokenResult.AccessToken);
        await SecureStorage.Default.SetAsync(SecurityConstants.AccessTokenExpiresAt, refreshTokenResult.AccessTokenExpiration.ToString(CultureInfo.CurrentCulture));

        return Result.Success();
    }

    private static async Task SetAuthDataToStorage(LoginResult result)
    {
        var setIdToken = SecureStorage.Default.SetAsync(SecurityConstants.IdToken, result.IdentityToken);
        var setAccessToken = SecureStorage.Default.SetAsync(SecurityConstants.AccessToken, result.AccessToken);
        var setRefreshToken = SecureStorage.Default.SetAsync(SecurityConstants.RefreshToken, result.RefreshToken);
        var setAccessTokenExpiration = SecureStorage.Default.SetAsync(SecurityConstants.AccessTokenExpiresAt, result.AccessTokenExpiration.ToString(CultureInfo.CurrentCulture));

        await Task.WhenAll(setIdToken, setAccessToken, setRefreshToken, setAccessTokenExpiration);
    }

    private static void ClearAuthDataFromStorage()
    {
        SecureStorage.Default.Remove(SecurityConstants.IdToken);
        SecureStorage.Default.Remove(SecurityConstants.AccessToken);
        SecureStorage.Default.Remove(SecurityConstants.RefreshToken);
        SecureStorage.Default.Remove(SecurityConstants.AccessTokenExpiresAt);
    }
}
