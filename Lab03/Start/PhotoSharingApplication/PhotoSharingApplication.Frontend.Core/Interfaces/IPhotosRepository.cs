using PhotoSharingApplication.Frontend.Core.Entities;

namespace PhotoSharingApplication.Frontend.Core.Interfaces;

public interface IPhotosRepository {
    Task<Photo?> UpdateAsync(Photo photo);
    Task<Photo?> FindAsync(int id);
    Task<Photo?> CreateAsync(Photo photo);
    Task<List<Photo>> GetPhotosAsync(int amount = 10);
    Task<Photo?> RemoveAsync(int id);
}