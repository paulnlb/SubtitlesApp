using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.SettingsItems;

public class EntrySettingsItem : VirtualSettingsItem<string>
{
    private bool _valueAsSubtitle;
    private readonly ICustomPopupService _popupService;

    public EntrySettingsItem(
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
        var value = GetValue();
        var result = await _popupService.ShowEntry(Title, value);

        if (result is null || result == value)
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
