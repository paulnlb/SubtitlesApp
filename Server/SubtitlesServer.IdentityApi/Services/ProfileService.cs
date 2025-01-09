using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using SubtitlesServer.IdentityApi.Models;
using System.Security.Claims;

namespace SubtitlesServer.IdentityApi.Services;

public class ProfileService(
    ILogger<ProfileService> logger,
    UserManager<SubAppUser> userManager) : DefaultProfileService(logger)
{
    public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var claims = await GetClaimsAsync(context);
        context.IssuedClaims.AddRange(claims);
    }

    private async Task<List<Claim>> GetClaimsAsync(ProfileDataRequestContext context)
    {
        var subject = context.Subject ?? throw new ArgumentNullException("context.Subject");

        var subjectId = subject.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;

        var user = await userManager.FindByIdAsync(subjectId) ?? throw new ArgumentException("Invalid subject identifier");

        return GetClaimsFromUser(user);
    }

    private static List<Claim> GetClaimsFromUser(SubAppUser user)
    {
        var claims = new List<Claim>
        {
            new ("subscription_level", user.SubscriptionLevel.ToString()),
        };
        return claims;
    }
}
