using PhotoSharingApplication.Frontend.Infrastructure.TypedHttpClients;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Infrastructure.Repositories.Rest {
    public class PhotosRepository : IPhotosRepository {
        private readonly PublicPhotosClient publicPhotosClient;
        private readonly ProtectedPhotosClient protectedPhotosClient;

        public PhotosRepository(PublicPhotosClient publicPhotosClient, ProtectedPhotosClient protectedPhotosClient) {
            this.publicPhotosClient = publicPhotosClient;
            this.protectedPhotosClient = protectedPhotosClient;
        }
        public async Task<Photo> CreateAsync(Photo photo) => await protectedPhotosClient.CreateAsync(photo);

        public async Task<Photo> FindAsync(int id) => await publicPhotosClient.FindAsync(id);

        public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await publicPhotosClient.GetPhotosAsync(amount);

        public async Task<Photo> RemoveAsync(int id) => await protectedPhotosClient.RemoveAsync(id);

        public async Task<Photo> UpdateAsync(Photo photo) => await protectedPhotosClient.UpdateAsync(photo);
    }
}
