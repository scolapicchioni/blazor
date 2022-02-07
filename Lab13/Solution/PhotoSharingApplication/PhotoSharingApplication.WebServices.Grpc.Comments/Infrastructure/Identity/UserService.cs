using PhotoSharingApplication.Shared.Interfaces;
using System.Security.Claims;

namespace PhotoSharingApplication.WebServices.Grpc.Comments.Infrastructure.Identity;

public class UserService : IUserService {
    private readonly IHttpContextAccessor accessor;
    public UserService(IHttpContextAccessor accessor) => this.accessor = accessor;
    public Task<ClaimsPrincipal> GetUserAsync() => Task.FromResult(accessor.HttpContext.User);
}
