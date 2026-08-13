using CommunityToolkit.Maui;
using Serilog;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Constants;
using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Core.Interfaces.ExternalClients;
using SubtitlesApp.Core.Interfaces.Repositories;
using SubtitlesApp.Core.Interfaces.Settings;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Services;
using SubtitlesApp.Infrastructure.Constants;
using SubtitlesApp.Infrastructure.ExternalClients;
using SubtitlesApp.Infrastructure.Interfaces.Settings;
using SubtitlesApp.Infrastructure.Repositories;
using SubtitlesApp.Infrastructure.Services;
using SubtitlesApp.Interfaces;
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
        services.AddTransient<IBuiltInDialogService, BuiltInDialogService>();
        services.AddTransient<ITranscriptionService, WhisperTranscriptionService>();
        services.AddTransient<ITranslationService, LlmTranslationService>();
        services.AddTransient<OpenAiTranscriptionClent>();
        services.AddTransient<SubtitlesViewModel>();
        services.AddTransient<ICustomPopupService, CustomPopupService>();
        services.AddTransient<LocalFileManager>();
        services.AddTransient<SubtitlesFileService>();
        #endregion

        #region singleton
        services.AddSingleton<LanguageService>();
        services.AddSingleton<ILlmClient, GenericLlmClient>();
        services.AddKeyedSingleton<ILlmClient, GeminiLlmClient>(LlmProviderConstants.Gemini);
        services.AddKeyedSingleton<ILlmClient, OpenAiLlmClient>(LlmProviderConstants.OpenAi);
        services.AddSingleton<ISubtitlesCache, SubtitlesCache>();
        services.AddSingleton<IVideoSessionRepository, VideoSessionRepository>();
        #endregion

        #region pages
        services.AddTransientWithShellRoute<PlayerWithSubtitlesPage, PlayerWithSubtitlesViewModel>(
            nameof(PlayerWithSubtitlesPage)
        );
        services.AddTransientWithShellRoute<MainPage, MainPageViewModel>(nameof(MainPage));
        services.AddTransientWithShellRoute<SettingsPage, SettingsViewModelNew>(nameof(SettingsPage));
        services.AddTransientWithShellRoute<TranscriptionSettingsPage, TranscriptionSettingsVm>(
            nameof(TranscriptionSettingsPage)
        );
        services.AddTransientWithShellRoute<LogsPage, LogsPageViewModel>(nameof(LogsPage));
        #endregion

        #region preferences
        services.AddSingleton(Preferences.Default);
        services.AddSingleton<ILlmSettings, LlmSettings>();
        services.AddSingleton<IOpenAiClientSettings, OpenAiClientSettings>();
        services.AddSingleton<IGeminiClientSettings, GeminiClientSettings>();
        services.AddSingleton<ITranscriptionClientSettings, TranscriptionClientSettings>();
        services.AddSingleton<ILlmTranslationSettings, LlmTranslationSettings>();
        services.AddSingleton<ITranscriptionSettings, Settings.TranscriptionSettings>();
        services.AddSingleton<IPersistenceSettings, PersistenceSettings>();
        #endregion

        #region popups
        services.AddTransientPopup<RadioButtonPopup<Language>, RadioButtonPopupVm<Language>>();
        services.AddTransientPopup<RadioButtonPopup<string>, RadioButtonPopupVm<string>>();
        services.AddTransientPopup<RadioButtonPopup<PickerItem>, RadioButtonPopupVm<PickerItem>>();
        services.AddTransientPopup<RadioButtonPopup<MediaTrack>, RadioButtonPopupVm<MediaTrack>>();
        services.AddTransientPopup<TranscribePopup, TranscribePopupViewModel>();
        services.AddTransientPopup<TranslatePopup, TranslatePopupViewModel>();
        services.AddTransientPopup<EntryPopup, StringEntryPopupVm>();
        services.AddTransientPopup<TimeEntryPopup, TimeEntryPopupVm>();
        services.AddTransientPopup<UrlEntryPopup, UrlEntryPopupVm>();
        services.AddTransientPopup<CounterPopup, CounterPopupVm>();
        services.AddTransientPopup<DoubleEntryPopup, DoubleEntryPopupVm>();
        #endregion
    }

    public static void AddAppLogging(this IServiceCollection services)
    {
        Directory.CreateDirectory(Path.Combine(FileSystem.Current.AppDataDirectory, FileConstants.LogsDir));

#if DEBUG
        var logConfig = new LoggerConfiguration().MinimumLevel.Verbose();

#else
        var logConfig = new LoggerConfiguration().MinimumLevel.Warning();
#endif

        services.AddSerilog(
            logConfig
                .WriteTo.File(
                    Path.Combine(FileSystem.Current.AppDataDirectory, FileConstants.LogsDir, FileConstants.LogsFile),
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 10000000,
                    retainedFileCountLimit: 7,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj} ({SourceContext}){NewLine}{Exception}"
                )
                .CreateLogger()
        );
    }
}
