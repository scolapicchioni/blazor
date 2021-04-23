using PhotoSharingApplication.Shared.Core.Entities;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Infrastructure.TypedHttpClients {
    public class PublicPhotosClient {
        private readonly HttpClient http;
        public PublicPhotosClient(HttpClient http) => this.http = http;

        public async Task<Photo> FindAsync(int id) => await http.GetFromJsonAsync<Photo>($"/photos/{id}");
        public async Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken) => await http.GetFromJsonAsync<List<Photo>>($"/photos/{startIndex}/{amount}", cancellationToken);
        public async Task<int> GetPhotosCountAsync() => int.Parse(await http.GetStringAsync($"/photos/count"));
        public async Task<Photo> FindWithImageAsync(int id) => await http.GetFromJsonAsync<Photo>($"/photos/withimage/{id}");
        public async Task<PhotoImage> GetImageAsync(int id) => await http.GetFromJsonAsync<PhotoImage>($"/photos/image/{id}");
    }
}
