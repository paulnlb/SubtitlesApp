using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Extensions;

public static class ErrorCodeExtensions
{
    public static string GetBriefDescription(this ErrorCode errorCode)
    {
        return errorCode switch
        {
            ErrorCode.Unspecified => "Unknown error.",
            ErrorCode.InternalClientError => "An error has occurred on the client side",
            ErrorCode.InvalidAudio => "Provided audio file is invalid.",
            ErrorCode.OperationCancelled => "Operation has been cancelled.",
            ErrorCode.ValidationFailed => "Data is invalid.",
            ErrorCode.FailedServerResponse => "Received failed status from server.",
            ErrorCode.RetryLimitExceeded => "Retry limit exceeded.",
            ErrorCode.InvalidLlmTranslation => "LLM translation failed. Please try again.",
            ErrorCode.InvalidInput => "Error: provided data or configuration is invalid",
            ErrorCode.SubtitlesPersistenceError => "An unexpected error occurred while saving subtitles to disk",
            ErrorCode.MediaPlayerError => "An error occured while trying to open your media",
            ErrorCode.MediaProcessingError => "An error occured while trying to process your media",
            _ => string.Empty,
        };
    }
}
