using Microsoft.AspNetCore.Authorization;

namespace PhotoSharingApplication.Shared.Authorization {
    //found on https://chrissainty.com/securing-your-blazor-apps-configuring-policy-based-authorization-with-blazor/
    public static class Policies {
        public const string EditDeletePhoto = "EditDeletePhoto";
        public const string EditDeleteComment = "EditDeleteComment";

        public static AuthorizationPolicy CanEditDeletePhotoPolicy() => new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
                                                   .AddRequirements(new SameAuthorRequirement())
                                                   .Build();

        public static AuthorizationPolicy CanEditDeleteCommentPolicy() => new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
                                                   .AddRequirements(new SameAuthorRequirement())
                                                   .Build();

        //public static AuthorizationPolicy IsUserPolicy()
        //{
        //    return new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
        //                                           .RequireRole("User")
        //                                           .Build();
        //}
    }
}
