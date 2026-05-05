using SubtitlesApp.ViewModels;

namespace SubtitlesApp.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;
    }

    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is SettingsViewModel vm && vm.IsDirty)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                bool discard = await DisplayAlert(
                    "Unsaved Changes",
                    "You have unsaved changes. Do you want to discard them and leave the page?",
                    "Leave",
                    "Stay here"
                );

                if (discard)
                {
                    await Shell.Current.GoToAsync("..");
                }
            });

            return true;
        }

        return base.OnBackButtonPressed();
    }
}
