using Microsoft.AspNetCore.Http;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PhotoSharingApplication.WebServices.Grpc.Comments {
    public class UserService : IUserService {
        private readonly IHttpContextAccessor accessor;

        public UserService(IHttpContextAccessor accessor) {
            this.accessor = accessor;
        }

        public Task<ClaimsPrincipal> GetUserAsync() {
            return Task.FromResult(accessor.HttpContext.User);
        }
    }
}
