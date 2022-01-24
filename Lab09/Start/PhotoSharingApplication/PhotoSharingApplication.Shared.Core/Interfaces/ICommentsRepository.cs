using PhotoSharingApplication.Shared.Core.Entities;

namespace PhotoSharingApplication.Shared.Core.Interfaces;

public interface ICommentsRepository {
    Task<List<Comment>?> GetCommentsForPhotoAsync(int photoId);
    Task<Comment?> FindAsync(int id);
    Task<Comment?> CreateAsync(Comment comment);
    Task<Comment?> UpdateAsync(Comment comment);
    Task<Comment?> RemoveAsync(int id);
}
