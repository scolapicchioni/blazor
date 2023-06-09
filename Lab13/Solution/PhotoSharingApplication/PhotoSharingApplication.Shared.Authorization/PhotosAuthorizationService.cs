using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;
using System.Security.Claims;

namespace PhotoSharingApplication.Shared.Authorization;
public class PhotosAuthorizationService : IAuthorizationService<Photo> {
    private readonly IAuthorizationService authorizationService;
    public PhotosAuthorizationService(IAuthorizationService authorizationService) => this.authorizationService = authorizationService;
    public async Task<bool> ItemMayBeCreatedAsync(ClaimsPrincipal User, Photo item) => (await authorizationService.AuthorizeAsync(User, item, Policies.CreatePhoto)).Succeeded;

    public async Task<bool> ItemMayBeDeletedAsync(ClaimsPrincipal User, Photo item) => (await authorizationService.AuthorizeAsync(User, item, Policies.DeletePhoto)).Succeeded;

    public async Task<bool> ItemMayBeUpdatedAsync(ClaimsPrincipal User, Photo item) => (await authorizationService.AuthorizeAsync(User, item, Policies.EditPhoto)).Succeeded;
}
