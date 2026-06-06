using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Options;
using SubtitlesApp.Interfaces;
using UraniumUI.Dialogs;
using UraniumUI.Extensions;
using UraniumUI.Material.Controls;

namespace SubtitlesApp.Services;

public class PopupService(IOptions<DialogOptions> dialogOptions) : ICustomPopupService
{
    private Page? page;

    protected DialogOptions DialogOptions { get; } = dialogOptions.Value;

    public Page Page
    {
        get => page ?? GetCurrentPage();
        set => page = value;
    }

    public Task<T?> DisplayRadioButtonPromptAsync<T>(
        string message,
        IEnumerable<T> selectionSource,
        Func<T, string> displaySelector,
        T? selected = default,
        string accept = "Ok",
        string cancel = "Cancel"
    )
    {
        var tcs = new TaskCompletionSource<T?>();
        var calculatedSize = CalculateSize(Page);

        var rootContainer = new Grid() { HeightRequest = calculatedSize.Height };

#if IOS || MACCATALYST
        var popup = new Popup
        {
            WidthRequest = calculatedSize.Width,
            HeightRequest = calculatedSize.Height,
            BackgroundColor = ColorResource.GetColor("Surface", "SurfaceDark", Colors.Transparent),
            CanBeDismissedByTappingOutsideOfPopup = false,
            Padding = 0,
            Content = rootContainer,
        };
#else
        var popup = new Popup()
        {
            WidthRequest = calculatedSize.Width,
            HeightRequest = calculatedSize.Height,
            BackgroundColor = Colors.Transparent,
            Padding = 0,
            CanBeDismissedByTappingOutsideOfPopup = false,
            Content = GetFrame(calculatedSize.Width, rootContainer),
        };
#endif

        rootContainer.HeightRequest = calculatedSize.Height;

        var rbGroup = new RadioButtonGroupView()
        {
            Margin = 20,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start,
        };

        foreach (var item in selectionSource)
        {
            rbGroup.Add(new InputKit.Shared.Controls.RadioButton { Text = displaySelector(item), Value = item });
        }

        rbGroup.SelectedItem = selected;

        var footer = GetFooter(
            new Dictionary<string, Command>
            {
                {
                    accept,
                    new Command(async () =>
                    {
                        tcs.SetResult((T?)rbGroup.SelectedItem);
                        await popup.CloseAsync();
                    })
                },
                {
                    cancel,
                    new Command(async () =>
                    {
                        tcs.SetResult(default);
                        await popup.CloseAsync();
                    })
                },
            }
        );

        rootContainer.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        rootContainer.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        rootContainer.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        rootContainer.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        rootContainer.Add(GetHeader(message));
        rootContainer.Add(new ScrollView { Content = rbGroup }, row: 1);
        rootContainer.Add(GetDivider(), row: 2);
        rootContainer.Add(footer, row: 3);
        Page.ShowPopup(popup, new PopupOptions { Shape = null, Shadow = null });

        return tcs.Task;
    }

    protected static View GetFrame(double width, View content)
    {
        var frame = new Border
        {
            Content = content,
            Padding = 0,
            StyleClass = ["PopupBorder", "PopupView"],
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            WidthRequest = width,
        };

        return frame;
    }

    protected virtual Page GetCurrentPage()
    {
        var page = Application.Current?.Windows[0].Page ?? throw new NullReferenceException("Page was not found");

        if (page is Shell shell)
        {
            return shell.CurrentPage;
        }

        if (page is NavigationPage nav)
        {
            return nav.CurrentPage;
        }

        if (page is TabbedPage tabbed)
        {
            return tabbed.CurrentPage;
        }
        if (page is FlyoutPage flyoutPage)
        {
            return flyoutPage.Flyout;
        }
        return page;
    }

    protected virtual View GetDivider()
    {
        if (DialogOptions.GetDivider != null)
        {
            return DialogOptions.GetDivider();
        }

        return new BoxView { StyleClass = new[] { "Divider" }, Margin = 0 };
    }

    protected virtual View GetHeader(string title)
    {
        if (DialogOptions.GetHeader != null)
        {
            return DialogOptions.GetHeader(title);
        }

        return new StackLayout
        {
            HorizontalOptions = LayoutOptions.Fill,
            Children =
            {
                new Label { Text = title, Margin = 20 },
                GetDivider(),
            },
        };
    }

    protected virtual View GetFooter(Dictionary<string, Command> footerButtons)
    {
        if (DialogOptions.GetFooter != null)
        {
            return DialogOptions.GetFooter(footerButtons);
        }

        var layout = new FlexLayout { JustifyContent = Microsoft.Maui.Layouts.FlexJustify.End, Margin = new Thickness(10) };

        foreach (var button in footerButtons.Reverse())
        {
            layout.Children.Add(
                new Button
                {
                    Text = button.Key,
                    StyleClass = new[] { "TextButton", "Dialog.Button" + layout.Children.Count },
                    Command = button.Value,
                }
            );
        }

        return layout;
    }

    protected virtual Size CalculateSize(Page page)
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

            var edge = (baseValue * .8).Clamp(200, 400);

            return new Size(edge, edge * .9);
        }

        return new Size(100, 100);
    }
}
