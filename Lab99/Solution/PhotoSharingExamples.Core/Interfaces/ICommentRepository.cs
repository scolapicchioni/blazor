using PhotoSharingExamples.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoSharingExamples.Backend.Core.Interfaces
{
    public interface ICommentsRepository
    {
        Task<List<Comment>> GetCommentsForPhotoAsync(int photoId);
        Task<Comment> FindAsync(int id);
        Task<Comment> CreateAsync(Comment comment);
        Task<Comment> UpdateAsync(Comment comment);
        Task<Comment> RemoveAsync(int id);
    }
}
