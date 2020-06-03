# Lab 13 - Validation

In this lab we're going to talk about validating our data.  

Our goal is to 
- prevent having the wrong data in our database
- reject invalid data
- show error messages explaining the rules, so that the user can correct the mistakes

.NET core has some builtin ways to validate data, for example [Data Annotations](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations?view=net-5.0) are a set of attributes used behind the scene by both [Asp.net Core](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-5.0) and [Entity Framework](https://docs.microsoft.com/en-us/ef/ef6/modeling/code-first/data-annotations).

We could use those, but there are a couple of considerations we have to make first.  

The Data Annotations are used by Entity Framework and by ASP.NET Core (Blazor included) so right now they would work, but what if we ever want to change the infrastructure and not rely on those two? We could decide to save the pictures in an Azure Blob storage, or Redis Cache or any other technology that comes to mind. In those cases Data Annotations wouldn't be appropriate anymore.  
If we decided to use Data Annotations, we would not be ready for change.

We are using a CLEAN architecture, which means that our *Core logic* resides in a *service* that is abstracted from the actual infrastructure. 
It should be the *Service* to decide if, where and when to validate.  
Once again, just like it happened for the Authorization part, we want the *service* to ask questions, but it should not know the answers. Whether a Photo is valid or not should be decided by someone else. Some sort of *validator* that can check some *rules* defined elsewhere.

To help us with validation, we can use [Fluent Validations](https://docs.fluentvalidation.net/en/latest/index.html).  
With this package, we will be able to define *validators* containing *rules* for our entities.  
The *service* will then just ask for a validator using *Dependency Injection* and when it's time (during *create* and *update*), it will demand that the validator validates and eventually throws a ValidationException. It will be as simple as that.

So our *Core* layer will be pretty simple.  
But that's not all.  
We can also use Fluent Validation at an *Application* level: 
- Server side - our ASP.NET Core REST Service 
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

- Add the `FluentValidation` NuGet Package to the `PhotoSharingApplication.Shared.Core` project.  
**NOTE: At the time of the writing of this tutorial, Blazor is incompatible with Fluent Validation 9 (which is still in preview), so be sure to include version 8, unless you read this is a future where they can work together**
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
using PhotoSharingApplication.Shared.Core.Entities;

namespace PhotoSharingApplication.Shared.Core.Validators {
    public class PhotoValidator : AbstractValidator<Photo> {
        public PhotoValidator() {
            RuleFor(photo => photo.Title).NotEmpty();
            RuleFor(photo => photo.Title).MaximumLength(255);

            RuleFor(photo => photo.PhotoImage).SetValidator(new PhotoImageValidator());
        }
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
using PhotoSharingApplication.Shared.Core.Entities;

namespace PhotoSharingApplication.Shared.Core.Validators {
    public class PhotoImageValidator : AbstractValidator<PhotoImage>{
        public PhotoImageValidator() {
            RuleFor(photoImage => photoImage.ImageMimeType).NotEmpty();
            RuleFor(photoImage => photoImage.ImageMimeType).MaximumLength(255);

            RuleFor(photoImage => photoImage.PhotoFile).NotEmpty();
        }
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
using PhotoSharingApplication.Shared.Core.Entities;

namespace PhotoSharingApplication.Shared.Core.Validators {
    public class CommentValidator : AbstractValidator<Comment> {
        public CommentValidator() {
            RuleFor(comment => comment.Subject).NotEmpty();
            RuleFor(comment => comment.Subject).MaximumLength(250);

            RuleFor(comment => comment.Body).NotEmpty();
        }
    }
}
```

## Validating from the *Core Layer*, Server Side

- Add the `FluentValidation` NuGet Package to the `PhotoSharingApplication.Backend.Core` project.  
**NOTE: At the time of the writing of this tutorial, Blazor is incompatible with Fluent Validation 9 (which is still in preview), so be sure to include version 8, unless you read this is a future where they can work together**

### PhotosService

- Open the `PhotosService` class located under the `Services` folder of the `PhotoSharingApplication.Backend.Core` project
- Use Dependency Injection from its constructor to ask for an `IValidator<Photo>` and save it into a private readonly variable
- In the `UpdateAsync` and `UploadAsync`, invoke the [ValidateAndThrow](https://docs.fluentvalidation.net/en/latest/start.html#throwing-exceptions) method before submitting the photo to the repository

The code should look like this:

```cs
using FluentValidation;
//(...code omitted for brevity...)

namespace PhotoSharingApplication.Backend.Core.Services {
    public class PhotosService : IPhotosService {

        //(...code omitted for brevity...)
        
        private readonly IValidator<Photo> validator;

        public PhotosService(IPhotosRepository repository, IAuthorizationService<Photo> photosAuthorizationService, IUserService userService, IValidator<Photo> validator) {

            //(...code omitted for brevity...)
            
            this.validator = validator;
        }
        
        //(...code omitted for brevity...)
        
        public async Task<Photo> UpdateAsync(Photo photo) {
            var user = await userService.GetUserAsync();
            if (await photosAuthorizationService.ItemMayBeUpdatedAsync(user, photo)) {
                validator.ValidateAndThrow(photo);
                return await repository.UpdateAsync(photo);
            } else {
                throw new UnauthorizedEditAttemptException<Photo>($"Unauthorized Edit Attempt of Photo {photo.Id}");
            }
        }
        public async Task<Photo> UploadAsync(Photo photo) {
            var user = await userService.GetUserAsync();
            if (await photosAuthorizationService.ItemMayBeCreatedAsync(user, photo)) {
                validator.ValidateAndThrow(photo);
                photo.CreatedDate = DateTime.Now;
                photo.UserName = user.Identity.Name;
                return await repository.CreateAsync(photo);
            } else throw new UnauthorizedCreateAttemptException<Photo>($"Unauthorized Create Attempt of Photo {photo.Id}");
        }
    }
}
```

### CommentsService

- Open the `CommentsService` class located under the `Services` folder of the `PhotoSharingApplication.Backend.Core` project
- Use Dependency Injection from its constructor to ask for an `IValidator<Comment>` and save it into a private readonly variable
- In the `UpdateAsync` and `CreateAsync`, invoke the [ValidateAndThrow](https://docs.fluentvalidation.net/en/latest/start.html#throwing-exceptions) method before submitting the photo to the repository

The code should look like this:

```cs
using FluentValidation;
//(...code omitted for brevity...)

namespace PhotoSharingApplication.Backend.Core.Services {
    public class CommentsService : ICommentsService {
        //(...code omitted for brevity...)
        private readonly IValidator<Comment> validator;

        public CommentsService(ICommentsRepository repository, IAuthorizationService<Comment> commentsAuthorizationService, IUserService userService, IValidator<Comment> validator) {
           //(...code omitted for brevity...)
            this.validator = validator;
        }

        //(...code omitted for brevity...)
        
        public async Task<Comment> CreateAsync(Comment comment) {
            var user = await userService.GetUserAsync();
            if (await commentsAuthorizationService.ItemMayBeCreatedAsync(user, comment)) {
                validator.ValidateAndThrow(comment);
                comment.SubmittedOn = DateTime.Now;
                comment.UserName = user.Identity.Name;
                return await repository.CreateAsync(comment);
            } else throw new UnauthorizedCreateAttemptException<Comment>($"Unauthorized Create Attempt of Comment {comment.Id}");
        }

        public async Task<Comment> UpdateAsync(Comment comment) {
            var user = await userService.GetUserAsync();
            Comment oldComment = await repository.FindAsync(comment.Id);
            if (await commentsAuthorizationService.ItemMayBeUpdatedAsync(user, oldComment)) {
                oldComment.Subject = comment.Subject;
                oldComment.Body = comment.Body;
                oldComment.SubmittedOn = DateTime.Now;
                validator.ValidateAndThrow(oldComment);
                return await repository.UpdateAsync(oldComment);
            }else throw new UnauthorizedEditAttemptException<Comment>($"Unauthorized Edit Attempt of Comment {comment.Id}");

        }
    }
}
```

## The Server Side *Application Layer* 

In order to try if this works, we need to register the `IValidator<Photo>` and the `IValidator<Comment>` in the DI Containers.

### Photos REST Service

As explained in the [documentation](https://docs.fluentvalidation.net/en/latest/aspnet.html):

- Add a `FluentValidation.AspNetCore` NuGet Package to the `PhotoSharingApplication.WebServices.REST.Photos` project
- Open the `Startup` class and locate `ConfigureServices` method.
- Invoke the `AddFluentValidation` extension method (which requires a `using FluentValidation.AspNetCore`)
- Register the `PhotoValidator` with the services collection by calling the `AddTransient` method

The code should look like this:

```cs
using FluentValidation;
using FluentValidation.AspNetCore;
using PhotoSharingApplication.Shared.Core.Validators;
// (...code omitted for brevity...)

namespace PhotoSharingApplication.WebServices.REST.Photos {
    public class Startup {
        
        // (...code omitted for brevity...)
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllers().AddFluentValidation();
            // (...code omitted for brevity...)
            services.AddTransient<IValidator<Photo>, PhotoValidator>();
        }

        // (...code omitted for brevity...)
    }
}

```

### Comments gRPC Service

As explained in the [documentation](https://docs.fluentvalidation.net/en/latest/aspnet.html):

- Add a `FluentValidation.AspNetCore` NuGet Package to the `PhotoSharingApplication.WebServices.REST.Photos` project
- Open the `Startup` class and locate `ConfigureServices` method.
- **THERE IS NO SUPPORT FOR GRPC AT THE MOMENT, SO SKIP THE AddFluentValidation Step**
- Register the `CommentValidator` with the services collection by calling the `AddTransient` method

The code should look like this:

```cs
using FluentValidation;
using PhotoSharingApplication.Shared.Core.Validators;
// (...code omitted for brevity...)
namespace PhotoSharingApplication.WebServices.Grpc.Comments {
    public class Startup {
        // (...code omitted for brevity...)
        public void ConfigureServices(IServiceCollection services) {
            // (...code omitted for brevity...)
            services.AddTransient<IValidator<Comment>, CommentValidator>();
        }
        // (...code omitted for brevity...)
    }
}

```
- Start the application now and try to upload a photo without a title.
- Inspect the network traffic by pressing F12 on your browser and going to the `Network` tab)  
You'll see that the server is returning a 400 (Bad Request) containing the list of all the errors found by the validation. The Bad Request comes from the fact that Validation is performed by ASP.NET Core even before our action is invoked, which means that our *core service* has not even been bothered at all
- Try to submit a comment with a title longer than 250 letters.  
- Inspect the network traffic by pressing F12 on your browser and going to the `Network` tab)  
You'll see that the server is returning a 500 (Internal Server Error), which means that our service threw an exception 

Great, now we can never get any bad data on our server, whether we use Entity Framework or not.

Now onto the Client Side.

## Client Side *Core Layer*


- Add the `FluentValidation` NuGet Package to the `PhotoSharingApplication.Frontend.Core` project.  
**NOTE: At the time of the writing of this tutorial, Blazor is incompatible with Fluent Validation 9 (which is still in preview), so be sure to include version 8, unless you read this is a future where they can work together**

### PhotosService

- Open the `PhotosService` class located under the `Services` folder of the `PhotoSharingApplication.Frontend.Core` project
- Use Dependency Injection from its constructor to ask for an `IValidator<Photo>` and save it into a private readonly variable
- In the `UpdateAsync` and `UploadAsync`, invoke the [ValidateAndThrow](https://docs.fluentvalidation.net/en/latest/start.html#throwing-exceptions) method before submitting the photo to the repository

The code should look like this:

```cs
using FluentValidation;
//(...code omitted for brevity...)
namespace PhotoSharingApplication.Frontend.Core.Services {
    public class PhotosService : IPhotosService {
        //(...code omitted for brevity...)
        private readonly IValidator<Photo> validator;

        public PhotosService(IPhotosRepository repository, IAuthorizationService<Photo> photosAuthorizationService, IUserService userService, IValidator<Photo> validator) {
            //(...code omitted for brevity...)
            this.validator = validator;
        }
        //(...code omitted for brevity...)
        public async Task<Photo> UpdateAsync(Photo photo) {
            var user = await userService.GetUserAsync();
            if (await photosAuthorizationService.ItemMayBeUpdatedAsync(user, photo)) {
                validator.ValidateAndThrow(photo);
                return await repository.UpdateAsync(photo);
            } else {
                throw new UnauthorizedEditAttemptException<Photo>($"Unauthorized Edit Attempt of Photo {photo.Id}");
            }
        }
        public async Task<Photo> UploadAsync(Photo photo) {
            var user = await userService.GetUserAsync();
            if (await photosAuthorizationService.ItemMayBeCreatedAsync(user, photo)) {
                validator.ValidateAndThrow(photo);
                photo.CreatedDate = DateTime.Now;
                photo.UserName = user.Identity.Name;
                return await repository.CreateAsync(photo);
            } else {
                throw new UnauthorizedCreateAttemptException<Photo>($"Unauthorized Create Attempt of Photo {photo.Id}");
            }
        }
    }
}
```

### CommentsService

- Open the `CommentsService` class located under the `Services` folder of the `PhotoSharingApplication.Frontend.Core` project
- Use Dependency Injection from its constructor to ask for an `IValidator<Comment>` and save it into a private readonly variable
- In the `UpdateAsync` and `CreateAsync`, invoke the [ValidateAndThrow](https://docs.fluentvalidation.net/en/latest/start.html#throwing-exceptions) method before submitting the photo to the repository

The code should look like this:

```cs
using FluentValidation;
//(...code omitted for brevity...)
namespace PhotoSharingApplication.Frontend.Core.Services {
    public class CommentsService : ICommentsService {
        //(...code omitted for brevity...)
        private readonly IValidator<Comment> validator;

        public CommentsService(ICommentsRepository repository, IAuthorizationService<Comment> commentsAuthorizationService, IUserService userService, IValidator<Comment> validator) {
            //(...code omitted for brevity...)
            this.validator = validator;
        }
        //(...code omitted for brevity...)
        public async Task<Comment> CreateAsync(Comment comment) {
            var user = await userService.GetUserAsync();
            if (await commentsAuthorizationService.ItemMayBeCreatedAsync(user, comment)) {
                validator.ValidateAndThrow(comment);
                comment.SubmittedOn = DateTime.Now;
                comment.UserName = user.Identity.Name;
                return await repository.CreateAsync(comment);
            } else throw new UnauthorizedCreateAttemptException<Comment>($"Unauthorized Create Attempt of Comment {comment.Id}"); 
        }

        public async Task<Comment> UpdateAsync(Comment comment) {
            var user = await userService.GetUserAsync();
            if (await commentsAuthorizationService.ItemMayBeUpdatedAsync(user, comment)) {
                validator.ValidateAndThrow(comment);
                return await repository.UpdateAsync(comment);
            } else throw new UnauthorizedEditAttemptException<Comment>($"Unauthorized Edit Attempt of Comment {comment.Id}");
        }
    }
}

```

## The Client Side *Application Layer* 

In order to try if this works, we need to register the `IValidator<Photo>` and `IValidator<Comment>` in the Blazor DI Container.

- Add the `Blazored.FluentValidation` NuGet Package to the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project
- In the `Program` class, locate the `Main` method
- Register the `PhotoValidator` with the services collection by calling the `AddTransient` method
- Register the `CommentValidator` with the services collection by calling the `AddTransient` method

The code should look like this:

```cs
using FluentValidation;
using PhotoSharingApplication.Shared.Core.Validators;
//(...code omitted for brevity...)
namespace PhotoSharingApplication.Frontend.BlazorWebAssembly {
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //(...code omitted for brevity...)
            builder.Services.AddTransient<IValidator<Photo>, PhotoValidator>();
            builder.Services.AddTransient<IValidator<Comment>, CommentValidator>();

            await builder.Build().RunAsync();
        }
    }
}
```

- Run the application
- Press F12 to open the Developer Tools
- Go to the Network Tab
- Try to upload a Photo without a title  
You'll see that there is no network traffic involved and that we get an exception with the list of the errors found by the validators
- Same happens if you try to submit a comment with no title or with a title longer than 250 letters.

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

The template should look like this (the `code` section does not change)

```html
<MatCard>
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
        <MatCardMedia Wide="true" ImageUrl="@(Photo.PhotoImage.PhotoFile == null ? "" : $"data:{Photo.PhotoImage.ImageMimeType};base64,{Convert.ToBase64String(Photo.PhotoImage.PhotoFile)}")"></MatCardMedia>
    </MatCardContent>
</MatCard>
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
You'll see an error message stating that the Image field cannot be empty.
- Try to submit a Comment without a title  
You'll see an error message stating that the Title field cannot be empty.
- Try to write a Title longer than 255 characters
You'll see an error message stating that the Title field cannot be longer than 255 characters.
- Try to upload a Photo without a Picture  
You'll see an error message stating that the Image field cannot be empty.

Also, there is no network traffic involved.

And we're done!

Our business logic validates our entities, no matter which application and infrastructure is used, rejecting invalid data.  
Our Application shows friendly error messages, by using the exact same validation logic that the server uses.  