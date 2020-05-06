using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commentsthingpackage;
using Grpc.Core;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using PhotoSharingExamples.Backend.Core.Interfaces;
using PhotoSharingExamples.Shared.Authorization;
using PhotoSharingExamples.Shared.Entities;

namespace PhotoSharingExamples.GrpcServices.GrpcComments
{
    public class CommentsService : CommentsThing.CommentsThingBase
    {
        private readonly ILogger<CommentsService> _logger;
        private readonly ICommentsService commentsService;
        private readonly IAuthorizationService authorizationService;

        public CommentsService(ILogger<CommentsService> logger, ICommentsService commentsService, IAuthorizationService authorizationService)
        {
            _logger = logger;
            this.commentsService = commentsService;
            this.authorizationService = authorizationService;
        }

        [Authorize]
        public override async Task<CreateReply> Create(CreateRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Create invoked");
            var user = context.GetHttpContext().User;
            //user.Identity.Name works because:
            // 1) on the Identity Server Project Config I added the JwtClaimTypes.Name in the UserClaims of the "photos" ApiResource
            // 2) in this startup I added options.TokenValidationParameters.NameClaimType = JwtClaimTypes.Name; in the AddJwtBearer("Bearer", options =>{ ... })
            Comment co = await commentsService.CreateAsync(request.PhotoId, user.Identity.Name, request.Subject, request.Body);
            return new CreateReply() { Id = co.Id, PhotoId = co.PhotoId, Body = co.Body, Subject = co.Subject, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(co.SubmittedOn.ToUniversalTime()), UserName = co.UserName};
        }

        public override async Task<FindReply> Find(FindRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Find Invoked");
            Comment co = await commentsService.FindAsync(request.Id);
            return new FindReply() { Id = co.Id, PhotoId = co.PhotoId, Subject = co.Subject, UserName = co.UserName, Body = co.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(co.SubmittedOn.ToUniversalTime()) };
        }

        public override async Task<GetCommentsForPhotosReply> GetCommentsForPhoto(GetCommentsForPhotosRequest request, ServerCallContext context)
        {
            List<Comment> comments = await commentsService.GetCommentsForPhotoAsync(request.PhotoId);
            var r = new GetCommentsForPhotosReply();
            r.Comments.AddRange(comments.Select(co => new GetCommentsForPhotosReplyItem() { Id = co.Id, PhotoId = co.PhotoId, Subject = co.Subject, UserName = co.UserName, Body = co.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(co.SubmittedOn.ToUniversalTime()) }));
            return r;
        }

        public override async Task<RemoveReply> Remove(RemoveRequest request, ServerCallContext context)
        {

            Comment co = await commentsService.FindAsync(request.Id);
            var user = context.GetHttpContext().User;
            var authorizationResult = await authorizationService.AuthorizeAsync(user, co, Policies.EditDeleteComment);

            if (authorizationResult.Succeeded)
            {
                co = await commentsService.RemoveAsync(request.Id);
                return  new RemoveReply() { Id = co.Id, PhotoId = co.PhotoId, Subject = co.Subject, UserName = co.UserName, Body = co.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(co.SubmittedOn.ToUniversalTime()) };
            }
            else
            {
                //found on https://docs.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/error-handling
                var metadata = new Metadata {
                    { "User", user.Identity.Name }
                };
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Permission denied"), metadata);
            }

            
        }

        public override async Task<UpdateReply> Update(UpdateRequest request, ServerCallContext context)
        {
            Comment co = await commentsService.UpdateAsync(request.Id, request.Subject, request.Body);
            return new UpdateReply() { Id = co.Id, PhotoId = co.PhotoId, Subject = co.Subject, UserName = co.UserName, Body = co.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(co.SubmittedOn.ToUniversalTime()) };
        }
    }
}
