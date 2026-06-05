using SubtitlesApp.ClientModels.Enums;

namespace SubtitlesApp.ClientModels.SettingsItems;

public abstract partial class VirtualSettingsItem : SettingsItem
{
    private readonly Func<string>? _getter;
    private readonly Action<string>? _setter;

    public VirtualSettingsItem(
        SecondaryTextMode secondaryTextMode,
        Func<string>? getter = null,
        Action<string>? setter = null
    )
        : base(secondaryTextMode)
    {
        _getter = getter;
        _setter = setter;

        if (secondaryTextMode == SecondaryTextMode.Value && getter is not null)
        {
            SecondaryText = getter.Invoke();
        }
    }

    protected string? GetValue() => _getter?.Invoke();

    protected void SetValue(string value)
    {
        if (_setter is null || value == GetValue())
        {
            return;
        }

        _setter.Invoke(value);

        if (SecondaryTextMode == SecondaryTextMode.Value)
        {
            SecondaryText = value;
        }
    }
}
