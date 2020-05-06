using PhotoSharingExamples.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoSharingExamples.Backend.Core.Interfaces
{
    public interface IPhotosService
    {
        Task<Photo> CreateAsync(string title, byte[] photoFile, string imageMimeType, string description, string userName);
        Task<Photo> UpdateAsync(int id, string title, byte[] photoFile, string imageMimeType, string description);
        Task<Photo> FindAsync(int id);
        Task<Photo> FindByTitle(string title);
        Task<List<Photo>> GetPhotosAsync(int number = 0);
        Task<List<Photo>> GetPhotosByIdsAsync(List<int> ids);
        Task<Photo> RemoveAsync(int id);
    }
}