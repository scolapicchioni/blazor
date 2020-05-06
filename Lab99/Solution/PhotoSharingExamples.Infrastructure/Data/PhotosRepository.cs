using Microsoft.EntityFrameworkCore;
using PhotoSharingExamples.Backend.Core.Interfaces;
using PhotoSharingExamples.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoSharingExamples.Backend.Infrastructure.Data
{
    public class PhotosRepository : IPhotosRepository
    {
        private readonly PhotoSharingApplicationContext _context;
        public PhotosRepository(PhotoSharingApplicationContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Photo photo)
        {
            _context.Add(photo);
            await _context.SaveChangesAsync();
        }

        public async Task<Photo> FindAsync(int id)
        {
            return await _context.Photos.SingleOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Photo> FindByTitle(string title)
        {
            return await _context.Photos.FirstOrDefaultAsync(p => p.Title.ToLower() == title.ToLower());
        }

        public async Task<List<Photo>> GetPhotosAsync(int number = 0)
        {
            List<Photo> photos;

            if (number == 0)
            {
                photos = await (from p in _context.Photos
                                orderby p.CreatedDate descending
                                select p).ToListAsync();
            }
            else
            {
                photos = await (from p in _context.Photos
                                orderby p.CreatedDate descending
                                select p).Take(number).ToListAsync();
            }
            return photos;
        }

        public async Task<List<Photo>> GetPhotosByIdsAsync(List<int> ids)
        {
            return await (from p in _context.Photos
                          where ids.Contains(p.Id)
                          select p).ToListAsync();
        }

        public async Task<Photo> RemoveAsync(int id)
        {
            var photo = await _context.Photos.SingleOrDefaultAsync(m => m.Id == id);
            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();
            return photo;
        }

        public async Task<Photo> UpdateAsync(Photo photo)
        {
            _context.Entry(photo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return photo;

        }
    }
}
