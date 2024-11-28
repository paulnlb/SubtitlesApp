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
    public async Task<string> GetAccessTokenAsync()
    {
        var token = await SecureStorage.Default.GetAsync(SecurityConstants.AccessToken).ConfigureAwait(false);

        if (token == null)
        {
            return string.Empty;
        }

        if (!await IsAccessTokenExpired(5))
        {
            return token;
        }

        var refreshTokenResult = await RefreshAccessTokenAsync();

        if (refreshTokenResult.IsFailure)
        {
            return string.Empty;
        }

        return await GetAccessTokenAsync();
    }

    public async Task<bool> IsAccessTokenExpired(uint minutesBeforeExpiration = 0)
    {
        var expiredAtStr = await SecureStorage.Default.GetAsync(SecurityConstants.AccessTokenExpiresAt).ConfigureAwait(false);
        var expiredAt = expiredAtStr == null ? default : DateTime.Parse(expiredAtStr, CultureInfo.CurrentCulture);

        return expiredAt < DateTime.Now.AddMinutes(minutesBeforeExpiration);
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
            IdTokenHint = await SecureStorage.Default.GetAsync(SecurityConstants.IdToken).ConfigureAwait(false),
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
        var refreshToken = await SecureStorage.Default.GetAsync(SecurityConstants.RefreshToken).ConfigureAwait(false);
        var refreshTokenResult = await _oidcClient.RefreshTokenAsync(refreshToken);
        if (refreshTokenResult.IsError)
        {
            var error = new Error(ErrorCode.AuthenticationError, refreshTokenResult.Error);
            return Result.Failure(error);
        }

        await SecureStorage.Default.SetAsync(SecurityConstants.AccessToken, refreshTokenResult.AccessToken).ConfigureAwait(false);
        await SecureStorage.Default.SetAsync(SecurityConstants.AccessTokenExpiresAt, refreshTokenResult.AccessTokenExpiration.ToString(CultureInfo.CurrentCulture)).ConfigureAwait(false);

        return Result.Success();
    }

    private static async Task SetAuthDataToStorage(LoginResult result)
    {
        await SecureStorage.Default.SetAsync(SecurityConstants.IdToken, result.IdentityToken).ConfigureAwait(false);
        await SecureStorage.Default.SetAsync(SecurityConstants.AccessToken, result.AccessToken).ConfigureAwait(false);
        await SecureStorage.Default.SetAsync(SecurityConstants.RefreshToken, result.RefreshToken).ConfigureAwait(false);
        await SecureStorage.Default.SetAsync(SecurityConstants.AccessTokenExpiresAt, result.AccessTokenExpiration.ToString(CultureInfo.CurrentCulture)).ConfigureAwait(false);
    }

    private static void ClearAuthDataFromStorage()
    {
        SecureStorage.Default.Remove(SecurityConstants.IdToken);
        SecureStorage.Default.Remove(SecurityConstants.AccessToken);
        SecureStorage.Default.Remove(SecurityConstants.RefreshToken);
        SecureStorage.Default.Remove(SecurityConstants.AccessTokenExpiresAt);
    }
}
