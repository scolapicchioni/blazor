// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace PhotoSharingExamples.IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };


        public static IEnumerable<ApiResource> Apis =>
            new ApiResource[]
            {
                new ApiResource("photos", "Photos gRpc Service") {UserClaims = new string[] { JwtClaimTypes.Name } }, //5010 - 5011
                new ApiResource("comments", "Comments gRpc Service") {UserClaims = new string[] { JwtClaimTypes.Name } }, //5020-5021
                new ApiResource("photosrest", "Photos REST Service") {UserClaims = new string[] { JwtClaimTypes.Name } }, //5040 - 5041
            };


        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                // client credentials flow client
                new Client
                {
                    ClientId = "client",
                    ClientName = "Client Credentials Client",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                    AllowedScopes = { "photos", "comments" }
                },

                // MVC client using code flow + pkce
                new Client
                {
                    ClientId = "mvc",
                    ClientName = "MVC Client",

                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    RequirePkce = true,
                    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                    RedirectUris = { "http://localhost:5003/signin-oidc" },
                    FrontChannelLogoutUri = "http://localhost:5003/signout-oidc",
                    PostLogoutRedirectUris = { "http://localhost:5003/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "photos", "comments" }
                },

                // SPA client using code flow + pkce
                new Client
                {
                    ClientId = "spa",
                    ClientName = "SPA Client",
                    ClientUri = "http://identityserver.io",

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,

                    RedirectUris =
                    {
                        "http://localhost:5002/index.html",
                        "http://localhost:5002/callback.html",
                        "http://localhost:5002/silent.html",
                        "http://localhost:5002/popup.html",
                    },

                    PostLogoutRedirectUris = { "http://localhost:5002/index.html" },
                    AllowedCorsOrigins = { "http://localhost:5002" },

                    AllowedScopes = { "openid", "profile", "photos", "comments" }
                },
                // Blazor client
                new Client
                {
                    ClientId = "blazorstandalone",
                    ClientName = "Blazor Client Standalone",
                    ClientUri = "https://localhost:5031",

                    AllowedGrantTypes = GrantTypes.Code, // you have to set this on the client as well
                    RequirePkce = true,
                    RequireClientSecret = false,

                    RedirectUris =
                    {
                        "https://localhost:5031/index.html",
                        "https://localhost:5031/authentication/login-callback",
                        "https://localhost:5031/authentication/silent",
                        "https://localhost:5031/authentication/popup",
                    },

                    PostLogoutRedirectUris = { "https://localhost:5031/index.html" },
                    AllowedCorsOrigins = { "https://localhost:5031" },

                    AllowedScopes = { "openid", "profile", "photos", "comments", "photosrest" },

                    //This feature refresh token
                    AllowOfflineAccess = true,
                    //Access token life time is 7200 seconds (2 hour)
                    AccessTokenLifetime = 7200,
                    //Identity token life time is 7200 seconds (2 hour)
                    IdentityTokenLifetime = 7200
                }
            };
    }
}
