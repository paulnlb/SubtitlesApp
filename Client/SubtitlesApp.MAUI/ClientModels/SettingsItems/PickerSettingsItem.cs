using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ClientModels.SettingsItems;

public partial class PickerSettingsItem : VirtualSettingsItem<string>
{
    private bool _valueAsSubtitle;
    private readonly IBuiltInDialogService _dialogService;

    public required string[] AllValues { get; set; }

    public PickerSettingsItem(
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
        var result = await _dialogService.DisplayActionSheet(Title, "Cancel", null, AllValues);

        if (result is null || result == "Cancel")
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
