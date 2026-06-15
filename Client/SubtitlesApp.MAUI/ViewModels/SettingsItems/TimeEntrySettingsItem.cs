using System.Text;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.SettingsItems;

public class TimeEntrySettingsItem : VirtualSettingsItem<TimeSpan>
{
    private bool _valueAsSubtitle;
    private readonly ICustomPopupService _popupService;

    public TimeEntrySettingsItem(
        ICustomPopupService popupService,
        bool valueAsSubTitle = false,
        Func<TimeSpan>? getter = null,
        Action<TimeSpan>? setter = null
    )
        : base(getter, setter)
    {
        _popupService = popupService;
        _valueAsSubtitle = valueAsSubTitle;

        if (valueAsSubTitle)
        {
            SubTitle = ConvertToDescription(GetValue());
        }
    }

    public override async Task EditValueAsync()
    {
        var value = GetValue();
        var result = await _popupService.ShowTimeEntry(Title, value);

        if (result == value)
        {
            return;
        }

        SetValue(result);

        if (_valueAsSubtitle)
        {
            SubTitle = ConvertToDescription(result);
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
