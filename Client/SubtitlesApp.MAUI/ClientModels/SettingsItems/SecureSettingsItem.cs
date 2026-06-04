using SubtitlesApp.Infrastructure.Interfaces.Settings;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ClientModels.SettingsItems;

public class SecureSettingsItem(IBuiltInDialogService dialogService, ISecureSettings settings) : SettingsItem
{
    public override async Task EditValueAsync()
    {
        Value ??= await settings.GetSecret();

        var result = await dialogService.DisplayPrompt(Title, "", Value);

        if (result is not null && result != Value)
        {
            Value = result;

            await settings.SetSecret(result);
        }
    }
}
