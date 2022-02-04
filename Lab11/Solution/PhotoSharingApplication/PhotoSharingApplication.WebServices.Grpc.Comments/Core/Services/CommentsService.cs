using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Exceptions;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.WebServices.Grpc.Comments.Core.Services;

public class CommentsService : ICommentsService {
    private readonly ICommentsRepository repository;
    private readonly IAuthorizationService<Comment> commentsAuthorizationService;
    private readonly IUserService userService;

    public CommentsService(ICommentsRepository repository, IAuthorizationService<Comment> commentsAuthorizationService, IUserService userService) =>
        (this.repository, this.commentsAuthorizationService, this.userService) = (repository, commentsAuthorizationService, userService);
    public async Task<Comment?> CreateAsync(Comment comment) {
        var user = await userService.GetUserAsync();
        if (await commentsAuthorizationService.ItemMayBeCreatedAsync(user, comment)) {
            comment.SubmittedOn = DateTime.Now;
            comment.UserName = user.Identity?.Name ?? "";
            return await repository.CreateAsync(comment);
        } else throw new CreateUnauthorizedException<Comment>($"Unauthorized Create Attempt of Comment {comment.Id}");
    }

    public async Task<Comment?> FindAsync(int id) => await repository.FindAsync(id);

    public async Task<List<Comment>?> GetCommentsForPhotoAsync(int photoId) => await repository.GetCommentsForPhotoAsync(photoId);

    public async Task<Comment?> RemoveAsync(int id) {
        Comment? comment = await FindAsync(id);
        if (comment is not null) {
            var user = await userService.GetUserAsync();
            if (!await commentsAuthorizationService.ItemMayBeDeletedAsync(user, comment)) {
                throw new DeleteUnauthorizedException<Comment>($"Unauthorized Deletion Attempt of Comment {comment.Id}");
            }
            comment = await repository.RemoveAsync(id);
        }
        return comment;
    }

    public async Task<Comment?> UpdateAsync(Comment comment) {
        var user = await userService.GetUserAsync();
        Comment? oldComment = await repository.FindAsync(comment.Id);
        if (oldComment is not null) {
            if (!await commentsAuthorizationService.ItemMayBeUpdatedAsync(user, oldComment))
                throw new EditUnauthorizedException<Comment>($"Unauthorized Edit Attempt of Comment {comment.Id}");
            oldComment.Subject = comment.Subject;
            oldComment.Body = comment.Body;
            oldComment.SubmittedOn = DateTime.Now;
            oldComment = await repository.UpdateAsync(oldComment);
        }
        return oldComment;
    }
}
