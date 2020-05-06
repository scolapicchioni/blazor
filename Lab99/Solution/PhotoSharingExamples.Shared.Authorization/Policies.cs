using Microsoft.AspNetCore.Authorization;
using System;

namespace PhotoSharingExamples.Shared.Authorization
{
    //found on https://chrissainty.com/securing-your-blazor-apps-configuring-policy-based-authorization-with-blazor/
    public static class Policies
    {
        public const string EditDeletePhoto = "EditDeletePhoto";
        public const string EditDeleteComment = "EditDeleteComment";

        public static AuthorizationPolicy CanEditDeletePhotoPolicy()
        {
            return new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
                                                   .AddRequirements(new SameAuthorRequirement())
                                                   .Build();
        }

        public static AuthorizationPolicy CanEditDeleteCommentPolicy()
        {
            return new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
                                                   .AddRequirements(new SameAuthorRequirement())
                                                   .Build();
        }

        //public static AuthorizationPolicy IsUserPolicy()
        //{
        //    return new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
        //                                           .RequireRole("User")
        //                                           .Build();
        //}
    }
}
