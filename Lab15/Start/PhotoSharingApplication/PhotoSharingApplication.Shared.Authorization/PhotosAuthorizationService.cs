using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Shared.Authorization {
    public class PhotosAuthorizationService : IAuthorizationService<Photo> {
        private readonly IAuthorizationService authorizationService;
        public PhotosAuthorizationService(IAuthorizationService authorizationService) => this.authorizationService = authorizationService;
        public async Task<bool> ItemMayBeCreatedAsync(ClaimsPrincipal User, Photo photo) => (await authorizationService.AuthorizeAsync(User, photo, Policies.CreatePhoto)).Succeeded;
        public async Task<bool> ItemMayBeDeletedAsync(ClaimsPrincipal User, Photo photo) => (await authorizationService.AuthorizeAsync(User, photo, Policies.DeletePhoto)).Succeeded;
        public async Task<bool> ItemMayBeUpdatedAsync(ClaimsPrincipal User, Photo photo) => (await authorizationService.AuthorizeAsync(User, photo, Policies.EditPhoto)).Succeeded;
    }
}
