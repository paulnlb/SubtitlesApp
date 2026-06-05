using SubtitlesApp.ClientModels.Enums;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ClientModels.SettingsItems;

public class EntrySettingsItem(
    IBuiltInDialogService dialogService,
    SecondaryTextMode secondaryTextMode,
    Func<string>? getter = null,
    Action<string>? setter = null
) : VirtualSettingsItem(secondaryTextMode, getter, setter)
{
    public override async Task EditValueAsync()
    {
        var value = GetValue();
        var result = await dialogService.DisplayPrompt(Title, "", value);

        if (result is not null && result != value)
        {
            SetValue(result);
        }
    }
}
