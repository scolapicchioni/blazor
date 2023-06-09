# Validation

In this lab we're going to talk about validating our data.  

Our goal is to 
- Prevent having the wrong data in our database
- Reject invalid data
- Show error messages explaining the rules, so that the user can correct the mistakes

.NET core has some builtin ways to validate data, for example [Data Annotations](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations?view=net-7.0) are a set of attributes used behind the scene by both [Asp.net Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-7.0) and [Entity Framework](https://learn.microsoft.com/en-us/ef/core/modeling/#use-data-annotations-to-configure-a-model).

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
- Blazor, by including the [MudBlazor](https://mudblazor.com/components/form#using-fluent-validation)

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
    - Ensure that the `Title` has a [maximum length](https://docs.fluentvalidation.net/en/latest/built-in-validators.html#maxlength-validator) of 100 characters
    - Ensure that the `Description` has a [maximum length](https://docs.fluentvalidation.net/en/latest/built-in-validators.html#maxlength-validator) of 255 characters
    - Ensure that the `PhotoImage` [complex property](https://docs.fluentvalidation.net/en/latest/start.html#complex-properties) is also validated

The code should look like this:

```cs
using FluentValidation;
using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.Shared.Validators;

public class PhotoValidator : AbstractValidator<Photo> {
    public PhotoValidator() {
        RuleFor(photo => photo.Title).NotEmpty();
        RuleFor(photo => photo.Title).MaximumLength(100);

        RuleFor(photo => photo.Description).MaximumLength(255);
        
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
    - Ensure that the `Subject` has a [maximum length](https://docs.fluentvalidation.net/en/latest/built-in-validators.html#maxlength-validator) of 100 characters
    - Ensure that the `Body` is [not empty](https://docs.fluentvalidation.net/en/latest/built-in-validators.html#notempty-validator)
    - Ensure that the `Body` has a [maximum length](https://docs.fluentvalidation.net/en/latest/built-in-validators.html#maxlength-validator) of 250 characters

The code should look like this:

```cs
using FluentValidation;
using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.Shared.Validators;

public class CommentValidator : AbstractValidator<Comment> {
    public CommentValidator() {
        RuleFor(comment => comment.Subject).NotEmpty();
        RuleFor(comment => comment.Subject).MaximumLength(100);

        RuleFor(comment => comment.Body).NotEmpty();
        RuleFor(comment => comment.Body).MaximumLength(250);
    }
}
```

## Blazor

In order for a `MudBlazor Form` to use `FluentValidation`, we need to add a new method to each validator. This method will be invoked by the form to display each error next to the field where it belongs.

- In the `Validators` folder, open the `PhotoValidator` class
- Add the following function

```cs
public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) => {
    var result = await ValidateAsync(ValidationContext<Photo>.CreateWithOptions((Photo)model, x => x.IncludeProperties(propertyName)));
    if (result.IsValid)
        return Array.Empty<string>();
    return result.Errors.Select(e => e.ErrorMessage);
};
```

- In the `Validators` folder, open the `PhotoImageValidator` class
- Add the following function

```cs
public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) => {
    var result = await ValidateAsync(ValidationContext<PhotoImage>.CreateWithOptions((PhotoImage)model, x => x.IncludeProperties(propertyName)));
    if (result.IsValid)
        return Array.Empty<string>();
    return result.Errors.Select(e => e.ErrorMessage);
};
```

- In the `Validators` folder, open the `PhotoImageValidator` class
- Add the following function

```cs
public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) => {
    var result = await ValidateAsync(ValidationContext<Comment>.CreateWithOptions((Comment)model, x => x.IncludeProperties(propertyName)));
    if (result.IsValid)
        return Array.Empty<string>();
    return result.Errors.Select(e => e.ErrorMessage);
};
```

Now open the `PhotoEditComponent.razor` component under the `Components` folder of the `PhotoSharingApplication.Frontend.BlazorComponents` project.  
- in the `@code` section, add:
  - a `private` variable of type `MudForm` called `form`
  - a `private` variable of type `PhotoValidator` called `photoValidator`, set to a new instance of a `PhotoValidator`
  - a method `async Task ValidateAndSubmit` which:
    - invokes `form.Validate()`
    - checks if `form.isValid` and if so, invokes the `OnSave` `EventCallback`
The code becomes:

```cs
@code {
    [Parameter, EditorRequired]
    public Photo Photo { get; set; } = default!;

    [Parameter]
    public EventCallback<Photo> OnSave { get; set; }

    private MudForm form;
    private readonly PhotoValidator photoValidator = new PhotoValidator();

    private async Task HandleFileSelected(IBrowserFile args) {
        if (Photo.PhotoImage is null) Photo.PhotoImage = new PhotoImage();
        Photo.PhotoImage.ImageMimeType = args.ContentType;

        using (var streamReader = new System.IO.MemoryStream()) {
            await args.OpenReadStream().CopyToAsync(streamReader);
            Photo.PhotoImage.PhotoFile = streamReader.ToArray();
        }
    }

    private async Task ValidateAndSubmit() {
        await form.Validate();
        if(form.IsValid) {
            await OnSave.InvokeAsync(Photo);
        }
    }
}
```

Then, we need to change the `MudForm` to reference the `form` variable we declared and to invoke the `photoValidator.ValidateValue` upon `Validation`:

```html
<MudForm Model="Photo" @ref="@form" Validation="@(photoValidator.ValidateValue)">
```

Lastly, we need to invoke our `ValidateAndSubmit` method when our `MudIconButton` at the bottom of our form is clicked:

```html
<MudIconButton Color="Color.Primary" Icon="@Icons.Material.Filled.FileUpload" OnClick="ValidateAndSubmit">Upload</MudIconButton>
```

If you try to add a photo without a title or with fields that are too long, you should see an error message and the form should not be submitted.

Let's repeat this for the `CommentEditComponent.razor`

Now open the `CommentEditComponent.razor` component under the `Components` folder of the `PhotoSharingApplication.Frontend.BlazorComponents` project.  
- in the `@code` section, add:
  - a `private` variable of type `MudForm` called `form`
  - a `private` variable of type `CommentValidator` called `commentValidator`, set to a new instance of a `CommentValidator`
  - a method `async Task ValidateAndSubmit` which:
    - invokes `form.Validate()`
    - checks if `form.isValid` and if so, invokes the `OnSave` `EventCallback`
The code becomes:

```cs
@code {
    [Parameter]
    public Comment CommentItem { get; set; } = default!;

    [Parameter]
    public EventCallback<Comment> OnCancel { get; set; }

    [Parameter]
    public EventCallback<Comment> OnSave { get; set; }

    private Comment originalComment = default!;

    private MudForm form;
    private readonly CommentValidator commentValidator = new CommentValidator();

    protected override void OnInitialized() {
        originalComment = new Comment { Id = CommentItem.Id, PhotoId = CommentItem.PhotoId, Subject = CommentItem.Subject, Body = CommentItem.Body, SubmittedOn = CommentItem.SubmittedOn, UserName = CommentItem.UserName };
    }

    private async Task ValidateAndSubmit() {
        await form.Validate();
        if (form.IsValid) {
            await OnSave.InvokeAsync(originalComment);
        }
    }
}
```

Then, we need to change the `MudForm` to reference the `form` variable we declared and to invoke the `commentValidator.ValidateValue` upon `Validation`:

```html
<MudForm Model="originalComment" @ref="@form" Validation="@(commentValidator.ValidateValue)">
```

Lastly, we need to invoke our `ValidateAndSubmit` method when our `MudIconButton` at the bottom of our form is clicked:

```html
<MudIconButton Color="Color.Primary" Icon="@Icons.Material.Filled.Check" OnClick="ValidateAndSubmit">Update</MudIconButton>
```

If you try to modify a comment without a title or with fields that are too long, you should see an error message and the form should not be submitted.

Let's repeat this for the `CommentCreateComponent.razor`

Now open the `CommentEditComponent.razor` component under the `Components` folder of the `PhotoSharingApplication.Frontend.BlazorComponents` project.  
- in the `@code` section, add:
  - a `private` variable of type `MudForm` called `form`
  - a `private` variable of type `CommentValidator` called `commentValidator`, set to a new instance of a `CommentValidator`
- Modify the `RaiseCreate` to:
    - invoke `form.Validate()`
    - checks if `form.isValid` and if so, invoke the `OnSave` `EventCallback` and reset the `originalComment`
The code becomes:

```cs
@code {
    [Parameter]
    public Comment CommentItem { get; set; } = default!;

    [Parameter]
    public EventCallback<Comment> OnCancel { get; set; }

    [Parameter]
    public EventCallback<Comment> OnSave { get; set; }

    private Comment originalComment = default!;

    protected override void OnInitialized() {
        originalComment = new Comment { Id = CommentItem.Id, PhotoId = CommentItem.PhotoId, Subject = CommentItem.Subject, Body = CommentItem.Body, SubmittedOn = CommentItem.SubmittedOn, UserName = CommentItem.UserName };
    }

    async Task RaiseCreate(MouseEventArgs args) {
        await form.Validate();
        if (form.IsValid)
        {
            await OnSave.InvokeAsync(originalComment);
            originalComment = new Comment { Id = CommentItem.Id, PhotoId = CommentItem.PhotoId, Subject = CommentItem.Subject, Body = CommentItem.Body, SubmittedOn = CommentItem.SubmittedOn, UserName = CommentItem.UserName };
        }
    }

    private MudForm form;
    private readonly CommentValidator commentValidator = new CommentValidator();
}
```

Then, we need to change the `MudForm` to reference the `form` variable we declared and to invoke the `commentValidator.ValidateValue` upon `Validation`:

```html
<MudForm Model="originalComment" @ref="@form" Validation="@(commentValidator.ValidateValue)">
```
If you try add a comment without a title or with fields that are too long, you should see an error message and the form should not be submitted.

## Photos Rest API

So far, we prevented inserting incorrect data into the form client side. This is a nice experience for the user, but if a malicious user would use a tool such as PostMan, the data would still be accepted since it is not validated server side.
Let's fix that.

### PhotosService

- Add the `FluentValidation.AspNetCore` NuGet Package to the `PhotoSharingApplication.WebServices.Rest.Photos` project.  
- Open the `PhotosService` class located under the `Core/Services` folder of the `PhotoSharingApplication.WebServices.Rest.Photos` project
- Use Dependency Injection from its constructor to ask for an `IValidator<Photo>` and save it into a private readonly variable
- In the `UpdateAsync` and `UploadAsync`, invoke the [ValidateAndThrow](https://docs.fluentvalidation.net/en/latest/start.html#throwing-exceptions) method before submitting the photo to the repository

The code should look like this:

```cs
using FluentValidation;
//(...code omitted for brevity...)

namespace PhotoSharingApplication.WebServices.REST.Photos.Services;
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

Try to upload a photo without a title, by using PostMan (you can just comment out the `[Authorize] attribute in order to be able to invoke the action without having to add an authentication token).  
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

[Anthoy Giretty](https://anthonygiretti.com/) created a very nice package to automatically include `Fluent Validation` in gRpc.Net.  
His [Repo](https://github.com/AnthonyGiretti/grpc-aspnetcore-validator/tree/master) has been recently updated so we can follow his [documentation](https://github.com/AnthonyGiretti/grpc-aspnetcore-validator/tree/master#server-side-usage)

### Server side

- Add the `Calzolari.Grpc.AspNetCore.Validation` NuGet package to the `PhotoSharingApplication.WebServices.Grpc.Comments` project.  

We do have a validator for a `Comment`, but remember that in gRpc what we get from the client is not a `Comment` but a `CreateRequest` and an `UpdateRequest`. We need to validate those. Fortunately we can use the same `Rules` we established for the `Comment`.

- Add a `Validators` folder to the `PhotoSharingApplication.WebServices.Grpc.Comments` project.
- Under that folder, add a `CreateRequestValidator`, let it derive from `AbstractValidator<CreateRequest>` and create the same rules we had written for the `CommentValidator`. The code becomes:

```cs
using FluentValidation;

namespace PhotoSharingApplication.WebServices.Grpc.Comments.Validators;
public class CreateRequestValidator : AbstractValidator<CreateRequest> {
    public CreateRequestValidator() {
        RuleFor(comment => comment.Subject).NotEmpty();
        RuleFor(comment => comment.Subject).MaximumLength(100);

        RuleFor(comment => comment.Body).NotEmpty();
        RuleFor(comment => comment.Body).MaximumLength(250);
    }
}
```

- Create an `UpdateRequestValidator` using the same rules

```cs
using FluentValidation;

namespace PhotoSharingApplication.WebServices.Grpc.Comments.Validators; 
public class UpdateRequestValidator : AbstractValidator<UpdateRequest> {
    public UpdateRequestValidator() {
        RuleFor(updateRequest => updateRequest.Subject).NotEmpty();
        RuleFor(updateRequest => updateRequest.Subject).MaximumLength(100);

        RuleFor(updateRequest => updateRequest.Body).NotEmpty();
        RuleFor(updateRequest => updateRequest.Body).MaximumLength(250);
    }
}
```

- Open `Program.cs` and replace

```cs
builder.Services.AddGrpc();
```

with

```cs
builder.Services.AddGrpc(options => options.EnableMessageValidation());
```

Then add

```cs
builder.Services.AddValidator<CreateRequestValidator>();
builder.Services.AddValidator<UpdateRequestValidator>();
builder.Services.AddGrpcValidation();
```

Great, now we can never get any bad data on our server.    
The validation server side is complete.  
Considering that the rules are the same on the client and on the server, in theory the client should never get the errors from the server, but let's prepare our client to handle and show the exceptions anyway, so that if ever someone updates the rules on the gRpc side without updating the client, we can still fail gracefully.

### Client Side

- Add the `Calzolari.Grpc.Net.Client.Validation` NuGetPackage to the `PhotoSharingApplication.Frontend.Client` project
- Open the `PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Grpc.CommentsRepository` class
- Add a catch block in both the `CreateAsync` and `UpdateAsync` methods to
  - Catch an `RpcException` when the `StatusCode` is `InvalidArgument`
  - Get all the errors by invoking the `GetValidationErrors` extension method
  - throwing a new `ValidationException` filled up with a `ValidationFailures` list

The code becomes:

```cs
public async Task<Comment?> CreateAsync(Comment comment) {
    CreateRequest createRequest = new CreateRequest() { PhotoId = comment.PhotoId, Subject = comment.Subject, Body = comment.Body };
    try {
        CreateReply c = await gRpcClient.CreateAsync(createRequest);
        return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
    } catch (RpcException ex) when (ex.StatusCode == StatusCode.PermissionDenied) {
        throw new CreateUnauthorizedException<Comment>(ex.Message);
    } catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument) {
        throw new FluentValidation.ValidationException(ex.GetValidationErrors().Select(t => new ValidationFailure(t.PropertyName, t.ErrorMessage, t.AttemptedValue)).ToList());
    } catch (RpcException ex) {
        throw new Exception(ex.Message);
    }
}
```

And

```cs
public async Task<Comment?> UpdateAsync(Comment comment) {
    try {
        UpdateReply c = await gRpcClient.UpdateAsync(new UpdateRequest { Id = comment.Id, Subject = comment.Subject, Body = comment.Body });
        return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
    } catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound) {
        return null;
    } catch (RpcException ex) when (ex.StatusCode == StatusCode.PermissionDenied) {
        throw new EditUnauthorizedException<Comment>(ex.Message);
    } catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument) {
        throw new FluentValidation.ValidationException(ex.GetValidationErrors().Select(t => new ValidationFailure(t.PropertyName, t.ErrorMessage, t.AttemptedValue)).ToList());
    } catch (RpcException ex) {
        throw new Exception(ex.Message);
    }
}
```

## The UI

## Show Validation Errors in Blazor Components

### CommentCreateComponent.razor

- Open the `CommentCreateComponent.razor` file of the  `PhotoSharingApplication.Frontend.Client` project
- Add a `private string message` variable
- During the `RaisCreate` method, try to await on the OnSave EventCallback, catch an `ValidationException` and set the `message` value to the `Message` property of the caught exception
- Show the `message` using a `MudText`

The `CommentCreate.razor` component becomes:

```
@using PhotoSharingApplication.Shared.Validators;
<MudCard>
    <MudForm Model="originalComment" @ref="@form" Validation="@(commentValidator.ValidateValue)">
        <MudCardContent>
            <MudTextField @bind-Value="originalComment.Subject"
                          For="@(() => originalComment.Subject)"
                          Label="Title" />
            <MudTextField @bind-Value="originalComment.Body"
                          Lines="3"
                          For="@(() => originalComment.Body)"
                          Label="Description" />
            <MudText Typo="Typo.caption" Color="Color.Error">@message</MudText>
        </MudCardContent>
    </MudForm>
    <MudCardActions>
        <MudIconButton Color="Color.Primary" Icon="@Icons.Material.Filled.Check" OnClick="RaiseCreate">Create</MudIconButton>
    </MudCardActions>    
