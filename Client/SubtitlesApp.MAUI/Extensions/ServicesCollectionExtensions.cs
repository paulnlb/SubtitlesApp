using CommunityToolkit.Maui;
using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Core.Interfaces.HttpClients;
using SubtitlesApp.Core.Interfaces.Settings;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Services;
using SubtitlesApp.Infrastructure.Constants;
using SubtitlesApp.Infrastructure.ExternalClients;
using SubtitlesApp.Infrastructure.Interfaces.Settings;
using SubtitlesApp.Interfaces;
using SubtitlesApp.Mapper;
using SubtitlesApp.Services;
using SubtitlesApp.Settings;
using SubtitlesApp.ViewModels;
using SubtitlesApp.ViewModels.Popups;
using SubtitlesApp.Views;
using SubtitlesApp.Views.Popups;

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
        services.AddTransient<ITranscriptionApiClient, OpenAiTranscriptionClent>();
        services.AddTransient<IAudioExtractor, FfmpegNativeService>();
        services.AddTransient<SubtitlesViewModel>();
        services.AddTransient<ICustomPopupService, CustomPopupService>();
        #endregion

        #region singleton
        services.AddSingleton<LanguageService>();
        services.AddSingleton<ILlmClient, GenericLlmClient>();
        services.AddKeyedSingleton<ILlmClient, GeminiLlmClient>(LlmProviderConstants.Gemini);
        services.AddKeyedSingleton<ILlmClient, OpenAiLlmClient>(LlmProviderConstants.OpenAi);
        #endregion

        #region pages
        services.AddTransientWithShellRoute<PlayerWithSubtitlesPage, PlayerWithSubtitlesViewModel>(
            nameof(PlayerWithSubtitlesPage)
        );
        services.AddTransientWithShellRoute<MainPage, MainPageViewModel>(nameof(MainPage));
        services.AddTransientWithShellRoute<SettingsPage, SettingsViewModelNew>(nameof(SettingsPage));
        #endregion

        #region preferences
        services.AddSingleton(Preferences.Default);
        services.AddSingleton<ILlmSettings, LlmSettings>();
        services.AddSingleton<IOpenAiClientSettings, OpenAiClientSettings>();
        services.AddSingleton<IGeminiClientSettings, GeminiClientSettings>();
        services.AddSingleton<ITranscriptionClientSettings, TranscriptionClientSettings>();
        services.AddSingleton<ILlmTranslationSettings, LlmTranslationSettings>();
        services.AddSingleton<ITranscriptionSettings, TranscriptionSettings>();

        #endregion

        #region popups
        services.AddTransientPopup<RadioButtonPopup<Language>, RadioButtonPopupVm<Language>>();
        services.AddTransientPopup<RadioButtonPopup<string>, RadioButtonPopupVm<string>>();
        services.AddTransientPopup<TranscribePopup, TranscribePopupViewModel>();
        services.AddTransientPopup<TranslatePopup, TranslatePopupViewModel>();
        services.AddTransientPopup<EntryPopup, StringEntryPopupVm>();
        services.AddTransientPopup<TimeEntryPopup, TimeEntryPopupVm>();
        services.AddTransientPopup<UrlEntryPopup, UrlEntryPopupVm>();
        #endregion
    }
}
