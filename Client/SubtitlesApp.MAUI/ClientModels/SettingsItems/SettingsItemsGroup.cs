using System.Collections.ObjectModel;

namespace SubtitlesApp.ClientModels.SettingsItems;

public class SettingsItemsGroup(string name, ObservableCollection<SettingsItem> settingsItems)
{
    public string Name { get; private set; } = name;

    public ObservableCollection<SettingsItem> Items { get; private set; } = settingsItems;
}
