using CommunityToolkit.Maui.Converters;
using CommunityToolkit.Maui.Views;
using SubtitlesApp.CustomControls;
using SubtitlesApp.Helpers;
using SubtitlesApp.ViewModels;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.Views.Popups;

public partial class ActionListPopup<T> : Popup<T>
{
    private ActionListPopupVm<T> Vm => (ActionListPopupVm<T>)BindingContext;

    public ActionListPopup(ActionListPopupVm<T> vm)
    {
        InitializeComponentEquivalent();
        BindingContext = vm;
        ViewSizeHelper.SetPopupSize(this);
    }

    private void InitializeComponentEquivalent()
    {
        ControlTemplate = (ControlTemplate?)Application.Current?.Resources["PopupTemplate"];
        Resources = new ResourceDictionary { { "IsNotNullConverter", new IsNotNullConverter() } };

        var description = new Label { Margin = new Thickness(0, 0, 0, 10) };

        description.SetBinding(Label.TextProperty, nameof(ActionListPopupVm<>.Description));
        description.SetBinding(
            Label.IsVisibleProperty,
            nameof(ActionListPopupVm<>.Description),
            converter: (IValueConverter)Resources["IsNotNullConverter"]
        );

        var collectionView = new CollectionView { SelectionMode = Microsoft.Maui.Controls.SelectionMode.None };

        collectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(ActionListPopupVm<>.SourceVms));
        collectionView.SetBinding(CollectionView.EmptyViewProperty, nameof(ActionListPopupVm<>.EmptyText));

        collectionView.ItemTemplate = new DataTemplate(() =>
        {
            var buttonStyle = (Style?)Application.Current?.Resources["TransparentButton"];
            var button = new LeftAlignedButton { Style = buttonStyle };

            button.SetBinding(
                LeftAlignedButton.TextProperty,
                new Binding(nameof(SelectedItemVm<>.Title), BindingMode.OneWay)
            );
            button.SetBinding(LeftAlignedButton.CommandParameterProperty, nameof(SelectedItemVm<>.Value));

            var commandBinding = new Binding { Source = Vm, Path = nameof(ActionListPopupVm<>.SelectActionCommand) };
            button.SetBinding(LeftAlignedButton.CommandProperty, commandBinding);

            return button;
        });

        collectionView.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 10 };

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
