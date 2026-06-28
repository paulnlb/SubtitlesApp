using CommunityToolkit.Maui.Views;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.Views.Popups;

public partial class DoubleEntryPopup : Popup<double>
{
    public DoubleEntryPopup(DoubleEntryPopupVm vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}
