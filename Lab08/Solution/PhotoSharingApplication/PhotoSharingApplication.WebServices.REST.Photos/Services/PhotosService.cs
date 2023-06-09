using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.WebServices.REST.Photos.Services;
public class PhotosService : IPhotosService {
    private readonly IPhotosRepository repository;
    public PhotosService(IPhotosRepository repository) => this.repository = repository;
    public async Task<Photo?> FindAsync(int id) => await repository.FindAsync(id);
    public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await repository.GetPhotosAsync(amount);
    public async Task<Photo?> RemoveAsync(int id) => await repository.RemoveAsync(id);
    public async Task<Photo?> UpdateAsync(Photo photo) => await repository.UpdateAsync(photo);
    public async Task<Photo?> UploadAsync(Photo photo) {
        photo.CreatedDate = DateTime.Now;
        return await repository.CreateAsync(photo);
    }
}