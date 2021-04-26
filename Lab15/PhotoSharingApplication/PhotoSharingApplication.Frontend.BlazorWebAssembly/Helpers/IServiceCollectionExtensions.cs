using FluentValidation;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhotoSharingApplication.Frontend.BlazorWebAssembly.AuthorizationMessageHandlers;
using PhotoSharingApplication.Frontend.Core.Services;
using PhotoSharingApplication.Frontend.Infrastructure.Identity;
using PhotoSharingApplication.Frontend.Infrastructure.TypedHttpClients;
using PhotoSharingApplication.Shared.Authorization;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using PhotoSharingApplication.Shared.Core.Validators;
using PhotoSharingApplication.WebServices.Grpc.Comments;
using System;
using System.Net.Http;

namespace PhotoSharingApplication.Frontend.BlazorWebAssembly.Helpers {
    public static class IServiceCollectionExtensions {
        public static IServiceCollection AddPhotoSharingValidation(this IServiceCollection services) {
            services.AddScoped<IValidator<Photo>, PhotoValidator>();
            services.AddScoped<IValidator<Comment>, CommentValidator>();
            return services;
        }

        public static IServiceCollection AddPhotoSharingAuthorization(this IServiceCollection services) {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthorizationService<Comment>, CommentsAuthorizationService>();
            services.AddScoped<IAuthorizationService<Photo>, PhotosAuthorizationService>();
            services.AddSingleton<IAuthorizationHandler, CommentEditDeleteAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, PhotoEditDeleteAuthorizationHandler>();

            services.AddAuthorizationCore(options => {
                options.AddPhotosPolicies();
                options.AddCommentsPolicies();
            });

            return services;
        }

        public static IServiceCollection AddPhotosHttpClients(this IServiceCollection services) {
            services.AddHttpClient<PublicPhotosClient>(client => client.BaseAddress = new Uri("https://localhost:44303/"));

            services.AddHttpClient<ProtectedPhotosClient>(client => client.BaseAddress = new Uri("https://localhost:44303/"))
                .AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
                .ConfigureHandler(authorizedUrls: new[] { "https://localhost:44303/" }, scopes: new[] { "photosrest" }));

            return services;
        }

        public static IServiceCollection AddCommentsGrpcClients(this IServiceCollection services) {
            services.AddScoped(services => {
                var backendUrl = "https://localhost:5005"; // Local debug URL
                var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
                var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions { HttpClient = httpClient });
                return new PublicCommentsClient(new Commenter.CommenterClient(channel));
            });

            services.AddScoped<CommentsAuthorizationMessageHandler>();

            services.AddScoped(sp => {
                var backendUrl = "https://localhost:5005"; // Local debug URL

                var commentsAuthorizationMessageHandler = sp.GetRequiredService<CommentsAuthorizationMessageHandler>();
                commentsAuthorizationMessageHandler.InnerHandler = new HttpClientHandler();
                var grpcWebHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, commentsAuthorizationMessageHandler);

                var httpClient = new HttpClient(grpcWebHandler);

                var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions { HttpClient = httpClient });

                return new ProtectedCommentsClient(new Commenter.CommenterClient(channel));
            });
            return services;
        }
        public static IServiceCollection AddPhotoSharingAuthentication(this IServiceCollection services, WebAssemblyHostBuilder builder) {
            services.AddOidcAuthentication(options => {
                // Configure your authentication provider options here.
                // For more information, see https://aka.ms/blazor-standalone-auth
                builder.Configuration.Bind("Local", options.ProviderOptions);
                options.ProviderOptions.DefaultScopes.Add("photosrest");
                options.ProviderOptions.DefaultScopes.Add("commentsgrpc");
            });
            return services;
        }

        public static IServiceCollection AddRepositoryAndServicesForPhotosAndComments(this IServiceCollection services) {
            services.AddScoped<IPhotosService, PhotosService>();
            services.AddScoped<IPhotosRepository, PhotoSharingApplication.Frontend.Infrastructure.Repositories.Rest.PhotosRepository>();

            services.AddScoped<ICommentsService, CommentsService>();
            services.AddScoped<ICommentsRepository, PhotoSharingApplication.Frontend.Infrastructure.Repositories.Grpc.CommentsRepository>();
            return services;
        }
    }
}
