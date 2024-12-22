using CommunityToolkit.Maui.Views;
using SubtitlesApp.ViewModels;

namespace SubtitlesApp.CustomControls;

public partial class SubtitlesSettingsPopup : Popup
{
	public SubtitlesSettingsPopup(SubtitlesSettingsPopupViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}