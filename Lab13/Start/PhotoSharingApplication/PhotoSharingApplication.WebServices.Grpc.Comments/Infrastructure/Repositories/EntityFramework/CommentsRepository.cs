using Microsoft.EntityFrameworkCore;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;
using PhotoSharingApplication.WebServices.Grpc.Comments.Infrastructure.Data;

namespace PhotoSharingApplication.WebServices.Grpc.Comments.Infrastructure.Repositories.EntityFramework;

public class CommentsRepository : ICommentsRepository {
    private readonly CommentsDbContext context;

    public CommentsRepository(CommentsDbContext context) => this.context = context;
    public async Task<Comment?> CreateAsync(Comment comment) {
        context.Add(comment);
        await context.SaveChangesAsync();
        return comment;
    }

    public async Task<Comment?> FindAsync(int id) => await context.Comments.AsNoTracking().SingleOrDefaultAsync(m => m.Id == id);

    public async Task<List<Comment>?> GetCommentsForPhotoAsync(int photoId) => await context.Comments.Where(c => c.PhotoId == photoId).ToListAsync();

    public async Task<Comment?> RemoveAsync(int id) {
        Comment comment = await context.Comments.SingleOrDefaultAsync(m => m.Id == id);
        context.Comments.Remove(comment);
        await context.SaveChangesAsync();
        return comment;
    }

    public async Task<Comment?> UpdateAsync(Comment comment) {
        context.Update(comment);
        await context.SaveChangesAsync();
        return comment;
    }
}
