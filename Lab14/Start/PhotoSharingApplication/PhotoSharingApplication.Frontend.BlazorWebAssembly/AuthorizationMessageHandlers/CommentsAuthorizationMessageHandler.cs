using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace PhotoSharingApplication.Frontend.BlazorWebAssembly.AuthorizationMessageHandlers {
    public class CommentsAuthorizationMessageHandler : AuthorizationMessageHandler {
        public CommentsAuthorizationMessageHandler(IAccessTokenProvider provider,
            NavigationManager navigationManager)
            : base(provider, navigationManager) {
            ConfigureHandler(
                authorizedUrls: new[] { "https://localhost:5005/photos" },
                scopes: new[] { "commentsgrpc" });
        }
    }
}
