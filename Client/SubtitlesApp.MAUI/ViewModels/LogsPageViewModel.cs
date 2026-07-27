using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.Constants;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels;

public partial class LogsPageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<string> _logPaths;

    [ObservableProperty]
    private string _logsText = string.Empty;

    [ObservableProperty]
    private string _selectedPath = string.Empty;

    private readonly IBuiltInDialogService _dialogService;

    public LogsPageViewModel(IBuiltInDialogService dialogService)
    {
        _logPaths = Directory
            .GetFiles(Path.Combine(FileSystem.Current.AppDataDirectory, FileConstants.LogsDir))
            .ToObservableCollection();
        _dialogService = dialogService;
    }

    [RelayCommand]
    public async Task ClearLog()
    {
        var shouldDelete = await _dialogService.DisplayAlert(
            "Clear Selected Log",
            "You are about to clear the selected log file. Are you sure?",
            "Yes",
            "Cancel"
        );

        if (!shouldDelete)
        {
            return;
        }

        await File.WriteAllTextAsync(SelectedPath, string.Empty);
        LogsText = string.Empty;
    }

    async partial void OnSelectedPathChanged(string value)
    {
        try
        {
            if (string.IsNullOrEmpty(value))
            {
                LogsText = string.Empty;
                return;
            }

            const int MaxLogSize = 500 * 1024;
            using var fileStream = File.OpenRead(value);

            if (fileStream.Length > MaxLogSize)
            {
                await _dialogService.DisplayAlert(
                    "Log file is too big",
                    $"Selected log file is too big, only the last {MaxLogSize / 1024}KB will be loaded",
                    "Ok"
                );
            }

            var offset = Math.Max(0, fileStream.Length - MaxLogSize);
            fileStream.Seek(offset, SeekOrigin.Begin);

            using var streamReader = new StreamReader(fileStream);

            LogsText = await streamReader.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
                await _dialogService.DisplayError(new Error(ErrorCode.InternalClientError, ex.Message))
            );
        }
    }
}
