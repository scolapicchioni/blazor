using System.Security.Claims;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Shared.Core.Interfaces {
    public interface IUserService {
        Task<ClaimsPrincipal> GetUserAsync();
    }
}