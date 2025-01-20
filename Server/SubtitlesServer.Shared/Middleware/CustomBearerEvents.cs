using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using SubtitlesApp.Core.Result;

namespace SubtitlesServer.Shared.Middleware;

public class CustomBearerEvents : JwtBearerEvents
{
    public override Task Challenge(JwtBearerChallengeContext context)
    {
        context.HandleResponse();
        Error error;

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

        if (context.Request.Method == "HEAD" || context.Request.Method == "OPTIONS")
        {
            return Task.FromResult(0);
        }

        if (context.AuthenticateFailure is SecurityTokenExpiredException)
        {
            error = new Error(ErrorCode.TokenExpired, context.AuthenticateFailure.Message);
        }
        else if (context.AuthenticateFailure is not null)
        {
            error = new Error(ErrorCode.Unauthorized, context.AuthenticateFailure.Message);
        }
        else if (!string.IsNullOrEmpty(context.ErrorDescription))
        {
            error = new Error(ErrorCode.Unauthorized, context.ErrorDescription);
        }
        else
        {
            error = new Error(ErrorCode.Unauthorized, "Authentication error");
        }

        var jsonError = JsonSerializer.Serialize(error);
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(jsonError);
    }
}
