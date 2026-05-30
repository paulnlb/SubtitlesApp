using SubtitlesApp.ViewModels;
using UraniumUI.Material.Controls;

namespace SubtitlesApp.Views;

public partial class CaptionsView : ContentView
{
    private CaptionsViewModel Vm =>
        BindingContext as CaptionsViewModel
        ?? throw new InvalidOperationException("BindingContext must be of type CaptionsViewModel");

    public CaptionsView()
    {
        InitializeComponent();
    }

    public void Cleanup()
    {
        Vm.SubsScrollRequested -= OnSubScrollRequested;
        Vm.TranslationsScrollRequested -= OnTranslationScrollRequested;
        Vm.SubtitlesAdapter.Dispose();
        Vm.TranslationsAdapter.Dispose();
    }

    private void OnBindingContextChanged(object? sender, EventArgs e)
    {
        Vm.SubsScrollRequested += OnSubScrollRequested;
        Vm.TranslationsScrollRequested += OnTranslationScrollRequested;
    }

    private void OnSelectedTabChanged(object? sender, TabItem e)
    {
        if (BindingContext is not PlayerWithSubtitlesViewModel vm)
        {
            return;
        }

        if (e is null)
        {
            return;
        }
        else if (e.Title == "Subtitles")
        {
            vm.CaptionsVm.IsSubtitlesSelected = true;
            vm.CaptionsVm.IsTranslationsSelected = false;
        }
        else if (e.Title == "Translations")
        {
            vm.CaptionsVm.IsSubtitlesSelected = false;
            vm.CaptionsVm.IsTranslationsSelected = true;
        }
    }

    private void OnSubScrollRequested(object? sender, EventArgs e)
    {
        subtitlesList.ScrollToIndex(Vm.SubtitlesCollectionState.CurrentSubtitleIndex);
    }

    private void OnTranslationScrollRequested(object? sender, EventArgs e)
    {
        translationsList.ScrollToIndex(Vm.TranslationsCollectionState.CurrentSubtitleIndex);
    }
}
