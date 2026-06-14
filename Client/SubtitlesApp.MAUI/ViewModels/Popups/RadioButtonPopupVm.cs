using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.Popups;

public partial class RadioButtonPopupVm<T>(ICustomPopupService popupService) : BasePopupVm, IQueryAttributable
{
    private RadioButtonViewModel<T>? _selectedVm;

    [ObservableProperty]
    private IEnumerable<T> _sourceItems = [];

    [ObservableProperty]
    private Func<T, string> _displaySelector = x => x?.ToString() ?? string.Empty;

    [ObservableProperty]
    private T? _selectedItem;

    [ObservableProperty]
    private ObservableCollection<RadioButtonViewModel<T>> _sourceVms = [];

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        query.TryGetValue(nameof(SourceItems), out var items);
        query.TryGetValue(nameof(DisplaySelector), out var selectorValue);
        query.TryGetValue(nameof(SelectedItem), out var selectedValue);
        query.TryGetValue(nameof(Title), out var titleValue);
        query.TryGetValue(nameof(AcceptText), out var acceptTextValue);
        query.TryGetValue(nameof(CancelText), out var cancelTextValue);

        if (titleValue is string title)
        {
            Title = title;
        }
        if (acceptTextValue is string acceptText)
        {
            AcceptText = acceptText;
        }
        if (cancelTextValue is string cancelText)
        {
            CancelText = cancelText;
        }
        if (selectorValue is Func<T, string> displaySelector)
        {
            DisplaySelector = displaySelector;
        }
        if (items is IEnumerable<T> sourceItems)
        {
            SourceItems = sourceItems;
        }
        if (selectedValue is T selectedItem)
        {
            SelectedItem = selectedItem;
        }

        foreach (var item in SourceItems)
        {
            var vm = new RadioButtonViewModel<T>
            {
                Title = DisplaySelector(item),
                Value = item,
                IsChecked = item is not null && item.Equals(SelectedItem),
            };

            if (vm.IsChecked)
            {
                _selectedVm = vm;
            }

            SourceVms.Add(vm);
        }

        query.Clear();
    }

    [RelayCommand]
    public void ItemSelected(RadioButtonViewModel<T> vm)
    {
        if (vm == _selectedVm)
        {
            return;
        }

        _selectedVm?.IsChecked = false;
        _selectedVm = vm;
    }

    public override Task Accept()
    {
        var result = _selectedVm is null ? default : _selectedVm.Value;
        return popupService.CloseCurrentAsync(result);
    }

    public override Task Cancel()
    {
        return popupService.CloseCurrentAsync();
    }
}
