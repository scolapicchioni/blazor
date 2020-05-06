using PhotoSharingExamples.Frontend.Core.Interfaces;
using PhotoSharingExamples.Shared.Entities;
using Photosthingpackage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSharingExamples.Frontend.Infrastructure.GrpcClients
{
    public class PhotosGrpcClient : IPhotosRepository
    {
        private readonly PhotosThing.PhotosThingClient photosThingClient;

        public PhotosGrpcClient(Photosthingpackage.PhotosThing.PhotosThingClient photosThingClient)
        {
            this.photosThingClient = photosThingClient;
        }
        

        public async Task<Photo> FindAsync(int id)
        {
            FindReply p = await photosThingClient.FindAsync(new FindRequest() { Id = id });
            return p.ToPhoto();
        }

        public Task<Photo> FindByTitle(string title)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Photo>> GetPhotosAsync(int number = 10)
        {
            GetPhotosReply resp = await photosThingClient.GetPhotosAsync(new GetPhotosRequest() { Number = number });
            return resp.Photos.Select(p => p.ToPhoto()).ToList();
        }

        public Task<List<Photo>> GetPhotosByIdsAsync(List<int> ids)
        {
            throw new NotImplementedException();
        }
        public async Task<Photo> CreateAsync(Photo photo, string tokenValue)
        {
            Grpc.Core.Metadata headers = new Grpc.Core.Metadata();
            headers.Add("Authorization", $"Bearer {tokenValue}");
            CreateReply p = await photosThingClient.CreateAsync(photo.ToCreateRequest(), headers);
            return p.ToPhoto();
        }
        public async Task<Photo> RemoveAsync(int id, string tokenValue)
        {
            Grpc.Core.Metadata headers = new Grpc.Core.Metadata();
            headers.Add("Authorization", $"Bearer {tokenValue}");

            RemoveReply p = await photosThingClient.RemoveAsync(new RemoveRequest() { Id = id }, headers);
            return p.ToPhoto();
        }

        public async Task<Photo> UpdateAsync(Photo photo, string tokenValue)
        {
            Grpc.Core.Metadata headers = new Grpc.Core.Metadata();
            headers.Add("Authorization", $"Bearer {tokenValue}");

            UpdateReply p = await photosThingClient.UpdateAsync(photo.ToUpdateRequest(), headers);

            return p.ToPhoto();
        }

        
    }
}
