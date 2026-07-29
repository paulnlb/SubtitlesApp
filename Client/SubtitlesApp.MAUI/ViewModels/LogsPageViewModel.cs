using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.Constants;
using SubtitlesApp.Interfaces;

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

    private readonly string _basePath;

    public LogsPageViewModel(IBuiltInDialogService dialogService)
    {
        _basePath = Path.Combine(FileSystem.Current.AppDataDirectory, FileConstants.LogsDir);
        _dialogService = dialogService;

        LogFileNames = Directory.GetFiles(_basePath).Select(x => Path.GetFileName(x)).ToObservableCollection();
    }

    [RelayCommand]
    public async Task ClearLog()
    {
        var shouldDelete = await _dialogService.DisplayAlert(
            "Clear Selected Logs File",
            $"You are about to clear {Selected} Are you sure?",
            "Yes",
            "Cancel"
        );

        if (!shouldDelete)
        {
            return;
        }

        var fullPath = Path.Combine(_basePath, Selected);
        await File.WriteAllTextAsync(fullPath, string.Empty);
        LogsText = string.Empty;
    }

    [RelayCommand]
    public async Task LoadLog()
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
    public async Task ExportLog() { }

    partial void OnSelectedChanged(string value)
    {
        LogsText = string.Empty;
    }
}
