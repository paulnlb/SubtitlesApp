using CommunityToolkit.Maui.Views;
using SubtitlesApp.ViewModels;

namespace SubtitlesApp.CustomControls;

public partial class InputPopup : Popup
{
	public InputPopup(InputPopupViewModel vm)
	{
        BindingContext = vm;
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