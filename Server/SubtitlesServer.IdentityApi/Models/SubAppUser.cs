using Microsoft.AspNetCore.Identity;

namespace SubtitlesServer.IdentityApi.Models;

public class SubAppUser : IdentityUser
{
    public string Name { get; set; }

    public short SubscriptionLevel { get; set; } = 0;
}
