using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.Frontend.BlazorComponents.BUnitTests.Components; 
public class PhotoDetailsComponentTestsBase : TestContext {
    protected Photo photo;
    protected Mock<IUserService> mockUserService;
    protected Mock<IAuthorizationService<Photo>> mockAuthorizationService;

    public PhotoDetailsComponentTestsBase()
    {
        photo = new Photo() {
            Id = 1,
            Title = "Photo 1",
            Description = "Description 1",
            ImageUrl = "https://localhost:44300/images/1.jpg",
            CreatedDate = DateTime.Now,
            UserName = "User 1"
        };

        mockUserService = new Mock<IUserService>();
        mockAuthorizationService = new Mock<IAuthorizationService<Photo>>();

        Services.AddSingleton<IUserService>(mockUserService.Object);
        Services.AddSingleton<IAuthorizationService<Photo>>(mockAuthorizationService.Object);
    }
}
