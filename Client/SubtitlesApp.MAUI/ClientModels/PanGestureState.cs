using SubtitlesApp.Helpers;

namespace SubtitlesApp.ClientModels;

public class PanGestureState
{
    public int Id { get; set; } = 0;
    public bool Locked { get; set; }
    public double RelativeProgress { get; set; } = 0;
    public double PanThreshold { get; set; } = 0.5;
}
