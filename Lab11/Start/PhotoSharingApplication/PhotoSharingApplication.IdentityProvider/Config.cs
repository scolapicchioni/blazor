using Duende.IdentityServer.Models;
using IdentityModel;

namespace PhotoSharingApplication.IdentityProvider;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
        new ApiScope("photosrest") { UserClaims = new string[] { JwtClaimTypes.Name }},
        new ApiScope("commentsgrpc") { UserClaims = new string[] { JwtClaimTypes.Name }},
        };

    public static IEnumerable<Client> Clients =>
  new Client[]
  {
      // interactive client using code flow + pkce
      new Client
      {
          ClientId = "photosharing.bff",
          ClientSecrets = { new Secret("A9B27D26-E71C-4C53-89A8-3DAB53CE1854".Sha256()) }, //generate your own GUID

          AllowedGrantTypes = GrantTypes.Code,

          RedirectUris = { "https://localhost:5001/signin-oidc" },
          FrontChannelLogoutUri = "https://localhost:5001/signout-oidc",
          PostLogoutRedirectUris = { "https://localhost:5001/signout-callback-oidc" },

          AllowOfflineAccess = true,
          AllowedScopes = { "openid", "profile", "photosrest", "commentsgrpc" }
      },
  };
}
