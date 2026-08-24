namespace SubtitlesApp.ViewModels.SettingsItems;

public class PageSettingsItem(string pageRoute) : SettingsItem
{
    public override Task EditValueAsync()
    {
        return Shell.Current.GoToAsync(pageRoute);
    }
}
