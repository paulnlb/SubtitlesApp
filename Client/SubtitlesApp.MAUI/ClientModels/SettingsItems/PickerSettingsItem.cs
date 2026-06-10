using SubtitlesApp.ClientModels.Enums;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ClientModels.SettingsItems;

public partial class PickerSettingsItem(
    IBuiltInDialogService dialogService,
    SecondaryTextMode secondaryTextMode,
    Func<string>? getter = null,
    Action<string>? setter = null
) : VirtualSettingsItem(secondaryTextMode, getter, setter)
{
    public required string[] AllValues { get; set; }

    public override async Task EditValueAsync()
    {
        var result = await dialogService.DisplayActionSheet(Title, "Cancel", null, AllValues);

        if (result is not null && result != "Cancel")
        {
            SetValue(result);
        }
    }
}
