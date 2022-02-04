using Grpc.Core;
using Microsoft.AspNetCore.Authentication;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;
using PhotoSharingApplication.WebServices.Grpc.Comments;

namespace PhotoSharingApplication.Frontend.Server.Infrastructure.Repositories.Grpc;

public class CommentsRepository : ICommentsRepository {
    private readonly Commenter.CommenterClient gRpcClient;
    private readonly IHttpContextAccessor httpContextAccessor;

    public CommentsRepository(Commenter.CommenterClient gRpcClient, IHttpContextAccessor httpContextAccessor) => (this.gRpcClient, this.httpContextAccessor) = (gRpcClient, httpContextAccessor);
    public async Task<Comment?> CreateAsync(Comment comment) {
        var headers = new Metadata();
        var token = await httpContextAccessor.HttpContext.GetUserAccessTokenAsync();
        headers.Add("Authorization", $"Bearer {token}");

        CreateRequest createRequest = new CreateRequest() { PhotoId = comment.PhotoId, Subject = comment.Subject, Body = comment.Body };
        CreateReply c = await gRpcClient.CreateAsync(createRequest, headers);
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
