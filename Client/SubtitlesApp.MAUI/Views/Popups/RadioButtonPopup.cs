using CommunityToolkit.Maui.Views;
using SubtitlesApp.Converters;
using SubtitlesApp.Helpers;
using SubtitlesApp.ViewModels;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.Views.Popups;

public partial class RadioButtonPopup<T> : Popup<T>
{
    public RadioButtonPopup(RadioButtonPopupVm<T> vm)
    {
        InitializeComponentEquivalent();
        BindingContext = vm;
        ViewSizeHelper.SetPopupSize(this);
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
}
