using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using PhotoSharingApplication.WebServices.Grpc.Comments;

namespace PhotoSharingApplication.Frontend.Infrastructure.Repositories.Grpc;

public class CommentsRepository : ICommentsRepository {
    private readonly Commenter.CommenterClient gRpcClient;

    public CommentsRepository(Commenter.CommenterClient gRpcClient) {
        this.gRpcClient = gRpcClient;
    }
    public async Task<Comment?> CreateAsync(Comment comment) {
        CreateRequest createRequest = new CreateRequest() { PhotoId = comment.PhotoId, Subject = comment.Subject, Body = comment.Body };
        CreateReply c = await gRpcClient.CreateAsync(createRequest);
        return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
    }

    public async Task<Comment?> FindAsync(int id) {
        FindReply c = await gRpcClient.FindAsync(new FindRequest() { Id = id });
        return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
    }

    public async Task<List<Comment>?> GetCommentsForPhotoAsync(int photoId) {
        GetCommentsForPhotosReply resp = await gRpcClient.GetCommentsForPhotoAsync(new GetCommentsForPhotosRequest() { PhotoId = photoId });
        return resp.Comments.Select(c => new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() }).ToList();
    }

    public async Task<Comment?> RemoveAsync(int id) {
        RemoveReply c = await gRpcClient.RemoveAsync(new RemoveRequest() { Id = id });
        return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
    }

    public async Task<Comment?> UpdateAsync(Comment comment) {
        UpdateReply c = await gRpcClient.UpdateAsync(new UpdateRequest { Id = comment.Id, Subject = comment.Subject, Body = comment.Body });
        return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
    }
}
