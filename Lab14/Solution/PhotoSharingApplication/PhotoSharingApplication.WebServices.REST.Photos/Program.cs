using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PhotoSharingApplication.Backend.Infrastructure.Data;

namespace PhotoSharingApplication.WebServices.REST.Photos {
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

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
