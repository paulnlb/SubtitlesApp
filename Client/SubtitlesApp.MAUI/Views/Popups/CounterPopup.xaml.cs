using CommunityToolkit.Maui.Views;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.Views.Popups;

public partial class CounterPopup : Popup<int>
{
    public CounterPopup(CounterPopupVm vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}
