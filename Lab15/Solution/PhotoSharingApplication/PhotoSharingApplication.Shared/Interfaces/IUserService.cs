using System.Security.Claims;

namespace PhotoSharingApplication.Shared.Interfaces; 
public interface IUserService {
    Task<ClaimsPrincipal> GetUserAsync();
}
