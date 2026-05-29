using SubtitlesApp.Extensions;
using SubtitlesApp.Helpers;

namespace SubtitlesApp.Layouts;

public class AdaptiveLayoutStateManager(AdaptiveLayout layout)
{
    private AdaptiveLayoutState? _currentState = null;
    private AdaptiveLayoutState? _nextState = null;

    public void SaveCurrentState()
    {
        _currentState = layout.MakeSnapshot();
    }

    public void SetNextState(List<double?> relativeVerticalLengths, List<double?> relativeHorizontalLengths)
    {
        _nextState = ComputeState(relativeVerticalLengths, relativeHorizontalLengths);
    }

    public async Task AnimateToCurrentState()
    {
        var tasksList = new List<Task>();

        foreach (var child in layout.Children)
        {
            if (child is not VisualElement visualElement)
            {
                throw new InvalidOperationException($"Child is not a VisualElement");
            }

            var translateX = NativeAnimation.AnimateAsync(
                visualElement.TranslationX,
                0,
                (v) => visualElement.TranslationX = v
            );
            var translateY = NativeAnimation.AnimateAsync(
                visualElement.TranslationY,
                0,
                (v) => visualElement.TranslationY = v
            );
            var scale = NativeAnimation.AnimateAsync(visualElement.Scale, 1, (v) => visualElement.Scale = v);

            tasksList.Add(translateX);
            tasksList.Add(translateY);
            tasksList.Add(scale);
        }

        await Task.WhenAll(tasksList);
    }

    public async Task AnimateToNextState()
    {
        var oldState = _currentState ?? throw new InvalidOperationException("Current state is not set");
        var newState = _nextState ?? throw new InvalidOperationException("Next state is not set");

        var tasksList = new List<Task>();

        for (int i = 0; i < layout.Children.Count; i++)
        {
            if (layout.Children[i] is not VisualElement visualElement)
            {
                throw new InvalidOperationException($"Child is not a VisualElement");
            }

            var transformation = ViewTransformHelper.CalculateTransformation(
                oldState.ChildrenStates[i].GetBounds(),
                newState.ChildrenStates[i].GetBounds()
            );

            var translateX = NativeAnimation.AnimateAsync(
                visualElement.TranslationX,
                transformation.TranslateX,
                (v) => visualElement.TranslationX = v
            );
            var translateY = NativeAnimation.AnimateAsync(
                visualElement.TranslationY,
                transformation.TranslateY,
                (v) => visualElement.TranslationY = v
            );
            var scale = NativeAnimation.AnimateAsync(
                visualElement.Scale,
                transformation.Scale,
                (v) => visualElement.Scale = v
            );

            tasksList.Add(translateX);
            tasksList.Add(translateY);
            tasksList.Add(scale);
        }

        await Task.WhenAll(tasksList);
    }

    public void InterpolateLayout(double relativeProgress)
    {
        if (relativeProgress < 0 || relativeProgress > 1)
        {
            throw new ArgumentException("Progress must be between 0 and 1");
        }

        var oldState = _currentState ?? throw new InvalidOperationException("Current state is not set");
        var newState = _nextState ?? throw new InvalidOperationException("Next state is not set");

        for (int i = 0; i < layout.Children.Count; i++)
        {
            var oldBounds = oldState.ChildrenStates[i].GetBounds();
            var newBounds = newState.ChildrenStates[i].GetBounds();

            if (layout.Children[i] is not VisualElement child)
            {
                throw new InvalidOperationException($"Child {i} is not a VisualElement");
            }

            var intermediateBounds = Lerp(relativeProgress, oldBounds, newBounds);

            Transformation transformation;

            if (oldBounds == intermediateBounds)
            {
                transformation = new Transformation(1, 0, 0);
            }
            else
            {
                transformation = ViewTransformHelper.CalculateTransformation(oldBounds, intermediateBounds);
            }

            child.Transform(transformation);
        }
    }

    private AdaptiveLayoutState ComputeState(List<double?> relativeVerticalLengths, List<double?> relativeHorizontalLengths)
    {
        var manager = new AdaptiveLayoutManager(layout);

        var bounds = new Rect(0, 0, layout.Width, layout.Height);
        var relHeights = relativeVerticalLengths;
        var relWidths = relativeHorizontalLengths;

        manager.MeasureChildren(new Size(layout.Width, layout.Height), relHeights, relWidths);
        var childrenSizes = manager.CalculateChildrenSizes(bounds, relHeights, relWidths);

        var childrenStates = new List<ChildState>()
        {
            new()
            {
                HorizontalLength = relWidths[0]!.Value,
                VerticalLength = relHeights[0]!.Value,
                TranslationX = 0,
                TranslationY = 0,
                Scale = 1,
                X = childrenSizes[0].X,
                Y = childrenSizes[0].Y,
                Width = childrenSizes[0].Width,
                Height = childrenSizes[0].Height,
            },
            new()
            {
                HorizontalLength = relWidths[1]!.Value,
                VerticalLength = relHeights[1]!.Value,
                TranslationX = 0,
                TranslationY = 0,
                Scale = 1,
                X = childrenSizes[1].X,
                Y = childrenSizes[1].Y,
                Width = childrenSizes[1].Width,
                Height = childrenSizes[1].Height,
            },
        };

        return new AdaptiveLayoutState(childrenStates, layout.Bounds);
    }

    private static Rect Lerp(double progress, Rect oldRect, Rect newRect)
    {
        var x = oldRect.X + (newRect.X - oldRect.X) * progress;
        var y = oldRect.Y + (newRect.Y - oldRect.Y) * progress;
        var width = oldRect.Width + (newRect.Width - oldRect.Width) * progress;
        var height = oldRect.Height + (newRect.Height - oldRect.Height) * progress;

        return new Rect(x, y, width, height);
    }
}
