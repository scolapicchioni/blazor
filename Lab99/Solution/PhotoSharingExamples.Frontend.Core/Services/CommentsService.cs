using Ardalis.GuardClauses;
using PhotoSharingExamples.Frontend.Core.Interfaces;
using PhotoSharingExamples.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSharingExamples.Frontend.Core.Services
{
    public class CommentsService : ICommentsService
    {
        private readonly ICommentsRepository repository;

        public CommentsService(ICommentsRepository repository)
        {
            this.repository = repository;
        }

        public async Task<Comment> CreateAsync(int photoId, string subject, string body, string tokenValue)
        {
            Guard.Against.NullOrEmpty(subject, nameof(subject));
            Guard.Against.NullOrEmpty(body, nameof(body));
            
            var comment = new Comment() { PhotoId = photoId, Subject = subject, Body = body };
            return await repository.CreateAsync(comment, tokenValue);
        }

        public async Task<Comment> FindAsync(int id) => await repository.FindAsync(id);

        public async Task<List<Comment>> GetCommentsForPhotoAsync(int photoId) => await repository.GetCommentsForPhotoAsync(photoId);

        public async Task<Comment> RemoveAsync(int id, string tokenValue) => await repository.RemoveAsync(id, tokenValue);

        public async Task<Comment> UpdateAsync(int id, string subject, string body, string tokenValue)
        {
            Comment comment = await repository.FindAsync(id);
            comment.Subject = subject;
            comment.Body = body;
            comment.SubmittedOn = DateTime.Now;

            return await repository.UpdateAsync(comment, tokenValue);
        }
    }
}
