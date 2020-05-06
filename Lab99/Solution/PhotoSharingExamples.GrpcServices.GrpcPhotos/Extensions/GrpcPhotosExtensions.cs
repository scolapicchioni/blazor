using Google.Protobuf;
using PhotoSharingExamples.Shared.Entities;
using Photosthingpackage;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PhotoSharingExamples.GrpcServices.GrpcPhotos.Extensions
{
    public static class GrpcPhotosExtensions
    {
        public static RemoveReply ToRemoveReply(this Photo ph) => new RemoveReply() { Id = ph.Id, Title = ph.Title, PhotoFile = ByteString.CopyFrom(ph.PhotoFile), ImageMimeType = ph.ImageMimeType, Description = ph.Description, CreatedDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(ph.CreatedDate.ToUniversalTime()), UserName = ph.UserName };

        public static CreateReply ToCreateReply(this Photo ph) => new CreateReply() { Id = ph.Id, Title = ph.Title, PhotoFile = ByteString.CopyFrom(ph.PhotoFile), ImageMimeType = ph.ImageMimeType, Description = ph.Description, CreatedDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(ph.CreatedDate.ToUniversalTime()), UserName = ph.UserName };
        public static FindReply ToFindReply(this Photo ph) => new FindReply() { Id = ph.Id, Title = ph.Title, PhotoFile = ByteString.CopyFrom(ph.PhotoFile), ImageMimeType = ph.ImageMimeType, Description = ph.Description, CreatedDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(ph.CreatedDate.ToUniversalTime()), UserName = ph.UserName };
    
        public static FindByTitleReply ToFindByTitleReply(this Photo ph) => new FindByTitleReply() { Id = ph.Id, Title = ph.Title, PhotoFile = ByteString.CopyFrom(ph.PhotoFile), ImageMimeType = ph.ImageMimeType, Description = ph.Description, CreatedDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(ph.CreatedDate.ToUniversalTime()), UserName = ph.UserName };

        public static GetPhotosReplyItem ToGetPhotoReplyItem(this Photo ph) => new GetPhotosReplyItem() { Id = ph.Id, Title = ph.Title, PhotoFile = ByteString.CopyFrom(ph.PhotoFile), ImageMimeType = ph.ImageMimeType, Description = ph.Description, CreatedDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(ph.CreatedDate.ToUniversalTime()), UserName = ph.UserName };

        public static GetPhotosByIdsReplyItem ToGetPhotosByIdReplyItem(this Photo ph) => new GetPhotosByIdsReplyItem() { Id = ph.Id, Title = ph.Title, PhotoFile = ByteString.CopyFrom(ph.PhotoFile), ImageMimeType = ph.ImageMimeType, Description = ph.Description, CreatedDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(ph.CreatedDate.ToUniversalTime()), UserName = ph.UserName };

        public static UpdateReply ToUpdateReply(this Photo ph) => new UpdateReply() { Id = ph.Id, Title = ph.Title, PhotoFile = ByteString.CopyFrom(ph.PhotoFile), ImageMimeType = ph.ImageMimeType, Description = ph.Description, CreatedDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(ph.CreatedDate.ToUniversalTime()), UserName = ph.UserName };
    }
}
