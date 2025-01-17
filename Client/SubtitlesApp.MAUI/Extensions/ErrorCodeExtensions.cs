using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Extensions;

public static class ErrorCodeExtensions
{
    public static string GetBriefDescription(this ErrorCode errorCode)
    {
        return errorCode switch
        {
            ErrorCode.Unspecified => "Unknown error.",
            ErrorCode.BadRequest => "Something is wrong with your request to the server.",
            ErrorCode.InternalServerError => "Something has broken on the server side. Please try again later.",
            ErrorCode.InternalClientError => "An error has occurred on the client side. Restart the app and try again.",
            ErrorCode.ConnectionError => "Error while connecting to the server. Check your connection and try again.",
            ErrorCode.InvalidAudio => "Provided audio file is invalid.",
            ErrorCode.OperationCanceled => "Operation has been cancelled.",
            ErrorCode.AuthenticationError => "Authentication failed: got error response from the identity provider.",
            ErrorCode.Unauthorized => "Provided credentials are invalid. Check your credentials and try again.",
            ErrorCode.BadGateway => "Something has broken on the server side (Bad Gateway). Please try again later.",
            ErrorCode.TokenExpired => "Access token expired.",
            ErrorCode.Forbidden => "You cannot access this resource.",
            ErrorCode.ValidationFailed => "Sent data is invalid.",
            _ => string.Empty,
        };
    }
}
