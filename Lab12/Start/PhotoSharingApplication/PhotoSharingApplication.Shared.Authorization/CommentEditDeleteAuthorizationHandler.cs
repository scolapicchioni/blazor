using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.Shared.Authorization;
public class CommentEditDeleteAuthorizationHandler : AuthorizationHandler<SameAuthorRequirement, Comment> {
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SameAuthorRequirement requirement, Comment comment) {
        if (context.User.Identity?.Name == comment.UserName) {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}