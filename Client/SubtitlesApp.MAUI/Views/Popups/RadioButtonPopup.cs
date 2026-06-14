using CommunityToolkit.Maui.Views;
using SubtitlesApp.Converters;
using SubtitlesApp.ViewModels.Popups;
using UraniumUI.Extensions;

namespace SubtitlesApp.Views.Popups;

public partial class RadioButtonPopup<T> : Popup<T>
{
    private RadioButtonPopupVm<T> Vm => (RadioButtonPopupVm<T>)BindingContext;

    public RadioButtonPopup(RadioButtonPopupVm<T> viewModel)
    {
        InitializeComponentEquivalent();
        BindingContext = viewModel;

        var calculatedSize = CalculateSize(Shell.Current.CurrentPage);

        MaximumWidthRequest = calculatedSize.Width;
        MaximumHeightRequest = calculatedSize.Height;
    }

    private void InitializeComponentEquivalent()
    {
        BackgroundColor = Colors.Transparent;
        Padding = 0;
        CanBeDismissedByTappingOutsideOfPopup = false;

        ControlTemplate = (ControlTemplate?)Application.Current?.Resources["PopupTemplate"];
        Resources = new ResourceDictionary { { "AddSpaceBeforeStringConverter", new AddSpaceBeforeStringConverter() } };

        var collectionView = new CollectionView { SelectionMode = Microsoft.Maui.Controls.SelectionMode.None };

        collectionView.SetBinding(ItemsView.ItemsSourceProperty, nameof(RadioButtonPopupVm<>.SourceVms));

        collectionView.ItemTemplate = new DataTemplate(() =>
        {
            var radioButton = new RadioButton { Margin = new Thickness(0, 5) };

            radioButton.SetBinding(
                RadioButton.ContentProperty,
                new Binding(
                    nameof(RadioButtonViewModel<>.Title),
                    BindingMode.OneWay,
                    converter: (IValueConverter)Resources["AddSpaceBeforeStringConverter"]
                )
            );
            radioButton.SetBinding(RadioButton.IsCheckedProperty, nameof(RadioButtonViewModel<>.IsChecked));
            radioButton.SetBinding(RadioButton.ValueProperty, nameof(RadioButtonViewModel<>.Value));

            radioButton.CheckedChanged += RadioButton_CheckedChanged;

            return radioButton;
        });

        Content = collectionView;
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

    private void RadioButton_CheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is not RadioButton rb || rb.BindingContext is not RadioButtonViewModel<T> langVm || !e.Value)
        {
            return;
        }

        if (Vm.ItemSelectedCommand != null && Vm.ItemSelectedCommand.CanExecute(langVm))
        {
            Vm.ItemSelectedCommand.Execute(langVm);
        }
    }
}
