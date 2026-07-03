using CommunityToolkit.Maui.Views;
using SubtitlesApp.Helpers;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.Views.Popups;

public partial class DoubleEntryPopup : Popup<double>
{
    public DoubleEntryPopup(DoubleEntryPopupVm vm)
    {
        InitializeComponent();
        BindingContext = vm;
        ViewSizeHelper.SetPopupSize(this);
    }
}
