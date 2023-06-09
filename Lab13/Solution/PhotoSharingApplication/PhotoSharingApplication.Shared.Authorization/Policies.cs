using Microsoft.AspNetCore.Authorization;

namespace PhotoSharingApplication.Shared.Authorization;
public static class Policies {
    public const string CreatePhoto = "CreatePhoto";
    public const string EditPhoto = "EditPhoto";
    public const string DeletePhoto = "DeletePhoto";
    public const string CreateComment = "CreateComment";
    public const string EditComment = "EditComment";
    public const string DeleteComment = "DeleteComment";

    public static AuthorizationPolicy MayCreatePhotoPolicy() => new AuthorizationPolicyBuilder()
      .RequireAuthenticatedUser()
      .Build();

    public static AuthorizationPolicy MayEditPhotoPolicy() => new AuthorizationPolicyBuilder()
      .RequireAuthenticatedUser()
      .AddRequirements(new SameAuthorRequirement())
      .Build();

    public static AuthorizationPolicy MayDeletePhotoPolicy() => new AuthorizationPolicyBuilder()
      .RequireAuthenticatedUser()
      .AddRequirements(new SameAuthorRequirement())
      .Build();

    public static AuthorizationPolicy MayCreateCommentPolicy() => new AuthorizationPolicyBuilder()
      .RequireAuthenticatedUser()
      .Build();

    public static AuthorizationPolicy MayEditCommentPolicy() => new AuthorizationPolicyBuilder()
      .RequireAuthenticatedUser()
      .AddRequirements(new SameAuthorRequirement())
      .Build();

    public static AuthorizationPolicy MayDeleteCommentPolicy() => new AuthorizationPolicyBuilder()
      .RequireAuthenticatedUser()
      .AddRequirements(new SameAuthorRequirement())
      .Build();

    public static void AddPhotosPolicies(this AuthorizationOptions options) {
        options.AddPolicy(EditPhoto, MayEditPhotoPolicy());
        options.AddPolicy(DeletePhoto, MayDeletePhotoPolicy());
        options.AddPolicy(CreatePhoto, MayCreatePhotoPolicy());
    }

    public static void AddCommentsPolicies(this AuthorizationOptions options) {
        options.AddPolicy(CreateComment, MayCreateCommentPolicy());
        options.AddPolicy(EditComment, MayEditCommentPolicy());
        options.AddPolicy(DeleteComment, MayDeleteCommentPolicy());
    }
}
