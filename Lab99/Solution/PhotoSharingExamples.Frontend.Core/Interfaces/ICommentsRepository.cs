using PhotoSharingExamples.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSharingExamples.Frontend.Core.Interfaces
{
    public interface ICommentsRepository
    {
        Task<List<Comment>> GetCommentsForPhotoAsync(int photoId);
        Task<Comment> FindAsync(int id);
        Task<Comment> CreateAsync(Comment comment, string tokenValue);
        Task<Comment> UpdateAsync(Comment comment, string tokenValue);
        Task<Comment> RemoveAsync(int id, string tokenValue);
    }
}
