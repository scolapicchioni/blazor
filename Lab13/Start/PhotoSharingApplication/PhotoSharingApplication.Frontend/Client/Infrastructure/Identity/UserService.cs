using Microsoft.AspNetCore.Components.Authorization;
using PhotoSharingApplication.Shared.Interfaces;
using System.Security.Claims;

namespace PhotoSharingApplication.Frontend.Client.Infrastructure.Identity; 
public class UserService : IUserService {
    private readonly AuthenticationStateProvider authenticationStateProvider;
    public UserService(AuthenticationStateProvider authenticationStateProvider) => this.authenticationStateProvider = authenticationStateProvider;
    public async Task<ClaimsPrincipal> GetUserAsync() => (await authenticationStateProvider.GetAuthenticationStateAsync()).User;
}