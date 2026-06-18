namespace SubtitlesApp.Helpers;

public readonly struct Transformation(double scale = 1, double translateX = 0, double translateY = 0)
    : IEquatable<Transformation>
{
    public double Scale { get; } = scale;
    public double TranslateX { get; } = translateX;
    public double TranslateY { get; } = translateY;

    public bool Equals(Transformation other)
    {
        return Scale == other.Scale && TranslateX == other.TranslateX && TranslateY == other.TranslateY;
    }

    public override bool Equals(object? obj) => obj is Transformation other && Equals(other);

    public override int GetHashCode() => (TranslateX, TranslateY, Scale).GetHashCode();

    public static bool operator ==(Transformation left, Transformation right) => left.Equals(right);

    public static bool operator !=(Transformation left, Transformation right) => !left.Equals(right);
}

public static class ViewTransformHelper
{
    /// <summary>
    /// Calculates a transform that virtually resizes and repositions a view
    /// from its current bounds into a desired frame while preserving aspect ratio.
    ///
    /// The result is suitable for:
    /// - Scale
    /// - TranslationX
    /// - TranslationY
    ///
    /// Assumes scaling happens around the view center (default MAUI behavior).
    /// </summary>
    public static Transformation CalculateTransformation(Rect currentBounds, Rect desiredFrame)
    {
        if (currentBounds.Width <= 0 || currentBounds.Height <= 0)
            return new Transformation(1, 0, 0);

        // Uniform scale preserving aspect ratio.
        double scaleX = desiredFrame.Width / currentBounds.Width;
        double scaleY = desiredFrame.Height / currentBounds.Height;

        double scale = Math.Min(scaleX, scaleY);

        // Current center.
        double currentCenterX = currentBounds.X + currentBounds.Width / 2.0;
        double currentCenterY = currentBounds.Y + currentBounds.Height / 2.0;

        // Desired center.
        double desiredCenterX = desiredFrame.X + desiredFrame.Width / 2.0;
        double desiredCenterY = desiredFrame.Y + desiredFrame.Height / 2.0;

        // Since scaling is around center,
        // translation only needs to move centers.
        double translateX = desiredCenterX - currentCenterX;
        double translateY = desiredCenterY - currentCenterY;

        return new Transformation(scale, translateX, translateY);
    }
}
