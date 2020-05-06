using PhotoSharingExamples.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSharingExamples.Frontend.Core.Interfaces
{
    public interface IPhotosRepository
    {
        Task<List<Photo>> GetPhotosAsync(int number = 10);
        Task<List<Photo>> GetPhotosByIdsAsync(List<int> ids);
        Task<Photo> FindAsync(int id);
        Task<Photo> FindByTitle(string title);
        Task<Photo> CreateAsync(Photo photo, string tokenValue);
        Task<Photo> UpdateAsync(Photo photo, string tokenValue);
        Task<Photo> RemoveAsync(int id, string tokenValue);
    }
}
