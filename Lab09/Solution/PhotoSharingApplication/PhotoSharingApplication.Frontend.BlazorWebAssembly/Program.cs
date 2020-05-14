using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PhotoSharingApplication.Shared.Core.Interfaces;
using PhotoSharingApplication.Frontend.Core.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Net.Client.Web;
using Grpc.Net.Client;
using PhotoSharingApplication.WebServices.Grpc.Comments;

namespace PhotoSharingApplication.Frontend.BlazorWebAssembly {
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri("https://localhost:44348/") });

            builder.Services.AddSingleton(services => {
                var backendUrl = "https://localhost:5001"; // Local debug URL
                var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
                var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions { HttpClient = httpClient });
                return new CommentsBaseService.CommentsBaseServiceClient(channel);
            });

            builder.Services.AddScoped<IPhotosService, PhotosService>();
            builder.Services.AddScoped<IPhotosRepository, PhotoSharingApplication.Frontend.Infrastructure.Repositories.Rest.PhotosRepository>();
            //builder.Services.AddScoped<IPhotosRepository, PhotoSharingApplication.Frontend.Infrastructure.Repositories.Memory.PhotosRepository>();

            builder.Services.AddScoped<ICommentsService, CommentsService>();
            //builder.Services.AddScoped<ICommentsRepository, PhotoSharingApplication.Frontend.Infrastructure.Repositories.Memory.CommentsRepository>();
            builder.Services.AddScoped<ICommentsRepository, PhotoSharingApplication.Frontend.Infrastructure.Repositories.Grpc.CommentsRepository>();
            await builder.Build().RunAsync();
        }
    }
}
