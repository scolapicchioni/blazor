using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Exceptions;
using PhotoSharingApplication.Shared.Interfaces;
using System.Net;

namespace PhotoSharingApplication.Frontend.Server.Infrastructure.Repositories.Rest;

public class PhotosRepository : IPhotosRepository {
    private readonly HttpClient http;
    private readonly IHttpContextAccessor httpContextAccessor;

    public PhotosRepository(HttpClient http, IHttpContextAccessor httpContextAccessor) => (this.http, this.httpContextAccessor) = (http, httpContextAccessor);
    
    public async Task<Photo?> CreateAsync(Photo photo) {
        var token = await httpContextAccessor.HttpContext.GetUserAccessTokenAsync();
        http.SetBearerToken(token);
        HttpResponseMessage response = await http.PostAsJsonAsync("/photos", photo);
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden) {
            throw new CreateUnauthorizedException<Photo>();
        }
        return await response.Content.ReadFromJsonAsync<Photo>();
    }

    public async Task<Photo?> FindAsync(int id) => await http.GetFromJsonAsync<Photo>($"/photos/{id}");

    public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await http.GetFromJsonAsync<List<Photo>>($"/photos");

    public async Task<Photo?> RemoveAsync(int id) {
        var token = await httpContextAccessor.HttpContext.GetUserAccessTokenAsync();
        http.SetBearerToken(token);
        HttpResponseMessage response = await http.DeleteAsync($"/photos/{id}");
        return response.StatusCode switch {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<Photo>(),
            HttpStatusCode.Forbidden => throw new DeleteUnauthorizedException<Photo>(),
            _ => throw new Exception(response.ReasonPhrase)
        }; 
    }

    public async Task<Photo?> UpdateAsync(Photo photo) {
        var token = await httpContextAccessor.HttpContext.GetUserAccessTokenAsync();
        http.SetBearerToken(token);
        HttpResponseMessage response = await http.PutAsJsonAsync($"/photos/{photo.Id}", photo);
        return response.StatusCode switch {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<Photo>(),
            HttpStatusCode.Forbidden => throw new EditUnauthorizedException<Photo>(),
            _ => throw new Exception(response.ReasonPhrase)
        };
    }
}
