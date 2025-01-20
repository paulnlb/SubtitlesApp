namespace SubtitlesApp.Core.Result;

public sealed record Error(ErrorCode Code, string Description = "")
{
    public static readonly Error None = new(ErrorCode.Unspecified, string.Empty);
}
