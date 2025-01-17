﻿namespace SubtitlesApp.Core.Result;

public enum ErrorCode
{
    Unspecified,
    BadRequest,
    InternalServerError,
    InternalClientError,
    ConnectionError,
    InvalidAudio,
    OperationCanceled,
    AuthenticationError,
    Unauthorized,
    BadGateway,
    TokenExpired,
    Forbidden,
    ValidationFailed,
}
