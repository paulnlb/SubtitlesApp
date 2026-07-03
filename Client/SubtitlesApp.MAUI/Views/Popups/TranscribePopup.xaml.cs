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

    private void OnAbsoluteModifierClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || !TimeSpan.TryParse(button.CommandParameter?.ToString(), out var time))
        {
            return;
        }

        var vm = (TranscribePopupViewModel)BindingContext;
        if (FromEntry.IsFocused)
        {
            vm.SetFromTime(time);
        }
        else if (ToEntry.IsFocused)
        {
            vm.SetToTime(time);
        }
    }

    private void OnRelativeModifierClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || !TimeSpan.TryParse(button.CommandParameter?.ToString(), out var delta))
        {
            return;
        }

        var vm = (TranscribePopupViewModel)BindingContext;
        if (FromEntry.IsFocused)
        {
            vm.SetFromTime(vm.FromTime + delta);
        }
        else if (ToEntry.IsFocused)
        {
            vm.SetToTime(vm.ToTime + delta);
        }
    }
}
