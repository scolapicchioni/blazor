using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Exceptions;
using PhotoSharingApplication.Shared.Interfaces;
using System.Net;
using System.Net.Http.Json;

namespace PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Rest;

public class CommentsRepository : ICommentsRepository {
    private readonly HttpClient http;

    public CommentsRepository(HttpClient http) {
        this.http = http;
    }

    public async Task<Comment?> CreateAsync(Comment comment) {
        HttpResponseMessage response = await http.PostAsJsonAsync("/comments", comment);
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden) {
            throw new CreateUnauthorizedException<Comment>();
        }
        return await response.Content.ReadFromJsonAsync<Comment>();
    }

    public async Task<List<Comment>?> GetCommentsForPhotoAsync(int photoId) => await http.GetFromJsonAsync<List<Comment>>($"/photos/{photoId}/comments");

    public async Task<Comment?> UpdateAsync(Comment comment) {
        HttpResponseMessage response = await http.PutAsJsonAsync($"/comments/{comment.Id}", comment);
        return response.StatusCode switch {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<Comment>(),
            HttpStatusCode.NotFound => null,
            HttpStatusCode.Forbidden => throw new EditUnauthorizedException<Comment>(),
            _ => throw new Exception(response.ReasonPhrase)
        };
    }

    public async Task<Comment?> FindAsync(int id) => await http.GetFromJsonAsync<Comment>($"/comments/{id}");

    public async Task<Comment?> RemoveAsync(int id) {
        HttpResponseMessage response = await http.DeleteAsync($"/comments/{id}");
        return response.StatusCode switch {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<Comment>(),
            HttpStatusCode.NotFound => null,
            HttpStatusCode.Forbidden => throw new DeleteUnauthorizedException<Comment>(),
            _ => throw new Exception(response.ReasonPhrase)
        };
    }
}
