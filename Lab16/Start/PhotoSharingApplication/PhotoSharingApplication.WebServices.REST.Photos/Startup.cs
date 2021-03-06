using FluentValidation;
using FluentValidation.AspNetCore;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
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
                options.UseSqlServer(Configuration.GetConnectionString("PhotoSharingApplicationContext")));

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PhotoSharingApplication.WebServices.REST.Photos", Version = "v1" });
            });
            services.AddScoped<IPhotosService, PhotosService>();
            services.AddScoped<IPhotosRepository, PhotosRepository>();

            services.AddCors(o => o.AddPolicy("AllowAll", builder => {
                builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
            }));

            services.AddAuthentication("Bearer")
              .AddJwtBearer("Bearer", options => {
                  options.Authority = "https://localhost:5007";
                  options.RequireHttpsMetadata = false;
                  options.TokenValidationParameters.NameClaimType = JwtClaimTypes.Name;
                  options.Audience = "photosrest";
              });

            services.AddAuthorization(options => {
                //options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy => {
                //    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                //    policy.RequireClaim(JwtClaimTypes.Name);
                //});

                options.AddPhotosPolicies();
            });

            services.AddHttpContextAccessor();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthorizationService<Photo>, PhotosAuthorizationService>();
            services.AddSingleton<IAuthorizationHandler, PhotoEditDeleteAuthorizationHandler>();

            services.AddScoped<IValidator<Photo>, PhotoValidator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PhotoSharingApplication.WebServices.REST.Photos v1"));
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
