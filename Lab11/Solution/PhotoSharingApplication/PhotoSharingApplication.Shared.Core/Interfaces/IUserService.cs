using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Shared.Core.Interfaces {
    public interface IUserService {
        Task<ClaimsPrincipal> GetUserAsync();
    }
}
