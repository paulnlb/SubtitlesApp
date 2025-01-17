using IdentityModel.OidcClient;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Interfaces;
using Result = SubtitlesApp.Core.Result.Result;

namespace SubtitlesApp.Services;

public class AuthService : IAuthService
{
    private readonly OidcClient _oidcClient;

    public AuthService(
        ISettingsService settingsService,
        IdentityModel.OidcClient.Browser.IBrowser browser,
        HttpsClientHandlerService httpsClientHandlerService
    )
    {
        _oidcClient = CreateOidcClient(settingsService, browser, httpsClientHandlerService);
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var token = await SecureStorage.Default.GetAsync(SecurityConstants.AccessToken).ConfigureAwait(false);

        if (token == null)
        {
            return string.Empty;
        }

        return token;
    }

    public async Task<Result> LogInAsync()
    {
        try
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
        catch (Exception ex)
        {
            return ExceptionToFailedResult(ex);
        }
    }

    public async Task<Result> LogOutAsync()
    {
        var logoutRequest = new LogoutRequest
        {
            IdTokenHint = await SecureStorage.Default.GetAsync(SecurityConstants.IdToken).ConfigureAwait(false),
        };

        try
        {
            var result = await _oidcClient.LogoutAsync(logoutRequest);

            if (result.IsError)
            {
                var error = new Error(ErrorCode.AuthenticationError, result.Error);
                return Result.Failure(error);
            }

            ClearAuthDataFromStorage();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return ExceptionToFailedResult(ex);
        }
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

        await SecureStorage
            .Default.SetAsync(SecurityConstants.AccessToken, refreshTokenResult.AccessToken)
            .ConfigureAwait(false);

        return Result.Success();
    }

    private static async Task SetAuthDataToStorage(LoginResult result)
    {
        await SecureStorage.Default.SetAsync(SecurityConstants.IdToken, result.IdentityToken).ConfigureAwait(false);
        await SecureStorage.Default.SetAsync(SecurityConstants.AccessToken, result.AccessToken).ConfigureAwait(false);
        await SecureStorage.Default.SetAsync(SecurityConstants.RefreshToken, result.RefreshToken).ConfigureAwait(false);
    }

    private static void ClearAuthDataFromStorage()
    {
        SecureStorage.Default.Remove(SecurityConstants.IdToken);
        SecureStorage.Default.Remove(SecurityConstants.AccessToken);
        SecureStorage.Default.Remove(SecurityConstants.RefreshToken);
    }

    private static OidcClient CreateOidcClient(
        ISettingsService settingsService,
        IdentityModel.OidcClient.Browser.IBrowser browser,
        HttpsClientHandlerService httpsClientHandlerService
    )
    {
        var options = new OidcClientOptions
        {
            Authority = settingsService.BackendBaseUrl + settingsService.IdentityPath,
            ClientId = settingsService.OidcClientId,
            Scope = settingsService.OidcScope,
            RedirectUri = settingsService.OidcRedirectUri,
            PostLogoutRedirectUri = settingsService.OidcPostLogoutRedirectUri,
            Browser = browser,
        };

#if DEBUG
        var debugMessageHandler = httpsClientHandlerService.GetPlatformMessageHandler();

        options.HttpClientFactory = (options) =>
        {
            return new HttpClient(debugMessageHandler);
        };
#endif

        return new OidcClient(options);
    }

    private static Result ExceptionToFailedResult(Exception ex)
    {
        var errorDescription = ex.Message;

        if (ex.InnerException != null)
        {
            errorDescription += $"\n{ex.InnerException.Message}";
        }

        var error = new Error(ErrorCode.AuthenticationError, errorDescription);

        return Result.Failure(error);
    }
}
