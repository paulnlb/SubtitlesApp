using SubtitlesApp.ClientModels.Enums;

namespace SubtitlesApp.ClientModels.SettingsItems;

public abstract class AsyncVirtualSettingsItem(
    Func<Task<string>>? asyncGetter = null,
    Func<string, Task>? asyncSetter = null
) : SettingsItem
{
    protected async Task<string?> GetValueAsync()
    {
        if (asyncGetter is null)
        {
            return null;
        }

        return await asyncGetter.Invoke();
    }

    protected async Task SetValueAsync(string value)
    {
        if (asyncSetter is null || value == await GetValueAsync())
        {
            return;
        }

        await asyncSetter.Invoke(value);

        if (SecondaryTextMode == SecondaryTextMode.Value)
        {
            SecondaryText = value;
        }
    }

    protected override async void SecondaryTextModeChangeHanlder(SecondaryTextMode value)
    {
        if (SecondaryTextMode == SecondaryTextMode.Value)
        {
            try
            {
                SecondaryText = await GetValueAsync();
            }
            catch
            {
                SecondaryText = "Error: could not retrieve value";
            }

            return;
        }

        SecondaryText = value switch
        {
            SecondaryTextMode.Description when Description is not null => Description,
            SecondaryTextMode.ValueMasked => ValueMask,
            SecondaryTextMode.None => string.Empty,
            // SecondaryTextMode.None case covered above
            _ => "Error: unknown secondary text mode",
        };
    }
}
