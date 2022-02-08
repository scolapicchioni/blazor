using FluentValidation;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Exceptions;
using PhotoSharingApplication.Shared.Interfaces;
using PhotoSharingApplication.Shared.Validators;
using PhotoSharingApplication.WebServices.Grpc.Comments.Validation;
using System.Text;
using System.Text.Json;

namespace PhotoSharingApplication.WebServices.Grpc.Comments.Services;

public class CommentsGrpcService : Commenter.CommenterBase {
    private readonly ICommentsService commentsService;

    public CommentsGrpcService(ICommentsService commentsService) => this.commentsService = commentsService;
    public override async Task<GetCommentsForPhotosReply> GetCommentsForPhoto(GetCommentsForPhotosRequest request, ServerCallContext context) {
        List<Comment>? comments = await commentsService.GetCommentsForPhotoAsync(request.PhotoId);
        GetCommentsForPhotosReply r = new GetCommentsForPhotosReply();
        IEnumerable<GetCommentsForPhotosReplyItem>? replyItems = comments?.Select(c => new GetCommentsForPhotosReplyItem() { Id = c.Id, PhotoId = c.PhotoId, Subject = c.Subject, UserName = c.UserName, Body = c.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()) });
        r.Comments.AddRange(replyItems);
        return r;
    }

    public override async Task<FindReply> Find(FindRequest request, ServerCallContext context) {
        Comment? c = await commentsService.FindAsync(request.Id);
        if (c is null) {
            throw new RpcException(new Status(StatusCode.NotFound, "Comment not found"));
        }
        return new FindReply() { Id = c.Id, PhotoId = c.PhotoId, Subject = c.Subject, UserName = c.UserName, Body = c.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()) };
    }

    [Authorize]
    public override async Task<CreateReply> Create(CreateRequest request, ServerCallContext context) {
        try {
            var user = context.GetHttpContext().User;
            Comment? c = await commentsService.CreateAsync(new Comment { PhotoId = request.PhotoId, Subject = request.Subject, Body = request.Body, UserName = user?.Identity?.Name ?? String.Empty });
            if (c is null) {
                throw new RpcException(new Status(StatusCode.Internal, "Something went wrong while creating the comment"));
            }
            return new CreateReply() { Id = c.Id, PhotoId = c.PhotoId, Body = c.Body, Subject = c.Subject, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()), UserName = c.UserName };
        } catch (CreateUnauthorizedException<Comment>) {
            var user = context.GetHttpContext().User;
            var metadata = new Metadata { { "User", user?.Identity?.Name ?? String.Empty } };
            throw new RpcException(new Status(StatusCode.PermissionDenied, "Create Permission Denied"), metadata);
        } catch (ValidationException ex) {
            throw ex.ToRpcException();
        }
    }

    public override async Task<UpdateReply> Update(UpdateRequest request, ServerCallContext context) {
        try {
            Comment? c = await commentsService.UpdateAsync(new Comment { Id = request.Id, Subject = request.Subject, Body = request.Body });
            if (c is null) {
                throw new RpcException(new Status(StatusCode.NotFound, "Comment not found"));
            }
            return new UpdateReply() { Id = c.Id, PhotoId = c.PhotoId, Subject = c.Subject, UserName = c.UserName, Body = c.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()) };
        } catch (EditUnauthorizedException<Comment>) {
            var user = context.GetHttpContext().User;
            var metadata = new Metadata { { "User", user?.Identity?.Name ?? String.Empty } };
            throw new RpcException(new Status(StatusCode.PermissionDenied, "Edit Permission Denied"), metadata);
        } catch (ValidationException ex) {
            throw ex.ToRpcException();
        }
    }

    public override async Task<RemoveReply> Remove(RemoveRequest request, ServerCallContext context) {
        try {
            Comment? c = await commentsService.RemoveAsync(request.Id);
            if (c is null) {
                throw new RpcException(new Status(StatusCode.NotFound, "Comment not found"));
            }
            return new RemoveReply() { Id = c.Id, PhotoId = c.PhotoId, Subject = c.Subject, UserName = c.UserName, Body = c.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()) };
        } catch (DeleteUnauthorizedException<Comment>) {
            var user = context.GetHttpContext().User;
            var metadata = new Metadata { { "User", user?.Identity?.Name ?? String.Empty } };
            throw new RpcException(new Status(StatusCode.PermissionDenied, "Delete Permission Denied"), metadata);
        } catch (Exception ex) {
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}


