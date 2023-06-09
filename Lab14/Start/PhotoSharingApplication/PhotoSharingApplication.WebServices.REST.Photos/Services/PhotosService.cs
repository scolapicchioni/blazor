using FluentValidation;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Exceptions;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.WebServices.REST.Photos.Services;
public class PhotosService : IPhotosService {
    private readonly IPhotosRepository repository;
    private readonly IAuthorizationService<Photo> photosAuthorizationService;
    private readonly IUserService userService;
    private readonly IValidator<Photo> validator;
    public PhotosService(IPhotosRepository repository,IAuthorizationService<Photo> photosAuthorizationService, IUserService userService, IValidator<Photo> validator) 
        => (this.repository, this.photosAuthorizationService, this.userService, this.validator) = (repository, photosAuthorizationService, userService, validator);

    public async Task<Photo?> FindAsync(int id) => await repository.FindAsync(id);

    public async Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken) => await repository.GetPhotosAsync(startIndex, amount, cancellationToken);

    public async Task<int> GetPhotosCountAsync() => await repository.GetPhotosCountAsync();

    public async Task<Photo?> RemoveAsync(int id) {
        Photo? photo = await FindAsync(id);
        if (photo is not null) {
            var user = await userService.GetUserAsync();
            if (!await photosAuthorizationService.ItemMayBeDeletedAsync(user, photo))
                throw new DeleteUnauthorizedException<Photo>($"Unauthorized Deletion Attempt of Photo {photo.Id}");
            photo = await repository.RemoveAsync(id);
        }
        return photo;
    }

    public async Task<Photo?> UpdateAsync(Photo photo) {
        var user = await userService.GetUserAsync();
        if (await photosAuthorizationService.ItemMayBeUpdatedAsync(user, photo)) {
            validator.ValidateAndThrow(photo);
            return await repository.UpdateAsync(photo);
        } else throw new EditUnauthorizedException<Photo>($"Unauthorized Edit Attempt of Photo {photo.Id}");
    }

    public async Task<Photo?> UploadAsync(Photo photo) {
        var user = await userService.GetUserAsync();
        if (await photosAuthorizationService.ItemMayBeCreatedAsync(user, photo)) {
            photo.CreatedDate = DateTime.Now;
            photo.UserName = user?.Identity?.Name;
            validator.ValidateAndThrow(photo);
            return await repository.CreateAsync(photo);
        } else throw new CreateUnauthorizedException<Photo>($"Unauthorized Create Attempt of Photo {photo.Id}");
    }

    public async Task<Photo?> FindWithImageAsync(int id) => await repository.FindWithImageAsync(id);
    public async Task<PhotoImage?> GetImageAsync(int id) => await repository.GetImageAsync(id);
}