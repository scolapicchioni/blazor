using PhotoSharingApplication.Frontend.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Core.Interfaces {
    public interface IPhotosRepository {
        Task<List<Photo>> GetPhotosAsync(int amount = 10);
        Task<Photo> FindAsync(int id);
        Task<Photo> CreateAsync(Photo photo);
        Task<Photo> UpdateAsync(Photo photo);
        Task<Photo> RemoveAsync(int id);
    }
}
