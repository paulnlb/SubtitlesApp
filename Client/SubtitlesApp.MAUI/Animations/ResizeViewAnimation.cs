using SubtitlesApp.Layouts;

namespace SubtitlesApp.Animations;

public class ResizeViewAnimation() : CommunityToolkit.Maui.Animations.BaseAnimation(250)
{
    public static readonly BindableProperty NewWidthFactorProperty =
        BindableProperty.Create(
            nameof(NewWidthFactor),
            typeof(double),
            typeof(ResizeViewAnimation),
            0.0);

    public static readonly BindableProperty NewHeightFactorProperty =
       BindableProperty.Create(
           nameof(NewHeightFactor),
           typeof(double),
           typeof(ResizeViewAnimation),
           0.0,
           BindingMode.TwoWay);

    public double NewWidthFactor
    {
        get => (double)GetValue(NewWidthFactorProperty);
        set => SetValue(NewWidthFactorProperty, value);
    }

    public double NewHeightFactor
    {
        get => (double)GetValue(NewHeightFactorProperty);
        set => SetValue(NewHeightFactorProperty, value);
    }

    Animation AnimationCallback(VisualElement view)
    {
        var oldWidthFactorValue = AdaptiveLayout.GetWidthFactor(view) ?? 0;
        var oldHeightFactorValue = AdaptiveLayout.GetHeightFactor(view) ?? 0;

        var animation = new Animation();

        animation.Add(0, 1, new Animation(v =>
        {
            AdaptiveLayout.SetWidthFactor(view, v);
        }, oldWidthFactorValue, NewWidthFactor));

        animation.Add(0, 1, new Animation(v =>
        {
            AdaptiveLayout.SetHeightFactor(view, v);
        }, oldHeightFactorValue, NewHeightFactor));

        return animation;
    }

    public override Task Animate(VisualElement view, CancellationToken token = default)
    {
        view.Animate("ChangeSizeFactor", AnimationCallback(view), length: Length, easing: Easing);
        return Task.CompletedTask;
    }
}
