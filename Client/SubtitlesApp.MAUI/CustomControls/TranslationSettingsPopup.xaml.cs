using CommunityToolkit.Maui.Views;
using SubtitlesApp.ViewModels;

namespace SubtitlesApp.CustomControls;

public partial class TranslationSettingsPopup : Popup
{
	public TranslationSettingsPopup(TranslationSettingsPopupViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}