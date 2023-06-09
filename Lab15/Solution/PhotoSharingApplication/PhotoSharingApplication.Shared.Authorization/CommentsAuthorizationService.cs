using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;
using System.Security.Claims;

namespace PhotoSharingApplication.Shared.Authorization;
public class CommentsAuthorizationService : IAuthorizationService<Comment> {
    private readonly IAuthorizationService authorizationService;

    public CommentsAuthorizationService(IAuthorizationService authorizationService) => this.authorizationService = authorizationService;
    public async Task<bool> ItemMayBeCreatedAsync(ClaimsPrincipal User, Comment item) => (await authorizationService.AuthorizeAsync(User, item, Policies.CreateComment)).Succeeded;

    public async Task<bool> ItemMayBeDeletedAsync(ClaimsPrincipal User, Comment item) => (await authorizationService.AuthorizeAsync(User, item, Policies.DeleteComment)).Succeeded;

    public async Task<bool> ItemMayBeUpdatedAsync(ClaimsPrincipal User, Comment item) => (await authorizationService.AuthorizeAsync(User, item, Policies.EditComment)).Succeeded;
}
