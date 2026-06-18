using System.Text;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.SettingsItems;

public class TimeEntrySettingsItem : VirtualSettingsItem<TimeSpan>
{
    private bool _valueAsSubtitle;
    private readonly ICustomPopupService _popupService;
    private readonly TimeSpan? _min;
    private readonly TimeSpan? _max;

    public TimeEntrySettingsItem(
        ICustomPopupService popupService,
        bool valueAsSubTitle = false,
        Func<TimeSpan>? getter = null,
        Action<TimeSpan>? setter = null,
        TimeSpan? min = null,
        TimeSpan? max = null
    )
        : base(getter, setter)
    {
        _popupService = popupService;
        _valueAsSubtitle = valueAsSubTitle;
        _min = min;
        _max = max;

        if (valueAsSubTitle)
        {
            SubTitle = ConvertToDescription(GetValue());
        }
    }

    public override async Task EditValueAsync()
    {
        var value = GetValue();
        var result = await _popupService.ShowTimeEntry(Title, value, _min, _max);

        if (result is null || result == value)
        {
            return;
        }

        SetValue(result.Value);

        if (_valueAsSubtitle)
        {
            SubTitle = ConvertToDescription(result.Value);
        }
    }

    private static string ConvertToDescription(TimeSpan time)
    {
        var builder = new StringBuilder();

        if (time.Hours == 1)
        {
            builder.Append("1 hour");
        }
        else if (time.Hours > 1)
        {
            builder.Append($"{time.Hours} hours");
        }

        if (time.Minutes == 1)
        {
            builder.Append(" 1 minute");
        }
        else if (time.Minutes > 1)
        {
            builder.Append($" {time.Minutes} minutes");
        }

        if (time.Seconds == 1)
        {
            builder.Append(" 1 second");
        }
        else if (time.Seconds != 0 || builder.Length == 0)
        {
            builder.Append($" {time.Seconds} seconds");
        }

        if (builder.Length != 0 && builder[0] == ' ')
        {
            builder.Remove(0, 1);
        }

        return builder.ToString();
    }
}
