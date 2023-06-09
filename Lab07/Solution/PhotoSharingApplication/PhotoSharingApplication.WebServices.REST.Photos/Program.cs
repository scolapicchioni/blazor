using Microsoft.EntityFrameworkCore;
using PhotoSharingApplication.Shared.Interfaces;
using PhotoSharingApplication.WebServices.REST.Photos.Infrastructure.Data;
using PhotoSharingApplication.WebServices.REST.Photos.Infrastructure.Repositories.EntityFramework;
using PhotoSharingApplication.WebServices.REST.Photos.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PhotosDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("PhotosDbContext")));
builder.Services.AddScoped<IPhotosService, PhotosService>();
builder.Services.AddScoped<IPhotosRepository, PhotosRepository>();

builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder => {
    builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
