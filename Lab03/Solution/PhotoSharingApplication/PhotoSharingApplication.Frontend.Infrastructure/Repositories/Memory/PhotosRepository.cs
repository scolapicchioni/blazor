using PhotoSharingApplication.Frontend.Core.Entities;
using PhotoSharingApplication.Frontend.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Infrastructure.Repositories.Memory {
    public class PhotosRepository : IPhotosRepository {
        private List<Photo> photos;
        public PhotosRepository() {
            photos = new List<Photo> {
        new Photo {Id=1, Title = "One photo", Description = "Lorem ipsum dolor sit amen", CreatedDate = DateTime.Now.AddDays(-2) },
        new Photo {Id=2, Title = "Another photo", Description = "Some description" ,CreatedDate= DateTime.Now.AddDays(-1)},
        new Photo {Id=3, Title = "Yet another photo", Description = "More description here", CreatedDate= DateTime.Now }
      };
        }
        public Task<Photo> CreateAsync(Photo photo) {
            photo.Id = photos.Max(p => p.Id) + 1;
            photos.Add(photo);
            return Task.FromResult(photo);
        }

        public Task<Photo> FindAsync(int id) => Task.FromResult(photos.FirstOrDefault(p => p.Id == id));

        public Task<List<Photo>> GetPhotosAsync(int amount = 10) => Task.FromResult(photos.OrderByDescending(p => p.CreatedDate).ThenBy(p => p.Title).Take(amount).ToList());

        public Task<Photo> RemoveAsync(int id) {
            Photo photo = photos.FirstOrDefault(p => p.Id == id);
            if (photo != null) photos.Remove(photo);
            return Task.FromResult(photo);
        }

        public Task<Photo> UpdateAsync(Photo photo) {
            Photo oldPhoto = photos.FirstOrDefault(p => p.Id == photo.Id);
            if (oldPhoto != null) {
                oldPhoto.Title = photo.Title;
                oldPhoto.PhotoFile = photo.PhotoFile;
                oldPhoto.ImageMimeType = photo.ImageMimeType;
                oldPhoto.Description = photo.Description;
                oldPhoto.CreatedDate = photo.CreatedDate;
                oldPhoto.UserName = photo.UserName;
            }
            return Task.FromResult(oldPhoto);
        }
    }
}