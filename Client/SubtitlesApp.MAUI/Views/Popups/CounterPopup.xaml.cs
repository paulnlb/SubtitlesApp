using CommunityToolkit.Maui.Views;
using SubtitlesApp.Helpers;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.Views.Popups;

public partial class CounterPopup : Popup<int>
{
    public CounterPopup(CounterPopupVm vm)
    {
        InitializeComponent();
        BindingContext = vm;
        ViewSizeHelper.SetPopupSize(this);
    }
}
