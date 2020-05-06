using Ardalis.GuardClauses;
using PhotoSharingExamples.Frontend.Core.Interfaces;
using PhotoSharingExamples.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSharingExamples.Frontend.Core.Services
{
    public class PhotosService : IPhotosService
    {
        private readonly IPhotosRepository _photosRepository;
        

        public PhotosService(IPhotosRepository photosRepository)
        {
            _photosRepository = photosRepository;
        }

        public async Task<List<Photo>> GetPhotosAsync(int number = 10) => await _photosRepository.GetPhotosAsync(number);
        public async Task<List<Photo>> GetPhotosByIdsAsync(List<int> ids) => await _photosRepository.GetPhotosByIdsAsync(ids);
        public async Task<Photo> FindAsync(int id) => await _photosRepository.FindAsync(id);

        public async Task<Photo> FindByTitle(string title) => await _photosRepository.FindByTitle(title);
        public async Task<Photo> CreateAsync(string title, byte[] photoFile, string imageMimeType, string description, string userName, string tokenValue)
        {
            Guard.Against.NullOrEmpty(title, nameof(title));
            DateTime createdDate = DateTime.Now;
            
            var photo = new Photo(title, photoFile, imageMimeType, description, createdDate, userName, new List<Comment>());
            await _photosRepository.CreateAsync(photo, tokenValue);
            return photo;
        }
        public async Task<Photo> RemoveAsync(int id, string tokenValue) => await _photosRepository.RemoveAsync(id, tokenValue);

        public async Task<Photo> UpdateAsync(int id, string title, byte[] photoFile, string imageMimeType, string description, string tokenValue)
        {
            Photo oldPhoto = await _photosRepository.FindAsync(id);
            oldPhoto.Title = title;
            oldPhoto.PhotoFile = photoFile;
            oldPhoto.ImageMimeType = imageMimeType;
            oldPhoto.Description = description;
            oldPhoto.CreatedDate = DateTime.Now;
            return await _photosRepository.UpdateAsync(oldPhoto, tokenValue);
        }
    }
}
