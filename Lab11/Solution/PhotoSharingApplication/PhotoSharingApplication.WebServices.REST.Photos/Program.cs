using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PhotoSharingApplication.Shared.Authorization;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;
using PhotoSharingApplication.WebServices.REST.Photos.Infrastructure.Data;
using PhotoSharingApplication.WebServices.REST.Photos.Infrastructure.Identity;
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

builder.Services.AddAuthentication("Bearer")
.AddJwtBearer("Bearer", options => {
    options.Authority = "https://localhost:5007";
    
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateAudience = false,
        NameClaimType = "name"
    };
    
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddSingleton<IAuthorizationHandler, PhotoEditDeleteAuthorizationHandler>();

builder.Services.AddAuthorization(options => {
    options.AddPhotosPolicies();
});

builder.Services.AddScoped<IAuthorizationService<Photo>, PhotosAuthorizationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
