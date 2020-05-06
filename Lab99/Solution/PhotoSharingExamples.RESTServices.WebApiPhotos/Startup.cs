using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PhotoSharingExamples.Backend.Core.Interfaces;
using PhotoSharingExamples.Backend.Infrastructure.Data;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using IdentityModel;
using PhotoSharingExamples.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;
using PhotoSharingExamples.Backend.Infrastructure.Logging;

namespace PhotoSharingExamples.RESTServices.WebApiPhotos
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddCors(o => o.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            services.AddTransient(typeof(IAppLogger<>), typeof(LoggerAdapter<>));

            services.AddScoped<IPhotosService, Backend.Core.Services.PhotosService>();
            services.AddScoped<IPhotosRepository, PhotosRepository>();

            services.AddDbContext<PhotoSharingApplicationContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("PhotoSharingApplicationContext")));

            services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = "http://localhost:5000";
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters.NameClaimType = JwtClaimTypes.Name;
                options.Audience = "photos";
            });

            //don't know if this is going to work, had to add it, found on 
            //https://github.com/grpc/grpc-dotnet/blob/master/examples/Ticketer/Server/Startup.cs
            services.AddAuthorization(options =>
            {
                options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireClaim(JwtClaimTypes.Name);
                });

                //found on https://chrissainty.com/securing-your-blazor-apps-configuring-policy-based-authorization-with-blazor/
                options.AddPolicy(Policies.EditDeletePhoto, Policies.CanEditDeletePhotoPolicy());
            });

            services.AddSingleton<IAuthorizationHandler, PhotoEditDeleteAuthorizationHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
