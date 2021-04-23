using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhotoSharingApplication.Backend.Infrastructure.Data;

namespace PhotoSharingApplication.WebServices.Grpc.Comments {
    public class Program {
        public static void Main(string[] args) {
            IHost host = CreateHostBuilder(args).Build();

            migrateDb(host.Services);

            host.Run();
        }

        private static void migrateDb(IServiceProvider serviceProvider) {
            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope()) {
                var context = scope.ServiceProvider.GetService<PhotoSharingApplicationContext>();
                if (!context.Database.CanConnect())
                    try {
                        context.Database.Migrate();
                    } catch { 
                        
                    }
            }
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
