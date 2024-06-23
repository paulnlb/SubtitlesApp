namespace SubtitlesApp
{
    public partial class App : IApplication
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
