using SubtitlesApp.ClientModels.Enums;

namespace SubtitlesApp.ClientModels.SettingsItems;

public abstract class AsyncVirtualSettingsItem : SettingsItem
{
    private readonly Func<Task<string>>? _asyncGetter;
    private readonly Func<string, Task>? _asyncSetter;

    public AsyncVirtualSettingsItem(
        SecondaryTextMode secondaryTextMode,
        Func<Task<string>>? asyncGetter = null,
        Func<string, Task>? asyncSetter = null
    )
        : base(secondaryTextMode)
    {
        _asyncGetter = asyncGetter;
        _asyncSetter = asyncSetter;
    }

    protected async Task<string?> GetValueAsync()
    {
        if (_asyncGetter is null)
        {
            return null;
        }

        return await _asyncGetter.Invoke();
    }

    protected async Task SetValueAsync(string value)
    {
        if (_asyncSetter is null || value == await GetValueAsync())
        {
            return;
        }

        await _asyncSetter.Invoke(value);

        if (SecondaryTextMode == SecondaryTextMode.Value)
        {
            SecondaryText = value;
        }
    }
}
