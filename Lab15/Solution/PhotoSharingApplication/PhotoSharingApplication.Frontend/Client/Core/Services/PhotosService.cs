using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.Frontend.Client.Core.Services;
public class PhotosService : IPhotosService {
    private readonly IPhotosRepository repository;
    public PhotosService(IPhotosRepository repository) => this.repository = repository;
    public async Task<Photo?> FindAsync(int id) => await repository.FindAsync(id);

    public async Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken) => await repository.GetPhotosAsync(startIndex, amount, cancellationToken);
    public async Task<int> GetPhotosCountAsync() => await repository.GetPhotosCountAsync();
    
    public async Task<Photo?> RemoveAsync(int id) => await repository.RemoveAsync(id);
    public async Task<Photo?> UpdateAsync(Photo photo) => await repository.UpdateAsync(photo);
    public async Task<Photo?> UploadAsync(Photo photo) {
        photo.CreatedDate = DateTime.Now;
        return await repository.CreateAsync(photo);
    }

    public async Task<Photo?> FindWithImageAsync(int id) => await repository.FindWithImageAsync(id);
    public async Task<PhotoImage?> GetImageAsync(int id) => await repository.GetImageAsync(id);
}