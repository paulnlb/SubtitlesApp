using System.Diagnostics;
using CommunityToolkit.Maui.Views;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Helpers;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.Views.Popups;

public partial class TranscribePopup : Popup<TranscriptionSettings>
{
    public TranscribePopup(TranscribePopupViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        ViewSizeHelper.SetPopupSize(this);
    }

    private void Border_Unfocused(object sender, FocusEventArgs e)
    {
        Debug.WriteLine("Unfocused");
    }

    private void Border_Focused(object sender, FocusEventArgs e)
    {
        Debug.WriteLine("Focused");
    }
}
