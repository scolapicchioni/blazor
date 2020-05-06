using PhotoSharingExamples.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoSharingExamples.Backend.Core.Interfaces
{
    public interface IPhotosRepository
    {
        Task<List<Photo>> GetPhotosAsync(int number = 0);
        Task<List<Photo>> GetPhotosByIdsAsync(List<int> ids);
        Task<Photo> FindAsync(int id);
        Task<Photo> FindByTitle(string title);
        Task CreateAsync(Photo photo);
        Task<Photo> UpdateAsync(Photo photo);
        Task<Photo> RemoveAsync(int id);
    }
}
