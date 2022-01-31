using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.Shared.Interfaces;

public interface IPhotosService {
    Task<Photo?> UploadAsync(Photo photo);
    Task<Photo?> UpdateAsync(Photo photo);
    Task<Photo?> FindAsync(int id);
    Task<List<Photo>> GetPhotosAsync(int amount = 10);
    Task<Photo?> RemoveAsync(int id);
}
