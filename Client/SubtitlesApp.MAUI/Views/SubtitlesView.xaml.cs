using SubtitlesApp.ViewModels;

namespace SubtitlesApp.Views;

public partial class SubtitlesView : ContentView
{
    private SubtitlesViewModel Vm =>
        BindingContext as SubtitlesViewModel
        ?? throw new InvalidOperationException($"BindingContext must be of type {nameof(SubtitlesViewModel)}");

    public SubtitlesView()
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

    private void OnSubScrollRequested(object? sender, EventArgs e)
    {
        subtitlesList.ScrollToIndex(Vm.SubtitlesCollectionState.CurrentSubtitleIndex);
    }

    private void OnTranslationScrollRequested(object? sender, EventArgs e)
    {
        translationsList.ScrollToIndex(Vm.TranslationsCollectionState.CurrentSubtitleIndex);
    }
}
