using Microsoft.AspNetCore.Authorization;
using PhotoSharingExamples.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoSharingExamples.Shared.Authorization
{
    public class PhotoEditDeleteAuthorizationHandler : AuthorizationHandler<SameAuthorRequirement, Photo>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       SameAuthorRequirement requirement,
                                                       Photo resource)
        {
            if (context.User.Identity?.Name == resource.UserName)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