</MudCard>


@code {
    [Parameter]
    public Comment CommentItem { get; set; } = default!;

    [Parameter]
    public EventCallback<Comment> OnCancel { get; set; }

    [Parameter]
    public EventCallback<Comment> OnSave { get; set; }

    private Comment originalComment = default!;

    private string message;
    protected override void OnInitialized() {
        originalComment = new Comment { Id = CommentItem.Id, PhotoId = CommentItem.PhotoId, Subject = CommentItem.Subject, Body = CommentItem.Body, SubmittedOn = CommentItem.SubmittedOn, UserName = CommentItem.UserName };
    }

    async Task RaiseCreate(MouseEventArgs args) {
        await form.Validate();
        if (form.IsValid)
        {
            try
            {
                message = string.Empty;
                await OnSave.InvokeAsync(originalComment);
                originalComment = new Comment { Id = CommentItem.Id, PhotoId = CommentItem.PhotoId, Subject = CommentItem.Subject, Body = CommentItem.Body, SubmittedOn = CommentItem.SubmittedOn, UserName = CommentItem.UserName };
            }
            catch (FluentValidation.ValidationException ex) {
                message = ex.Message;
            }
        }
    }

    private MudForm form;
    private readonly CommentValidator commentValidator = new CommentValidator();
}
```

Repeat the process for the `CommentEditComponent.razor`, which becomes:

```
@using PhotoSharingApplication.Shared.Validators;
<MudCard>
    <MudForm Model="originalComment" @ref="@form" Validation="@(commentValidator.ValidateValue)">
        <MudCardContent>
            <MudTextField @bind-Value="originalComment.Subject"
                          For="@(() => originalComment.Subject)"
                          Label="Title" />
            <MudTextField @bind-Value="originalComment.Body"
                          Lines="3"
                          For="@(() => originalComment.Body)"
                          Label="Description" />
            <MudText Typo="Typo.caption" Color="Color.Error">@message</MudText>
        </MudCardContent>
    </MudForm>
    <MudCardActions>
        <MudIconButton Color="Color.Primary" Icon="@Icons.Material.Filled.NavigateBefore" OnClick="@(async ()=> await OnCancel.InvokeAsync(originalComment))">Cancel</MudIconButton>
        <MudIconButton Color="Color.Primary" Icon="@Icons.Material.Filled.Check" OnClick="ValidateAndSubmit">Update</MudIconButton>
    </MudCardActions>
