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
        ViewSizeHelper.SetPopupSize(this);
        vm.TimeScopeChanged += OnTimeScopeChanged;
    }

    private void OnTimeScopeChanged(object? sender, EventArgs e)
    {
        var formattedStr = new FormattedString();

        if (Vm.TimeScope == TimeEntryScope.Hours)
        {
            var hourSpan = new Span { Text = Vm.HourSection, FontSize = 22 };
            hourSpan.SetBinding(Span.TextProperty, nameof(Vm.HourSection), BindingMode.OneWay);

            formattedStr.Spans.Add(hourSpan);
            formattedStr.Spans.Add(new Span { Text = "h ", FontSize = 15 });
        }

        if (Vm.TimeScope >= TimeEntryScope.Minutes)
        {
            var minuteSpan = new Span { Text = Vm.MinuteSection, FontSize = 22 };
            minuteSpan.SetBinding(Span.TextProperty, nameof(Vm.MinuteSection), BindingMode.OneWay);

            formattedStr.Spans.Add(minuteSpan);
            formattedStr.Spans.Add(new Span { Text = "m ", FontSize = 15 });
        }

        var secondSpan = new Span { Text = Vm.SecondSection, FontSize = 22 };
        secondSpan.SetBinding(Span.TextProperty, nameof(Vm.SecondSection), BindingMode.OneWay);

        formattedStr.Spans.Add(secondSpan);
        formattedStr.Spans.Add(new Span { Text = "s ", FontSize = 15 });

        timeLabel.FormattedText = formattedStr;
    }
}
