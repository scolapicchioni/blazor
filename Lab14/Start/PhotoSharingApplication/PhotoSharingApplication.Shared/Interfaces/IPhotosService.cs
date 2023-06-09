using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.Shared.Interfaces;
public interface IPhotosService {
    Task<Photo?> UploadAsync(Photo photo);
    Task<Photo?> UpdateAsync(Photo photo);
    Task<Photo?> FindAsync(int id);

    Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken);
    Task<int> GetPhotosCountAsync();
    
    Task<Photo?> RemoveAsync(int id);

    Task<Photo> FindWithImageAsync(int id);
    Task<PhotoImage> GetImageAsync(int id);
}
