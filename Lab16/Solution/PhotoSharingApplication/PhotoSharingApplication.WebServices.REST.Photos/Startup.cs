using FluentValidation;
using FluentValidation.AspNetCore;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhotoSharingApplication.Backend.Core.Services;
using PhotoSharingApplication.Backend.Infrastructure.Data;
using PhotoSharingApplication.Backend.Infrastructure.Identity;
using PhotoSharingApplication.Backend.Infrastructure.Repositories.EntityFramework;
using PhotoSharingApplication.Shared.Authorization;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using PhotoSharingApplication.Shared.Core.Validators;

namespace PhotoSharingApplication.WebServices.REST.Photos {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllers().AddFluentValidation();
            services.AddDbContext<PhotoSharingApplicationContext>(options =>
            //"PhotoSharingApplicationContext": "Server=(localdb)\\mssqllocaldb;Database=PhotoSharingApplicationContextBlazorLabs;Trusted_Connection=True;MultipleActiveResultSets=true"
                options.UseSqlServer(Configuration["ConnectionString"]));
            services.AddScoped<IPhotosService, PhotosService>();
            services.AddScoped<IPhotosRepository, PhotosRepository>();
            services.AddCors(o => o.AddPolicy("AllowAll", builder => {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options => {
                options.Authority = Configuration["Authority"]; // "https://localhost:5031";
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters.NameClaimType = JwtClaimTypes.Name;
                options.Audience = "photosrest";
            });
            services.AddAuthorization(options => {
                options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy => {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireClaim(JwtClaimTypes.Name);
                });
                options.AddPhotosPolicies();
            });

            services.AddSingleton<IAuthorizationHandler, PhotoSameAuthorAuthorizationHandler>();
            services.AddScoped<IAuthorizationService<Photo>, PhotosAuthorizationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddHttpContextAccessor();

            services.AddTransient<IValidator<Photo>, PhotoValidator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
