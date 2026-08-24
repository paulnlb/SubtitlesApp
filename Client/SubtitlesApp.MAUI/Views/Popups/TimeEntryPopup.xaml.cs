using CommunityToolkit.Maui.Views;
using SubtitlesApp.ClientModels.Enums;
using SubtitlesApp.Helpers;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.Views.Popups;

public partial class TimeEntryPopup : Popup<TimeSpan>
{
    private TimeEntryPopupVm Vm => (TimeEntryPopupVm)BindingContext;

    public TimeEntryPopup(TimeEntryPopupVm vm)
    {
        InitializeComponent();
        BindingContext = vm;
        UpdateOrientation();

        vm.TimeScopeChanged += OnTimeScopeChanged;
        Shell.Current.Window.SizeChanged += OnSizeChanged;

        Closed += (s, e) => Shell.Current.Window.SizeChanged -= OnSizeChanged;
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        UpdateOrientation();
    }

    private void OnTimeScopeChanged(object? sender, EventArgs e)
    {
        var formattedStr = new FormattedString();

        if (Vm.TimeScope == TimeEntryScope.Hours)
        {
            var hourSpan = new Span { Text = Vm.HourSection, FontSize = 28 };
            hourSpan.SetBinding(Span.TextProperty, nameof(Vm.HourSection), BindingMode.OneWay);

            formattedStr.Spans.Add(hourSpan);
            formattedStr.Spans.Add(new Span { Text = "h ", FontSize = 18 });
        }

        if (Vm.TimeScope >= TimeEntryScope.Minutes)
        {
            var minuteSpan = new Span { Text = Vm.MinuteSection, FontSize = 28 };
            minuteSpan.SetBinding(Span.TextProperty, nameof(Vm.MinuteSection), BindingMode.OneWay);

            formattedStr.Spans.Add(minuteSpan);
            formattedStr.Spans.Add(new Span { Text = "m ", FontSize = 18 });
        }

        var secondSpan = new Span { Text = Vm.SecondSection, FontSize = 28 };
        secondSpan.SetBinding(Span.TextProperty, nameof(Vm.SecondSection), BindingMode.OneWay);

        formattedStr.Spans.Add(secondSpan);
        formattedStr.Spans.Add(new Span { Text = "s ", FontSize = 18 });

        timeLabel.FormattedText = formattedStr;
    }

    private void UpdateOrientation()
    {
        if (Shell.Current.Window.Width > Shell.Current.Window.Height)
        {
            rootLayout.Orientation = StackOrientation.Horizontal;
            presetsCollection.Orientation = StackOrientation.Vertical;
            presetsCollectionScroll.Orientation = ScrollOrientation.Vertical;
            rootLayout.HorizontalOptions = LayoutOptions.Center;

            WidthRequest = -1;
            ViewSizeHelper.SetWidePopupSize(this);
        }
        else
        {
            rootLayout.Orientation = StackOrientation.Vertical;
            presetsCollection.Orientation = StackOrientation.Horizontal;
            presetsCollectionScroll.Orientation = ScrollOrientation.Horizontal;
            rootLayout.HorizontalOptions = LayoutOptions.Fill;

            ViewSizeHelper.SetPopupSize(this);
        }
    }
}
