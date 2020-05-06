using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Unicode;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using PhotoSharingExamples.Backend.Core.Interfaces;
using PhotoSharingExamples.GrpcServices.GrpcPhotos.Extensions;
using PhotoSharingExamples.Shared.Authorization;
using PhotoSharingExamples.Shared.Entities;
using Photosthingpackage;

namespace PhotoSharingExamples.GrpcServices.GrpcPhotos
{
    public class PhotosService : PhotosThing.PhotosThingBase {
        private readonly ILogger<PhotosService> _logger;
        private readonly IPhotosService photosService;
        private readonly IAuthorizationService authorizationService;

        public PhotosService(ILogger<PhotosService> logger, IPhotosService photosService, IAuthorizationService authorizationService)
        {
            _logger = logger;
            this.photosService = photosService;
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
            Photo ph = await photosService.CreateAsync(request.Title, request.PhotoFile.ToByteArray(), request.ImageMimeType, request.Description, user.Identity.Name);
            return ph.ToCreateReply();
        }

        public override async Task<FindReply> Find(FindRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"Find invoked with Id {request.Id}");
            Photo ph = await photosService.FindAsync(request.Id);
            return ph.ToFindReply();
        }

        public override async Task<FindByTitleReply> FindByTitle(FindByTitleRequest request, ServerCallContext context)
        {
            Photo ph = await photosService.FindByTitle(request.Title);
            return ph.ToFindByTitleReply();
        }

        //[Authorize]
        public override async Task<GetPhotosReply> GetPhotos(GetPhotosRequest request, ServerCallContext context)
        {
            List<Photo> photos = await photosService.GetPhotosAsync(request.Number);
            var r = new GetPhotosReply();
            r.Photos.AddRange(photos.Select(ph => ph.ToGetPhotoReplyItem()));
            return r;
        }

        public override async Task<GetPhotosByIdsReply> GetPhotosByIds(GetPhotosByIdsRequest request, ServerCallContext context)
        {
            List<Photo> photos = await photosService.GetPhotosByIdsAsync(request.Ids.Select(item=>item.Id).ToList());
            var r = new  GetPhotosByIdsReply();
            r.Photos.AddRange(photos.Select(ph => ph.ToGetPhotosByIdReplyItem()));
            return r;
        }

        [Authorize]
        public override async Task<RemoveReply> Remove(RemoveRequest request, ServerCallContext context)
        {
            Photo ph = await photosService.FindAsync(request.Id);
            var user = context.GetHttpContext().User;
            var authorizationResult = await authorizationService.AuthorizeAsync(user, ph, Policies.EditDeletePhoto);

            if (authorizationResult.Succeeded)
            {
                ph = await photosService.RemoveAsync(request.Id);
                return ph.ToRemoveReply();
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

        [Authorize]
        public override async Task<UpdateReply> Update(UpdateRequest request, ServerCallContext context)
        {
            Photo ph = await photosService.FindAsync(request.Id);
            var user = context.GetHttpContext().User;
            var authorizationResult = await authorizationService.AuthorizeAsync(user, ph, Policies.EditDeletePhoto);

            if (authorizationResult.Succeeded)
            {
                ph = await photosService.UpdateAsync(request.Id, request.Title, request.PhotoFile.ToArray(), request.ImageMimeType, request.Description);
                return ph.ToUpdateReply();
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
    }
}
