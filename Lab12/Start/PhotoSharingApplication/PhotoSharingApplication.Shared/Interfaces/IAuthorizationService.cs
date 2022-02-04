using System.Security.Claims;

namespace PhotoSharingApplication.Shared.Interfaces;

public interface IAuthorizationService<T> {
    Task<bool> ItemMayBeCreatedAsync(ClaimsPrincipal User, T item);
    Task<bool> ItemMayBeUpdatedAsync(ClaimsPrincipal User, T item);
    Task<bool> ItemMayBeDeletedAsync(ClaimsPrincipal User, T item);
}
