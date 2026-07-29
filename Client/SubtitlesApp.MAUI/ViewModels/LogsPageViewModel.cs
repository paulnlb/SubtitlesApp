using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.Constants;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Interfaces;
using SubtitlesApp.Services;

namespace SubtitlesApp.ViewModels;

public partial class LogsPageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<string> _logFileNames;

    [ObservableProperty]
    private string _logsText = string.Empty;

    [ObservableProperty]
    private string _selected = string.Empty;

    private readonly IBuiltInDialogService _dialogService;
    private readonly LocalFileManager _localFileManager;

    private readonly string _basePath;

    public LogsPageViewModel(IBuiltInDialogService dialogService, LocalFileManager localFileManager)
    {
        _basePath = Path.Combine(FileSystem.Current.AppDataDirectory, FileConstants.LogsDir);
        _dialogService = dialogService;
        _localFileManager = localFileManager;

        LogFileNames = Directory.GetFiles(_basePath).Select(x => Path.GetFileName(x)).ToObservableCollection();
    }

    [RelayCommand]
    public async Task Load()
    {
        if (string.IsNullOrEmpty(Selected))
        {
            LogsText = string.Empty;
            return;
        }

        var fullPath = Path.Combine(_basePath, Selected);
        const int MaxLogSize = 500 * 1024;
        using var fileStream = File.OpenRead(fullPath);

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

    [RelayCommand]
    public async Task Export()
    {
        var result = await _localFileManager.SaveInternalTextFile(Selected, Path.Combine(_basePath, Selected));

        if (result.IsFailure && result.Error.Code == ErrorCode.OperationCanceled)
        {
            return;
        }
        else if (result.IsFailure)
        {
            await _dialogService.DisplayError(result.Error);
        }
        else
        {
            await _dialogService.DisplayAlert("Export Successful", $"{Selected} has been exported successfully.", "Ok");
        }
    }

    [RelayCommand]
    public async Task Delete()
    {
        var shouldDelete = await _dialogService.DisplayAlert(
            "Delete Selected Logs File",
            $"Are you sure you want to delete {Selected}?",
            "Yes",
            "Cancel"
        );

        if (!shouldDelete)
        {
            return;
        }

        var fullPath = Path.Combine(_basePath, Selected);
        File.Delete(fullPath);
        LogsText = string.Empty;
        LogFileNames.Remove(Selected);
        Selected = string.Empty;
    }

    partial void OnSelectedChanged(string value)
    {
        LogsText = string.Empty;
    }
}
