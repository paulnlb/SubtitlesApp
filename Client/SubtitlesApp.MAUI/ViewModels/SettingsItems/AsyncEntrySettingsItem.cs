using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.SettingsItems;

public class AsyncEntrySettingsItem(
    ICustomPopupService popupService,
    Func<Task<string>>? asyncGetter = null,
    Func<string, Task>? asyncSetter = null
) : AsyncVirtualSettingsItem<string>(asyncGetter, asyncSetter)
{
    public override async Task EditValueAsync()
    {
        var value = await GetValueAsync();
        var result = await popupService.ShowEntry(Title, value);

        if (result is not null && result != value)
        {
            await SetValueAsync(result);
        }
    }
}
