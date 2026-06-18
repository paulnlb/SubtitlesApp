using CommunityToolkit.Maui.Views;
using SubtitlesApp.Converters;
using SubtitlesApp.ViewModels;
using SubtitlesApp.ViewModels.Popups;
using UraniumUI.Extensions;

namespace SubtitlesApp.Views.Popups;

public partial class RadioButtonPopup<T> : Popup<T>
{
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
        ControlTemplate = (ControlTemplate?)Application.Current?.Resources["PopupTemplate"];
        Resources = new ResourceDictionary { { "AddSpaceBeforeStringConverter", new AddSpaceBeforeStringConverter() } };

        var collectionView = new CollectionView { SelectionMode = Microsoft.Maui.Controls.SelectionMode.Single };

        collectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(RadioButtonPopupVm<>.SourceVms));
        collectionView.SetBinding(CollectionView.SelectedItemProperty, nameof(RadioButtonPopupVm<>.SelectedVm));

        collectionView.ItemTemplate = new DataTemplate(() =>
        {
            var radioButton = new RadioButton { Margin = new Thickness(0, 5) };

            radioButton.SetBinding(
                RadioButton.ContentProperty,
                new Binding(
                    nameof(SelectedItemVm<>.Title),
                    BindingMode.OneWay,
                    converter: (IValueConverter)Resources["AddSpaceBeforeStringConverter"]
                )
            );
            radioButton.SetBinding(RadioButton.IsCheckedProperty, nameof(SelectedItemVm<>.IsSelected));
            radioButton.SetBinding(RadioButton.ValueProperty, nameof(SelectedItemVm<>.Value));
            radioButton.InputTransparent = true;

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
}
