using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Memory {
    public class CommentsRepository : ICommentsRepository {
        private List<Comment> comments;
        public CommentsRepository() {
            comments = new() {
                new() { Id = 1, Subject = "A Comment", Body = "The Body of the comment", SubmittedOn = DateTime.Now.AddDays(-1), PhotoId = 1 },
                new() { Id = 2, Subject = "Another Comment", Body = "Another Body of the comment", SubmittedOn = DateTime.Now.AddDays(-2), PhotoId = 1 },
                new() { Id = 3, Subject = "Yet another Comment", Body = "Yet Another Body of the comment", SubmittedOn = DateTime.Now, PhotoId = 2 },
                new() { Id = 4, Subject = "More Comment", Body = "More Body of the comment", SubmittedOn = DateTime.Now.AddDays(-3), PhotoId = 2 }
            };
        }
        public Task<Comment?> CreateAsync(Comment comment) {
            comment.Id = comments.Max(p => p.Id) + 1;
            comments.Add(comment);
            return Task.FromResult(comment);
        }

        public Task<Comment?> FindAsync(int id) => Task.FromResult(comments.FirstOrDefault(p => p.Id == id));

        public Task<List<Comment>?> GetCommentsForPhotoAsync(int photoId) => Task.FromResult(comments.Where(c => c.PhotoId == photoId).OrderByDescending(c => c.SubmittedOn).ThenBy(c => c.Subject).ToList());

        public Task<Comment?> RemoveAsync(int id) {
            Comment? comment = comments.FirstOrDefault(c => c.Id == id);
            if (comment is not null) comments.Remove(comment);
            return Task.FromResult(comment);
        }

        public Task<Comment?> UpdateAsync(Comment comment) {
            Comment? oldComment = comments.FirstOrDefault(c => c.Id == comment.Id);
            if (oldComment is not null) {
                oldComment.Subject = comment.Subject;
                oldComment.Body = comment.Body;
            }
            return Task.FromResult(oldComment);
        }
    }
}
