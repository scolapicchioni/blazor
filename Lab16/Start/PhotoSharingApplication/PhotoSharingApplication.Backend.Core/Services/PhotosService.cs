using FluentValidation;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Exceptions;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Backend.Core.Services {
    public class PhotosService : IPhotosService {
        private readonly IPhotosRepository repository;
        private readonly IAuthorizationService<Photo> photosAuthorizationService;
        private readonly IUserService userService;
        private readonly IValidator<Photo> validator;

        public PhotosService(IPhotosRepository repository, IAuthorizationService<Photo> photosAuthorizationService, IUserService userService, IValidator<Photo> validator) {
            (this.repository, this.photosAuthorizationService, this.userService) = (repository, photosAuthorizationService, userService);
            this.validator = validator;
        }

        public async Task<Photo> FindAsync(int id) => await repository.FindAsync(id);
        public async Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken) => await repository.GetPhotosAsync(startIndex, amount, cancellationToken);
        public async Task<int> GetPhotosCountAsync() => await repository.GetPhotosCountAsync();
        public async Task<Photo> FindWithImageAsync(int id) => await repository.FindWithImageAsync(id);
        public async Task<PhotoImage> GetImageAsync(int id) => await repository.GetImageAsync(id);
        public async Task<Photo> RemoveAsync(int id) {
            Photo photo = await FindAsync(id);
            var user = await userService.GetUserAsync();
            if (await photosAuthorizationService.ItemMayBeDeletedAsync(user, photo))
                return await repository.RemoveAsync(id);
            else throw new UnauthorizedDeleteAttemptException<Photo>($"Unauthorized Deletion Attempt of Photo {photo.Id}");
        }
        public async Task<Photo> UpdateAsync(Photo photo) {
            var user = await userService.GetUserAsync();
            if (await photosAuthorizationService.ItemMayBeUpdatedAsync(user, photo)) {
                validator.ValidateAndThrow(photo);
                return await repository.UpdateAsync(photo);
            } else throw new UnauthorizedEditAttemptException<Photo>($"Unauthorized Edit Attempt of Photo {photo.Id}");
        }
        public async Task<Photo> UploadAsync(Photo photo) {
            var user = await userService.GetUserAsync();
            if (await photosAuthorizationService.ItemMayBeCreatedAsync(user, photo)) {
                photo.CreatedDate = DateTime.Now;
                photo.UserName = user.Identity.Name;
                validator.ValidateAndThrow(photo);
                return await repository.CreateAsync(photo);
            } else throw new UnauthorizedCreateAttemptException<Photo>($"Unauthorized Create Attempt of Photo {photo.Id}");
        }
    }
}
