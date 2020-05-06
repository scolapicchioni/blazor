using PhotoSharingExamples.Frontend.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PhotoSharingExamples.Shared.Entities;
using System.Linq;

namespace PhotoSharingExamples.Frontend.Infrastructure.RestClients
{
    public class PhotosApiClient : IPhotosRepository
    {
        private HttpClient http;
        
        public PhotosApiClient(HttpClient http)
        {
            this.http = http;
        }


        public async Task<Photo> FindAsync(int id)
        {
            return await http.GetFromJsonAsync<Photo>($"{id}");
        }

        public async Task<Photo> FindByTitle(string title)
        {
            return await http.GetFromJsonAsync<Photo>($"title/{title}");
        }

        public async Task<List<Photo>> GetPhotosAsync(int number = 10)
        {
            return await http.GetFromJsonAsync<List<Photo>>($"all/{number}");
        }

        public async Task<List<Photo>> GetPhotosByIdsAsync(List<int> ids)
        {
            string queryString = string.Concat(ids.Select(id => $"ids={id}&"));
            return await http.GetFromJsonAsync<List<Photo>>($"?{queryString.Substring(0, queryString.Length - 1)}");
        }

        public async Task<Photo> CreateAsync(Photo photo, string tokenValue)
        {

            //var addItem = new TodoItem { Name = _newItemName, IsComplete = false };
            //await http.PostAsJsonAsync("/", addItem);

            var requestMessage = new HttpRequestMessage()
            {
                Method = new HttpMethod("POST"),
                RequestUri = http.BaseAddress,
                Content = JsonContent.Create(photo)
            };

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);

            var response = await http.SendAsync(requestMessage);
            return await response.Content.ReadFromJsonAsync<Photo>();

        }
        public async Task<Photo> RemoveAsync(int id, string tokenValue)
        {
            var requestMessage = new HttpRequestMessage()
            {
                Method = new HttpMethod("DELETE"),
                RequestUri = new Uri($"{http.BaseAddress.AbsoluteUri}/{id}")
            };

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);

            var response = await http.SendAsync(requestMessage);
            return await response.Content.ReadFromJsonAsync<Photo>();
        }

        public async Task<Photo> UpdateAsync(Photo photo, string tokenValue)
        {
            var requestMessage = new HttpRequestMessage()
            {
                Method = new HttpMethod("PUT"),
                RequestUri = new Uri($"{http.BaseAddress.AbsoluteUri}/{photo.Id}"),
                Content = JsonContent.Create(photo)
            };

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);

            var response = await http.SendAsync(requestMessage);
            return await response.Content.ReadFromJsonAsync<Photo>();
        }
    }
}
