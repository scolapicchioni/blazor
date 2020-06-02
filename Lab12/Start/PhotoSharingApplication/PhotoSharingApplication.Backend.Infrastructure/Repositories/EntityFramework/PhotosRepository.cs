using Microsoft.EntityFrameworkCore;
using PhotoSharingApplication.Backend.Infrastructure.Data;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Backend.Infrastructure.Repositories.EntityFramework {
    public class PhotosRepository : IPhotosRepository {
        private readonly PhotoSharingApplicationContext context;

        public PhotosRepository(PhotoSharingApplicationContext context) {
            this.context = context;
        }

        public async Task<Photo> CreateAsync(Photo photo) {
            context.Add(photo);
            await context.SaveChangesAsync();
            return photo;
        }

        public async Task<Photo> FindAsync(int id) => await context.Photos.AsNoTracking().SingleOrDefaultAsync(m => m.Id == id);

        public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await (from p in context.Photos
                                                                                 orderby p.CreatedDate descending
                                                                                 select p).Take(amount).ToListAsync();

    

        public async Task<Photo> RemoveAsync(int id) {
            var photo = await context.Photos.SingleOrDefaultAsync(m => m.Id == id);
            context.Photos.Remove(photo);
            await context.SaveChangesAsync();
            return photo;
        }

        public async Task<Photo> UpdateAsync(Photo photo) {
            //context.Entry(photo).State = EntityState.Modified;
            context.Update(photo);
            await context.SaveChangesAsync();
            return photo;
        }
    }
}
