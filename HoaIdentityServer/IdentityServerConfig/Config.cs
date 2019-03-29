using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace HoaIdentityServer.IdentityServerConfig
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>()
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Email(),
                new IdentityResources.Profile(),
                new IdentityResource("roles",
                                     "Your Roles",
                                     new List<string>() { JwtClaimTypes.Role, "readonly", "postcreation"})
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>()
            {
                new Client
                {
                    ClientName = "HOA Web Application",
                    ClientId = "hoawebapp",
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    AllowOfflineAccess = true,
                    RedirectUris =
                    {
                         "https://localhost:44360/signin-oidc"
                    },
                    PostLogoutRedirectUris =
                    {
                        "https://localhost:44360/signout-callback-oidc"
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Profile,
                        "roles",
                        "hoawebapi"
                    },
                    AlwaysIncludeUserClaimsInIdToken = true,
                    UpdateAccessTokenClaimsOnRefresh = true,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha512())
                    }
                },

                new Client
                {
                    ClientId = "idpclient",

                    AccessTokenLifetime = 86400,                    

                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // scopes that client has access to
                    AllowedScopes = { "usermgapi" }
                },

                 new Client
                {
                    ClientId = "apiclient",

                    AccessTokenLifetime = 86400,                    

                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // scopes that client has access to
                    AllowedScopes = { "hoawebapi" }
                }
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
                {
                    new ApiResource("hoawebapi", "HOA Web API", new List<string>() {JwtClaimTypes.Role, "readonly", "postcreation"}),
                    new ApiResource("usermgapi", "IDP Access Token API")
                };
        }
    }
}