</MudCard>

@code {
    [Parameter]
    public Comment CommentItem { get; set; } = default!;

    [Parameter]
    public EventCallback<Comment> OnCancel { get; set; }

    [Parameter]
    public EventCallback<Comment> OnSave { get; set; }

    private Comment originalComment = default!;

    private MudForm form;
    private readonly CommentValidator commentValidator = new CommentValidator();

    private string message;

    protected override void OnInitialized() {
        originalComment = new Comment { Id = CommentItem.Id, PhotoId = CommentItem.PhotoId, Subject = CommentItem.Subject, Body = CommentItem.Body, SubmittedOn = CommentItem.SubmittedOn, UserName = CommentItem.UserName };
    }

    private async Task ValidateAndSubmit() {
        await form.Validate();
        if (form.IsValid) {
            try {
                message = string.Empty;
                await OnSave.InvokeAsync(originalComment);
            } catch (FluentValidation.ValidationException ex) {
                message = ex.Message;
            }
        }
    }
}
```

If you want to test what happens, change the rules on the gRpc validators. For example, have the Title rule ensure that it cannot be longer than 5 letters. You should see the errors coming from the server. Don't forget to put the rules back to match the ones on the client when you're done!

And we're done!

Our business logic validates our entities, no matter which application and infrastructure is used, rejecting invalid data.  
Our Application shows friendly error messages, by using the exact same validation logic that the server uses.  

---

In the next lab, we're going to explore Unit Testing of Blazor Components.