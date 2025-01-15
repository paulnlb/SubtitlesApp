using SubtitlesApp.Layouts;

namespace SubtitlesApp.Animations;

public class ResizeViewAnimation() : CommunityToolkit.Maui.Animations.BaseAnimation(250)
{
    public static readonly BindableProperty NewRelativeHorizontalLengthProperty = BindableProperty.Create(
        nameof(NewRelativeHorizontalLength),
        typeof(double),
        typeof(ResizeViewAnimation),
        0.0
    );

    public static readonly BindableProperty NewRelativeVerticalLengthProperty = BindableProperty.Create(
        nameof(NewRelativeVerticalLength),
        typeof(double),
        typeof(ResizeViewAnimation),
        0.0
    );

    public double NewRelativeHorizontalLength
    {
        get => (double)GetValue(NewRelativeHorizontalLengthProperty);
        set => SetValue(NewRelativeHorizontalLengthProperty, value);
    }

    public double NewRelativeVerticalLength
    {
        get => (double)GetValue(NewRelativeVerticalLengthProperty);
        set => SetValue(NewRelativeVerticalLengthProperty, value);
    }

    Animation AnimationCallback(VisualElement view, double oldHorizontalValue, double oldVerticalValue)
    {
        var animation = new Animation();

        animation.Add(
            0,
            1,
            new Animation(
                v =>
                {
                    AdaptiveLayout.SetRelativeHorizontalLength(view, v);
                },
                oldHorizontalValue,
                NewRelativeHorizontalLength
            )
        );

        animation.Add(
            0,
            1,
            new Animation(
                v =>
                {
                    AdaptiveLayout.SetRelativeVerticalLength(view, v);
                },
                oldVerticalValue,
                NewRelativeVerticalLength
            )
        );

        return animation;
    }

    public override Task Animate(VisualElement view, CancellationToken token = default)
    {
        var oldRelativeHorizontalLengthValue = AdaptiveLayout.GetRelativeHorizontalLength(view) ?? 0;
        var oldRelativeVerticalLengthValue = AdaptiveLayout.GetRelativeVerticalLength(view) ?? 0;

        if (
            oldRelativeHorizontalLengthValue != NewRelativeHorizontalLength
            || oldRelativeVerticalLengthValue != NewRelativeVerticalLength
        )
        {
            view.Animate(
                "ChangeSizeFactor",
                AnimationCallback(view, oldRelativeHorizontalLengthValue, oldRelativeVerticalLengthValue),
                length: Length,
                easing: Easing
            );
        }

        return Task.CompletedTask;
    }
}
