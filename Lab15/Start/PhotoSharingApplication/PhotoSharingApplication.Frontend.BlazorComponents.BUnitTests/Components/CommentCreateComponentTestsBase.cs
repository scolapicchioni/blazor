using Bunit;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PhotoSharingApplication.Shared.Entities;
using System;

namespace PhotoSharingApplication.Frontend.BlazorComponents.BUnitTests.Components {
    public class CommentCreateComponentTestsBase : TestContext {
        protected Comment comment;
        protected bool saveInvoked;
        protected Comment actual;
        protected Action<Comment> Save;
        protected Mock<IValidator<Comment>> validationMock;

        public CommentCreateComponentTestsBase() {
            comment = new Comment();
            saveInvoked = false;
            actual = null;
            Save = c => {
                saveInvoked = true;
                actual = c;
            };
            JSInterop.Mode = JSRuntimeMode.Loose;

            validationMock = new Mock<IValidator<Comment>>();
            Services.AddSingleton<IValidator<Comment>>(validationMock.Object);
        }
    }
}
