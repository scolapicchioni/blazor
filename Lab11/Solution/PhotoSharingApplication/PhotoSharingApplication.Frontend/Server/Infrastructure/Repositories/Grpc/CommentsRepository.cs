using Grpc.Core;
using Microsoft.AspNetCore.Authentication;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Exceptions;
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
        try {
            CreateReply c = await gRpcClient.CreateAsync(createRequest, headers);
            return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
        } catch (RpcException ex) when (ex.StatusCode == StatusCode.PermissionDenied) {
            throw new CreateUnauthorizedException<Comment>(ex.Message);
        } catch (RpcException ex) {
            throw new Exception(ex.Message);
        }
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
        try {
            var headers = new Metadata();
            var token = await httpContextAccessor.HttpContext.GetUserAccessTokenAsync();
            headers.Add("Authorization", $"Bearer {token}");
            RemoveReply c = await gRpcClient.RemoveAsync(new RemoveRequest() { Id = id }, headers);
            return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
        } catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound) { 
            return null;
        } catch (RpcException ex) when (ex.StatusCode == StatusCode.PermissionDenied) {
            throw new DeleteUnauthorizedException<Comment>(ex.Message);
        } catch (RpcException ex){
            throw new Exception(ex.Message);
        }
    }

    public async Task<Comment?> UpdateAsync(Comment comment) {
        try {
            var headers = new Metadata();
            var token = await httpContextAccessor.HttpContext.GetUserAccessTokenAsync();
            headers.Add("Authorization", $"Bearer {token}");
            UpdateReply c = await gRpcClient.UpdateAsync(new UpdateRequest { Id = comment.Id, Subject = comment.Subject, Body = comment.Body }, headers);
            return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
        } catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound) {
            return null;
        } catch (RpcException ex) when (ex.StatusCode == StatusCode.PermissionDenied) {
            throw new EditUnauthorizedException<Comment>(ex.Message);
        } catch (RpcException ex) {
            throw new Exception(ex.Message);
        }
    }
}
