using System.Security.Claims;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Shared.Core.Interfaces {
    public interface IAuthorizationService<T> {
        Task<bool> ItemMayBeCreatedAsync(ClaimsPrincipal User, T item);
        Task<bool> ItemMayBeUpdatedAsync(ClaimsPrincipal User, T item);
        Task<bool> ItemMayBeDeletedAsync(ClaimsPrincipal User, T item);
    }
}
