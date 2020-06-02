using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Core.Entities;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Shared.Authorization {
    public class PhotoSameAuthorAuthorizationHandler : AuthorizationHandler<SameAuthorRequirement, Photo> {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       SameAuthorRequirement requirement,
                                                       Photo resource) {
            if (context.User.Identity?.Name == resource.UserName) {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
