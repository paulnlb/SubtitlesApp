using SubtitlesApp.ClientModels.Enums;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ClientModels.SettingsItems;

public class AsyncEntrySettingsItem(
    IBuiltInDialogService dialogService,
    SecondaryTextMode secondaryTextMode,
    Func<Task<string>>? asyncGetter = null,
    Func<string, Task>? asyncSetter = null
) : AsyncVirtualSettingsItem(secondaryTextMode, asyncGetter, asyncSetter)
{
    public override async Task EditValueAsync()
    {
        var value = await GetValueAsync();
        var result = await dialogService.DisplayPrompt(Title, "", value);

        if (result is not null && result != value)
        {
            await SetValueAsync(result);
        }
    }
}
