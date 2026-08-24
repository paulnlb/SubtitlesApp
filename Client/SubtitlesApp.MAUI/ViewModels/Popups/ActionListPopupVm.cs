using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.Popups;

public partial class ActionListPopupVm<T> : BasePopupVm, IQueryAttributable
{
    private readonly ICustomPopupService _popupService;

    public ActionListPopupVm(ICustomPopupService popupService)
    {
        _popupService = popupService;
        IsCancelVisible = false;
        AcceptText = "Close";
    }

    [ObservableProperty]
    private IEnumerable<T> _sourceItems = [];

    [ObservableProperty]
    private Func<T, string> _displaySelector = x => x?.ToString() ?? string.Empty;

    [ObservableProperty]
    private ObservableCollection<SelectedItemVm<T>> _sourceVms = [];

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _emptyText;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        query.TryGetValue(nameof(SourceItems), out var items);
        query.TryGetValue(nameof(DisplaySelector), out var selectorValue);
        query.TryGetValue(nameof(Title), out var titleValue);
        query.TryGetValue(nameof(Description), out var descriptonValue);
        query.TryGetValue(nameof(EmptyText), out var emptyTextValue);

        if (titleValue is string title)
        {
            Title = title;
        }
        if (selectorValue is Func<T, string> displaySelector)
        {
            DisplaySelector = displaySelector;
        }
        if (items is IEnumerable<T> sourceItems)
        {
            SourceItems = sourceItems;
        }
        if (descriptonValue is string description)
        {
            Description = description;
        }
        if (emptyTextValue is string emptyText)
        {
            EmptyText = emptyText;
        }

        foreach (var item in SourceItems)
        {
            var vm = new SelectedItemVm<T> { Title = DisplaySelector(item), Value = item };

            SourceVms.Add(vm);
        }

        query.Clear();
    }

    [RelayCommand]
    public Task SelectAction(T parameter)
    {
        return _popupService.CloseCurrentAsync(parameter);
    }

    public override Task Accept()
    {
        return Cancel();
    }

    public override Task Cancel()
    {
        return _popupService.CloseCurrentAsync();
    }
}
