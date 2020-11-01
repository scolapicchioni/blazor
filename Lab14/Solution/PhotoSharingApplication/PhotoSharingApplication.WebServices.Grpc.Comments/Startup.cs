using FluentValidation;
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
using PhotoSharingApplication.Backend.Core.Services;
using PhotoSharingApplication.Backend.Infrastructure.Data;
using PhotoSharingApplication.Backend.Infrastructure.Repositories.EntityFramework;
using PhotoSharingApplication.Shared.Authorization;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using PhotoSharingApplication.Shared.Core.Validators;

namespace PhotoSharingApplication.WebServices.Grpc.Comments {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {
            services.AddGrpc();
            services.AddCors(o => o.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
            }));
            services.AddDbContext<PhotoSharingApplicationContext>(options =>
                options.UseSqlServer(Configuration["ConnectionString"]));
            services.AddScoped<ICommentsService, CommentsService>();
            services.AddScoped<ICommentsRepository, CommentsRepository>();

            services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options => {
                options.Authority = Configuration["Authority"]  ;// "https://localhost:5031";
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters.NameClaimType = JwtClaimTypes.Name;
                options.Audience = "commentsgrpc";
            });
            services.AddAuthorization(options => {
                options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy => {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireClaim(JwtClaimTypes.Name);
                });
                //found on https://chrissainty.com/securing-your-blazor-apps-configuring-policy-based-authorization-with-blazor/
                options.AddCommentsPolicies();
            });
            services.AddSingleton<IAuthorizationHandler, CommentSameAuthorAuthorizationHandler>();
            services.AddScoped<IAuthorizationService<Comment>, CommentsAuthorizationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddHttpContextAccessor();
            services.AddTransient<IValidator<Comment>, CommentValidator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseGrpcWeb();
            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapGrpcService<GreeterService>();
                endpoints.MapGrpcService<Services.CommentsService>().EnableGrpcWeb().RequireCors("AllowAll"); 
                endpoints.MapGet("/", async context => {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
