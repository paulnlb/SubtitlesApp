using CommunityToolkit.Maui.Views;
using SubtitlesApp.Core.Models;
using SubtitlesApp.ViewModels.Popups;
using UraniumUI.Extensions;

namespace SubtitlesApp.Views.Popups;

public partial class SelectLanguagePopup : Popup<Language>
{
    private SelectLanguagePopupVm Vm => (SelectLanguagePopupVm)BindingContext;

    public SelectLanguagePopup(SelectLanguagePopupVm viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        var calculatedSize = CalculateSize(Shell.Current.CurrentPage);

        MaximumWidthRequest = calculatedSize.Width;
        MaximumHeightRequest = calculatedSize.Height;
    }

    private Size CalculateSize(Page page)
    {
        if (DeviceInfo.Current.Idiom == DeviceIdiom.Desktop || DeviceInfo.Current.Idiom == DeviceIdiom.Tablet)
        {
            return new Size(400, 400);
        }

        if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone)
        {
            var baseValue = page.Width;
            if (page.Width > page.Height)
            {
                baseValue = page.Height;
            }

            var edge = (baseValue * .8).Clamp(200, 600);

            return new Size(edge, edge * .9);
        }

        return new Size(100, 100);
    }

    private void RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is not RadioButton rb || rb.BindingContext is not LanguageViewModel langVm || !e.Value)
        {
            return;
        }

        if (Vm.ItemSelectedCommand != null && Vm.ItemSelectedCommand.CanExecute(langVm))
        {
            Vm.ItemSelectedCommand.Execute(langVm);
        }
    }
}
