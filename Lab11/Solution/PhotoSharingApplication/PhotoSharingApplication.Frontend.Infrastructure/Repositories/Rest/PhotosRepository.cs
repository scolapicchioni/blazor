using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Infrastructure.Repositories.Rest {
    public class PhotosRepository : IPhotosRepository {
        private readonly HttpClient http;
        private readonly IAccessTokenProvider tokenProvider;

        public PhotosRepository(HttpClient http, IAccessTokenProvider tokenProvider) {
            this.http = http;
            this.tokenProvider = tokenProvider;
        }

        public async Task<Photo> CreateAsync(Photo photo) {
            //try {
                var tokenResult = await tokenProvider.RequestAccessToken(new AccessTokenRequestOptions() { Scopes = new string[] { "photosrest" } });
                if (tokenResult.TryGetToken(out var token)) {
                    var requestMessage = new HttpRequestMessage() {
                        Method = new HttpMethod("POST"),
                        RequestUri = new Uri(http.BaseAddress, "/photos"),
                        Content = JsonContent.Create(photo)
                    };

                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);

                    var response = await http.SendAsync(requestMessage);
                    return await response.Content.ReadFromJsonAsync<Photo>();
                }
            //} catch (AccessTokenNotAvailableException exception) {
            //    exception.Redirect();
            //} catch (Exception ex) {
            //    Console.WriteLine(ex + " - " + ex.Message);
            //}
            //return null;

            //try { 
            //    HttpResponseMessage response = await http.PostAsJsonAsync("/photos", photo);
            //    return await response.Content.ReadFromJsonAsync<Photo>();
            //} catch (AccessTokenNotAvailableException exception) {
            //    exception.Redirect();
            //}
            return null;
        }

        public async Task<Photo> FindAsync(int id) => await http.GetFromJsonAsync<Photo>($"/photos/{id}");

        public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await http.GetFromJsonAsync<List<Photo>>("/photos");

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
