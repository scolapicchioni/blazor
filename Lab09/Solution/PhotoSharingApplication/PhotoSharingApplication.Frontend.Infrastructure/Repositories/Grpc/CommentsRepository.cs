using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using PhotoSharingApplication.WebServices.Grpc.Comments;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace PhotoSharingApplication.Frontend.Infrastructure.Repositories.Grpc {
    public class CommentsRepository : ICommentsRepository {
        private readonly CommentsBaseService.CommentsBaseServiceClient serviceClient;

        public CommentsRepository(CommentsBaseService.CommentsBaseServiceClient serviceClient) {
            this.serviceClient = serviceClient;
        }

        public async Task<Comment> CreateAsync(Comment comment) {
            CreateRequest createRequest = new CreateRequest() { PhotoId = comment.PhotoId, Subject = comment.Subject, Body = comment.Body };
            CreateReply c = await serviceClient.CreateAsync(createRequest);
            return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
        }

        public async Task<Comment> FindAsync(int id) {
            FindReply c = await serviceClient.FindAsync(new FindRequest() { Id = id });
            return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
        }

        public async Task<List<Comment>> GetCommentsForPhotoAsync(int photoId) {
            GetCommentsForPhotosReply resp = await serviceClient.GetCommentsForPhotoAsync(new GetCommentsForPhotosRequest() { PhotoId = photoId });
            return resp.Comments.Select(c => new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() }).ToList();
        }

        public async Task<Comment> RemoveAsync(int id) {
            RemoveReply c = await serviceClient.RemoveAsync(new RemoveRequest() { Id = id });
            return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
        }

        public async Task<Comment> UpdateAsync(Comment comment) {
            UpdateReply c = await serviceClient.UpdateAsync(new UpdateRequest { Id = comment.Id, Subject = comment.Subject, Body = comment.Body });
            return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
        }
    }
}
