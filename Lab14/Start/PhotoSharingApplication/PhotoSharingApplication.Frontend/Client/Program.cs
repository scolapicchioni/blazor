using FluentValidation;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PhotoSharingApplication.Frontend.Client;
using PhotoSharingApplication.Frontend.Client.Core.Services;
using PhotoSharingApplication.Frontend.Client.DuendeAuth;
using PhotoSharingApplication.Frontend.Client.Infrastructure.Identity;
using PhotoSharingApplication.Shared.Authorization;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;
using PhotoSharingApplication.Shared.Validators;
using PhotoSharingApplication.WebServices.Grpc.Comments;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTransient<AntiforgeryHandler>();

builder.Services.AddHttpClient("backend", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<AntiforgeryHandler>();
builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("backend"));

builder.Services.AddScoped<IPhotosService, PhotosService>();
builder.Services.AddScoped<IPhotosRepository, PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Rest.PhotosRepository>();

builder.Services.AddScoped<ICommentsService, CommentsService>();
builder.Services.AddScoped<ICommentsRepository, PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Grpc.CommentsRepository>();
builder.Services.AddSingleton(services => {
    var backendUrl = new Uri(builder.HostEnvironment.BaseAddress);
    var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions {
        HttpHandler = new GrpcWebHandler(new AntiforgeryHandler(new HttpClientHandler())),
    });
    return new Commenter.CommenterClient(channel);
});

builder.Services.AddAuthorizationCore(options => {
    options.AddPhotosPolicies();
    options.AddCommentsPolicies();
});

builder.Services.AddScoped<AuthenticationStateProvider, BffAuthenticationStateProvider>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddSingleton<IAuthorizationHandler, PhotoEditDeleteAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, CommentEditDeleteAuthorizationHandler>();

builder.Services.AddScoped<IAuthorizationService<Photo>, PhotosAuthorizationService>();
builder.Services.AddScoped<IAuthorizationService<Comment>, CommentsAuthorizationService>();

builder.Services.AddScoped<IValidator<Photo>, PhotoValidator>();
builder.Services.AddScoped<IValidator<Comment>, CommentValidator>();

await builder.Build().RunAsync();
