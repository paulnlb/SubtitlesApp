using SubtitlesApp.Helpers;

namespace SubtitlesApp.ClientModels;

public class PanGestureState
{
    public int Id { get; set; } = 0;
    public bool Locked { get; set; }
    public Transformation PlayerBeforeInterpolation { get; set; } = new(1, 0, 0);
    public double TotalY { get; set; } = 0;
    public double TotalX { get; set; } = 0;
    public double PanThreshold { get; set; } = 150;
}
