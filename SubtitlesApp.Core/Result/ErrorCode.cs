namespace SubtitlesApp.Core.Result;

public enum ErrorCode
{
    Unspecified,
    InternalClientError,
    InvalidAudio,
    OperationCancelled,
    ValidationFailed,
    FailedServerResponse,
    RetryLimitExceeded,
    InvalidLlmTranslation,
    InvalidInput,
    SubtitlesPersistenceError,
    MediaPlayerError,
    MediaProcessingError,
}
