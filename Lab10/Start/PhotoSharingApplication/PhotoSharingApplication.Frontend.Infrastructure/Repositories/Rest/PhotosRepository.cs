using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System.Net.Http.Json;

namespace PhotoSharingApplication.Frontend.Infrastructure.Repositories.Rest;

public class PhotosRepository : IPhotosRepository {
    private readonly HttpClient http;

    public PhotosRepository(HttpClient http) {
        this.http = http;
    }
    public async Task<Photo?> CreateAsync(Photo photo) {
        HttpResponseMessage response = await http.PostAsJsonAsync("/photos", photo);
        return await response.Content.ReadFromJsonAsync<Photo>();
    }

    public async Task<Photo?> FindAsync(int id) => await http.GetFromJsonAsync<Photo>($"/photos/{id}");

    public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await http.GetFromJsonAsync<List<Photo>>($"/photos");

    public async Task<Photo?> RemoveAsync(int id) {
        HttpResponseMessage response = await http.DeleteAsync($"/photos/{id}");
        return await response.Content.ReadFromJsonAsync<Photo>();
    }

    public async Task<Photo?> UpdateAsync(Photo photo) {
        HttpResponseMessage response = await http.PutAsJsonAsync($"/photos/{photo.Id}", photo);
        return await response.Content.ReadFromJsonAsync<Photo>();
    }
}
