using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Authorization;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Exceptions;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoSharingApplication.WebServices.Grpc.Comments.Services {
    public class CommentsService : CommentsBaseService.CommentsBaseServiceBase {
        private readonly ICommentsService commentsService;
        
        public CommentsService(ICommentsService commentsService) {
            this.commentsService = commentsService;
        }

        public override async Task<GetCommentsForPhotosReply> GetCommentsForPhoto(GetCommentsForPhotosRequest request, ServerCallContext context) {
            List<Comment> comments = await commentsService.GetCommentsForPhotoAsync(request.PhotoId);
            GetCommentsForPhotosReply r = new GetCommentsForPhotosReply();
            IEnumerable<GetCommentsForPhotosReplyItem> replyItems = comments.Select(c => new GetCommentsForPhotosReplyItem() { Id = c.Id, PhotoId = c.PhotoId, Subject = c.Subject, UserName = c.UserName, Body = c.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()) });
            r.Comments.AddRange(replyItems);
            return r;
        }

        public override async Task<FindReply> Find(FindRequest request, ServerCallContext context) {
            Comment c = await commentsService.FindAsync(request.Id);
            return new FindReply() { Id = c.Id, PhotoId = c.PhotoId, Subject = c.Subject, UserName = c.UserName, Body = c.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()) };
        }

        [Authorize]
        public override async Task<CreateReply> Create(CreateRequest request, ServerCallContext context) {
            var user = context.GetHttpContext().User;
            Comment c = await commentsService.CreateAsync(new Comment { PhotoId = request.PhotoId, Subject = request.Subject, Body = request.Body, UserName = user.Identity.Name });
            return new CreateReply() { Id = c.Id, PhotoId = c.PhotoId, Body = c.Body, Subject = c.Subject, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()), UserName = c.UserName };
        }

        [Authorize]
        public override async Task<UpdateReply> Update(UpdateRequest request, ServerCallContext context) {
            try { 
                Comment c = await commentsService.UpdateAsync(new Comment { Id = request.Id, Subject = request.Subject, Body = request.Body });
                return new UpdateReply() { Id = c.Id, PhotoId = c.PhotoId, Subject = c.Subject, UserName = c.UserName, Body = c.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()) };
            } catch(UnauthorizedEditAttemptException<Comment>) {
                //found on https://docs.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/error-handling
                var user = context.GetHttpContext().User;
                var metadata = new Metadata { { "User", user.Identity.Name } };
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Permission denied"), metadata);
            }
        }

        [Authorize]
        public override async Task<RemoveReply> Remove(RemoveRequest request, ServerCallContext context) {
            try { 
                Comment c = await commentsService.RemoveAsync(request.Id);
                return new RemoveReply() { Id = c.Id, PhotoId = c.PhotoId, Subject = c.Subject, UserName = c.UserName, Body = c.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()) };
            } catch (UnauthorizedDeleteAttemptException<Comment>) { 
                //found on https://docs.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/error-handling
                var user = context.GetHttpContext().User;
                var metadata = new Metadata { { "User", user.Identity.Name } };
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Permission denied"), metadata);
            }
        }
    }
}
