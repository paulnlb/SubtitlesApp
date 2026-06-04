using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ClientModels.SettingsItems;

public class EntrySettingsItem(IBuiltInDialogService dialogService) : SettingsItem
{
    public override async Task EditValueAsync()
    {
        var result = await dialogService.DisplayPrompt(Title, "", Value);

        if (result is not null && result != Value)
        {
            Value = result;
        }
    }
}
