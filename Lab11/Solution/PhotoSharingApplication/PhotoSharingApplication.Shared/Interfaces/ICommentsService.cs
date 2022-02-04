using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.Shared.Interfaces;

public interface ICommentsService {
    Task<List<Comment>?> GetCommentsForPhotoAsync(int photoId);
    Task<Comment?> FindAsync(int id);
    Task<Comment?> CreateAsync(Comment comment);
    Task<Comment?> UpdateAsync(Comment comment);
    Task<Comment?> RemoveAsync(int id);
}
