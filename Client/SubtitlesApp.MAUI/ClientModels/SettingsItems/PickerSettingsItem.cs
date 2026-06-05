using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ClientModels.SettingsItems;

public partial class PickerSettingsItem(
    IBuiltInDialogService dialogService,
    Func<string>? getter = null,
    Action<string>? setter = null
) : SettingsItem(getter, setter)
{
    public required string[] AllValues { get; set; }

    public override async Task EditValueAsync()
    {
        var result = await dialogService.DisplayActionSheet(Title, "Cancel", null, AllValues);

        if (result != "Cancel")
        {
            SetValue(result);
        }
    }
}
