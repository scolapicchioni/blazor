using PhotoSharingApplication.Frontend.Infrastructure.TypedHttpClients;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Infrastructure.Repositories.Grpc {
    public class CommentsRepository : ICommentsRepository {
        private readonly PublicCommentsClient publicCommentsClient;
        private readonly ProtectedCommentsClient protectedCommentsClient;

        public CommentsRepository(PublicCommentsClient publicCommentsClient, ProtectedCommentsClient protectedCommentsClient) {
            this.publicCommentsClient = publicCommentsClient;
            this.protectedCommentsClient = protectedCommentsClient;
        }
        public async Task<Comment> CreateAsync(Comment comment) => await protectedCommentsClient.CreateAsync(comment);

        public async Task<Comment> FindAsync(int id) => await publicCommentsClient.FindAsync(id);

        public async Task<List<Comment>> GetCommentsForPhotoAsync(int photoId) => await publicCommentsClient.GetCommentsForPhotoAsync(photoId);

        public async Task<Comment> RemoveAsync(int id) => await protectedCommentsClient.RemoveAsync(id);

        public async Task<Comment> UpdateAsync(Comment comment) => await protectedCommentsClient.UpdateAsync(comment);
    }
}