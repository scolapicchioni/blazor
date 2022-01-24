using PhotoSharingApplication.Shared.Core.Entities;

namespace PhotoSharingApplication.Shared.Core.Interfaces;

public interface IPhotosRepository {
    Task<Photo?> UpdateAsync(Photo photo);
    Task<Photo?> FindAsync(int id);
    Task<Photo?> CreateAsync(Photo photo);
    Task<List<Photo>> GetPhotosAsync(int amount = 10);
    Task<Photo?> RemoveAsync(int id);
}