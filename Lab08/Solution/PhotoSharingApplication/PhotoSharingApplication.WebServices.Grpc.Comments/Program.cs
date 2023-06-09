using Microsoft.EntityFrameworkCore;
using PhotoSharingApplication.Shared.Interfaces;
using PhotoSharingApplication.WebServices.Grpc.Comments.Core.Services;
using PhotoSharingApplication.WebServices.Grpc.Comments.Infrastructure.Data;
using PhotoSharingApplication.WebServices.Grpc.Comments.Infrastructure.Repositories;
using PhotoSharingApplication.WebServices.Grpc.Comments.Services;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddDbContext<CommentsDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("CommentsDbContext")));

builder.Services.AddScoped<ICommentsService, CommentsService>();
builder.Services.AddScoped<ICommentsRepository, CommentsRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<CommentsGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
