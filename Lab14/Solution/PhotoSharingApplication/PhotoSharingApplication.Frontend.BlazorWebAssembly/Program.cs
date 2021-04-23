using MatBlazor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PhotoSharingApplication.Frontend.BlazorWebAssembly.Helpers;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.BlazorWebAssembly {
    public class Program {
        public static async Task Main(string[] args) {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddRepositoryAndServicesForPhotosAndComments();

            builder.Services.AddMatBlazor();

            builder.Services.AddPhotoSharingAuthentication(builder);

            builder.Services.AddPhotoSharingAuthorization();

            builder.Services.AddPhotosHttpClients();
            builder.Services.AddCommentsGrpcClients();

            builder.Services.AddPhotoSharingValidation();

            await builder.Build().RunAsync();
        }

    }
}
