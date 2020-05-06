using PhotoSharingExamples.Shared.Entities;
using Photosthingpackage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhotoSharingExamples.Frontend.Infrastructure.GrpcClients
{
    public static class GrpcPhotosExtensions
    {
        public static Photo ToPhoto(this CreateReply p) => new Photo(p.Id, p.Title, p.PhotoFile.ToArray(), p.ImageMimeType, p.Description, p.CreatedDate.ToDateTime(), p.UserName, null);

        public static Photo ToPhoto(this FindReply p) => new Photo(p.Id, p.Title, p.PhotoFile.ToArray(), p.ImageMimeType, p.Description, p.CreatedDate.ToDateTime(), p.UserName, null);

        public static Photo ToPhoto(this GetPhotosReplyItem p) => new Photo(p.Id, p.Title, p.PhotoFile.ToArray(), p.ImageMimeType, p.Description, p.CreatedDate.ToDateTime(), p.UserName, null);

        public static Photo ToPhoto(this RemoveReply p) => new Photo(p.Id, p.Title, p.PhotoFile.ToArray(), p.ImageMimeType, p.Description, p.CreatedDate.ToDateTime(), p.UserName, null);

        public static Photo ToPhoto(this UpdateReply p) => new Photo(p.Id, p.Title, p.PhotoFile.ToArray(), p.ImageMimeType, p.Description, p.CreatedDate.ToDateTime(), p.UserName, null);

        public static CreateRequest ToCreateRequest(this Photo photo) => new CreateRequest()
        {
            Id = 0,
            Title = photo.Title,
            Description = photo.Description,
            PhotoFile = Google.Protobuf.ByteString.CopyFrom(photo.PhotoFile),
            ImageMimeType = photo.ImageMimeType,
            UserName = ""
        };

        public static UpdateRequest ToUpdateRequest(this Photo photo) => new UpdateRequest()
        {
            Id = photo.Id,
            Title = photo.Title,
            Description = photo.Description,
            PhotoFile = Google.Protobuf.ByteString.CopyFrom(photo.PhotoFile),
            ImageMimeType = photo.ImageMimeType,
            UserName = photo.UserName
        };
    }
}
