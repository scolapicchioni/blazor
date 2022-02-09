using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PhotoSharingApplication.Shared.Authorization;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;
using PhotoSharingApplication.Shared.Validators;
using PhotoSharingApplication.WebServices.Rest.Photos.Core.Services;
using PhotoSharingApplication.WebServices.Rest.Photos.Infrastructure.Data;
using PhotoSharingApplication.WebServices.Rest.Photos.Infrastructure.Identity;
using PhotoSharingApplication.WebServices.Rest.Photos.Infrastructure.Repositories.EntityFramework;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PhotosDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("PhotosDbContext")));

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
        ValidateAudience = false
    };
});
builder.Services.AddAuthorization(options => {
    options.AddPhotosPolicies();
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IAuthorizationHandler, PhotoEditDeleteAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationService<Photo>, PhotosAuthorizationService>();

builder.Services.AddFluentValidation();
builder.Services.AddScoped<IValidator<Photo>, PhotoValidator>();

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
