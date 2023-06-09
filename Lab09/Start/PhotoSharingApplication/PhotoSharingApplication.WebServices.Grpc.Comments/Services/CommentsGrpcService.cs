using Grpc.Core;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.WebServices.Grpc.Comments.Services; 
public class CommentsGrpcService : Commenter.CommenterBase {
    private readonly ICommentsService commentsService;

    public CommentsGrpcService(ICommentsService commentsService)
    {
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
        if (c is null) {
            throw new RpcException(new Status(StatusCode.NotFound, "Comment not found"));
        }
        return new FindReply() { Id = c.Id, PhotoId = c.PhotoId, Subject = c.Subject, UserName = c.UserName, Body = c.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()) };
    }
    public override async Task<CreateReply> Create(CreateRequest request, ServerCallContext context) {
        try {
            Comment c = await commentsService.CreateAsync(new Comment { PhotoId = request.PhotoId, Subject = request.Subject, Body = request.Body });
            return new CreateReply() { Id = c.Id, PhotoId = c.PhotoId, Body = c.Body, Subject = c.Subject, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()), UserName = c.UserName };
        } catch (Exception ex) {
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
    public override async Task<UpdateReply> Update(UpdateRequest request, ServerCallContext context) {
        try {
            Comment c = await commentsService.UpdateAsync(new Comment { Id = request.Id, Subject = request.Subject, Body = request.Body });
            return new UpdateReply() { Id = c.Id, PhotoId = c.PhotoId, Subject = c.Subject, UserName = c.UserName, Body = c.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()) };
        } catch (Exception ex) {
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
    public override async Task<RemoveReply> Remove(RemoveRequest request, ServerCallContext context) {
        try {
            Comment c = await commentsService.RemoveAsync(request.Id);
            return new RemoveReply() { Id = c.Id, PhotoId = c.PhotoId, Subject = c.Subject, UserName = c.UserName, Body = c.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()) };
        } catch (Exception ex) {
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}
