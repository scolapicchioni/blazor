using Microsoft.AspNetCore.Authorization;

namespace PhotoSharingApplication.Shared.Authorization {
    //found on https://chrissainty.com/securing-your-blazor-apps-configuring-policy-based-authorization-with-blazor/
    public static class Policies {
        public const string CreatePhoto = "CreatePhoto";
        public const string EditPhoto = "EditPhoto";
        public const string DeletePhoto = "DeletePhoto";
        public const string CreateComment = "CreateComment";
        public const string EditComment = "EditComment";
        public const string DeleteComment = "DeleteComment";

        public static AuthorizationPolicy MayCreatePhotoPolicy() => new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        public static AuthorizationPolicy MayEditPhotoPolicy() => new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
                                                   .AddRequirements(new SameAuthorRequirement())
                                                   .Build();
        public static AuthorizationPolicy MayDeletePhotoPolicy() => new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
                                                   .AddRequirements(new SameAuthorRequirement())
                                                   .Build();

        public static AuthorizationPolicy MayCreateCommentPolicy() => new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        public static AuthorizationPolicy MayEditCommentPolicy() => new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
                                                   .AddRequirements(new SameAuthorRequirement())
                                                   .Build();
        public static AuthorizationPolicy MayDeleteCommentPolicy() => new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
                                                   .AddRequirements(new SameAuthorRequirement())
                                                   .Build();

        public static void AddPhotosPolicies(this AuthorizationOptions options) {
            options.AddPolicy(Policies.EditPhoto, Policies.MayEditPhotoPolicy());
            options.AddPolicy(Policies.DeletePhoto, Policies.MayDeletePhotoPolicy());
            options.AddPolicy(Policies.CreatePhoto, Policies.MayCreatePhotoPolicy());
        }

        public static void AddCommentsPolicies(this AuthorizationOptions options) {
            options.AddPolicy(Policies.CreateComment, Policies.MayCreateCommentPolicy());
            options.AddPolicy(Policies.EditComment, Policies.MayEditCommentPolicy());
            options.AddPolicy(Policies.DeleteComment, Policies.MayDeleteCommentPolicy());
        }
        //public static AuthorizationPolicy IsUserPolicy()
        //{
        //    return new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
        //                                           .RequireRole("User")
        //                                           .Build();
        //}
    }
}
