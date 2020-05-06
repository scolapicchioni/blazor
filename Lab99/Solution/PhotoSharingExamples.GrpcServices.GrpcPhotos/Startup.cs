using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhotoSharingExamples.Backend.Core.Interfaces;
using PhotoSharingExamples.Backend.Infrastructure.Data;
using PhotoSharingExamples.Backend.Infrastructure.Logging;
using PhotoSharingExamples.Shared.Authorization;

namespace PhotoSharingExamples.GrpcServices.GrpcPhotos
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();

            services.AddCors(o => o.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .WithExposedHeaders("Grpc-Status", "Grpc-Message");
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

            //        services.AddIdentity<User, IdentityRole>(options =>
            //        {
            //            ...
            //options.ClaimsIdentity.UserIdClaimType = JwtRegisteredClaimNames.Sub;
            //        })
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            
            app.UseCors();
            
            app.UseGrpcWeb();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<PhotosService>()
                .EnableGrpcWeb().RequireCors("AllowAll");

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
