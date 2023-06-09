using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Exceptions;
using PhotoSharingApplication.Shared.Interfaces;
using System.Net;
using System.Net.Http.Json;

namespace PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Rest; 
public class PhotosRepository : IPhotosRepository {
    private readonly HttpClient http;
    public PhotosRepository(HttpClient http) => this.http = http;

    public async Task<Photo?> FindAsync(int id) => await http.GetFromJsonAsync<Photo>($"/api/photos/{id}");

    public async Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken) => await http.GetFromJsonAsync<List<Photo>>($"/api/photos/{startIndex}/{amount}", cancellationToken);
    public async Task<int> GetPhotosCountAsync() => int.Parse(await http.GetStringAsync($"/api/photos/count"));

    public async Task<Photo?> CreateAsync(Photo photo) {
        HttpResponseMessage response = await http.PostAsJsonAsync("/api/photos", photo);
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden) {
            throw new CreateUnauthorizedException<Photo>();
        }
        return await response.Content.ReadFromJsonAsync<Photo>();
    }
    public async Task<Photo?> UpdateAsync(Photo photo) {
        HttpResponseMessage response = await http.PutAsJsonAsync($"/api/photos/{photo.Id}", photo);
        return response.StatusCode switch {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<Photo>(),
            HttpStatusCode.NotFound => null,
            HttpStatusCode.Forbidden => throw new EditUnauthorizedException<Photo>(),
            _ => throw new Exception(response.ReasonPhrase)
        };
    }
    public async Task<Photo?> RemoveAsync(int id) {
        HttpResponseMessage response = await http.DeleteAsync($"/api/photos/{id}");
        return response.StatusCode switch {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<Photo>(),
            HttpStatusCode.NotFound => null,
            HttpStatusCode.Forbidden => throw new DeleteUnauthorizedException<Photo>(),
            _ => throw new Exception(response.ReasonPhrase)
        };
    }
    public async Task<PhotoImage?> GetImageAsync(int id) => await http.GetFromJsonAsync<PhotoImage>($"/api/photos/image/{id}");

    public async Task<Photo> FindWithImageAsync(int id) => await http.GetFromJsonAsync<Photo>($"/api/photos/withimage/{id}");
}
