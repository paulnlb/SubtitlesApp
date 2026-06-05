using SubtitlesApp.ClientModels.Enums;

namespace SubtitlesApp.ClientModels.SettingsItems;

public abstract partial class VirtualSettingsItem(Func<string>? getter = null, Action<string>? setter = null) : SettingsItem
{
    protected string? GetValue() => getter?.Invoke();

    protected void SetValue(string value)
    {
        if (setter is null || value == GetValue())
        {
            return;
        }

        setter.Invoke(value);

        if (SecondaryTextMode == SecondaryTextMode.Value)
        {
            SecondaryText = value;
        }
    }

    protected override void SecondaryTextModeChangeHanlder(SecondaryTextMode value)
    {
        SecondaryText = value switch
        {
            SecondaryTextMode.Description when Description is not null => Description,
            SecondaryTextMode.ValueMasked => ValueMask,
            SecondaryTextMode.None => string.Empty,
            SecondaryTextMode.Value => GetValue(),
            _ => "Error: unknown secondary text mode",
        };
    }
}
