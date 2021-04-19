using PhotoSharingApplication.Shared.Core.Entities;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Infrastructure.TypedHttpClients {
    public class ProtectedPhotosClient {
        private readonly HttpClient http;
        public ProtectedPhotosClient(HttpClient http) {
            this.http = http;
        }
        public async Task<Photo> CreateAsync(Photo photo) {
            HttpResponseMessage response = await http.PostAsJsonAsync("/photos", photo);
            return await response.Content.ReadFromJsonAsync<Photo>();
        }
        public async Task<Photo> RemoveAsync(int id) {
            HttpResponseMessage response = await http.DeleteAsync($"/photos/{id}");
            return await response.Content.ReadFromJsonAsync<Photo>();
        }

        public async Task<Photo> UpdateAsync(Photo photo) {
            HttpResponseMessage response = await http.PutAsJsonAsync($"/photos/{photo.Id}", photo);
            return await response.Content.ReadFromJsonAsync<Photo>();
        }
    }
}
