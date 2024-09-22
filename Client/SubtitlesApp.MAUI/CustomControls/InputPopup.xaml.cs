using CommunityToolkit.Maui.Views;

namespace SubtitlesApp.CustomControls;

public partial class InputPopup : Popup
{
	public InputPopup()
	{
		InitializeComponent();
	}

	async void OnOkClicked(object sender, EventArgs e)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync(urlEntry.Text, cts.Token);
    }

	async void OnCancelClicked(object sender, EventArgs e)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync(null, cts.Token);
    }
}