using Duende.IdentityServer.Models;

namespace SubtitlesServer.IdentityApi;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
    ];

    public static IEnumerable<ApiScope> ApiScopes =>
    [
        new ApiScope(name: "api1", displayName: "My API"),
        new ApiScope(name: "api", displayName: "My API 2")
    ];


    public static IEnumerable<Client> Clients =>
    [
        new Client
        {
            ClientId = "interactive.public",

            RequireClientSecret = false,

            AllowedGrantTypes = GrantTypes.Code,
            AllowOfflineAccess = true,
            
            // where to redirect to after login
            RedirectUris = { "subtitlesapp://" },

            // where to redirect to after logout
            PostLogoutRedirectUris = { "subtitlesapp://" },

            CoordinateLifetimeWithUserSession = true,

            AllowedScopes =
            {
                "openid",
                "profile",
                "email",
                "api",
                "offline_access"
            }
        }
    ];

}