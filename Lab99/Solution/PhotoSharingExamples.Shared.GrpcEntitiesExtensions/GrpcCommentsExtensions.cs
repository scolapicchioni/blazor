using Commentsthingpackage;
using PhotoSharingExamples.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoSharingExamples.Frontend.Infrastructure.GrpcClients
{
    public static class GrpcCommentsExtensions
    {
        public static Comment ToComment(this CreateReply c) => new Comment(c.Id, c.PhotoId, c.UserName, c.Subject, c.Body, c.SubmittedOn.ToDateTime());

        public static Comment ToComment(this FindReply c) => new Comment(c.Id,c.PhotoId, c.UserName, c.Subject, c.Body, c.SubmittedOn.ToDateTime());

        public static Comment ToComment(this GetCommentsForPhotosReplyItem c) => new Comment(c.Id,c.PhotoId,c.UserName,c.Subject,c.Body,c.SubmittedOn.ToDateTime());

        public static Comment ToComment(this RemoveReply c) => new Comment(c.Id,c.PhotoId,c.UserName,c.Subject,c.Body, c.SubmittedOn.ToDateTime());

        public static Comment ToComment(this UpdateReply c) => new Comment(c.Id, c.PhotoId, c.UserName, c.Subject, c.Body, c.SubmittedOn.ToDateTime());

        public static CreateRequest ToCreateRequest(this Comment comment) => new CreateRequest()
        {
            PhotoId = comment.PhotoId,
            Body = comment.Body,
            Subject = comment.Subject
        };

        public static UpdateRequest ToUpdateRequest(this Comment comment) => new UpdateRequest()
        {
            Id = comment.Id,
            Body = comment.Body,
            Subject = comment.Subject
        };
    }
}
