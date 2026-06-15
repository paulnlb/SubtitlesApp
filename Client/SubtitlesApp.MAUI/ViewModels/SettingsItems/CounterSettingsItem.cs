using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.SettingsItems;

public class CounterSettingsItem : VirtualSettingsItem<int>
{
    private bool _valueAsSubtitle;
    private readonly ICustomPopupService _popupService;
    private readonly int _min;
    private readonly int _max;

    public CounterSettingsItem(
        ICustomPopupService popupService,
        bool valueAsSubTitle = false,
        Func<int>? getter = null,
        Action<int>? setter = null,
        int min = 0,
        int max = int.MaxValue
    )
        : base(getter, setter)
    {
        _popupService = popupService;
        _valueAsSubtitle = valueAsSubTitle;
        _min = min;
        _max = max;

        if (valueAsSubTitle)
        {
            SubTitle = GetValue().ToString();
        }
    }

    public override async Task EditValueAsync()
    {
        var value = GetValue();
        value = Math.Clamp(value, _min, _max);

        var result = await _popupService.ShowCounter(Title, value, _min, _max);

        if (result == value)
        {
            return;
        }

        SetValue(result);

        if (_valueAsSubtitle)
        {
            SubTitle = result.ToString();
        }
    }
}
