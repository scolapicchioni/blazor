using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using PhotoSharingExamples.Backend.Core.Interfaces;
using PhotoSharingExamples.Shared.Entities;

namespace PhotoSharingExamples.Backend.Core.Services
{
    public class PhotosService : IPhotosService
    {
        private readonly IPhotosRepository _photosRepository;
        private readonly IAppLogger<PhotosService> _logger;

        public PhotosService(IPhotosRepository photosRepository, IAppLogger<PhotosService> logger)
        {
            _photosRepository = photosRepository;
            _logger = logger;
        }


        public async Task<List<Photo>> GetPhotosAsync(int number = 0)
        {
            _logger.LogInformation("GetPhotosAsync called", number);
            return await _photosRepository.GetPhotosAsync(number);
        }
        public async Task<List<Photo>> GetPhotosByIdsAsync(List<int> ids)
        {
            _logger.LogInformation("GetPhotosByIdsAsync called", ids);
            return await _photosRepository.GetPhotosByIdsAsync(ids);
        }
        public async Task<Photo> FindAsync(int id)
        {
            _logger.LogInformation("FindAsync called", id);
            return await _photosRepository.FindAsync(id);
        }

        public async Task<Photo> FindByTitle(string title)
        {
            _logger.LogInformation("FindByTitle called", title);
            return await _photosRepository.FindByTitle(title);
        }
        public async Task<Photo> CreateAsync(string title, byte[] photoFile, string imageMimeType, string description, string userName)
        {
            Guard.Against.NullOrEmpty(title, nameof(title));
            DateTime createdDate = DateTime.Now;
            _logger.LogInformation("CreateAsync called", title, imageMimeType, description, createdDate, userName);
            var photo = new Photo(title, photoFile, imageMimeType, description, createdDate, userName, new List<Comment>());
            await _photosRepository.CreateAsync(photo);
            return photo;
        }
        public async Task<Photo> RemoveAsync(int id)
        {
            _logger.LogInformation("RemoveAsync called", id);
            return await _photosRepository.RemoveAsync(id);
        }

        public async Task<Photo> UpdateAsync(int id, string title, byte[] photoFile, string imageMimeType, string description)
        {
            _logger.LogInformation("UpdateAsync called", id);
            Photo oldPhoto = await _photosRepository.FindAsync(id);
            oldPhoto.Title = title;
            oldPhoto.PhotoFile = photoFile;
            oldPhoto.ImageMimeType = imageMimeType;
            oldPhoto.Description = description;
            oldPhoto.CreatedDate = DateTime.Now;
            return await _photosRepository.UpdateAsync(oldPhoto);
        }
    }
}
