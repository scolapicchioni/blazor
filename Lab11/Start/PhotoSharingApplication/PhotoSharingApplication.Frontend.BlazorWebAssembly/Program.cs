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
using Microsoft.Extensions.Configuration;
using PhotoSharingApplication.Frontend.BlazorWebAssembly.AuthorizationMessageHandlers;
using PhotoSharingApplication.Frontend.Infrastructure.TypedHttpClients;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace PhotoSharingApplication.Frontend.BlazorWebAssembly {
    public class Program {
        public static async Task Main(string[] args) {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped<IPhotosService, PhotosService>();
            builder.Services.AddScoped<IPhotosRepository, PhotoSharingApplication.Frontend.Infrastructure.Repositories.Rest.PhotosRepository>();

            builder.Services.AddScoped<ICommentsService, CommentsService>();
            builder.Services.AddScoped<ICommentsRepository, PhotoSharingApplication.Frontend.Infrastructure.Repositories.Grpc.CommentsRepository>();

            builder.Services.AddMatBlazor();
            
            builder.Services.AddOidcAuthentication(options => {
                // Configure your authentication provider options here.
                // For more information, see https://aka.ms/blazor-standalone-auth
                builder.Configuration.Bind("Local", options.ProviderOptions);
                options.ProviderOptions.DefaultScopes.Add("photosrest");
                options.ProviderOptions.DefaultScopes.Add("commentsgrpc");
            });
            
            builder.Services.AddHttpClient<PublicPhotosClient>(client => client.BaseAddress = new Uri("https://localhost:44303/"));

            builder.Services.AddHttpClient<ProtectedPhotosClient>(client => client.BaseAddress = new Uri("https://localhost:44303/"))
                .AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
                .ConfigureHandler(authorizedUrls: new[] { "https://localhost:44303/" }, scopes: new[] { "photosrest" }));

            builder.Services.AddScoped(services => {
                var backendUrl = "https://localhost:5005"; // Local debug URL
                var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
                var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions { HttpClient = httpClient });
                return new PublicCommentsClient(new Commenter.CommenterClient(channel));
            });
            
            builder.Services.AddScoped<CommentsAuthorizationMessageHandler>();
            
            builder.Services.AddScoped(sp => {
                var backendUrl = "https://localhost:5005"; // Local debug URL

                var commentsAuthorizationMessageHandler = sp.GetRequiredService<CommentsAuthorizationMessageHandler>();
                commentsAuthorizationMessageHandler.InnerHandler = new HttpClientHandler();
                var grpcWebHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, commentsAuthorizationMessageHandler);

                var httpClient = new HttpClient(grpcWebHandler);

                var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions { HttpClient = httpClient });

                return new ProtectedCommentsClient(new Commenter.CommenterClient(channel));
            });

            await builder.Build().RunAsync();
        }
    }
}
