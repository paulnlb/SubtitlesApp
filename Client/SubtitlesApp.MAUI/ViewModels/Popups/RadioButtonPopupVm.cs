using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.Popups;

public partial class RadioButtonPopupVm<T>(ICustomPopupService popupService) : BasePopupVm, IQueryAttributable
{
    private SelectedItemVm<T>? _selectedVm;

    public SelectedItemVm<T>? SelectedVm
    {
        get => _selectedVm;
        set
        {
            if (_selectedVm == value)
                return;

            _selectedVm?.IsSelected = false;

            _selectedVm = value;

            _selectedVm?.IsSelected = true;

            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    private IEnumerable<T> _sourceItems = [];

    [ObservableProperty]
    private Func<T, string> _displaySelector = x => x?.ToString() ?? string.Empty;

    [ObservableProperty]
    private T? _selectedItem;

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
        query.TryGetValue(nameof(SelectedItem), out var selectedValue);
        query.TryGetValue(nameof(Title), out var titleValue);
        query.TryGetValue(nameof(AcceptText), out var acceptTextValue);
        query.TryGetValue(nameof(CancelText), out var cancelTextValue);
        query.TryGetValue(nameof(Description), out var descriptonValue);
        query.TryGetValue(nameof(IsCancelVisible), out var isCancelVisibleValue);
        query.TryGetValue(nameof(EmptyText), out var emptyTextValue);

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
        if (descriptonValue is string description)
        {
            Description = description;
        }
        if (isCancelVisibleValue is bool isCancelVisible)
        {
            IsCancelVisible = isCancelVisible;
        }
        if (emptyTextValue is string emptyText)
        {
            EmptyText = emptyText;
        }

        foreach (var item in SourceItems)
        {
            var vm = new SelectedItemVm<T>
            {
                Title = DisplaySelector(item),
                Value = item,
                IsSelected = item is not null && item.Equals(SelectedItem),
            };

            if (vm.IsSelected)
            {
                SelectedVm = vm;
            }

            SourceVms.Add(vm);
        }

        query.Clear();
    }

    public override Task Accept()
    {
        var result = SelectedVm is null ? default : SelectedVm.Value;
        return popupService.CloseCurrentAsync(result);
    }

    public override Task Cancel()
    {
        return popupService.CloseCurrentAsync();
    }
}
