using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.Shared.Authorization; 
public class PhotoEditDeleteAuthorizationHandler : AuthorizationHandler<SameAuthorRequirement, Photo> {
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SameAuthorRequirement requirement, Photo photo) {
        if (context.User.Identity?.Name == photo.UserName) {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
