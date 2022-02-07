# Validation

In this lab we're going to talk about validating our data.  

Our goal is to 
- Prevent having the wrong data in our database
- Reject invalid data
- Show error messages explaining the rules, so that the user can correct the mistakes

.NET core has some builtin ways to validate data, for example [Data Annotations](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations?view=net-6.0) are a set of attributes used behind the scene by both [Asp.net Core](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-6.0) and [Entity Framework](https://docs.microsoft.com/en-us/ef/ef6/modeling/code-first/data-annotations).

We could use those, but there are a couple of considerations we have to make first.  

The Data Annotations are used by Entity Framework and by ASP.NET Core (Blazor included) so right now they would work, but what if we ever want to change the infrastructure and not rely on those two? We could decide to save the pictures in an Azure Blob storage, or Redis Cache or any other technology that comes to mind. In those cases Data Annotations wouldn't be appropriate anymore.  
If we decided to use Data Annotations, we would not be ready for change.  
Also, maybe the most important thing: attributes are hard to unit test, while code that validates can be unit tested.  

We are using a CLEAN architecture, which means that our *Core logic* resides in a *service* that is abstracted from the actual infrastructure. 
It should be the *Service* to decide if, where and when to validate.  
Once again, just like it happened for the Authorization part, we want the *service* to ask questions, but it should not know the answers. Whether a Photo is valid or not should be decided by someone else. Some sort of *validator* that can check some *rules* defined elsewhere.

To help us with validation, we can use [Fluent Validations](https://docs.fluentvalidation.net/en/latest/index.html).  
With this package, we will be able to define *validators* containing *rules* for our entities.  
The *service* will then just ask for a validator using *Dependency Injection* and when it's time (during *create* and *update*), it will demand that the validator validates and eventually throws a `ValidationException`. It will be as simple as that.  

Our *Core* layer will be pretty simple: we will just need to ask the validators to check for validation errors.  
But that's not all.  
We can also use Fluent Validation at an *Application* level: 
- Server side - our ASP.NET Core REST Service and gRpc Service
- Client Side - our Blazor Web Assembly application

FluentValidations are in fact also supported by
- ASP.NET Core, by including the [FluentValidation.AspNetCore](https://docs.fluentvalidation.net/en/latest/aspnet.html#asp-net-core)  Package
- Blazor, by including the [Blazored FluentValidation](https://github.com/Blazored/FluentValidation) Package

So, as a recap.

1. We're going to define *validators* and *rules* for each of our *entities*
2. We're going to check those rules in the *core* layer
3. We're going to check those rules in the *application* layer

Let's start.

## Validators and Rules

- Add the `FluentValidation` NuGet Package to the `PhotoSharingApplication.Shared` project.  
- Add a new folder `Validators`
- In the `Validators` folder, add a new `PhotoValidator` class
- As explained in the [documentation](https://docs.fluentvalidation.net/en/latest/start.html#creating-your-first-validator), let the class derive from `AbstractValidator<Photo>`
- In the constructor, define rules by using the [builtin validators](https://docs.fluentvalidation.net/en/latest/built-in-validators.html)
    - Ensure that the `Title` is [not empty](https://docs.fluentvalidation.net/en/latest/built-in-validators.html#notempty-validator)
    - Ensure that the `Title` has a [maximum length](https://docs.fluentvalidation.net/en/latest/built-in-validators.html#maxlength-validator) of 255 characters
    - Ensure that the `PhotoImage` [complex property](https://docs.fluentvalidation.net/en/latest/start.html#complex-properties) is also validated

The code should look like this:

```cs
using FluentValidation;
using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.Shared.Validators;

public class PhotoValidator : AbstractValidator<Photo> {
    public PhotoValidator() {
        RuleFor(photo => photo.Title).NotEmpty();
        RuleFor(photo => photo.Title).MaximumLength(255);

        RuleFor(photo => photo.PhotoImage).SetValidator(new PhotoImageValidator());
    }
}
```

- In the `Validators` folder, add a new `PhotoImageValidator` class
- Let the class derive from `AbstractValidator<PhotoImage>`
- In the constructor, define rules by using the [builtin validators](https://docs.fluentvalidation.net/en/latest/built-in-validators.html)
    - Ensure that the `ImageMimeType` is [not empty](https://docs.fluentvalidation.net/en/latest/built-in-validators.html#notempty-validator)
    - Ensure that the `ImageMimeType` has a [maximum length](https://docs.fluentvalidation.net/en/latest/built-in-validators.html#maxlength-validator) of 255 characters
    - Ensure that the `PhotoImage`  is [not empty](https://docs.fluentvalidation.net/en/latest/built-in-validators.html#notempty-validator)

The code should look like this:

```cs
using FluentValidation;
using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.Shared.Validators;

public class PhotoImageValidator : AbstractValidator<PhotoImage> {
    public PhotoImageValidator() {
        RuleFor(photoImage => photoImage.ImageMimeType).NotEmpty();
        RuleFor(photoImage => photoImage.ImageMimeType).MaximumLength(255);

        RuleFor(photoImage => photoImage.PhotoFile).NotEmpty();
    }
}
```

- In the `Validators` folder, add a new `CommentValidator` class
- Let the class derive from `AbstractValidator<Comment>`
- In the constructor, define rules by using the [builtin validators](https://docs.fluentvalidation.net/en/latest/built-in-validators.html)
    - Ensure that the `Subject` is [not empty](https://docs.fluentvalidation.net/en/latest/built-in-validators.html#notempty-validator)
    - Ensure that the `Subject` has a [maximum length](https://docs.fluentvalidation.net/en/latest/built-in-validators.html#maxlength-validator) of 250 characters
    - Ensure that the `Body` is [not empty](https://docs.fluentvalidation.net/en/latest/built-in-validators.html#notempty-validator)

The code should look like this:

```cs
using FluentValidation;
using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.Shared.Validators;

public class CommentValidator : AbstractValidator<Comment> {
    public CommentValidator() {
        RuleFor(comment => comment.Subject).NotEmpty();
        RuleFor(comment => comment.Subject).MaximumLength(250);

        RuleFor(comment => comment.Body).NotEmpty();
    }
}
```

## Photos Rest API

### PhotosService

- Add the `FluentValidation.AspNetCore` NuGet Package to the `PhotoSharingApplication.WebServices.Rest.Photos` project.  
- Open the `PhotosService` class located under the `Core/Services` folder of the `PhotoSharingApplication.WebServices.Rest.Photos` project
- Use Dependency Injection from its constructor to ask for an `IValidator<Photo>` and save it into a private readonly variable
- In the `UpdateAsync` and `UploadAsync`, invoke the [ValidateAndThrow](https://docs.fluentvalidation.net/en/latest/start.html#throwing-exceptions) method before submitting the photo to the repository

The code should look like this:

```cs
using FluentValidation;
//(...code omitted for brevity...)

namespace PhotoSharingApplication.Backend.Core.Services;
public class PhotosService : IPhotosService {

    //(...code omitted for brevity...)
    
    private readonly IValidator<Photo> validator;

    public PhotosService(IPhotosRepository repository, IAuthorizationService<Photo> photosAuthorizationService, IUserService userService, IValidator<Photo> validator) =>
    (this.repository, this.photosAuthorizationService, this.userService, this.validator) = (repository, photosAuthorizationService, userService, validator);
    
    //(...code omitted for brevity...)
    
    public async Task<Photo?> UpdateAsync(Photo photo) {
        var user = await userService.GetUserAsync();
        if (await photosAuthorizationService.ItemMayBeUpdatedAsync(user, photo)) {
            validator.ValidateAndThrow(photo);
            return await repository.UpdateAsync(photo);
        }
        else throw new EditUnauthorizedException<Photo>($"Unauthorized Edit Attempt of Photo {photo.Id}");
    }

    public async Task<Photo?> UploadAsync(Photo photo) {
        var user = await userService.GetUserAsync();
        if (await photosAuthorizationService.ItemMayBeCreatedAsync(user, photo)) {
            photo.CreatedDate = DateTime.Now;
            photo.UserName = user?.Identity?.Name;
            validator.ValidateAndThrow(photo);
            return await repository.CreateAsync(photo);
        } else throw new CreateUnauthorizedException<Photo>($"Unauthorized Create Attempt of Photo {photo.Id}");
    }
}
```

In order to try if this works, we need to register the `IValidator<Photo>` in the DI Containers.

As explained in the [documentation](https://docs.fluentvalidation.net/en/latest/aspnet.html):

- Open the `Program.cs` class
- Invoke the `AddFluentValidation` extension method
- Register the `PhotoValidator` with the services collection by calling the `AddScoped` method

The code should look like this:

```cs
builder.Services.AddFluentValidation();
builder.Services.AddScoped<IValidator<Photo>, PhotoValidator>();
```

which requires:

```cs
using FluentValidation;
using FluentValidation.AspNetCore;
using PhotoSharingApplication.Shared.Validators;
```

- Try to upload a photo without a title.
- Inspect the network traffic by pressing F12 on your browser and going to the `Network` tab    
You'll see that the server is returning a 400 (Bad Request) containing the list of all the errors found by the validation. The Bad Request comes from the fact that Validation is performed by ASP.NET Core even before our action is invoked, which means that our *core service* has not even been bothered at all.  

### CommentsService

- Add the `FluentValidation.AspNetCore` NuGet Package to the `PhotoSharingApplication.WebServices.Grpc.Comments` project.  
- Open the `CommentsService` class located under the `Core/Services` folder of the `PhotoSharingApplication.WebServices.Grpc.Comments` project
- Use Dependency Injection from its constructor to ask for an `IValidator<Comment>` and save it into a private readonly variable
- In the `UpdateAsync` and `CreateAsync`, invoke the [ValidateAndThrow](https://docs.fluentvalidation.net/en/latest/start.html#throwing-exceptions) method before submitting the photo to the repository

The code should look like this:

```cs
using FluentValidation;
//(...code omitted for brevity...)

namespace PhotoSharingApplication.Backend.Core.Services ;

public class CommentsService : ICommentsService {
    //(...code omitted for brevity...)
    private readonly IValidator<Comment> validator;

    public CommentsService(ICommentsRepository repository, IAuthorizationService<Comment> commentsAuthorizationService, IUserService userService, IValidator<Comment> validator) =>
    (this.repository, this.commentsAuthorizationService, this.userService, this.validator) = (repository, commentsAuthorizationService, userService, validator);
    //(...code omitted for brevity...)
    
    public async Task<Comment?> CreateAsync(Comment comment) {
        var user = await userService.GetUserAsync();
        if (await commentsAuthorizationService.ItemMayBeCreatedAsync(user, comment)) {
            comment.SubmittedOn = DateTime.Now;
            comment.UserName = user.Identity?.Name ?? "";
            validator.ValidateAndThrow(comment);
            return await repository.CreateAsync(comment);
        } else throw new CreateUnauthorizedException<Comment>($"Unauthorized Create Attempt of Comment {comment.Id}");
    }

    public async Task<Comment?> UpdateAsync(Comment comment) {
        var user = await userService.GetUserAsync();
        Comment? oldComment = await repository.FindAsync(comment.Id);
        if (oldComment is not null) {
            if (!await commentsAuthorizationService.ItemMayBeUpdatedAsync(user, oldComment))
                throw new EditUnauthorizedException<Comment>($"Unauthorized Edit Attempt of Comment {comment.Id}");
            oldComment.Subject = comment.Subject;
            oldComment.Body = comment.Body;
            oldComment.SubmittedOn = DateTime.Now;
            validator.ValidateAndThrow(comment);
            oldComment = await repository.UpdateAsync(oldComment);
        }
        return oldComment;
    }
}

```

- In the `Program.cs` class add the following code:
```cs
builder.Services.AddFluentValidation();
builder.Services.AddScoped<IValidator<Comment>, CommentValidator>();
```
which requires
```cs
using FluentValidation;
using FluentValidation.AspNetCore;
using PhotoSharingApplication.Shared.Validators;
```

## Comments gRPC Service

[Anthoy Giretty](https://anthonygiretti.com/) is working to include `Fluent Validation` in gRpc.Net. Unfortunately at the moment of this writing (February 2022), his package works with .NET 3.1 but not (yet?) with .Net 5.0 nor 6.0.
So, go check his [Repo](https://github.com/AnthonyGiretti/grpc-aspnetcore-validator/tree/master) to see if he updated it to .NET 6. **IF HE DID**, you can follow his [documentation](https://github.com/AnthonyGiretti/grpc-aspnetcore-validator/tree/master#server-side-usage)

We will have to use `FluentValidation` ourselves, but since this one throws a `ValidationException` while we need to throw an `RpcException`, we will translate it back and forth so that our Services don't need to know that gRpc is involved but we can still communicate back and forth using gRpc.

So the flow will be:
1. The gRpcService passes the data to the Service 
2. The Service validates the data but throws a `ValidationException`
3. The gRpcService transforms the `ValidationException` into an `RpcException` and sends it to the Client, adding the ValidationError in the [`trailers`](https://docs.microsoft.com/en-us/aspnet/core/grpc/client?view=aspnetcore-6.0#access-grpc-trailers)
4. The Client Repository receives the `RpcException` and transforms it back to a `ValidationException`

We have already taken care of the service, now let's implement the gRpc service.

- Open the `CommentsGrpcService` class located under the `Services` folder of the `PhotoSharingApplication.WebServices.Grpc.Comments` project
- Locate both the `Create` and `Update` methods and add a new `catch` clause to handle a `ValidationException` and throw an `RpcException` using an extension method that we will implement later

```cs
catch (ValidationException ex) {
    throw ex.ToRpcException();
}
```

- Add a new folder `Validation` to the `PhotoSharingApplication.WebServices.Grpc.Comments` project
- In the `Validation` folder, add a new `ValidationExtensions` static class
- In the `ValidationExtensions` class, add a new `ToRpcException` extension method that takes a `ValidationException` and returns an `RpcException`
- In the `ToRpcException` extension method, add the following code:

```cs
using Grpc.Core;
using PhotoSharingApplication.Shared.Validators;
using System.Text.Json;
using FluentValidation;

namespace PhotoSharingApplication.WebServices.Grpc.Comments.Validation;

public static class ValidationExtensions {
    public static RpcException ToRpcException(this ValidationException ex) {
        var metadata = new Metadata();
        List<ValidationTrailer> trailers = ex.Errors.Select(x => new ValidationTrailer {
            PropertyName = x.PropertyName,
            AttemptedValue = x.AttemptedValue?.ToString(),
            ErrorMessage = x.ErrorMessage
        }).ToList();
        string json = JsonSerializer.Serialize(trailers);
        metadata.Add(new Metadata.Entry("validation-errors-text", json));
        return new RpcException(new Status(StatusCode.InvalidArgument, "Validation Failed"), metadata);
    }
}
```

`ValidationTrailer` is a class that we need to serialize and deserialize the errors as trailers in the gRpc response. Let's add it to the `Validators` folder of the `PhotoSharingApplication.Shared.Validators` project:

```cs
namespace PhotoSharingApplication.Shared.Validators;

[Serializable]
public class ValidationTrailer {
    public string PropertyName { get; set; }

    public string ErrorMessage { get; set; }

    public string AttemptedValue { get; set; }
}
```

Now let's think about the client repository.

- Add a new `Validation` folder to the `PhotoSharingApplication.Frontend.Client` project
- In the `Validation` folder, add a new `ValidationExtensions` static class
- In the `ValidationExtensions` class, add a new `ToValidationException` extension method that takes a `RpcException` and returns a `ValidationException`

```cs
using FluentValidation;
using FluentValidation.Results;
using Grpc.Core;
using PhotoSharingApplication.Shared.Validators;
using System.Text.Json;

namespace PhotoSharingApplication.Frontend.Client.Validation;

public static class ValidationExtensions {
    public static ValidationException ToValidationException(this RpcException ex) {
        var validationTrailer = ex.Trailers.FirstOrDefault(x => x.Key == "validation-errors-text");

        var trailers = JsonSerializer.Deserialize<List<ValidationTrailer>>(validationTrailer.Value);
        List<ValidationFailure> validationFailures = trailers.Select(t => new ValidationFailure(t.PropertyName, t.ErrorMessage, t.AttemptedValue)).ToList();
        throw new FluentValidation.ValidationException(validationFailures);
    }
}
```

- Open the `CommentsRepository` class under the `Infrastructure/Repositories/Grpc` folder of the `PhotoSharingApplication.Frontend.Client` project
- Add a new `catch` clause in both the `Create` and `Update` methods that handles a `RpcException` and transforms it back to a `ValidationException`

```cs
catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument) {
    throw ex.ToValidationException();
} 
```

which requires

```cs
using PhotoSharingApplication.Frontend.Client.Validation;
```

Try running the application submitting a comment without any text. If you inspect the Network tab in the browser, you will see that the Response contains the following headers:

```
grpc-message: Validation Failed    
grpc-status: 3  
validation-errors-text: [{"PropertyName":"Subject","ErrorMessage":"\u0027Subject\u0027 must not be empty.","AttemptedValue":""},{"PropertyName":"Body","ErrorMessage":"\u0027Body\u0027 must not be empty.","AttemptedValue":""}]  
```

Great, now we can never get any bad data on our server.    
The validation server side is complete, but if we leave the application like this, the user will have to wait for the data to reach the server and the error to come back to the client before it can see the error message. We can enhance the user experience by adding the validation even client side and the nice thing is that we can reuse the same validators we used server side.

So let's add the validation to the UI Client Side.

## The Client Side

First, we need to register the `IValidator<Photo>` and `IValidator<Comment>` in the Blazor DI Container.

- Add the `FluentValidation` NuGet Package to the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project
- In the `Program` class, locate the `Main` method
- Register the `PhotoValidator` with the services collection by calling the `AddScoped` method
- Register the `CommentValidator` with the services collection by calling the `AddScoped` method

```cs
builder.Services.AddScoped<IValidator<Photo>, PhotoValidator>();
builder.Services.AddScoped<IValidator<Comment>, CommentValidator>();
```

which requires

```cs
using FluentValidation;
using PhotoSharingApplication.Shared.Validators;
```

Now let's show the error messages to the user.

## Show Validation Errors in Blazor Components

- Add the `Blazored.FluentValidation` NuGet Package to the `PhotoSharingApplication.Frontend.BlazorComponents` project
- As explained in the [documentation](https://github.com/Blazored/FluentValidation#basic-usage), add the following using statement to your root `_Imports.razor`

```cs
@using Blazored.FluentValidation
```

### PhotoEditComponent.razor

- Open the `PhotoEditComponent.razor` file 
- Add a `<FluentValidationValidator />` component right under the `<EditForm>` Tag
- Add a `<ValidationMessage For="@(() => Photo.Title)" />` tag under the `MatTextField` for the `Photo.Title`
- Add a `<ValidationMessage For="@(() => Photo.PhotoImage.PhotoFile)" />` tag under the `MatFileUpload` for the `Photo.PhotoImage.PhotoFile`
- Make sure that the `PhotoImage` property is not null when the component is initialized

The template should look like this

```html
<MatCard>
    <MatH3>Upload Photo</MatH3>
    <MatCardContent>
        <EditForm Model="@Photo" OnValidSubmit="@(async ()=> await OnSave.InvokeAsync(Photo))">
            <FluentValidationValidator />
            <p>
                <MatTextField @bind-Value="@Photo.Title" Label="Title" FullWidth></MatTextField>
                <ValidationMessage For="@(() => Photo.Title)" />
            </p>
            <p>
                <MatTextField @bind-Value="@Photo.Description" Label="Description" TextArea FullWidth></MatTextField>
            </p>
            <p>
                <MatFileUpload OnChange="@HandleMatFileSelected"></MatFileUpload>
                <ValidationMessage For="@(() => Photo.PhotoImage.PhotoFile)" />
            </p>
            <p>
                <MatButton Type="submit">Upload</MatButton>
            </p>
        </EditForm>
        <PhotoPictureComponent Photo="Photo" IsLocal="true"></PhotoPictureComponent>
    </MatCardContent>
</MatCard>
```

In the `code` section, you should add

```cs
protected override void OnInitialized() {
    if (Photo.PhotoImage is null) Photo.PhotoImage = new PhotoImage(); 
}
```

### CommentCreateComponent.razor

- Open the `CommentCreateComponent.razor` file 
- Add a `<FluentValidationValidator />` component right under the `<EditForm>` Tag
- Add a `<ValidationMessage For="@(() => CommentItem.Subject)" />` tag under the `MatTextField` for the `CommentItem.Subject`
- Add a `<ValidationMessage For="@(() => CommentItem.Body)" />` tag under the `MatTextField` for the `CommentItem.Body`

The template should look like this (the `code` section does not change)

```html
<MatCardContent>
    <EditForm Model="@CommentItem" OnValidSubmit="HandleValidSubmit">
        <FluentValidationValidator />
        <p>
            <MatTextField @bind-Value="@CommentItem.Subject" Label="Subject" FullWidth></MatTextField>
            <ValidationMessage For="@(() => CommentItem.Subject)" />
        </p>
        <p>
            <MatTextField @bind-Value="@CommentItem.Body" Label="Description" TextArea FullWidth></MatTextField>
            <ValidationMessage For="@(() => CommentItem.Body)" />
        </p>
        <p>
            <MatButton Type="submit">Submit</MatButton>
        </p>
    </EditForm>
</MatCardContent>
```

### CommentEditComponent.razor

- Open the `CommentEditComponent.razor` file 
- Add a `<FluentValidationValidator />` component right under the `<EditForm>` Tag
- Add a `<ValidationMessage For="@(() => CommentItem.Subject)" />` tag under the `MatTextField` for the `CommentItem.Subject`
- Add a `<ValidationMessage For="@(() => CommentItem.Body)" />` tag under the `MatTextField` for the `CommentItem.Body`

The template should look like this (the `code` section does not change)

```html
<MatCardContent>
    <EditForm Model="@CommentItem" OnValidSubmit="HandleValidSubmit">
        <FluentValidationValidator />
        <p>
            <MatTextField @bind-Value="@CommentItem.Subject" Label="Subject" FullWidth></MatTextField>
            <ValidationMessage For="@(() => CommentItem.Subject)" />
        </p>
        <p>
            <MatTextField @bind-Value="@CommentItem.Body" Label="Description" TextArea FullWidth></MatTextField>
            <ValidationMessage For="@(() => CommentItem.Body)" />
        </p>
        <p>
            <MatButton Type="submit">Submit</MatButton>
        </p>
    </EditForm>
</MatCardContent>
```

- Run the application
- Try to upload a Photo without a title  
You'll see an error message stating that the Title field cannot be empty.
- Try to write a Title longer than 255 characters
You'll see an error message stating that the Title field cannot be longer than 255 characters.
- Try to upload a Photo without a Picture  
You'll see an error message stating that the `Photo File` field cannot be empty.
- Try to submit a Comment without a title  
You'll see an error message stating that the Title field cannot be empty.
- Try to write a Title longer than 255 characters
You'll see an error message stating that the Title field cannot be longer than 255 characters.

Also, there is no network traffic involved.

And we're done!

Our business logic validates our entities, no matter which application and infrastructure is used, rejecting invalid data.  
Our Application shows friendly error messages, by using the exact same validation logic that the server uses.  

---

In the next lab, we're going to explore Unit Testing of Blazor Components.