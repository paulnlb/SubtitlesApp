using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.SettingsItems;

public class FloatEntrySettingsItem : VirtualSettingsItem<float?>
{
    private readonly bool _valueAsSubtitle;
    private readonly ICustomPopupService _popupService;
    private readonly float? _min = null;
    private readonly float? _max = null;

    public FloatEntrySettingsItem(
        ICustomPopupService popupService,
        bool valueAsSubTitle = false,
        Func<float?>? getter = null,
        Action<float?>? setter = null,
        float? min = null,
        float? max = null
    )
        : base(getter, setter)
    {
        _popupService = popupService;
        _valueAsSubtitle = valueAsSubTitle;
        _min = min;
        _max = max;

        if (_valueAsSubtitle)
        {
            SubTitle = GetValue()?.ToString();
        }
    }

    public override async Task EditValueAsync()
    {
        var value = GetValue();
        var result = await _popupService.ShowDoubleEntry(Title, value ?? 0f, _min, _max);

        if (result is null || result == value)
        {
            return;
        }

        SetValue((float)result.Value);

        if (_valueAsSubtitle)
        {
            SubTitle = result.Value.ToString();
        }
    }
}
