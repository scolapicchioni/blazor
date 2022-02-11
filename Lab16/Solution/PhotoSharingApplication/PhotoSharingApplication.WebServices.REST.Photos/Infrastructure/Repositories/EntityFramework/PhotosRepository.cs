﻿using Microsoft.EntityFrameworkCore;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;
using PhotoSharingApplication.WebServices.Rest.Photos.Infrastructure.Data;

namespace PhotoSharingApplication.WebServices.Rest.Photos.Infrastructure.Repositories.EntityFramework;

public class PhotosRepository : IPhotosRepository {
    private readonly PhotosDbContext context;

    public PhotosRepository(PhotosDbContext context) => this.context = context;
    public async Task<Photo?> CreateAsync(Photo photo) {
        context.Add(photo);
        await context.SaveChangesAsync();
        return photo;
    }

    public async Task<Photo?> FindAsync(int id) => await context.Photos.AsNoTracking().SingleOrDefaultAsync(m => m.Id == id);

    public async Task<Photo?> FindWithImageAsync(int id) => await context.Photos.Include(nameof(PhotoImage)).AsNoTracking().SingleOrDefaultAsync(m => m.Id == id);

    public async Task<PhotoImage?> GetImageAsync(int id)=> await context.PhotoImages.AsNoTracking().SingleOrDefaultAsync(prop => prop.Id == id);

    public async Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken) =>
        await (from p in context.Photos
               orderby p.CreatedDate descending
               select p).Skip(startIndex).Take(amount).ToListAsync(cancellationToken);

    public async Task<int> GetPhotosCountAsync() => await context.Photos.CountAsync();

    public async Task<Photo?> RemoveAsync(int id) {
        var photo = await context.Photos.SingleOrDefaultAsync(m => m.Id == id);
        if (photo is not null) {
            context.Photos.Remove(photo);
            await context.SaveChangesAsync();
        }
        return photo;
    }

    public async Task<Photo?> UpdateAsync(Photo photo) {
        context.Update(photo);
        await context.SaveChangesAsync();
        return photo;
    }
}
