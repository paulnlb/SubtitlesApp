namespace SubtitlesApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(Views.MediaElementPage), typeof(Views.MediaElementPage));
        }
    }
}
