using System.Collections.ObjectModel;

namespace SubtitlesApp.ClientModels.SettingsItems;

public class SettingsItemsGroup(string name, ObservableCollection<SettingsItem> settingsItems)
    : ObservableCollection<SettingsItem>(settingsItems)
{
    public string Name { get; private set; } = name;
}
