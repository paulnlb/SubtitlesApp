using CommunityToolkit.Maui.Converters;
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
        Resources = new ResourceDictionary
        {
            { "AddSpaceBeforeStringConverter", new AddSpaceBeforeStringConverter() },
            { "IsNotNullConverter", new IsNotNullConverter() },
        };

        var description = new Label { Margin = new Thickness(0, 0, 0, 10) };

        description.SetBinding(Label.TextProperty, nameof(RadioButtonPopupVm<>.Description));
        description.SetBinding(
            Label.IsVisibleProperty,
            nameof(RadioButtonPopupVm<>.Description),
            converter: (IValueConverter)Resources["IsNotNullConverter"]
        );

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

        var grid = new Grid { description, collectionView };

        grid.RowDefinitions =
        [
            new RowDefinition(new GridLength(0, GridUnitType.Auto)),
            new RowDefinition(new GridLength(1, GridUnitType.Star)),
        ];

        Grid.SetRow(description, 0);
        Grid.SetRow(collectionView, 1);

        Content = grid;
    }
}
