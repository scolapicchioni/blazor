# Unit Testing Blazor Components with bUnit

In this lab we're going to talk about Unit Testing Blazor components.   
I am giving for granted that you know about unit testing and Mocking. Many people have written articles and books about it, so I'm not going to repeat their words. If you want to learn about Unit Testing you can check many different resources. For example:
- [Martin Fowler](https://martinfowler.com/testing/)  
- [Test Driven Development - An Extensive Tutorial - By Grzegorz Gałęzowski](https://leanpub.com/tdd-ebook/read)
- [Unit Testing in C#](https://docs.educationsmediagroup.com/unit-testing-csharp/)

And of course, [Google](https://www.google.com/) and [ChatGpt](https://chat.openai.com) are your friends. 

As for the technologies, we're going to use:
- [xUnit](https://xunit.net/)
- [Moq](https://github.com/Moq/moq4/wiki/Quickstart)
- [bUnit](https://bunit.dev/index.html), created by Egil Hansen, because Microsoft does not have its own testing framework for Blazor
- [FluetAssertions](https://fluentassertions.com/)

> bUnit is a testing library for Blazor Components. Its goal is to make it easy to write comprehensive, stable unit tests. With bUnit, you can:
> 
> - Setup and define components under tests using C# or Razor syntax
> - Verify outcomes using semantic HTML comparer
> - Interact with and inspect components as well as trigger event handlers
> - Pass parameters, cascading values and inject services into components under test
> - Mock IJSRuntime, Blazor authentication and authorization, and others
> bUnit builds on top of existing unit testing frameworks such as xUnit, NUnit, and MSTest, which run the Blazor components tests in just the same way as any normal unit test. bUnit runs a test in milliseconds, compared to browser-based UI tests which usually take seconds to run.

We're going to follow the [documentation](https://bunit.dev/docs/getting-started/index.html) to create a new project and start testing our components.  
In this lab, we're not going to test every scenario of every component, because:
- It would take too much time
- Many tests would be very similar to other tests we already wrote. The goal of this lab is to learn the techniques. Once you've seen one technique, there's no need to repeat it over and over.
We're going to focus on a couple of components that offer us the opportunity to learn the many uses of bUnit. Feel free to test each and every other component if you want.  

Our `PhotoDetailsComponent.razor` component, located under the `PhotoSharingApplication.Frontend.BlazorComponents` project, is a good candidate to start with.  
This means that we're going to add yet another project. This one is going to be an `xUnit` project.  
We're going to follow the [documentation](https://bunit.dev/docs/getting-started/create-test-project.html?tabs=xunit#creating-a-test-project-manually) to create an `xUnit` project and change it to be ready for `bUnit`  

- In the `Solution Explorer`, right click on the solution, then select `Add` -> `New Project`
- Select `xUnit Test Project`
- Name the project `PhotoSharingApplication.Frontend.BlazorComponents.BUnitTests`
- When the project is ready, add the `bUnit` NuGet Package to the project
- Add the `Moq` NuGet Package to the project
- Add the `FluentAssertions` NuGet Package to the project
- Change the *target sdk* in the `.csproj` to `Sdk="Microsoft.NET.Sdk.Razor"`
    - In the Solution Explorer, double click on the name of the `PhotoSharingApplication.Frontend.BlazorComponents.BUnitTests` project. You should be able to edit the `.csproj` file.
    - Change the first line to `<Project Sdk="Microsoft.NET.Sdk.Razor">`. 
    - Leave the rest unchanged. Save and close the `.csproj` file.
- Add a project reference to the `PhotoSharingApplication.Frontend.BlazorComponents` project

We can start to [Create our tests in a .razor file](https://bunit.dev/docs/getting-started/writing-tests.html?tabs=xunit#creating-basic-tests-in-razor-files).  

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
@using FluentAssertions
@using PhotoSharingApplication.Frontend.BlazorComponents
@using PhotoSharingApplication.Frontend.BlazorComponents.Components
@using PhotoSharingApplication.Frontend.BlazorComponents.Pages
@using PhotoSharingApplication.Shared.Entities
@using PhotoSharingApplication.Shared.Interfaces
@using MudBlazor
```

The first scenario that we want to test, checks that our component correctly renders a Mud Card Header with a `Photo.Id` and a `Photo.Title` in the correct format.  
- Add a new `Components` folder to the `PhotoSharingApplication.Frontend.BlazorComponents.BUnitTests` project
- Under the `Components` folder, add a new `Razor Component`
- Name the component `PhotoDetailsComponentTests.razor`
- Inherit from the bUnit [TestContext](https://bunit.dev/api/Bunit.TestContext.html). This base class offers the majority of functions.
- In the `code` section, create an `xUnit` test by adding a `[Fact]` attribute to a new `PhotoDetailsComponentRendersIdAndTitleInCardHeader` method
- Create a [Mock](https://github.com/Moq/moq4/wiki/Quickstart) of the `IUserService` and `IAuthorizationService<Photo>` services
- [Inject the services into the component](https://bunit.dev/docs/providing-input/inject-services-into-components.html)
- Render the `<PhotoDetailsComponent>` component using `TestContext`, which is done through the [Render(RenderFragment)](https://bunit.dev/api/Bunit.TestContext.html#Bunit_TestContext_Render_Microsoft_AspNetCore_Components_RenderFragment_) method, using [inline razor syntax](https://bunit.dev/docs/getting-started/writing-tests.html?tabs=xunit#secret-sauce-of-razor-files-tests). 
- [Pass a `Photo` parameter to the component](https://bunit.dev/docs/providing-input/passing-parameters-to-components.html?tabs=razor)
- [Find](https://bunit.dev/docs/verification/verify-markup.html#finding-nodes-with-the-find-and-findall-methods) the `div` whose `class` is `mud-card-header-content`
- [Verify that the rendered markup matches our expectations](https://bunit.dev/docs/verification/verify-markup.html#semantic-comparison-of-markup) using the [MarkupMatches](https://bunit.dev/docs/verification/verify-markup.html#the-markupmatches-method) method. 

The code becomes

```cs
@inherits TestContext

@code {
    [Fact]
    public void PhotoDetailsComponentRendersIdAndTitleInCardHeader() {
        // Arrange
        var photo = new Photo() {
            Id = 1,
            Title = "Photo 1",
            Description = "Description 1",
            ImageUrl = "https://localhost:44300/images/1.jpg",
            CreatedDate = DateTime.Now,
            UserName = "User 1"
        };
        // Inject Mocks
        Services.AddSingleton<IUserService>(new Mock<IUserService>().Object);
        Services.AddSingleton<IAuthorizationService<Photo>>(new Mock<IAuthorizationService<Photo>>().Object);

        var cut = Render(@<PhotoDetailsComponent Photo="photo" />);

        // Assert
        cut.Find("div.mud-card-header-content").MarkupMatches(
            @<div class="mud-card-header-content">
                <p class="mud-typography mud-typography-body1">@photo.Id</p>
                <p class="mud-typography mud-typography-body2">@photo.Title</p>
            </div>
        );
    }
}
```

[Run the test with the Test Explorer](https://learn.microsoft.com/en-us/visualstudio/test/run-unit-tests-with-test-explorer?view=vs-2022). The test should pass.

For the second scenario, we're going to check if `Description`, `CreateDate` and `UserName` are rendered correctly.  
Technically it's the same kind of test, but we'll use a different technique to find the items and to check if their content conforms our expectations.

- In the `code` section, create an `xUnit` test by adding a `[Fact]` attribute to a new `PhotoDetailsComponentRendersDescriptionCreatedDateAndUserName` method
- Create a [Mock](https://github.com/Moq/moq4/wiki/Quickstart) of the `IUserService` and `IAuthorizationService<Photo>` services
- [Inject the services into the component](https://bunit.dev/docs/providing-input/inject-services-into-components.html)
- Render the `<PhotoDetailsComponent>` component, but this time use [RenderComponent](https://bunit.dev/api/Bunit.TestContext.html#Bunit_TestContext_RenderComponent__1_System_Action_Bunit_ComponentParameterCollectionBuilder___0___) method. 
- [Pass a `Photo` parameter to the component](https://bunit.dev/docs/providing-input/passing-parameters-to-components.html?tabs=csharp)
- [Find](https://bunit.dev/docs/verification/verify-component-state.html#finding-components-in-the-render-tree) the `MudCardContent` and then all the `MudText` components in the component under test.
- [Find](https://bunit.dev/docs/verification/verify-markup.html#finding-nodes-with-the-find-and-findall-methods) the `p` in the first of the MudText fields and verify that the `TextContent` property is equal to the `photo.Description`
- - [Find](https://bunit.dev/docs/verification/verify-markup.html#finding-nodes-with-the-find-and-findall-methods) the `p` in the first of the MudText fields and [Verify that the rendered markup matches our expectations](https://bunit.dev/docs/verification/verify-markup.html#semantic-comparison-of-markup) using the [MarkupMatches](https://bunit.dev/docs/verification/verify-markup.html#the-markupmatches-method) method but ignoring the attributes by [customizing the semantic HTML comparison](https://bunit.dev/docs/verification/semantic-html-comparison.html). 

Your code should look something like this:

```cs
[Fact]
public void PhotoDetailsComponentRendersDescriptionCreatedDateAndUserName() {
    // Arrange
    var photo = new Photo() {
        Id = 1,
        Title = "Photo 1",
        Description = "Description 1",
        ImageUrl = "https://localhost:44300/images/1.jpg",
        CreatedDate = DateTime.Now,
        UserName = "User 1"
    };
    // Inject Mocks
    Services.AddSingleton<IUserService>(new Mock<IUserService>().Object);
    Services.AddSingleton<IAuthorizationService<Photo>>(new Mock<IAuthorizationService<Photo>>().Object);

    // Act
    var cut = RenderComponent<PhotoDetailsComponent>(parameters => parameters
        .Add(p => p.Photo, photo));

    var mudTextFields = cut.FindComponent<MudCardContent>().FindComponents<MudText>();

    // Assert
    mudTextFields.Count.Should().Be(2);
    mudTextFields[0].Find("p").TextContent.Should().Be(photo.Description);
    mudTextFields[1].MarkupMatches(@<h6 diff:ignoreAttributes>Uploaded on @photo.CreatedDate.ToShortDateString() by @photo.UserName</h6>);
}
```

[Run the test with the Test Explorer](https://learn.microsoft.com/en-us/visualstudio/test/run-unit-tests-with-test-explorer?view=vs-2022). The test should pass.

In our second test, we want to make sure that the `Photo` property of the `PhotoPictureComponent` is set properly.  
As you can imagine, the Arrange phase is pretty much the same, so it would be nice to refactor it into a common initialization.  
xUnit makes use of a [constructor and Dispose](https://xunit.net/docs/shared-context#constructor) to share the context between tests.  
We cannot write a constructor directly in a `.razor` file, but we can [specify a base class](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/?view=aspnetcore-7.0#specify-a-base-class) and write our constructor there.  

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
```

The code of the `PhotoDetailsComponentTests.razor` file should look like this:

```cs
@inherits PhotoDetailsComponentTestsBase

@code {
    [Fact]
    public void PhotoDetailsComponentRendersIdAndTitleInCardHeader() {
        //Act
        var cut = Render(@<PhotoDetailsComponent Photo="photo" />);

        // Assert
        cut.Find("div.mud-card-header-content").MarkupMatches(
            @<div class="mud-card-header-content">
                <p class="mud-typography mud-typography-body1">@photo.Id</p>
                <p class="mud-typography mud-typography-body2">@photo.Title</p>
            </div>
        );
    }

    [Fact]
    public void PhotoDetailsComponentRendersDescriptionCreatedDateAndUserName() {
        // Act
        var cut = RenderComponent<PhotoDetailsComponent>(parameters => parameters
            .Add(p => p.Photo, photo));

        var mudTextFields = cut.FindComponent<MudCardContent>().FindComponents<MudText>();

        // Assert
        mudTextFields.Count.Should().Be(2);
        mudTextFields[0].Find("p").TextContent.Should().Be(photo.Description);
        mudTextFields[1].MarkupMatches(@<h6 diff:ignoreAttributes>Uploaded on @photo.CreatedDate.ToShortDateString() by @photo.UserName</h6>);
    }
}
```

- Run the tests. They should still pass

Let's write a test to make sure that the `Details` button does not get rendered when the `Details` property of the component is set to `false`. 

```cs
[Fact]
public void PhotoDetailsComponentShouldNotRenderDeleteButton_WhenDetailsPropertyNotSet() {
    //Act
    var cut = Render(@<PhotoDetailsComponent Photo=photo/>);

    //Assert
    var buttons = cut.FindComponents<MudIconButton>();
    buttons.FirstOrDefault(b => b.Instance.Icon == Icons.Material.Filled.Delete).Should().BeNull();
}
```

We can add two more tests: 
- One to make sure that the `Details` button *does* get rendered when the `Details` property of the component is set to `true` and the user is authorized
- One to make sure that the `Details` button *does not* get rendered when the `Details` property of the component is set to `true` but the user is unauthorized

They're very similar to the tests we already wrote but we need to setup the our Mock of the authorizationService to return whether the user is authorized to perform the `delete` operation


```cs
[Fact]
public void PhotoDetailsComponentShouldRenderDeleteButton_WhenDeletePropertyIsSetAndUserIsAuthorized() {
    //Arrange
    mockAuthorizationService.Setup(a => a.ItemMayBeDeletedAsync(It.IsAny<ClaimsPrincipal>(), photo)).ReturnsAsync(true);

    //Act
    var cut = Render(@<PhotoDetailsComponent Photo=photo Delete/>);

    //Assert
    var buttons = cut.FindComponents<MudIconButton>();
    buttons.Count(b => b.Instance.Icon == Icons.Material.Filled.Delete).Should().Be(1);
    buttons.Count(b => b.Instance.Href == "photos/delete/1").Should().Be(1);
}

[Fact]
public void PhotoDetailsComponentShouldNotRenderDeleteButton_WhenDeletePropertyIsSetAndUserIsNotAuthorized() {
    //Arrange
    mockAuthorizationService.Setup(a => a.ItemMayBeDeletedAsync(It.IsAny<ClaimsPrincipal>(), photo)).ReturnsAsync(false);

    //Act
    var cut = Render(@<PhotoDetailsComponent Photo=photo Delete />);

    //Assert
    var buttons = cut.FindComponents<MudIconButton>();
    buttons.FirstOrDefault(b => b.Instance.Icon == Icons.Material.Filled.Delete).Should().BeNull();
}
```

All the tests should pass.

Our very last test for this component is to make sure that pressing the `Delete` button raises the `OnDeleteConfirmed` event.  
We need to pass an [Event Callback parameter](https://bunit.egilhansen.com/docs/providing-input/passing-parameters-to-components.html?tabs=razor#eventcallback-parameters) in order to check if it gets invoked.  
We're going to use the technique described in the bUnit docs under the [Triggering a render life cycle on a component](https://bunit.egilhansen.com/docs/interaction/trigger-renders.html#invokeasync) page.

```cs
[Fact]
public async Task DeleteForeverInvokesOnDeleteConfirmed() {
    //Arrange
    mockAuthorizationService.Setup(a => a.ItemMayBeDeletedAsync(It.IsAny<ClaimsPrincipal>(), photo)).ReturnsAsync(true);

    int actualPhotoId = 0;
    Action<int> eventHandler = photoId => actualPhotoId = photoId;
    
    //Act
    var cut = Render(@<PhotoDetailsComponent Photo=photo DeleteConfirm OnDeleteConfirmed="eventHandler" />);

    var deleteButton = cut.FindComponents<MudIconButton>().First(b => b.Instance.Icon == Icons.Material.Filled.DeleteForever).Instance;
    await cut.InvokeAsync(async() => await deleteButton.OnClick.InvokeAsync(null));

    actualPhotoId.Should().Be(photo.Id);
}
```

That's it for our `PhotoDetailsComponent` tests.  

One last technique that we want to learn is how to [fake authentication and authorization](https://bunit.egilhansen.com/docs/test-doubles/faking-auth.html) in order to test the `<AuthorizeView>` component.  
To see this in action, we're going to write a couple of tests for the `CommentsComponent` 

- Add a new `CommentsComponentTestsBase.cs` class
- Let the class inherit from `TestContext`
- Add a constructor
- Initialize and setup a mock for 
  - `ICommentsService`
  - `IUserService`
  - `IAuthorizationService<Comment>`
- Call `AddTestAuthorization()` to add the necessary services to the Services collection and the CascadingAuthenticationState component to the root render tree

Your code should look as follows:

```cs
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
```


- Add a new `CommentsComponentTests.razor` component
- Let the component inherit from `CommentsComponentTestsBase`
- Add a new `CommentsComponentShouldNotRenderCreateCommentWhenUserIsUnauthorized` test
- Setup the an [authenticated and unauthorized](https://bunit.dev/docs/test-doubles/faking-auth.html#authenticated-and-unauthorized-state) state
- Render the `<CommentsComponent>` component
- Find all the `CommentComponent` components whose `Instance.ViewMode` is equal to `CommentComponent.ViewModes.Create`
- Assert that there is no component whose `ViewMode` is set to `Create`

Your code should look like this:

```cs
@using System.Security.Claims;
@inherits CommentsComponentTestsBase

@code {
    [Fact]
    public void CommentsComponentShouldNotRenderCreateCommentWhenUserIsUnauthorized() {
        //Arrange
        authContext.SetAuthorized("TEST USER", AuthorizationState.Unauthorized);

        //Act
        var cut = Render(@<CommentsComponent PhotoId=photoId />);

        //Assert
        //find the create comment button
        IRenderedComponent<CommentComponent>? createCommentButton = cut.FindComponents<CommentComponent>().FirstOrDefault(c => c.Instance.ViewMode == CommentComponent.ViewModes.Create);
        createCommentButton.Should().BeNull();
    }
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
public void CommentsComponentShouldRenderCreateCommentWhenUserIsAuthorized() {
    //Arrange
    authContext.SetAuthorized("TEST USER", AuthorizationState.Authorized);

    //Act
    var cut = Render(@<CommentsComponent PhotoId=photoId/>);

    //Assert
    //find the create comment button
    IRenderedComponent<CommentComponent>? createCommentButton = cut.FindComponents<CommentComponent>().FirstOrDefault(c => c.Instance.ViewMode == CommentComponent.ViewModes.Create);
    createCommentButton.Should().NotBeNull();
}
```

And we're done!  
Of course there are many other tests we could write, but I'm not going to describe them all since they are a repetition of what we have already seen here, so feel free to write them yourself if you feel like it.

---

In the next lab, we're going to explore the interoperability between Blazor and javascript.