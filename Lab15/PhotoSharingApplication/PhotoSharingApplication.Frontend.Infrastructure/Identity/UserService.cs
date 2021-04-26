using Microsoft.AspNetCore.Components.Authorization;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Infrastructure.Identity {
    public class UserService : IUserService {
        private readonly AuthenticationStateProvider authenticationStateProvider;
        public UserService(AuthenticationStateProvider authenticationStateProvider) => this.authenticationStateProvider = authenticationStateProvider;
        public async Task<ClaimsPrincipal> GetUserAsync() => (await authenticationStateProvider.GetAuthenticationStateAsync()).User;
    }
}