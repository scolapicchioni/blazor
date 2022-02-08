# Unit Testing Blazor Components with bUnit

In this lab we're going to talk about Unit Testing Blazor components.   
I am giving for granted that you know about unit testing and Mocking. Many people have written articles and books about it, so I'm not going to repeat their words. If you want to learn about Unit Testing you can check many different resources. For example:
- [Martin Fowler](https://martinfowler.com/testing/)  
- [Test Driven Development - An Extensive Tutorial - By Grzegorz Gałęzowski](https://leanpub.com/tdd-ebook/read)
- [Unit Testing in C#](https://docs.educationsmediagroup.com/unit-testing-csharp/)

And of course, [Google](https://www.google.com/) is your friend. 

As for the technologies, we're going to use:
- [xUnit](https://xunit.net/)
- [Moq](https://github.com/Moq/moq4/wiki/Quickstart)
- [bUnit](https://bunit.egilhansen.com/index.html), created by Egil Hansen, because Microsoft does not have its own testing framework for Blazor

> bUnit is a testing library for Blazor Components. Its goal is to make it easy to write comprehensive, stable unit tests. With bUnit, you can:
> 
> - Setup and define components under tests using C# or Razor syntax
> - Verify outcomes using semantic HTML comparer
> - Interact with and inspect components as well as trigger event handlers
> - Pass parameters, cascading values and inject services into components under test
> - Mock IJSRuntime, Blazor authentication and authorization, and others
> bUnit builds on top of existing unit testing frameworks such as xUnit, NUnit, and MSTest, which run the Blazor components tests in just the same way as any normal unit test. bUnit runs a test in milliseconds, compared to browser-based UI tests which usually take seconds to run.

We're going to follow the [documentation](https://bunit.egilhansen.com/docs/getting-started/index.html) to create a new project and start testing our components.  
In this lab, we're not going to test every scenario of every component, because:
- It would take too much time
- Many tests would be very similar to other tests we already wrote. The goal of this lab is to learn the techniques. Once you've seen one technique, there's no need to repeat it over and over.
We're going to focus on a couple of components that offer us the opportunity to learn the many uses of bUnit. Feel free to test each and every other component if you want.  

Our `PhotoDetailsComponent.razor` component, located under the `PhotoSharingApplication.Frontend.BlazorComponents` project, is a good candidate to start with.  
This means that we're going to add yet another project. This one is going to be an `xUnit` project.  
We're going to follow the [documentation](https://bunit.egilhansen.com/docs/getting-started/create-test-project.html?tabs=xunit#creating-a-test-project-manually) to create an `xUnit` project and change it to be ready for `bUnit`  

- In the `Solution Explorer`, right click on the solution, then select `Add` -> `New Project`
- Select `xUnit Test Project`
- Name the project `PhotoSharingApplication.Frontend.BlazorComponents.BUnitTests`
- When the project is ready, add the `bUnit` NuGet Package to the project
- Add the `Moq` NuGet Package to the project
- Change the *target sdk* in the `.csproj` to `Sdk="Microsoft.NET.Sdk.Razor"`
    - In the Solution Explorer, double click on the name of the `PhotoSharingApplication.Frontend.BlazorComponents.BUnitTests` project. You should be able to edit the `.csproj` file.
    - Change the first line to `<Project Sdk="Microsoft.NET.Sdk.Razor">`. 
    - Leave the rest unchanged. Save and close the `.csproj` file.
- Add a project reference to the `PhotoSharingApplication.Frontend.BlazorComponents` project

We can start to [Create our tests in a .razor file](https://bunit.dev/docs/getting-started/writing-tests.html#creating-basic-tests-in-razor-files).  

> - Add an `_Imports.razor` file to the test project. It serves the same purpose as `_Imports.razor` files in regular Blazor projects. These using statements are useful to add right away:
```cs
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.JSInterop
@using Microsoft.Extensions.DependencyInjection
@using AngleSharp.Dom
@using Bunit
@using Bunit.TestDoubles
@using Xunit
@using Moq
@using PhotoSharingApplication.Frontend.BlazorComponents
@using PhotoSharingApplication.Frontend.BlazorComponents.Components
@using PhotoSharingApplication.Frontend.BlazorComponents.Pages
@using PhotoSharingApplication.Shared.Entities
@using PhotoSharingApplication.Shared.Interfaces
@using MatBlazor
```

The first scenario that we want to test, checks that our component correctly renders a `MatHeader6` with a `Photo.Id` and a `Photo.Title` in the correct format.  
- Add a new `Components` folder to the `PhotoSharingApplication.Frontend.BlazorComponents.BUnitTests` project
- Under the `Components` folder, add a new `Razor Component`
- Name the component `PhotoDetailsComponentTests.razor`
- In the `code` section, create an `xUnit` test by adding a `[Fact]` attribute to a new `ShouldRenderH6WithPhotoIdAndTitle` method
- Create a new instance of the disposable bUnit [TestContext](https://bunit.egilhansen.com/api/Bunit.TestContext.html), and assign it to ctx the variable.
- Create a [Mock](https://github.com/Moq/moq4/wiki/Quickstart) of the `IUserService` and `IAuthorizationService<Photo>` services
- [Inject the services into the component](https://bunit.egilhansen.com/docs/providing-input/inject-services-into-components.html)
- Render the `<PhotoDetailsComponent>` component using `TestContext`, which is done through the [Render(RenderFragment)](https://bunit.dev/api/Bunit.TestContext.html#Bunit_TestContext_Render_RenderFragment_) method, using [inline razor syntax](https://bunit.egilhansen.com/docs/getting-started/writing-tests.html?tabs=xunit#secret-sauce-of-razor-files-tests). 
- [Pass a `Photo` parameter to the component](https://bunit.egilhansen.com/docs/providing-input/passing-parameters-to-components.html)
- [Find](https://bunit.egilhansen.com/docs/verification/verify-component-state.html#finding-components-in-the-render-tree) the `MatHeader6` component
- [Find](https://bunit.egilhansen.com/docs/verification/verify-markup.html#finding-nodes-with-the-find-and-findall-methods) the TextContent of the `h6` node
- [Verify that the rendered markup matches our expectations](https://bunit.egilhansen.com/docs/verification/verify-markup.html#semantic-comparison-of-markup) using the [MarkupMatches](https://bunit.egilhansen.com/docs/verification/verify-markup.html#the-markupmatches-method) method. 

Your code should look something like this:

```cs
@code {
    [Fact]
    public void ShouldRenderH6WithPhotoIdAndTitle() {
        //Arrange
        using var ctx = new TestContext();
        Mock<IUserService> userServiceMock = new Mock<IUserService>();
        Mock<IAuthorizationService<Photo>> authorizationServiceMock  = new Mock<IAuthorizationService<Photo>>();
        ctx.Services.AddSingleton<IUserService>(userServiceMock.Object);
        ctx.Services.AddSingleton<IAuthorizationService<Photo>>(authorizationServiceMock.Object);
        Photo photo = new Photo();
        photo.Id = 1;
        photo.Title = "Photo Title";
        //Act
        var cut = ctx.Render(@<PhotoDetailsComponent Photo="photo"></PhotoDetailsComponent>);
        var h6 = cut.FindComponent<MatHeadline6>().Find("h6").TextContent;
        //Assert
        h6.MarkupMatches("1 - Photo Title");
    }
}
```

[Run the test with the Test Explorer](https://docs.microsoft.com/en-us/visualstudio/test/run-unit-tests-with-test-explorer?view=vs-2022). The test should pass.

In our second test, we want to make sure that the `Photo` property of the `PhotoPictureComponent` is set properly.  
As you can imagine, the Arrange phase is pretty much the same, so it would be nice to refactor it into a common initialization.  
xUnit makes use of a [constructor and Dispose](https://xunit.net/docs/shared-context#constructor) to share the context between tests.  
We cannot write a constructor directly in a `.razor` file, but we can [specify a base class](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/?view=aspnetcore-6.0#specify-a-base-class) and write our constructor there.  
We can further [remove boilerplate code](https://bunit.egilhansen.com/docs/getting-started/writing-tests.html#remove-boilerplate-code-from-tests-1) by deriving from `TestContext`.

- In the `PhotoSharingApplication.Frontend.BlazorComponents.BUnitTests` project, add a new class. 
- Name the class `PhotoDetailsComponentTestsBase`
- Let the class inherit from `TestContext`
- Move the initialization code of the `Arrange` phase in this class 
    - Declare protected fields for the two services and the `Photo`
    - Create a constructor 
    - Initialize the services
    - Inject the services in the context
    - Initialize the Photo
- Let the `PhotoDetailsComponentTests.razor` component inherit from `PhotoDetailsComponentTestsBase`
- Remove the initialization code from the `Arrange` phase of the test

The code of the `PhotoDetailsComponentTestsBase.cs` file should look like this:
```cs
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.Frontend.BlazorComponents.BUnitTests.Components;

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
}
```

The code of the `PhotoDetailsComponentTests.razor` file should look like this:

```cs
@inherits PhotoDetailsComponentTestsBase

<h3>PhotoDetailsComponentTests</h3>

@code {
[Fact]
    public void ShouldRenderH6WithPhotoIdAndTitle() {
        // Arrange
        photo.Id = 1;
        photo.Title = "Photo Title";

        // Act
        var cut = Render(@<PhotoDetailsComponent Photo="photo" />);
        var h6 = cut.FindComponent<MatHeadline6>().Find("h6").TextContent;

        //Assert
        h6.MarkupMatches("1 - Photo Title");
    }
}
```

- Run the test. It should still pass

Now we're ready to write the second test where we make sure that the `Photo` property of the `PhotoPictureComponent` is properly set.

- In the `code` section, create an `xUnit` test by adding a `[Fact]` attribute to a new `ShouldSetPhotoOfPhotoPicture` method
- Render the `<PhotoDetailsComponent>` component by invoking the [Render(RenderFragment)](https://bunit.dev/api/Bunit.TestContext.html#Bunit_TestContext_Render_RenderFragment_) method, using [inline razor syntax](https://bunit.egilhansen.com/docs/getting-started/writing-tests.html?tabs=xunit#secret-sauce-of-razor-files-tests). 
- [Pass the `Photo` parameter to the component](https://bunit.egilhansen.com/docs/providing-input/passing-parameters-to-components.html)
- [Find](https://bunit.egilhansen.com/docs/verification/verify-component-state.html#finding-components-in-the-render-tree) the `PhotoPictureComponent` component
- [Inspect the Instance](https://bunit.egilhansen.com/docs/verification/verify-component-state.html#inspecting-the-component-under-test)
- [Assert](https://xunit.net/docs/getting-started/netfx/visual-studio#write-first-tests) that the photo of the component is equal to the one we expect.

Your code should look like this:

```cs
[Fact]
public void ShouldSetPhotoOfPhotoPicture() {
    var cut = Render(@<PhotoDetailsComponent Photo="photo"></PhotoDetailsComponent>);
    var photoPictureComponent = cut.FindComponent<PhotoPictureComponent>();
    Assert.Equal(photo, photoPictureComponent.Instance.Photo);
}
```

Run the test. It should pass.

Let's write a test to make sure that the `Details` button does not get rendered when the `Details` property of the component is set to `false`. 

```cs
[Fact]
public void ShouldNotRenderDetailsButton_WhenDetailsPropertyNotSet() {
    var cut = Render(@<PhotoDetailsComponent Photo="photo"></PhotoDetailsComponent>);
    var renderedButtons = cut.FindComponents<MatButton>();
    Assert.DoesNotContain(renderedButtons, rb => rb.Find("button").TextContent.Contains("Details"));
}
```

We can add two more tests: 
- One to make sure that the `Details` button *does* get rendered when the `Details` property of the component is set to `true`
- One to assert that the `Link` property is set correctly.

They're very similar to the tests we already wrote but there's one new thing. We need to setup the [jsRuntime](https://bunit.egilhansen.com/docs/test-doubles/emulating-ijsruntime.html) because the `MatBlazor` button uses javascript internally and the test would complain if it's not setup correctly.

The only line we have to add is to [setup `bUnit JSInterop` in loose mode](https://bunit.egilhansen.com/docs/test-doubles/emulating-ijsruntime.html#strict-vs-loose-mode) 
> (...) to just return the default value when it receives an invocation that has not been explicitly set up, e.g. if a component calls InvokeAsync<int>(...) the mock will simply return default(int) back to it immediately.


```cs
[Fact]
public void ShouldRenderDetailsButton_WhenDetailsPropertyIsSet() {
    JSInterop.Mode = JSRuntimeMode.Loose;
    var cut = Render(@<PhotoDetailsComponent Photo="photo" Details></PhotoDetailsComponent>);
    var renderedButtons = cut.FindComponents<MatButton>();
    Assert.Contains(renderedButtons, rb => rb.Find("button").TextContent.Contains("Details"));
}
[Fact]
public void ShouldRenderDetailsButtonWithLinkSetToPhotoId() {
    photo.Id = 1;
    JSInterop.Mode = JSRuntimeMode.Loose;
    var cut = Render(@<PhotoDetailsComponent Photo="photo" Details></PhotoDetailsComponent>);
    var detailsButton = cut.FindComponent<MatButton>();
    Assert.Equal("photos/details/1", detailsButton.Instance.Link);
}
```

The next 4 tests we're writing are going to deal with authorized and not authorized users, to check whether the `Delete` gets rendered or not.  
We have four different combinations:

User | Delete Property | Should Render
--- | --- | ---
not authorized | set to `true` | no
not authorized | not set (`false`) | no
authorized | set to `true` | yes
authorized | not set (`false`) | no

The authorization is taken care by our own `IUserService` and `AuthorizationService<Photo>`, so it's just a matter of setup them correctly. Let's create an `UserIsAuthorized(bool)` method in our base class so that we can have [DAMP and DRY unit tests](https://stackoverflow.com/questions/6453235/what-does-damp-not-dry-mean-when-talking-about-unit-tests).

- In your `PhotoDetailsComponentTestsBase` class, add the following code

```cs
protected void UserIsAuthorized(bool authorized) {
    var User = new System.Security.Claims.ClaimsPrincipal();
    userServiceMock.Setup(us => us.GetUserAsync()).ReturnsAsync(User);
    authorizationServiceMock.Setup(auth => auth.ItemMayBeDeletedAsync(User, photo)).ReturnsAsync(authorized);
}
```

Now we can write our tests in the `.razor` component:

```cs
[Fact]
public void ShouldNotRenderDeleteButton_WhenUserNotAuthorized_AndDeletePropertyNotSet() {
    UserIsAuthorized(false);
    var cut = Render(@<PhotoDetailsComponent Photo="photo"></PhotoDetailsComponent>);
    var renderedButtons = cut.FindComponents<MatButton>();
    Assert.DoesNotContain(renderedButtons, rb => rb.Find("button").TextContent.Contains("Delete"));
}
[Fact]
public void ShouldNotRenderDeleteButton_WhenUserNotAuthorized_AndDeletePropertyIsSet() {
    UserIsAuthorized(false);
    var cut = Render(@<PhotoDetailsComponent Photo="photo" Delete></PhotoDetailsComponent>);
    var renderedButtons = cut.FindComponents<MatButton>();
    Assert.DoesNotContain(renderedButtons, rb => rb.Find("button").TextContent.Contains("Delete"));
}
[Fact]
public void ShouldNotRenderDeleteButton_WhenUserAuthorized_AndDeletePropertyNotSet() {
    UserIsAuthorized(true);
    var cut = Render(@<PhotoDetailsComponent Photo="photo"></PhotoDetailsComponent>);
    var renderedButtons = cut.FindComponents<MatButton>();
    Assert.DoesNotContain(renderedButtons, rb => rb.Find("button").TextContent.Contains("Delete"));
}
[Fact]
public void ShouldRenderDeleteButton_WhenUserAuthorized_AndDeletePropertyIsSet() {
    UserIsAuthorized(true);
    JSInterop.Mode = JSRuntimeMode.Loose;
    var cut = Render(@<PhotoDetailsComponent Photo="photo" Delete></PhotoDetailsComponent>);
    var renderedButtons = cut.FindComponents<MatButton>();
    Assert.Contains(renderedButtons, rb => rb.Find("button").TextContent.Contains("Delete"));
}
```

All the tests should pass.

Our very last test for this component is to make sure that pressing the `Delete` button raises the `` event.  
We need to pass an [Event Callback parameter](https://bunit.egilhansen.com/docs/providing-input/passing-parameters-to-components.html?tabs=razor#eventcallback-parameters) in order to check if it gets invoked.  
We're going to use the technique described in the bUnit docs under the [Triggering a render life cycle on a component](https://bunit.egilhansen.com/docs/interaction/trigger-renders.html#invokeasync) page.

```cs
[Fact]
public async Task ShouldInvokeDeleted_WhenConfirmDeleteButtonIsPressed() {
    JSInterop.Mode = JSRuntimeMode.Loose;
    photo.Id = 1;
    int actualPhotoId = 0;
    UserIsAuthorized(true);
    Action<int> deleteConfirmed = photoId => {
        actualPhotoId = photoId;
    };
    var cut = Render(@<PhotoDetailsComponent Photo="photo" DeleteConfirm OnDeleteConfirmed="deleteConfirmed"></PhotoDetailsComponent>);

    var deleteConfirmButton = cut.FindComponent<MatButton>();
    await cut.InvokeAsync(async () => await deleteConfirmButton.Instance.OnClick.InvokeAsync(null));

    Assert.Equal(photo.Id, actualPhotoId);
}
```

That's it for our `PhotoDetailsComponent` tests.  

The next tests will give us an opportunity to see how to setup a Mock for the validation engine of `Blazored.FluentValidation`.  
Our `CommentCreateComponent` component uses the `<FluentValidationValidator />`, which in turn invokes the `ValidateAsync` method of an `IValidator<Comment>` passing a `ValidationContext`. This means that if we mock, setup and inject an `IValidator<Comment>` into the services of the test, we can simulate errors to test whether our `OnSave` event is raised or not.

Let's first setup the common elements for both the tests by creating a base class with a constructor, like we did for our previous tests.

- In the `PhotoSharingApplication.Frontend.BlazorComponents.BUnitTests` project, add a new `CommentCreateComponentTestsBase.cs` class
- Let the class inherit from `TestContext`
- Declare the following protected variables:
```cs
protected Comment comment;
protected bool saveInvoked;
protected Comment actual;
protected Action<Comment> Save;
protected Mock<IValidator<Comment>> validationMock;
```

which require the following using:

```cs
using Bunit;
using FluentValidation;
using Moq;
using PhotoSharingApplication.Shared.Entities;
using System;
```
- Add a constructor
- Initialize the variables as follows:
```cs
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
```

which requires

```cs
using Microsoft.Extensions.DependencyInjection;
```

- Add a new `CommentCreateComponentTests.razor` component
- Let the component inherit from `CommentCreateComponentTestBase`
- Add a new `ShouldInvokeSaveOnSubmit_WhenModelIsValid` test
- Setup the validationmock to return an empty ValidationResult object. This will simulate that there are no errors in the form
- Render the `<CommentCreateComponent>` component
- Invoke the Submit method of the `form` component
- Assert that the `saveInvoked` is true
- Assert that the `actual` object is equal to the `comment` object

Your code should look like this:

```cs
[Fact]
public void ShouldInvokeSaveOnSubmit_WhenModelIsValid() {
    //Arrange
    validationMock
        .Setup(cv => cv.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<System.Threading.CancellationToken>()))
        .ReturnsAsync(new ValidationResult());
    var cut = Render(@<CommentCreateComponent CommentItem="comment" OnSave="Save"></CommentCreateComponent>);

    //Act
    cut.Find("form").Submit();

    //Assert
    Assert.True(saveInvoked);
    Assert.Equal(comment, actual);
}
```

which requires

```cs
@using FluentValidation
@using FluentValidation.Results
```

In order to test what happens when there are errors, we need to setup the ValidateAsync with a list of errors:

- Add a new `ShouldNotInvokeSaveOnSubmit_WhenModelIsNotValid` test
- Setup the validationmock to return a `ValidationResult` filled with a `List<ValidationFailure>` object. This will simulate that there are errors in the form
- Render the `<CommentCreateComponent>` component
- Invoke the Submit method of the `form` component
- Assert that the `saveInvoked` is false
- Assert that the `actual` object is still `null`

Your code should look like this:

```cs
[Fact]
public void ShouldNotInvokeSaveOnSubmit_WhenModelIsNotValid() {
    //Arrange
    validationMock
        .Setup(cv => cv.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<System.Threading.CancellationToken>()))
        .ReturnsAsync(new ValidationResult(new List<ValidationFailure>() {
            new ValidationFailure("Subject","Too long!"),
            new ValidationFailure("Body","Too mean!")
        }));

    var cut = Render(@<CommentCreateComponent CommentItem="comment" OnSave="Save"></CommentCreateComponent>);

    //Act
    cut.Find("form").Submit();

    //Assert
    Assert.False(saveInvoked);
}
```

Both tests should pass.

One last technique that we want to learn is how to [fake authentication and authorization](https://bunit.egilhansen.com/docs/test-doubles/faking-auth.html) in order to test the `<AuthorizeView>` component.  
To see this in action, we're going to write a couple of tests for the `CommentsComponent` component, located in the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project.  
Since this is a different project, let's create a different test project, repeating the steps we followed at the beginning.

- In the `Solution Explorer`, right click on the solution, then select `Add` -> `New Project`
- Select `xUnit Test Project`
- Name the project `PhotoSharingApplication.Frontend.BlazorWebAssembly.BUnitTests`
- When the project is ready, add the `bUnit` NuGet Package to the project
- Add the `Moq` NuGet Package to the project
- Change the *target sdk* in the `.csproj` to `Sdk="Microsoft.NET.Sdk.Razor"`
    - In the Solution Explorer, double click on the name of the `PhotoSharingApplication.Frontend.BlazorWebAssembly.BUnitTests` project. You should be able to edit the `.csproj` file.
    - Change the first line to `<Project Sdk="Microsoft.NET.Sdk.Razor">`. 
    - Leave the rest unchanged. Save and close the `.csproj` file.
- Add a project reference to the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project

We can start to [Create our tests in a .razor file](https://bunit.egilhansen.com/docs/getting-started/writing-tests.html#creating-basic-tests-in-razor-files).  

> - Add an `_Imports.razor` file to the test project. It serves the same purpose as `_Imports.razor` files in regular Blazor projects. These using statements are useful to add right away:
```cs
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.JSInterop
@using Microsoft.Extensions.DependencyInjection
@using AngleSharp.Dom
@using Bunit
@using Bunit.TestDoubles
@using Xunit
@using Moq
@using PhotoSharingApplication.Frontend.BlazorWebAssembly
```

- In the `PhotoSharingApplication.Frontend.BlazorWebAssembly.BUnitTests` project, add a new `CommentsComponentTestsBase.cs` class
- Let the class inherit from `TestContext`
- Add a constructor
- Initialize and setup a `CommentsService` to return a `Comment` the variables
- Setup the JS interop in loose mode

Your code should look as follows:

```cs
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;
using System.Collections.Generic;

namespace PhotoSharingApplication.Frontend.BlazorComponents.BUnitTests.Components;

public class CommentsComponentTestsBase : TestContext {
    public CommentsComponentTestsBase() {
        Mock<ICommentsService> commentsServiceMock = new Mock<ICommentsService>();
        commentsServiceMock.Setup(cs => cs.GetCommentsForPhotoAsync(1)).ReturnsAsync(new List<Comment>());
        Services.AddSingleton<ICommentsService>(commentsServiceMock.Object);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }
}
```


- Add a new `CommentsComponentTests.razor` component
- Let the component inherit from `CommentsComponentTestsBase`
- Add a new `ShouldNotRenderCommentInCreateMode_WhenNotAuthenticated_NotAuthorized` test
- Setup the an [unauthenticated and unauthorized](https://bunit.egilhansen.com/docs/test-doubles/faking-auth.html#unauthenticated-and-unauthorized-state) state
- Render the `<CommentsComponent>` component
- Find all the `CommentComponent` components
- Assert that there is no component whose `ViewMode` is set to `Create`

Your code should look like this:

```cs
@inherits CommentsComponentTestsBase
@using PhotoSharingApplication.Frontend.BlazorComponents
@code{
[Fact]
public void ShouldNotRenderCommentInCreateMode_WhenNotAuthenticated_NotAuthorized() {
    //unauthenticated and unauthorized
    this.AddTestAuthorization();

    var cut = Render(@<CommentsComponent PhotoId="1"></CommentsComponent>);

    var commentComponents = cut.FindComponents<CommentComponent>();

    Assert.DoesNotContain(commentComponents, c => c.Instance.ViewMode == CommentComponent.ViewModes.Create);
}
```

- Add a new `ShouldNotRenderCommentInCreateMode_WhenAuthenticated_NotAuthorized` test
- Setup the an [authenticated and unauthorized](https://bunit.egilhansen.com/docs/test-doubles/faking-auth.html#authenticated-and-unauthorized-state) state
- Render the `<CommentsComponent>` component
- Find all the `CommentComponent` components
- Assert that there is no component whose `ViewMode` is set to `Create`

Your code should look like this:

```cs
[Fact]
public void ShouldNotRenderCommentInCreateMode_WhenAuthenticated_NotAuthorized() {
    //authenticated and unauthorized
    var authContext = this.AddTestAuthorization();
    authContext.SetAuthorized("TEST USER", AuthorizationState.Unauthorized);

    var cut = Render(@<CommentsComponent PhotoId="1"></CommentsComponent>);

    var commentComponents = cut.FindComponents<CommentComponent>();

    Assert.DoesNotContain(commentComponents, c => c.Instance.ViewMode == CommentComponent.ViewModes.Create);
}
```

- Add a new `ShouldRenderCommentInCreateMode_WhenAuthenticated_Authorized` test
- Setup the an [authenticated and authorized](https://bunit.egilhansen.com/docs/test-doubles/faking-auth.html#authenticated-and-authorized-state) state
- Render the `<CommentsComponent>` component
- Find all the `CommentComponent` components
- Assert that there is a component whose `ViewMode` is set to `Create`

Your code should look like this:

```cs
[Fact]
public void ShouldRenderCommentInCreateMode_WhenAuthenticated_Authorized() {
    //authenticated and authorized
    var authContext = this.AddTestAuthorization();
    authContext.SetAuthorized("TEST USER", AuthorizationState.Authorized);

    var cut = Render(@<CommentsComponent PhotoId="1"></CommentsComponent>);

    var commentComponents = cut.FindComponents<CommentComponent>();

    Assert.Contains(commentComponents, c => c.Instance.ViewMode == CommentComponent.ViewModes.Create);
}
```

And we're done!  
Of course there are many other tests we could write, but I'm not going to describe them all since they are a repetition of what we have already seen here, so feel free to write them yourself if you feel like it.

---

In the next lab, we're going to explore the interoperability between Blazor and javascript.