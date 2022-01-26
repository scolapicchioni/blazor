using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.WebServices.Grpc.Comments;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Infrastructure.TypedHttpClients {
    public class PublicCommentsClient {
        private readonly Commenter.CommenterClient gRpcClient;

        public PublicCommentsClient(Commenter.CommenterClient gRpcClient) {
            this.gRpcClient = gRpcClient;
        }

        public async Task<Comment> FindAsync(int id) {
            FindReply c = await gRpcClient.FindAsync(new FindRequest() { Id = id });
            return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
        }

        public async Task<List<Comment>> GetCommentsForPhotoAsync(int photoId) {
            GetCommentsForPhotosReply resp = await gRpcClient.GetCommentsForPhotoAsync(new GetCommentsForPhotosRequest() { PhotoId = photoId });
            return resp.Comments.Select(c => new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() }).ToList();
        }
    }
}
