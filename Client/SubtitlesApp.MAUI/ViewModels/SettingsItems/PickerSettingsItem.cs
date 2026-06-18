using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.SettingsItems;

public partial class PickerSettingsItem : VirtualSettingsItem<string>
{
    private bool _valueAsSubtitle;
    private readonly ICustomPopupService _popupService;

    public required string[] AllValues { get; set; }

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
        var result = await _popupService.ShowRadioButtons(Title, AllValues, x => x, GetValue());

        if (result is null)
        {
            return;
        }

        SetValue(result);

        if (_valueAsSubtitle)
        {
            SubTitle = result;
        }
    }
}
