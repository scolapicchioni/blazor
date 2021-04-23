using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Shared.Authorization {
    public class CommentsAuthorizationService : IAuthorizationService<Comment> {
        private readonly IAuthorizationService authorizationService;
        public CommentsAuthorizationService(IAuthorizationService authorizationService) => this.authorizationService = authorizationService;
        public async Task<bool> ItemMayBeCreatedAsync(ClaimsPrincipal User, Comment comment) => (await authorizationService.AuthorizeAsync(User, comment, Policies.CreateComment)).Succeeded;
        public async Task<bool> ItemMayBeDeletedAsync(ClaimsPrincipal User, Comment comment) => (await authorizationService.AuthorizeAsync(User, comment, Policies.DeleteComment)).Succeeded;
        public async Task<bool> ItemMayBeUpdatedAsync(ClaimsPrincipal User, Comment comment) => (await authorizationService.AuthorizeAsync(User, comment, Policies.EditComment)).Succeeded;
    }
}
