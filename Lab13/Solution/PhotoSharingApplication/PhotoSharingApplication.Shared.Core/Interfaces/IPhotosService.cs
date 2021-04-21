﻿using PhotoSharingApplication.Shared.Core.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Shared.Core.Interfaces {
    public interface IPhotosService {
        Task<Photo> UploadAsync(Photo photo);
        Task<Photo> UpdateAsync(Photo photo);
        Task<Photo> FindAsync(int id);
        Task<Photo> FindWithImageAsync(int id);
        Task<PhotoImage> GetImageAsync(int id);
        Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken);
        Task<int> GetPhotosCountAsync();
        Task<Photo> RemoveAsync(int id);
    }
}
