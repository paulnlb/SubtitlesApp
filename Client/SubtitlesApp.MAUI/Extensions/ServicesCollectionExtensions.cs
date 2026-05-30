using CommunityToolkit.Maui;
using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Core.Interfaces.HttpClients;
using SubtitlesApp.Core.Interfaces.Settings;
using SubtitlesApp.Core.Services;
using SubtitlesApp.CustomControls.Popups;
using SubtitlesApp.Infrastructure.HttpClients;
using SubtitlesApp.Infrastructure.Interfaces.Settings;
using SubtitlesApp.Interfaces;
using SubtitlesApp.Interfaces.Settings;
using SubtitlesApp.Mapper;
using SubtitlesApp.Services;
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
        services.AddTransient<IVideoPicker, VideoPicker>();
        services.AddTransient<IBuiltInDialogService, BuiltInDialogService>();
        services.AddTransient<SubtitlesMapper>();
        services.AddTransient<ITranscriptionService, TranscriptionService>();
        services.AddTransient<ITranslationService, LlmTranslationService>();
        services.AddTransient<ILlmClient, OpenAiLlmClient>();
        services.AddTransient<ITranscriptionApiClient, OpenAiTranscriptionClent>();
        services.AddTransient<IAudioExtractor, FfmpegNativeService>();
        services.AddTransient<CaptionsViewModel>();
        #endregion

        #region singleton
        services.AddSingleton<LanguageService>();
        #endregion

        #region pages
        services.AddTransientWithShellRoute<PlayerWithSubtitlesPage, PlayerWithSubtitlesViewModel>("PlayerWithSubtitles");
        services.AddTransientWithShellRoute<MainPage, MainPageViewModel>("MainPage");
        services.AddTransientWithShellRoute<SettingsPage, SettingsViewModel>("settings");
        #endregion

        #region preferences
        services.AddSingleton(Preferences.Default);
        services.AddSingleton<IOpenAiSettings, OpenAiSettings>();
        services.AddSingleton<ITranscriptionClientSettings, TranscriptionClientSettings>();
        services.AddSingleton<ILlmTranslationSettings, LlmTranslationSettings>();
        services.AddSingleton<ITranscriptionSettings, TranscriptionSettings>();

        // Use transient because layout settings have to be reset to defaults after the page is reopened
        services.AddTransient<ILayoutSettings, LayoutSettings>();

        #endregion

        #region popups
        services.AddTransientPopup<InputPopup, InputPopupViewModel>();
        services.AddTransientPopup<LoadingPopup, LoadingPopupViewModel>();
        services.AddTransientPopup<TranscribePopup, TranscribePopupViewModel>();
        services.AddTransientPopup<TranslatePopup, TranslatePopupViewModel>();
        #endregion

        #region third-party
        services.AddCommunityToolkitDialogs();
        #endregion
    }
}
