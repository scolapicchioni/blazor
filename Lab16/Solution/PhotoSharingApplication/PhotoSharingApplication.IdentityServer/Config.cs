// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace PhotoSharingApplication.IdentityServer
{
    public class Config
    {
        private readonly IConfiguration configuration;

        public Config(IConfiguration configuration) {
            this.configuration = configuration;
        }
        public IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };


        public IEnumerable<ApiResource> Apis =>
            new ApiResource[] {
                new ApiResource("commentsgrpc", "Comments gRpc Service") {UserClaims = new string[] { JwtClaimTypes.Name } }, 
                new ApiResource("photosrest", "Photos REST Service") {UserClaims = new string[] { JwtClaimTypes.Name } }
            };

        public IEnumerable<Client> Clients =>
            new Client[]
            { 
                // Blazor client using code flow + pkce
                new Client
                {
                    ClientId = "blazorclient",
                    ClientName = "Blazor Client",
                    ClientUri = configuration["blazorclient"], //"https://localhost:5001/",

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,

                    RedirectUris =
                    {
                        configuration["blazorclient"],
                        $"{configuration["blazorclient"]}/authentication/login-callback",
                        $"{configuration["blazorclient"]}/authentication/silent",
                        $"{configuration["blazorclient"]}/authentication/popup",
                    },

                    PostLogoutRedirectUris = { configuration["blazorclient"] },
                    AllowedCorsOrigins = { configuration["blazorclient"] },

                    AllowedScopes = { "openid", "profile", "photosrest", "commentsgrpc" }
                }
            };
    }
}
