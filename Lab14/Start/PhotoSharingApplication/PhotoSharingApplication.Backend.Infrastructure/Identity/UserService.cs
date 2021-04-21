using Microsoft.AspNetCore.Http;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Backend.Infrastructure.Identity {
    public class UserService : IUserService {
        private readonly IHttpContextAccessor accessor;
        public UserService(IHttpContextAccessor accessor) => this.accessor = accessor;
        public Task<ClaimsPrincipal> GetUserAsync() => Task.FromResult(accessor.HttpContext.User);
    }
}