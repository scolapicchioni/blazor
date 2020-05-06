using Microsoft.EntityFrameworkCore;
using PhotoSharingExamples.Backend.Core.Interfaces;
using PhotoSharingExamples.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSharingExamples.Backend.Infrastructure.Data
{
    public class CommentsRepository : ICommentsRepository
    {
        private readonly PhotoSharingApplicationContext _context;

        public CommentsRepository(PhotoSharingApplicationContext context)
        {
            _context = context;
        }

        public async Task<Comment> CreateAsync(Comment comment)
        {
            _context.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment> FindAsync(int id) => await _context.Comments.SingleOrDefaultAsync(m => m.Id == id);

        public async Task<List<Comment>> GetCommentsForPhotoAsync(int photoId) => await _context.Comments.Where(c => c.PhotoId == photoId).ToListAsync();

        public async Task<Comment> RemoveAsync(int id)
        {
            Comment comment = await _context.Comments.SingleOrDefaultAsync(m => m.Id == id);
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment> UpdateAsync(Comment comment)
        {
            _context.Entry(comment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return comment;
        }
    }
}
