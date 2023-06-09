using Bunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.Frontend.BlazorComponents.BUnitTests.Components;
public class CommentsComponentTestsBase : TestContext {
    protected int photoId;
    protected Mock<ICommentsService> commentServiceMock;
    protected Mock<IUserService> mockUserService;
    protected Mock<IAuthorizationService<Comment>> mockAuthorizationService;
    protected TestAuthorizationContext authContext;
    public CommentsComponentTestsBase()
    {
        //Arrange
        photoId = 1;

        commentServiceMock = new Mock<ICommentsService>();
        mockUserService = new Mock<IUserService>();
        mockAuthorizationService = new Mock<IAuthorizationService<Comment>>();

        Services.AddSingleton<ICommentsService>(commentServiceMock.Object);
        Services.AddSingleton<IUserService>(mockUserService.Object);
        Services.AddSingleton<IAuthorizationService<Comment>>(mockAuthorizationService.Object);

        authContext = this.AddTestAuthorization();
    }
}
