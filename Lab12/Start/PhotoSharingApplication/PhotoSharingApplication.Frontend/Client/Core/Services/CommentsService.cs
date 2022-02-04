﻿using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.Frontend.Client.Core.Services {
    public class CommentsService : ICommentsService {
        private readonly ICommentsRepository repository;
        public CommentsService(ICommentsRepository repository) => this.repository = repository;

        public async Task<Comment?> CreateAsync(Comment comment) => await repository.CreateAsync(comment);

        public async Task<Comment?> FindAsync(int id) => await repository.FindAsync(id);

        public async Task<List<Comment>?> GetCommentsForPhotoAsync(int photoId) => await repository.GetCommentsForPhotoAsync(photoId);

        public async Task<Comment?> RemoveAsync(int id) => await repository.RemoveAsync(id);

        public async Task<Comment?> UpdateAsync(Comment comment) {
            comment.SubmittedOn = DateTime.Now;
            return await repository.UpdateAsync(comment);
        }
    }
}
