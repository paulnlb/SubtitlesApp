using SubtitlesApp.ClientModels;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.SettingsItems;

public partial class PickerSettingsItem : VirtualSettingsItem<string>
{
    private bool _valueAsSubtitle;
    private readonly ICustomPopupService _popupService;

    public required PickerItem[] AllValues { get; set; }

    public PickerSettingsItem(
        ICustomPopupService popupService,
        bool valueAsSubTitle = false,
        Func<string>? getter = null,
        Action<string>? setter = null
    )
        : base(getter, setter)
    {
        _popupService = popupService;
        _valueAsSubtitle = valueAsSubTitle;

        if (valueAsSubTitle)
        {
            SubTitle = GetValue();
        }
    }

    public override async Task EditValueAsync()
    {
        var selected = AllValues.FirstOrDefault(x => x.Action == GetValue());

        var result = await _popupService.ShowRadioButtons(Title, AllValues, x => x.Title, selected);

        if (result is null)
        {
            return;
        }

        SetValue(result.Action);

        if (_valueAsSubtitle)
        {
            SubTitle = result.Action;
        }
    }
}
