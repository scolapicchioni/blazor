using Commentsthingpackage;
using PhotoSharingExamples.Frontend.Core.Interfaces;
using PhotoSharingExamples.Shared.Entities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSharingExamples.Frontend.Infrastructure.GrpcClients
{
    public class CommentsGrpcClient : ICommentsRepository
    {
        private readonly Commentsthingpackage.CommentsThing.CommentsThingClient commentsThingClient;

        public CommentsGrpcClient(Commentsthingpackage.CommentsThing.CommentsThingClient commentsThingClient)
        {
            this.commentsThingClient = commentsThingClient;
        }

        public async Task<Comment> CreateAsync(Comment comment, string tokenValue)
        {
            Grpc.Core.Metadata headers = new Grpc.Core.Metadata();
            headers.Add("Authorization", $"Bearer {tokenValue}");
            CreateReply c = await commentsThingClient.CreateAsync(comment.ToCreateRequest(), headers);
            return c.ToComment();
        }

        public async Task<Comment> FindAsync(int id)
        {
            FindReply c = await commentsThingClient.FindAsync(new FindRequest() { Id = id });
            return c.ToComment();
        }

        public async Task<List<Comment>> GetCommentsForPhotoAsync(int photoId)
        {
            GetCommentsForPhotosReply resp = await commentsThingClient.GetCommentsForPhotoAsync(new GetCommentsForPhotosRequest() { PhotoId = photoId});
            return resp.Comments.Select(c => c.ToComment()).ToList();
        }

        public async Task<Comment> RemoveAsync(int id, string tokenValue)
        {
            Grpc.Core.Metadata headers = new Grpc.Core.Metadata();
            headers.Add("Authorization", $"Bearer {tokenValue}");

            RemoveReply c = await commentsThingClient.RemoveAsync(new RemoveRequest() { Id = id }, headers);
            return c.ToComment();
        }

        public async Task<Comment> UpdateAsync(Comment comment, string tokenValue)
        {
            Grpc.Core.Metadata headers = new Grpc.Core.Metadata();
            headers.Add("Authorization", $"Bearer {tokenValue}");

            UpdateReply c = await commentsThingClient.UpdateAsync(comment.ToUpdateRequest(), headers);

            return c.ToComment();
        }
    }
}
