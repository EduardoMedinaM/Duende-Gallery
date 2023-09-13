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
            new IdentityResources.Profile()
        };

    /*
     * A client app can get access to an API(s)
     */
    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
            { };
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
                    AllowedScopes =
                    {
                        // Provided by Duende
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
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