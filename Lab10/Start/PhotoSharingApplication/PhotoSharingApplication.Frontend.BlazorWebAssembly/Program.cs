using MatBlazor;
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
    public class Program {
        public static async Task Main(string[] args) {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            //builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            //builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:44379/") });
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5003/") });

            builder.Services.AddScoped<IPhotosService, PhotosService>();
            builder.Services.AddScoped<IPhotosRepository, PhotoSharingApplication.Frontend.Infrastructure.Repositories.Rest.PhotosRepository>();
            //builder.Services.AddScoped<IPhotosRepository, PhotoSharingApplication.Frontend.Infrastructure.Repositories.Memory.PhotosRepository>();
            builder.Services.AddScoped<ICommentsService, CommentsService>();
            //builder.Services.AddScoped<ICommentsRepository, PhotoSharingApplication.Frontend.Infrastructure.Repositories.Memory.CommentsRepository>();
            builder.Services.AddScoped<ICommentsRepository, PhotoSharingApplication.Frontend.Infrastructure.Repositories.Grpc.CommentsRepository>();

            builder.Services.AddSingleton(services => {
                var backendUrl = "https://localhost:5005"; // Local debug URL
                var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
                var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions { HttpClient = httpClient });
                return new Commenter.CommenterClient(channel);
            });

            builder.Services.AddMatBlazor();

            await builder.Build().RunAsync();
        }
    }
}
