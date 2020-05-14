using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using System.Net.Http;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using PhotoSharingExamples.Frontend.Core.Services;
using PhotoSharingExamples.Frontend.Core.Interfaces;
using PhotoSharingExamples.Frontend.Infrastructure.GrpcClients;
using Microsoft.AspNetCore.Authorization;
using PhotoSharingExamples.Shared.Authorization;
using Microsoft.Extensions.Configuration;
using PhotoSharingExamples.Frontend.Infrastructure.RestClients;

namespace PhotoSharingExamples.Frontend.BlazorClientStandalone
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //Update service registrations that depend on IAccessTokenProvider to be scoped services instead of singleton services!

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            
            builder.RootComponents.Add<App>("app");

            builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddOidcAuthentication(options =>
            {
                //builder.Configuration.Bind("Local", options.ProviderOptions);
                // Configure your authentication provider options here.
                // For more information, see https://aka.ms/blazor-standalone-auth
                options.ProviderOptions.Authority = "http://localhost:5000"; // "https://localhost:5001"; 
                options.ProviderOptions.ClientId = "blazorstandalone";

                options.ProviderOptions.ResponseType = "code"; //this is fundamental to talk to Identity Server 4

                options.ProviderOptions.DefaultScopes.Add("photos"); //you add these so that the user can consent
                options.ProviderOptions.DefaultScopes.Add("photosrest"); 
                options.ProviderOptions.DefaultScopes.Add("comments"); //and the access token contains the granted audiences

            });

            #region grpcPhotos
            //            builder.Services.AddSingleton(services =>
            //            {
            //#if DEBUG
            //                var backendUrl = "https://localhost:5011"; // Local debug URL
            //#else
            //                var backendUrl = "https://localhost:5011"; // Production URL
            //#endif

            //                // Now we can instantiate gRPC clients for this channel
            //                var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
            //                var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions { HttpClient = httpClient });
            //                return new Photosthingpackage.PhotosThing.PhotosThingClient(channel);
            //            });

            //            builder.Services.AddTransient<IPhotosService, PhotosService>();
            //            builder.Services.AddTransient<IPhotosRepository, PhotosGrpcClient>();
            #endregion

            #region WebApiPhotos
            builder.Services.AddSingleton(
                new HttpClient
                {
                    BaseAddress = new Uri("https://localhost:5041/api/photos/")
                });

            builder.Services.AddTransient<IPhotosService, PhotosService>();
            builder.Services.AddTransient<IPhotosRepository, PhotosApiClient>();

            #endregion

            #region gRPCComments
            builder.Services.AddSingleton(services =>
            {
#if DEBUG
                var backendUrl = "https://localhost:5021"; // Local debug URL
#else
                var backendUrl = "https://localhost:5021"; // Production URL
#endif

                // Now we can instantiate gRPC clients for this channel
                var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
                var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions { HttpClient = httpClient });
                return new Commentsthingpackage.CommentsThing.CommentsThingClient(channel);
            });
            #endregion
            builder.Services.AddTransient<ICommentsService, CommentsService>();
            builder.Services.AddTransient<ICommentsRepository, CommentsGrpcClient>();

            builder.Services
              .AddBlazorise(options =>
              {
                  options.ChangeTextOnKeyPress = true;
              })
              .AddBootstrapProviders()
              .AddFontAwesomeIcons();

            builder.Services.AddAuthorizationCore(options =>
            {
                options.AddPolicy(Policies.EditDeletePhoto, Policies.CanEditDeletePhotoPolicy());
                options.AddPolicy(Policies.EditDeleteComment, Policies.CanEditDeleteCommentPolicy());
            });

            builder.Services.AddSingleton<IAuthorizationHandler, PhotoEditDeleteAuthorizationHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler, CommentEditDeleteAuthorizationHandler>();

            var host =  builder.Build();

            host.Services
                .UseBootstrapProviders()
                .UseFontAwesomeIcons();
            
            await host.RunAsync();
        }
    }
}
