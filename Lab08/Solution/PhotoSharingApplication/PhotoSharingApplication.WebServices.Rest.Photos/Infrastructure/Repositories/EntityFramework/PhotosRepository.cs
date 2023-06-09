using Microsoft.EntityFrameworkCore;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;
using PhotoSharingApplication.WebServices.REST.Photos.Infrastructure.Data;

namespace PhotoSharingApplication.WebServices.REST.Photos.Infrastructure.Repositories.EntityFramework; 
public class PhotosRepository : IPhotosRepository {
    private readonly PhotosDbContext context;

    public PhotosRepository(PhotosDbContext context) {
        this.context = context;
    }
    public async Task<Photo> CreateAsync(Photo photo) {
        context.Add(photo);
        await context.SaveChangesAsync();
        return photo;
    }
    public async Task<Photo> UpdateAsync(Photo photo) {
        context.Update(photo);
        await context.SaveChangesAsync();
        return photo;
    }
    public async Task<Photo> RemoveAsync(int id) {
        var photo = await context.Photos.SingleOrDefaultAsync(m => m.Id == id);
        if (photo is not null) {
            context.Photos.Remove(photo);
            await context.SaveChangesAsync();
        }
        return photo;
    }
    public async Task<List<Photo>> GetPhotosAsync(int amount = 10) =>
        await (from p in context.Photos
            orderby p.CreatedDate descending
            select p).Take(amount).ToListAsync();

    public async Task<Photo> FindAsync(int id) => await context.Photos.SingleOrDefaultAsync(m => m.Id == id);
}
