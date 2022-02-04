using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Exceptions;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Core.Services {
    public class CommentsService : ICommentsService {
        private readonly ICommentsRepository repository;
        private readonly IAuthorizationService<Comment> commentsAuthorizationService;
        private readonly IUserService userService;

        public CommentsService(ICommentsRepository repository, IAuthorizationService<Comment> commentsAuthorizationService, IUserService userService) =>
            (this.repository, this.commentsAuthorizationService, this.userService) = (repository, commentsAuthorizationService, userService);

        public async Task<Comment> CreateAsync(Comment comment) {
            var user = await userService.GetUserAsync();
            if (await commentsAuthorizationService.ItemMayBeCreatedAsync(user, comment)) {
                comment.SubmittedOn = DateTime.Now;
                comment.UserName = user.Identity.Name;
                return await repository.CreateAsync(comment);
            } else throw new UnauthorizedCreateAttemptException<Comment>($"Unauthorized Create Attempt of Comment {comment.Id}");
        }
        public async Task<Comment> FindAsync(int id) => await repository.FindAsync(id);
        public async Task<List<Comment>> GetCommentsForPhotoAsync(int photoId) => await repository.GetCommentsForPhotoAsync(photoId);

        public async Task<Comment> RemoveAsync(int id) {
            Comment comment = await FindAsync(id);
            var user = await userService.GetUserAsync();
            if (await commentsAuthorizationService.ItemMayBeDeletedAsync(user, comment))
                return await repository.RemoveAsync(id);
            else throw new UnauthorizedDeleteAttemptException<Comment>($"Unauthorized Deletion Attempt of Comment {comment.Id}");
        }

        public async Task<Comment> UpdateAsync(Comment comment) {
            var user = await userService.GetUserAsync();
            Comment oldComment = await repository.FindAsync(comment.Id);
            if (await commentsAuthorizationService.ItemMayBeUpdatedAsync(user, oldComment)) {
                oldComment.Subject = comment.Subject;
                oldComment.Body = comment.Body;
                oldComment.SubmittedOn = DateTime.Now;
                return await repository.UpdateAsync(oldComment);
            } else throw new UnauthorizedEditAttemptException<Comment>($"Unauthorized Edit Attempt of Comment {comment.Id}");
        }
    }
}