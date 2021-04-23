using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Shared.Authorization {
    public class CommentsAuthorizationService : IAuthorizationService<Comment> {
        private readonly IAuthorizationService authorizationService;

        public CommentsAuthorizationService(IAuthorizationService authorizationService) {
            this.authorizationService = authorizationService;
        }

        public async Task<bool> ItemMayBeCreatedAsync(ClaimsPrincipal User, Comment comment) {
            var authorizationResult = await authorizationService.AuthorizeAsync(User, comment, Policies.CreateComment);
            return authorizationResult.Succeeded;
        }

        public async Task<bool> ItemMayBeDeletedAsync(ClaimsPrincipal User, Comment comment) {
            var authorizationResult = await authorizationService.AuthorizeAsync(User, comment, Policies.DeleteComment);
            return authorizationResult.Succeeded;
        }

        public async Task<bool> ItemMayBeUpdatedAsync(ClaimsPrincipal User, Comment comment) {
            var authorizationResult = await authorizationService.AuthorizeAsync(User, comment, Policies.EditComment);
            return authorizationResult.Succeeded;
        }
    }
}
