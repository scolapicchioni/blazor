using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;

namespace PhotoSharingApplication.Frontend.BlazorComponents.BUnitTests {
    public class PhotoDetailsComponentTestsBase : TestContext {
        protected Mock<IUserService> userServiceMock;
        protected Mock<IAuthorizationService<Photo>> authorizationServiceMock;
        protected Photo photo;
        public PhotoDetailsComponentTestsBase() {
            userServiceMock = new Mock<IUserService>();
            authorizationServiceMock = new Mock<IAuthorizationService<Photo>>();

            Services.AddSingleton<IUserService>(userServiceMock.Object);
            Services.AddSingleton<IAuthorizationService<Photo>>(authorizationServiceMock.Object);

            photo = new Photo();
        }
        protected void UserIsAuthorized(bool authorized) {
            var User = new System.Security.Claims.ClaimsPrincipal();
            userServiceMock.Setup(us => us.GetUserAsync()).ReturnsAsync(User);
            authorizationServiceMock.Setup(auth => auth.ItemMayBeDeletedAsync(User, photo)).ReturnsAsync(authorized);
        }
    }
}
