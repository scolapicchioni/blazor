using Ardalis.GuardClauses;
using PhotoSharingExamples.Backend.Core.Interfaces;
using PhotoSharingExamples.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSharingExamples.Backend.Core.Services
{
    public class CommentsService : ICommentsService
    {
        private readonly ICommentsRepository _commentsRepository;
        private readonly IAppLogger<CommentsService> _logger;

        public CommentsService(ICommentsRepository commentsRepository, IAppLogger<CommentsService> logger)
        {
            _commentsRepository = commentsRepository;
            _logger = logger;
        }

        public async Task<Comment> CreateAsync(int photoId, string userName, string subject, string body)
        {
            Guard.Against.NullOrEmpty(subject, nameof(subject));
            Guard.Against.Null(photoId, nameof(photoId));
            Comment comment = new Comment(photoId, userName, subject, body, DateTime.Now);
            _logger.LogInformation("CreateAsync called", comment);
            return await _commentsRepository.CreateAsync(comment);
        }

        public async Task<Comment> FindAsync(int id)
        {
            _logger.LogInformation("FindAsync called", id);
            return await _commentsRepository.FindAsync(id);
        }

        public async Task<List<Comment>> GetCommentsForPhotoAsync(int photoId)
        {
            _logger.LogInformation("GetCommentsForPhotoAsync called", photoId);
            return await _commentsRepository.GetCommentsForPhotoAsync(photoId);
        }

        public async Task<Comment> RemoveAsync(int id)
        {
            _logger.LogInformation("RemoveAsync called", id);
            return await _commentsRepository.RemoveAsync(id);
        }

        public async Task<Comment> UpdateAsync(int id, string subject, string body)
        {
            _logger.LogInformation("UpdateAsync called", id);
            Guard.Against.NullOrEmpty(subject, nameof(subject));
            Comment oldComment = await _commentsRepository.FindAsync(id);
            oldComment.Subject = subject;
            oldComment.Body = body;
            oldComment.SubmittedOn = DateTime.Now;
            return await _commentsRepository.UpdateAsync(oldComment);
        }
    }
}
