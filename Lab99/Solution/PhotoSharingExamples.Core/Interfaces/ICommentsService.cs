using PhotoSharingExamples.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSharingExamples.Backend.Core.Interfaces
{
    public interface ICommentsService
    {
        Task<List<Comment>> GetCommentsForPhotoAsync(int photoId);
        Task<Comment> FindAsync(int id);
        Task<Comment> CreateAsync(int photoId, string userName, string subject, string body);
        Task<Comment> UpdateAsync(int id, string subject, string body);
        Task<Comment> RemoveAsync(int id);
    }
}
