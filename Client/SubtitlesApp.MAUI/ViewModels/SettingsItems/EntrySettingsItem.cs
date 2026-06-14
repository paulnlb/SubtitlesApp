using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.SettingsItems;

public class EntrySettingsItem : VirtualSettingsItem<string>
{
    private bool _valueAsSubtitle;
    private readonly IBuiltInDialogService _dialogService;

    public EntrySettingsItem(
        IBuiltInDialogService dialogService,
        bool valueAsSubTitle = false,
        Func<string>? getter = null,
        Action<string>? setter = null
    )
        : base(getter, setter)
    {
        _dialogService = dialogService;
        _valueAsSubtitle = valueAsSubTitle;

        if (valueAsSubTitle)
        {
            SubTitle = GetValue();
        }
    }

    public override async Task EditValueAsync()
    {
        var value = GetValue();
        var result = await _dialogService.DisplayPrompt(Title, null, value);

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
