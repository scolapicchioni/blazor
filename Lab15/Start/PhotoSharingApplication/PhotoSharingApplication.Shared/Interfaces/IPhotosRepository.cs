using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.Shared.Interfaces; 
public interface IPhotosRepository {
    Task<Photo?> UpdateAsync(Photo photo);
    Task<Photo?> FindAsync(int id);
    Task<Photo?> CreateAsync(Photo photo);

    Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken);
    Task<int> GetPhotosCountAsync();

    Task<Photo?> RemoveAsync(int id);

    Task<Photo> FindWithImageAsync(int id);
    Task<PhotoImage> GetImageAsync(int id);
}
