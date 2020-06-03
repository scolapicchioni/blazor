using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Exceptions;
using PhotoSharingApplication.Shared.Core.Interfaces;
using PhotoSharingApplication.WebServices.Grpc.Comments;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrpCore = Grpc.Core;

namespace PhotoSharingApplication.Frontend.Infrastructure.Repositories.Grpc {
    public class CommentsRepository : ICommentsRepository {
        private readonly CommentsBaseService.CommentsBaseServiceClient serviceClient;
        private readonly IAccessTokenProvider tokenProvider;

        public CommentsRepository(CommentsBaseService.CommentsBaseServiceClient serviceClient, IAccessTokenProvider tokenProvider) {
            this.serviceClient = serviceClient;
            this.tokenProvider = tokenProvider;
        }

        public async Task<Comment> CreateAsync(Comment comment) {
            var tokenResult = await tokenProvider.RequestAccessToken(new AccessTokenRequestOptions() { Scopes = new string[] { "commentsgrpc" } });
            if (tokenResult.TryGetToken(out var token)) {
                GrpCore.Metadata headers = new GrpCore.Metadata();
                headers.Add("Authorization", $"Bearer {token.Value}");

                CreateRequest createRequest = new CreateRequest() { PhotoId = comment.PhotoId, Subject = comment.Subject, Body = comment.Body };
                CreateReply c = await serviceClient.CreateAsync(createRequest, headers);
                return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
            } else {
                throw new UnauthorizedCreateAttemptException<Comment>();
            }
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
            var tokenResult = await tokenProvider.RequestAccessToken(new AccessTokenRequestOptions() { Scopes = new string[] { "commentsgrpc" } });
            if (tokenResult.TryGetToken(out var token)) {
                GrpCore.Metadata headers = new GrpCore.Metadata();
                headers.Add("Authorization", $"Bearer {token.Value}");
                RemoveReply c = await serviceClient.RemoveAsync(new RemoveRequest() { Id = id }, headers);
                return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
            } else {
                throw new UnauthorizedDeleteAttemptException<Comment>();
            }
        }

        public async Task<Comment> UpdateAsync(Comment comment) {
            var tokenResult = await tokenProvider.RequestAccessToken(new AccessTokenRequestOptions() { Scopes = new string[] { "commentsgrpc" } });
            if (tokenResult.TryGetToken(out var token)) {
                GrpCore.Metadata headers = new GrpCore.Metadata();
                headers.Add("Authorization", $"Bearer {token.Value}");

                UpdateReply c = await serviceClient.UpdateAsync(new UpdateRequest { Id = comment.Id, Subject = comment.Subject, Body = comment.Body }, headers);
                return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
            } else {
                throw new UnauthorizedEditAttemptException<Comment>();
            }
        }
    }
}
