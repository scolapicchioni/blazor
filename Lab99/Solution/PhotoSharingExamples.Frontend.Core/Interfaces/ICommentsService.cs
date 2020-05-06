using PhotoSharingExamples.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSharingExamples.Frontend.Core.Interfaces
{
    public interface ICommentsService
    {
        Task<List<Comment>> GetCommentsForPhotoAsync(int photoId);
        Task<Comment> FindAsync(int id);
        Task<Comment> CreateAsync(int photoId, string subject, string body, string tokenValue);
        Task<Comment> UpdateAsync(int id, string subject, string body, string tokenValue);
        Task<Comment> RemoveAsync(int id, string tokenValue);
    }
}
