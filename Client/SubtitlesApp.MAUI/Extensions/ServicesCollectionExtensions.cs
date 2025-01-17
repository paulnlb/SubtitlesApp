using CommunityToolkit.Maui;
using SubtitlesApp.Core.Services;
using SubtitlesApp.CustomControls.Popups;
using SubtitlesApp.Interfaces;
using SubtitlesApp.Interfaces.Socket;
using SubtitlesApp.Mapper;
using SubtitlesApp.Services;
using SubtitlesApp.Services.Sockets;
using SubtitlesApp.Settings;
using SubtitlesApp.ViewModels;
using SubtitlesApp.ViewModels.Popups;
using SubtitlesApp.Views;
using UraniumUI;

namespace SubtitlesApp.Extensions;

public static class ServicesCollectionExtensions
{
    public static void AddSubtitlesAppServices(this IServiceCollection services)
    {
        #region transient
        services.AddTransient<IMediaProcessor, FfmpegService>();
        services.AddTransient<IVideoPicker, VideoPicker>();
        services.AddTransient<ISocketListener, UnixSocketListener>();
        services.AddTransient<ISocketSender, UnixSocketSender>();
        services.AddTransient<ITranscriptionService, TranscriptionService>();
        services.AddTransient<IBuiltInDialogService, BuiltInDialogService>();
        services.AddTransient<SubtitlesMapper>();
        #endregion

        #region scoped
        services.AddScoped<ISubtitlesService, SubtitlesService>();
        services.AddScoped<IdentityModel.OidcClient.Browser.IBrowser, MauiAuthenticationBrowser>();
        services.AddScoped<HttpsClientHandlerService>();
        services.AddScoped<IHttpRequestService, HttpRequestService>();
        services.AddScoped<ITranslationService, TranslationService>();
        services.AddScoped<ISubtitlesTimeSetService, SubtitlesTimeSetService>();
        #endregion

        #region singleton
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<LanguageService>();
        #endregion

        #region HttpClient
#if DEBUG
        var service = new HttpsClientHandlerService();
        var handler = service.GetPlatformMessageHandler();

        services.AddHttpClient<IHttpRequestService, HttpRequestService>().ConfigurePrimaryHttpMessageHandler(() => handler);
#else
        services.AddHttpClient<IHttpRequestService, HttpRequestService>();
#endif
        #endregion

        #region pages
        services.AddTransientWithShellRoute<PlayerWithSubtitlesPage, PlayerWithSubtitlesViewModel>("PlayerWithSubtitles");
        services.AddTransientWithShellRoute<MainPage, MainPageViewModel>("MainPage");
        services.AddTransientWithShellRoute<SettingsPage, SettingsViewModel>("settings");
        #endregion

        #region preferences
        services.AddSingleton(Preferences.Default);

#if RELEASE
        services.AddSingleton<ISettingsService, SettingsService>();
#else
        services.AddSingleton<ISettingsService, SettingsServiceDevelopment>();
#endif
        #endregion

        #region popups
        services.AddTransientPopup<SubtitlesSettingsPopup, SubtitlesSettingsPopupViewModel>();
        services.AddTransientPopup<TranslationSettingsPopup, TranslationSettingsPopupViewModel>();
        services.AddTransientPopup<InputPopup, InputPopupViewModel>();
        services.AddTransientPopup<LoadingPopup, LoadingPopupViewModel>();
        #endregion

        #region third-party
        services.AddCommunityToolkitDialogs();
        #endregion
    }
}
