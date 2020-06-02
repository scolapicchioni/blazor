using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Exceptions;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Core.Services {
    public class PhotosService : IPhotosService {
        private readonly IPhotosRepository repository;
        private readonly IAuthorizationService<Photo> photosAuthorizationService;
        private readonly IUserService userService;

        public PhotosService(IPhotosRepository repository, IAuthorizationService<Photo> photosAuthorizationService, IUserService userService) {
            this.repository = repository;
            this.photosAuthorizationService = photosAuthorizationService;
            this.userService = userService;
        }
        public async Task<Photo> FindAsync(int id) => await repository.FindAsync(id);
        public async Task<Photo> FindWithImageAsync(int id) => await repository.FindWithImageAsync(id);
        public async Task<PhotoImage> GetImageAsync(int id) => await repository.GetImageAsync(id);
        public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await repository.GetPhotosAsync(amount);
        public async Task<Photo> RemoveAsync(int id) {
            Photo photo = await FindAsync(id);
            var user = await userService.GetUserAsync();
            if (await photosAuthorizationService.ItemMayBeDeletedAsync(user, photo))
                return await repository.RemoveAsync(id);
            else throw new UnauthorizedDeleteAttemptException<Photo>($"Unauthorized Deletion Attempt of Photo {photo.Id}");
             
        }
        public async Task<Photo> UpdateAsync(Photo photo) {
            var user = await userService.GetUserAsync();
            if (await photosAuthorizationService.ItemMayBeUpdatedAsync(user,photo))
                return await repository.UpdateAsync(photo);
            else throw new UnauthorizedEditAttemptException<Photo>($"Unauthorized Edit Attempt of Photo {photo.Id}");
        }
        public async Task<Photo> UploadAsync(Photo photo) {
            var user = await userService.GetUserAsync();
            if (await photosAuthorizationService.ItemMayBeCreatedAsync(user,photo)) {
                photo.CreatedDate = DateTime.Now;
                photo.UserName = user.Identity.Name;
                return await repository.CreateAsync(photo);
            }else throw new UnauthorizedCreateAttemptException<Photo>($"Unauthorized Create Attempt of Photo {photo.Id}");
        }
    }
}
