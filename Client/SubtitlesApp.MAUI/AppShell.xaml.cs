namespace SubtitlesApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(Views.PlayerWithSubtitlesPage), typeof(Views.PlayerWithSubtitlesPage));
            Routing.RegisterRoute("settings", typeof(Views.SettingsPage));
        }
    }
}
