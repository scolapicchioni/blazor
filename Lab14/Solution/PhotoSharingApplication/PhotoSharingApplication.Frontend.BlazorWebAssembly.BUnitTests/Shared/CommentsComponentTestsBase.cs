using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System.Collections.Generic;

namespace PhotoSharingApplication.Frontend.BlazorWebAssembly.BUnitTests.Shared {
    public class CommentsComponentTestsBase : TestContext {
        public CommentsComponentTestsBase() {
            Mock<ICommentsService> commentsServiceMock = new Mock<ICommentsService>();
            commentsServiceMock.Setup(cs => cs.GetCommentsForPhotoAsync(1)).ReturnsAsync(new List<Comment>());
            Services.AddSingleton<ICommentsService>(commentsServiceMock.Object);
            JSInterop.Mode = JSRuntimeMode.Loose;
        }
    }
}
