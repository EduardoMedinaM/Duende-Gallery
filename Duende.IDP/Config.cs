 using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Duende.IDP;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            // User identifier:
            // allows the client to access to the SubjectId claim
            new IdentityResources.OpenId(),
            // if a user client requests profile as scope, 
            // Username and GivenName claims will be returned.
            new IdentityResources.Profile(),
            // Role is not part of Open Id
            new IdentityResource("roles",
                "Your role(s)",
                new []{"role"}),

            // Country claim to use in the Policy
            new IdentityResource("country",
                "The country you're living in",
                new List<string>(){"country"})

        };

    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
            { 
                new ApiResource("imagegalleryapi", "Image Gallery API", new [] {"role"})
                {
                    Scopes = { "imagegalleryapi.fullaccess" }
                }
            };

    /*
     * A client app can get access to an API(s)
     */
    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
            {
                new ApiScope("imagegalleryapi.fullaccess")
            };
    /*
     * The client apps
     */
    public static IEnumerable<Client> Clients =>
        new Client[]
            {
                new Client()
                {
                    ClientName = "image_gallery",
                    ClientId = "imagegalleryclient",
                    // Sets the code flow, delivered to the UI via uri
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = 
                    {
                        "https://localhost:7184/signin-oidc"
                    },
                    PostLogoutRedirectUris=
                    {
                        "https://localhost:7184/signout-callback-oidc"
                    },
                    AllowedScopes =
                    {
                        // Provided by Duende
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "roles",
                        "imagegalleryapi.fullaccess",
                        "country"
                    },
                    ClientSecrets =
                    {
                        // Allows the client application to execute an authenticated call
                        // to the token endpoint. It hashes
                        new Secret("secret".Sha256())
                    },
                    RequireConsent = true
                }
            };
}