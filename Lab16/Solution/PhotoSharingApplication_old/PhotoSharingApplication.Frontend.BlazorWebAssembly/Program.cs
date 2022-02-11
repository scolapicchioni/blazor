using FluentValidation;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhotoSharingApplication.Frontend.Core.Services;
using PhotoSharingApplication.Frontend.Infrastructure.Identity;
using PhotoSharingApplication.Shared.Authorization;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using PhotoSharingApplication.Shared.Core.Validators;
using PhotoSharingApplication.WebServices.Grpc.Comments;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.BlazorWebAssembly {
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

    //        builder.Services.AddTransient(sp => {
    //            return new HttpClient(sp.GetRequiredService<AuthorizationMessageHandler>()
    //                .ConfigureHandler(
    //                    new[] { "https://localhost:44348/photos" },
    //                    scopes: new[] { "photosrest" })) {
    //                BaseAddress = new Uri("https://localhost:44348/")
    //            };
    //        });


    //        builder.Services.AddHttpClient<PhotosService>(client => client.BaseAddress = new Uri("https://www.example.com/base"))
    //.AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
    //.ConfigureHandler(new[] { "https://localhost:44348/photos" },
    //    scopes: new[] { "photosrest" }));

            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["PhotosRestUrl"]) /*new Uri("https://localhost:44348/")*/ });

            builder.Services.AddSingleton(services => {
                var backendUrl = new Uri(builder.Configuration["CommentsGrpcUrl"]); // "https://localhost:5021"; // Local debug URL
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

            builder.Services.AddOidcAuthentication(options =>
            {
                // Configure your authentication provider options here.
                // For more information, see https://aka.ms/blazor-standalone-auth
                builder.Configuration.Bind("Local", options.ProviderOptions);

                options.ProviderOptions.DefaultScopes.Add("photosrest");
                options.ProviderOptions.DefaultScopes.Add("commentsgrpc"); //and the access token contains the granted audiences

            });

            builder.Services.AddAuthorizationCore(options => {
                options.AddPhotosPolicies();
                options.AddCommentsPolicies();
            });

            builder.Services.AddSingleton<IAuthorizationHandler, PhotoSameAuthorAuthorizationHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler, CommentSameAuthorAuthorizationHandler>();

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAuthorizationService<Photo>, PhotosAuthorizationService>();
            builder.Services.AddScoped<IAuthorizationService<Comment>, CommentsAuthorizationService>();

            builder.Services.AddTransient<IValidator<Photo>, PhotoValidator>();
            builder.Services.AddTransient<IValidator<PhotoImage>, PhotoImageValidator>();
            builder.Services.AddTransient<IValidator<Comment>, CommentValidator>();

            await builder.Build().RunAsync();
        }
    }
}
