# Security: Resource Based Authorization

We did not protect the update and delete operations, yet.

What we would like to have is an application where:
- Photos may be updated and deleted only by their owners
- Comments may be updated and deleted only by their owners

All of this while maintaining an *as CLEAN as possible* architecture.

So let's think about who does what.

Wondering if the current user is allowed to perform a specific operation is something that should remain the same no matter what technology we use, right? It's just part of the core logic of our application.  So it does make sense to put that question in the Core Service. No matter if we run a REST or gRPC service, Blazor or MAUI, the logic remains something like:

- who's the user?
- is the user authorized to update / delete?
- if so, do the thing
- if not, throw


On the other end, the actual checks *do* depend on the technology.  
For example: how do we get the current user?  
There are different ways depending on where we check it. If we're running in a web service, we get the User in the current HttpContext, but Blazor Web Assembly does not have an HttpContext, nor would MAUI or Xamarin or any other native mobile application and so on.  
So that job is not for the Core Service. The Core Service can ask, but the answer has to be given by someone else. Someone that is specific for a given technology.

Luckily, we already follow a CLEAN architecture, so we can continue with it.  
In our Core project we're going to introduce our own interfaces and exceptions:
- `IUserService`  
`Task<ClaimsPrincipal> GetUserAsync();`
- `IAuthorizationService<T>`  
`Task<bool> ItemMayBeUpdatedAsync(ClaimsPrincipal User, T item);`  
`Task<bool> ItemMayBeDeletedAsync(ClaimsPrincipal User, T item);`
- `UnauthorizeEditAttemptException<T>`
- `UnauthorizeDeleteAttemptException<T>`

Our Core Service will explicitly state that it depends on an `IUserService` and an `IAuthorizationService`.      
During its actions, it will ask to the `IUserService` who the user is, then will call the right method of the `IAuthorizationService` to know if the user is allowed to delete / update the current `Photo` (or `Comment`).  
By creating our own interfaces, we can avoid any dependency  on .NET, .NET Core, ASP.NET and so on. Nice and clean.  
Also, I know that technically we already solved the `Create` in our previous lab, but we can use the same technique for that as well, so we'll also provide methods and exceptions for the Create. This way, we are ready for change, just in case we decide that being authenticated is not enough anymore to upload a photo.   
We can reuse the same interfaces and exceptions both on the client and on the server side, so let's put them on our `Core.Shared` project.  

## Shared Core

### IUserService
- In the `Solution Explorer`, under the `Interfaces` folder of the `PhotoSharingApplication.Shared.Core` project, right click on `Add -> New Item`
- Select `Interface`, name the interface `IUserService` and click `Ok`
- Define a `GetUserAsync` method returning a `Task<ClaimsPrincipal>`

```cs
using System.Security.Claims;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Shared.Core.Interfaces {
  public interface IUserService {
    Task<ClaimsPrincipal> GetUserAsync();
  }
}
```

### IAuthorizationService<T>
- In the `Solution Explorer`, under the `Interfaces` folder of the `PhotoSharingApplication.Shared.Core` project, right click on `Add -> New Item`
- Select `Interface`, name the interface `IAuthorizationService` and click `Ok`
- Let the interface be a generic of type `T`
- Define a 
  - `ItemMayBeCreatedAsync`
  - `ItemMayBeUpdatedAsync`
  - `ItemMayBeDeletedAsync`  
 methods, each accepting a `ClaimsPrincipal` User and a `T` Item, and returning a `Task<bool>`

```cs
using System.Security.Claims;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Shared.Core.Interfaces {
  public interface IAuthorizationService<T> {
    Task<bool> ItemMayBeCreatedAsync(ClaimsPrincipal User, T item);
    Task<bool> ItemMayBeUpdatedAsync(ClaimsPrincipal User, T item);
    Task<bool> ItemMayBeDeletedAsync(ClaimsPrincipal User, T item);
  }
}
```

### The Exceptions
- In the `Solution Explorer`, in the `PhotoSharingApplication.Shared.Core` project, create a new folder named `Exceptions`.
- Under the `Exceptions` folder, add three new classes:
  - `UnauthorizedCreateAttemptException<T>`
  - `UnauthorizedDeleteAttemptException<T>`
  - `UnauthorizedEditAttemptException<T>`  
 In each exception, create the necessary constructors, like this:

 ```cs
using System;
using System.Runtime.Serialization;

namespace PhotoSharingApplication.Shared.Core.Exceptions {
    [Serializable]
    public class UnauthorizedCreateAttemptException<T> : Exception {
        public UnauthorizedCreateAttemptException() { }
        public UnauthorizedCreateAttemptException(string message) : base(message) { }
        public UnauthorizedCreateAttemptException(string message, Exception inner) : base(message, inner) { }
        protected UnauthorizedCreateAttemptException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
``` 

```cs
using System;
using System.Runtime.Serialization;

namespace PhotoSharingApplication.Shared.Core.Exceptions {
    [Serializable]
    public class UnauthorizedDeleteAttemptException<T> : Exception {
        public UnauthorizedDeleteAttemptException() { }
        public UnauthorizedDeleteAttemptException(string message) : base(message) { }
        public UnauthorizedDeleteAttemptException(string message, Exception inner) : base(message, inner) { }
        protected UnauthorizedDeleteAttemptException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
```

```cs
using System;
using System.Runtime.Serialization;

namespace PhotoSharingApplication.Shared.Core.Exceptions {
    [Serializable]
    public class UnauthorizedEditAttemptException<T> : Exception {
        public UnauthorizedEditAttemptException() { }
        public UnauthorizedEditAttemptException(string message) : base(message) { }
        public UnauthorizedEditAttemptException(string message, Exception inner) : base(message, inner) { }
        protected UnauthorizedEditAttemptException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
```

It's time to use them from within our Core Service.

## Backend Core

### PhotosService

- In the `Solution Explorer`, open the `PhotosService` located under the `Services` folder of the `PhotoSharingApplication.Backend.Core` project.
- Modify the constructor to accept and save an `IUserService` and an `IAuthenticationService<Photo>` parameter
- Modify the `UploadAsync` to
  - Get the User by invoking the `GetUserAsync` of the `userService`
  - Decide whether or not to upload the photo by invoking the `ItemMayBeCreated` of the `authorizationService`
  - Eventually throw an `UnauthorizedCreateAttempt<Photo>` exception
- Modify the `UpdateAsync` to
  - Get the User by invoking the `GetUserAsync` of the `userService`
  - Decide whether or not to update the photo by invoking the `ItemMayBeUpdatedAsync` of the `authorizationService`
  - Eventually throw an `UnauthorizedEditAttemptException<Photo>` exception
- Modify the `RemoveAsync` to
  - Get the User by invoking the `GetUserAsync` of the `userService`
  - Decide whether or not to delete the photo by invoking the `ItemMayBeDeletedAsync` of the `authorizationService`
  - Eventually throw an `UnauthorizedDeleteAttemptException<Photo>` exception

The code should look like this:

```cs
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Exceptions;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Backend.Core.Services {
    public class PhotosService : IPhotosService {
        private readonly IPhotosRepository repository;
        private readonly IAuthorizationService<Photo> photosAuthorizationService;
        private readonly IUserService userService;

        public PhotosService(IPhotosRepository repository, IAuthorizationService<Photo> photosAuthorizationService, IUserService userService) =>
            (this.repository, this.photosAuthorizationService, this.userService) = (repository, photosAuthorizationService, userService);

        public async Task<Photo> FindAsync(int id) => await repository.FindAsync(id);
        public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await repository.GetPhotosAsync(amount);
        public async Task<Photo> RemoveAsync(int id) {
            Photo photo = await FindAsync(id);
            var user = await userService.GetUserAsync();
            if (await photosAuthorizationService.ItemMayBeDeletedAsync(user, photo))
                return await repository.RemoveAsync(id);
            else throw new UnauthorizedDeleteAttemptException<Photo>($"Unauthorized Deletion Attempt of Photo {photo.Id}");
        }
        public async Task<Photo> UpdateAsync(Photo photo) {
            var user = await userService.GetUserAsync();
            if (await photosAuthorizationService.ItemMayBeUpdatedAsync(user, photo))
                return await repository.UpdateAsync(photo);
            else throw new UnauthorizedEditAttemptException<Photo>($"Unauthorized Edit Attempt of Photo {photo.Id}");
        }
        public async Task<Photo> UploadAsync(Photo photo) {
            var user = await userService.GetUserAsync();
            if (await photosAuthorizationService.ItemMayBeCreatedAsync(user, photo)) {
                photo.CreatedDate = DateTime.Now;
                photo.UserName = user.Identity.Name;
                return await repository.CreateAsync(photo);
            } else throw new UnauthorizedCreateAttemptException<Photo>($"Unauthorized Create Attempt of Photo {photo.Id}");
        }
    }
}
```

### CommentsService

- In the `Solution Explorer`, open the `CommentsService` located under the `Services` folder of the `PhotoSharingApplication.Backend.Core` project.
- Modify the constructor to accept and save an `IUserService` and an `IAuthenticationService<Photo>` parameter
- Modify the `CreateAsync` to
  - Get the User by invoking the `GetUserAsync` of the `userService`
  - Decide whether or not to submit the comment by invoking the `ItemMayBeCreated` of the `authorizationService`
  - Eventually throw an `UnauthorizedCreateAttempt<Comment>` exception
- Modify the `UpdateAsync` to
  - Get the User by invoking the `GetUserAsync` of the `userService`
  - Decide whether or not to update the comment by invoking the `ItemMayBeUpdatedAsync` of the `authorizationService`
  - Eventually throw an `UnauthorizedEditAttemptException<Comment>` exception
- Modify the `RemoveAsync` to
  - Get the User by invoking the `GetUserAsync` of the `userService`
  - Decide whether or not to delete the comment by invoking the `ItemMayBeDeletedAsync` of the `authorizationService`
  - Eventually throw an `UnauthorizedDeleteAttemptException<Photo>` exception
The code should look like this:

```cs
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Exceptions;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CommentsService : ICommentsService {
  private readonly ICommentsRepository repository;
  private readonly IAuthorizationService<Comment> commentsAuthorizationService;
  private readonly IUserService userService;
  public CommentsService(ICommentsRepository repository, IAuthorizationService<Comment> commentsAuthorizationService, IUserService userService) =>
    (this.repository, this.commentsAuthorizationService, this.userService) = (repository, commentsAuthorizationService, userService);

  public async Task<Comment> CreateAsync(Comment comment) {
    var user = await userService.GetUserAsync();
    if (await commentsAuthorizationService.ItemMayBeCreatedAsync(user, comment)) {
      comment.SubmittedOn = DateTime.Now;
      comment.UserName = user.Identity.Name;
      return await repository.CreateAsync(comment);
    } else throw new UnauthorizedCreateAttemptException<Comment>($"Unauthorized Create Attempt of Comment {comment.Id}");
  }

  public async Task<Comment> FindAsync(int id) => await repository.FindAsync(id);

  public async Task<List<Comment>> GetCommentsForPhotoAsync(int photoId) => await repository.GetCommentsForPhotoAsync(photoId);

  public async Task<Comment> RemoveAsync(int id) {
    Comment comment = await FindAsync(id);
    var user = await userService.GetUserAsync();
    if (await commentsAuthorizationService.ItemMayBeDeletedAsync(user, comment))
      return await repository.RemoveAsync(id);
    else throw new UnauthorizedDeleteAttemptException<Comment>($"Unauthorized Deletion Attempt of Comment {comment.Id}");
  }

  public async Task<Comment> UpdateAsync(Comment comment) {
    var user = await userService.GetUserAsync();
    Comment oldComment = await repository.FindAsync(comment.Id);
    if (await commentsAuthorizationService.ItemMayBeUpdatedAsync(user, oldComment)) {
      oldComment.Subject = comment.Subject;
      oldComment.Body = comment.Body;
      oldComment.SubmittedOn = DateTime.Now;
      return await repository.UpdateAsync(oldComment);
    } else throw new UnauthorizedEditAttemptException<Comment>($"Unauthorized Edit Attempt of Comment {comment.Id}");
  }
}
```

## Frontend Core

Repeat the same process for the `PhotoSharingApplication.Frontend.Core`:

### PhotosService

```cs
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Exceptions;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Core.Services {
    public class PhotosService : IPhotosService {
        private readonly IPhotosRepository repository;
        private readonly IAuthorizationService<Photo> photosAuthorizationService;
        private readonly IUserService userService;

        public PhotosService(IPhotosRepository repository, IAuthorizationService<Photo> photosAuthorizationService, IUserService userService) =>
            (this.repository, this.photosAuthorizationService, this.userService) = (repository, photosAuthorizationService, userService);
        public async Task<Photo> FindAsync(int id) => await repository.FindAsync(id);
        public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await repository.GetPhotosAsync(amount);
        public async Task<Photo> RemoveAsync(int id) {
            Photo photo = await FindAsync(id);
            var user = await userService.GetUserAsync();
            if (await photosAuthorizationService.ItemMayBeDeletedAsync(user, photo))
                return await repository.RemoveAsync(id);
            else throw new UnauthorizedDeleteAttemptException<Photo>($"Unauthorized Deletion Attempt of Photo {photo.Id}");
        }
        public async Task<Photo> UpdateAsync(Photo photo) {
            var user = await userService.GetUserAsync();
            if (await photosAuthorizationService.ItemMayBeUpdatedAsync(user, photo))
                return await repository.UpdateAsync(photo);
            else throw new UnauthorizedEditAttemptException<Photo>($"Unauthorized Edit Attempt of Photo {photo.Id}");
        }
        public async Task<Photo> UploadAsync(Photo photo) {
            var user = await userService.GetUserAsync();
            if (await photosAuthorizationService.ItemMayBeCreatedAsync(user, photo)) {
                photo.CreatedDate = DateTime.Now;
                photo.UserName = user.Identity.Name;
                return await repository.CreateAsync(photo);
            } else throw new UnauthorizedCreateAttemptException<Photo>($"Unauthorized Create Attempt of Photo {photo.Id}");
        }
    }
}
```

### CommentsService

```cs
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Exceptions;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Core.Services {
    public class CommentsService : ICommentsService {
        private readonly ICommentsRepository repository;
        private readonly IAuthorizationService<Comment> commentsAuthorizationService;
        private readonly IUserService userService;

        public CommentsService(ICommentsRepository repository, IAuthorizationService<Comment> commentsAuthorizationService, IUserService userService) =>
            (this.repository, this.commentsAuthorizationService, this.userService) = (repository, commentsAuthorizationService, userService);

        public async Task<Comment> CreateAsync(Comment comment) {
            var user = await userService.GetUserAsync();
            if (await commentsAuthorizationService.ItemMayBeCreatedAsync(user, comment)) {
                comment.SubmittedOn = DateTime.Now;
                comment.UserName = user.Identity.Name;
                return await repository.CreateAsync(comment);
            } else throw new UnauthorizedCreateAttemptException<Comment>($"Unauthorized Create Attempt of Comment {comment.Id}");
        }
        public async Task<Comment> FindAsync(int id) => await repository.FindAsync(id);
        public async Task<List<Comment>> GetCommentsForPhotoAsync(int photoId) => await repository.GetCommentsForPhotoAsync(photoId);

        public async Task<Comment> RemoveAsync(int id) {
            Comment comment = await FindAsync(id);
            var user = await userService.GetUserAsync();
            if (await commentsAuthorizationService.ItemMayBeDeletedAsync(user, comment))
                return await repository.RemoveAsync(id);
            else throw new UnauthorizedDeleteAttemptException<Comment>($"Unauthorized Deletion Attempt of Comment {comment.Id}");
        }

        public async Task<Comment> UpdateAsync(Comment comment) {
            var user = await userService.GetUserAsync();
            Comment oldComment = await repository.FindAsync(comment.Id);
            if (await commentsAuthorizationService.ItemMayBeUpdatedAsync(user, oldComment)) {
                oldComment.Subject = comment.Subject;
                oldComment.Body = comment.Body;
                oldComment.SubmittedOn = DateTime.Now;
                return await repository.UpdateAsync(oldComment);
            } else throw new UnauthorizedEditAttemptException<Comment>($"Unauthorized Edit Attempt of Comment {comment.Id}");
        }
    }
}
```
Ok, great, our logic is sound, neat and clean. We still don't know how to actually check users and permissions, but that's a job for the infrastructure, we'll figure it out later.  
In the meantime we can change our *Application* layer.  
Server side, the *Application layer* is represented by our two web services.  
Client side, we have our Blazor Components.

## Backend 

### Photos Rest Controller

Our actions are already calling the service. What we have to do is to intercept the eventual exceptions and transform them into a response that the client can handle.  
We do that by returning a `ForbidResult`.  
Our `PhotosController`, located under the `Controllers` folder of the `PhotoSharingApplication.WebServices.REST.Photos` project, becomes:

```cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Exceptions;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoSharingApplication.WebServices.REST.Photos.Controllers {
    [Route("[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase {
        private readonly IPhotosService service;

        public PhotosController(IPhotosService service) {
            this.service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos() => await service.GetPhotosAsync();

        [HttpGet("{id:int}", Name = "Find")]
        public async Task<ActionResult<Photo>> Find(int id) {
            Photo ph = await service.FindAsync(id);
            if (ph == null) return NotFound();
            return ph;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Photo>> CreateAsync(Photo photo) {
            try {
                photo.UserName = User.Identity.Name;
                Photo p = await service.UploadAsync(photo);
                return CreatedAtRoute(nameof(Find), p, new { id = p.Id });
            } catch (UnauthorizedCreateAttemptException<Photo>) {
                return Forbid();
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Photo>> Update(int id, Photo photo) {
            if (id != photo.Id)
                return BadRequest();
            Photo ph = await service.FindAsync(id);
            if (ph == null) return NotFound();

            try {
                return await service.UpdateAsync(photo);
            } catch (UnauthorizedEditAttemptException<Photo>) {
                return Forbid();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Photo>> Remove(int id) {
            Photo ph = await service.FindAsync(id);
            if (ph == null) return NotFound();
            try {
                return await service.RemoveAsync(id);
            } catch (UnauthorizedDeleteAttemptException<Photo>) {
                return Forbid();
            }
        }
    }
}
```

### Comments gRPC Service

We can follow the same idea in the `Comments` `gRPC` service. The difference is that instead of a `ForbidResult`, a `gRPC` service should return an `RpcException` with a `StatusCode` of `Permission Denied`, as shown in the [Error Handling](https://docs.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/error-handling) article from the Microsoft documentation.

Under the `Services` folder of the `PhotoSharingApplication.WebServices.Grpc.Comments` project, replace the `CommentsService` code with the following:
```cs
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Exceptions;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoSharingApplication.WebServices.Grpc.Comments.Services {
    public class CommentsGrpcService : Commenter.CommenterBase {
        private readonly ICommentsService commentsService;
        public CommentsGrpcService(ICommentsService commentsService) => this.commentsService = commentsService;
        public override async Task<GetCommentsForPhotosReply> GetCommentsForPhoto(GetCommentsForPhotosRequest request, ServerCallContext context) {
            List<Comment> comments = await commentsService.GetCommentsForPhotoAsync(request.PhotoId);
            GetCommentsForPhotosReply r = new GetCommentsForPhotosReply();
            IEnumerable<GetCommentsForPhotosReplyItem> replyItems = comments.Select(c => new GetCommentsForPhotosReplyItem() { Id = c.Id, PhotoId = c.PhotoId, Subject = c.Subject, UserName = c.UserName, Body = c.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()) });
            r.Comments.AddRange(replyItems);
            return r;
        }
        public override async Task<FindReply> Find(FindRequest request, ServerCallContext context) {
            Comment c = await commentsService.FindAsync(request.Id);
            if (c is null) {
                throw new RpcException(new Status(StatusCode.NotFound, "Comment not found"));
            }
            return new FindReply() { Id = c.Id, PhotoId = c.PhotoId, Subject = c.Subject, UserName = c.UserName, Body = c.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()) };
        }

        [Authorize]
        public override async Task<CreateReply> Create(CreateRequest request, ServerCallContext context) {
            return await executeCommand(async () => {
                var user = context.GetHttpContext().User;
                Comment c = await commentsService.CreateAsync(new Comment { PhotoId = request.PhotoId, Subject = request.Subject, Body = request.Body, UserName = user.Identity.Name });
                return new CreateReply() { Id = c.Id, PhotoId = c.PhotoId, Body = c.Body, Subject = c.Subject, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()), UserName = c.UserName };
            }, context);
        }
        public override async Task<UpdateReply> Update(UpdateRequest request, ServerCallContext context) {
            return await executeCommand(async () => {
                Comment c = await commentsService.UpdateAsync(new Comment { Id = request.Id, Subject = request.Subject, Body = request.Body });
                return new UpdateReply() { Id = c.Id, PhotoId = c.PhotoId, Subject = c.Subject, UserName = c.UserName, Body = c.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()) };
            }, context);
        }

        public override async Task<RemoveReply> Remove(RemoveRequest request, ServerCallContext context) {
            return await executeCommand(async () => {
                Comment c = await commentsService.RemoveAsync(request.Id);
                return new RemoveReply() { Id = c.Id, PhotoId = c.PhotoId, Subject = c.Subject, UserName = c.UserName, Body = c.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()) };
            }, context);
        }

        private Task<TReply> executeCommand<TReply>(Func<Task<TReply>> commandToExecute, ServerCallContext context) {
            try {
                return commandToExecute();
            } catch (UnauthorizedDeleteAttemptException<Comment>) {
                var user = context.GetHttpContext().User;
                var metadata = new Metadata { { "User", user.Identity.Name } };
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Permission denied"), metadata);
            } catch (Exception ex) {
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }
    }
}
```

## Frontend

In our `PhotoSharingApplication.Frontend.BlazorWebAssembly`, under the `Pages` folder, we have three pages where we have to update our logic for the Photo:

- `UploadPhoto`
- `UpdatePhoto`
- `DeletePhoto`

In all three we'll try to talk to the service.  
If we catch an `Unauthorized***AttemptException`, we send our user to a new `Forbidden` page where we tell the user that nope, no can do.

### UploadPhoto.razor

```cs
@using PhotoSharingApplication.Shared.Core.Exceptions
@page "/photos/upload"
@attribute [Authorize]
@inject IPhotosService photosService
@inject NavigationManager navigationManager


<div class="mat-layout-grid">
    <div class="mat-layout-grid-inner">
        <div class="mat-layout-grid-cell mat-layout-grid-cell-span-12">
            <PhotoEditComponent Photo="photo" OnSave="Upload"></PhotoEditComponent>
        </div>
    </div>
</div>
@code {
    Photo photo;

    protected override void OnInitialized() {
        photo = new Photo();
    }

    private async Task Upload() {
        try {
            await photosService.UploadAsync(photo);
            navigationManager.NavigateTo("/photos/all");
        } catch (UnauthorizedCreateAttemptException<Photo>) {
            navigationManager.NavigateTo("/forbidden");
        }
    }
}
```

### UpdatePhoto.razor

```cs
@using PhotoSharingApplication.Shared.Core.Exceptions
@page "/photos/update/{id:int}"

@inject IPhotosService photosService
@inject NavigationManager navigationManager

@if (photo == null) {
    <p>...Loading...</p>
} else {
    <div class="mat-layout-grid">
        <div class="mat-layout-grid-inner">
            <div class="mat-layout-grid-cell mat-layout-grid-cell-span-12">
                <PhotoEditComponent Photo="photo" OnSave="Update"></PhotoEditComponent>
            </div>
        </div>
    </div>
}

@code {
    [Parameter]
    public int Id { get; set; }

    Photo photo;

    protected override async Task OnInitializedAsync() {
        photo = await photosService.FindAsync(Id);
    }

    private async Task Update() {
        try {
            await photosService.UpdateAsync(photo);
            navigationManager.NavigateTo("/photos/all");
        } catch (UnauthorizedEditAttemptException<Photo>) {
            navigationManager.NavigateTo("/forbidden");
        }
    }
}
```

### DeletePhoto.razor

```cs
@using PhotoSharingApplication.Shared.Core.Exceptions
@page "/photos/delete/{id:int}"

@inject IPhotosService photosService
@inject NavigationManager navigationManager

<MatH3>Delete</MatH3>

@if (photo == null) {
    <p>...Loading...</p>
} else {
    <div class="mat-layout-grid">
        <div class="mat-layout-grid-inner">
            <div class="mat-layout-grid-cell mat-layout-grid-cell-span-12">
                <PhotoDetailsComponent Photo="photo" DeleteConfirm OnDeleteConfirmed="Delete"></PhotoDetailsComponent>
            </div>
        </div>
    </div>
}

@code {
    [Parameter]
    public int Id { get; set; }

    Photo photo;

    protected override async Task OnInitializedAsync() {
        photo = await photosService.FindAsync(Id);
    }
    private async Task Delete(int id) {
        try {
            await photosService.RemoveAsync(id);
            navigationManager.NavigateTo("/photos/all");
        } catch (UnauthorizedDeleteAttemptException<Photo>) {
            navigationManager.NavigateTo("/forbidden");
        }
    }
}
```

Create a new `Forbidden.razor` file in the same folder and configure its route:

```cs
@page "/forbidden"

<MatH3>Forbidden</MatH3>

@code {
}
```

### CommentsComponent

For the `Comments` part, we have to use the same logic but from within the methods of the `CommentsComponent.razor` file located under the `Shared` folder of our `PhotoSharingApplication.Frontend.BlazorWebAssembly` project:

```cs
@using PhotoSharingApplication.Shared.Core.Interfaces
@using PhotoSharingApplication.Shared.Core.Entities

@using PhotoSharingApplication.Shared.Core.Exceptions
@inject NavigationManager navigationManager
@inject ICommentsService CommentsService

<MatH3>Comments</MatH3>

@if (comments == null) {
  <p><em>No Comments for this Photo</em></p>
} else {
    @foreach (var comment in comments) {
      <CommentComponent CommentItem="comment" ViewMode="CommentComponent.ViewModes.Read" OnUpdate="UpdateComment" OnDelete="DeleteComment"></CommentComponent>
    }
    <AuthorizeView>
      <Authorized>
        <CommentComponent CommentItem="new Comment() {PhotoId = PhotoId}" ViewMode="CommentComponent.ViewModes.Create" OnCreate="CreateComment"></CommentComponent>
      </Authorized>
      <NotAuthorized>
        <MatButton Link="authentication/login">Log In to Comment</MatButton>
      </NotAuthorized>
    </AuthorizeView>
}
@code {
  [Parameter]
  public int PhotoId { get; set; }

  private List<Comment> comments;

  protected override async Task OnInitializedAsync() {
    comments = await CommentsService.GetCommentsForPhotoAsync(PhotoId);
  }

  async Task CreateComment(Comment comment) {
    try {
      comments.Add(await CommentsService.CreateAsync(comment));
    } catch (UnauthorizedCreateAttemptException<Comment>) {
      navigationManager.NavigateTo("/forbidden");
    }
  }

  async Task UpdateComment(Comment comment) {
    try {
      comment = await CommentsService.UpdateAsync(comment);
    } catch (UnauthorizedEditAttemptException<Comment>) {
      navigationManager.NavigateTo("/forbidden");
    }
  }

  async Task DeleteComment(Comment comment) {
    try {
      await CommentsService.RemoveAsync(comment.Id);
      comments.Remove(comment);
    } catch (UnauthorizedDeleteAttemptException<Photo>) {
      navigationManager.NavigateTo("/forbidden");
    }
  }
}
```

So far so good, we're done with the *Application Layer*, both server side and client side.  
Everything should compile, but nothig works anymore, because we're missing the code that actually checks the permission.  

Let's move to the *Infrastructure* layer, where' we're going to create classes that implement our `IUserService` and `IAuthorizationService<T>` interfaces, plus a bunch of other stuff.

## UserService

Blazor WebAssembly and ASP.NET Core have two different ways to retrieve the current User. That's why we need two different classes to implement our `IUserService`: one on the frontend and one on the backend.  

### Backend

As explained in the [Retrieve the current user in an ASP.NET Core app](https://docs.microsoft.com/en-us/aspnet/core/migration/claimsprincipal-current?view=aspnetcore-6.0#retrieve-the-current-user-in-an-aspnet-core-app) documentation, we need an `HttpContext`in order to get the current user. We can register an `IHttpContextAccessor` as a service and ask it as a dependency as explained in [Use HttpContext from custom components](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-context?view=aspnetcore-6.0#use-httpcontext-from-custom-components). The `IHttpContextAccessor` can give us access to `HttpContext` which in turn can give us the `User`.

- In the `Solution Explorer`, create a new folder `Identity` under the `PhotoSharingApplication.Backend.Infrastructure` project.
- In the `Identity` folder, create a new class. Name the class `UserService`
- Let the class implement our `IUserService` interface
- Add a constructor that asks for an `IHttpContextAccessor`, saving it into a private readonly field
- Implement the `GetUserAsync` method by returning the `HttpContext.User` property of the field saved in the constructor

The code should look like this:

```cs
using Microsoft.AspNetCore.Http;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Backend.Infrastructure.Identity {
    public class UserService : IUserService {
        private readonly IHttpContextAccessor accessor;
        public UserService(IHttpContextAccessor accessor) => this.accessor = accessor;
        public Task<ClaimsPrincipal> GetUserAsync() => Task.FromResult(accessor.HttpContext.User);
    }
}
```

The `IHttpContextAccessor` interface is to be found in the `Microsoft.AspNetCore.Http` package, which you have to add as a NuGet package.

We also need to add the `HttpContextAccessor` in the services of our Rest and gRPC applications.

### Rest

- Open the `Startup` class of the `PhotoSharingApplication.WebServices.REST.Photos` project
- Locate the `ConfigureServices` method
- Add a call to `services.AddHttpContextAccessor();`
- Add a call to `services.AddScoped<IUserService, UserService>();`  
which requires a `using PhotoSharingApplication.Backend.Infrastructure.Identity;`

### gRPC

- Open the `Startup` class of the `PhotoSharingApplication.WebServices.Grpc.Comments` project
- Locate the `ConfigureServices` method
- Add a call to `services.AddHttpContextAccessor();`
- Add a call to `services.AddScoped<IUserService, UserService>();`  
which requires a `using PhotoSharingApplication.Backend.Infrastructure.Identity;`

### Frontend
To get the current User, Blazor WebAssembly makes use of the `AuthenticationStateProvider`, as explained in the [AuthenticationStateProvider service](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-6.0#authenticationstateprovider-service) documentation.


- In the `Solution Explorer`, create a new folder `Identity` under the `PhotoSharingApplication.Frontent.Infrastructure` project.
- In the `Identity` folder, create a new class. Name the class `UserService`
- Let the class implement our `IUserService` interface
- Add a constructor that asks for an `AuthenticationStateProvider`, saving it into a private readonly field
- Implement the `GetUserAsync` method by returning the `User` property of the value returned by the `GetAuthenticationStateAsync` method of the field saved in the constructor

The code should look like this:

```cs
using Microsoft.AspNetCore.Components.Authorization;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Infrastructure.Identity {
    public class UserService : IUserService {
        private readonly AuthenticationStateProvider authenticationStateProvider;
        public UserService(AuthenticationStateProvider authenticationStateProvider) => this.authenticationStateProvider = authenticationStateProvider;
        public async Task<ClaimsPrincipal> GetUserAsync() => (await authenticationStateProvider.GetAuthenticationStateAsync()).User;
    }
}
```

You will need to add the `Microsoft.AspNetCore.Components.Authorization` NuGet package in order to find the `AuthenticationStateProvider`.

Now let's register our `UserService` as a service in our Blazor project.
- Open the `Program` class of the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project
- Add a call to `builder.Services.AddScoped<IUserService, UserService>();`  
which requires a `using PhotoSharingApplication.Frontend.Infrastructure.Identity;`

## Authorization

Now we have to tackle the `IAuthorizationService<T>` by creating our own `PhotosAuthorizationService` and `CommentsAuthorizationService` classes.
As explained in the [Resource Based Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased?view=aspnetcore-6.0) documentation, our classes can use a `Microsoft.AspNetCore.Authorization.IAuthorizationService` to check if the user is authorized to perform a specific operation.  
The Microsoft `IAuthorizationService` has an `AuthorizeAsync` method that accepts 
- a *User*  
This is the `ClaimsIdentity` returned by our `UserService`
- a *Resource*  
This is the object we're trying to secure (either a `Photo` or a `Comment` in our case)
- a *Policy*  
This a string with the name of a `Policy`. We still need to define and enforce our policies, but we already know that:
  - A `User` may create a `Photo` only if she's authenticated
  - A `User` may create a `Comment` only if she's authenticated
  - A `User` may update a `Photo` only if she's authenticated and she's the photo owner
  - A `User` may update a `Comment` only if she's authenticated and she's the comment owner
  - A `User` may delete a `Photo` only if she's authenticated and she's the photo owner
  - A `User` may delete a `Comment` only if she's authenticated and she's the photo owner  
We'll see later what a `Policy` is and how to configure it. For now we'll just need to define the policy names.

We can reuse the exact same code on both our Backend and FrontEnd, so we will introduce another shared project.

## PhotoSharingApplication.Shared.Authorization

- In the Solution Explorer, add a new project
- As a template, select `Class Library`
- As a name, type `PhotoSharingApplication.Shared.Authorization`
- Add a project reference to `PhotoSharingApplication.Shared.Core`
- Add a reference to the `Microsoft.AspNetCore.Authorization` NuGet Package

### PhotosAuthorizationService

- Add a new class. Name the class `PhotosAuthorizationService`
- Let the class implement the `IAuthorizationService<Photo>` interface
- Create a constructor with a `Microsoft.AspNetCore.Authorization.IAuthorizationService` parameter.  
Save the parameter in a readonly field.
- Implement our interface by defining its three methods
  - In the `ItemMayBeCreatedAsync`, invoke `AuthorizeAsync` method of our field, passing the user, the photo and a `Policies.CreatePhoto` constant (that we will define in our next step)
  - In the `ItemMayBeDeletedAsync`, invoke `AuthorizeAsync` method of our field, passing the user, the photo and a `Policies.DeletePhoto` constant (that we will also define in our next step)
  - In the `ItemMayBeUpdatedAsync`, invoke `AuthorizeAsync` method of our field, passing the user, the photo and a `Policies.EditPhoto` constant (that we will also define in our next step)  
Return whether the authorization succeded

The code should look like this:

```cs
using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Shared.Authorization {
    public class PhotosAuthorizationService : IAuthorizationService<Photo> {
        private readonly IAuthorizationService authorizationService;
        public PhotosAuthorizationService(IAuthorizationService authorizationService) => this.authorizationService = authorizationService;
        public async Task<bool> ItemMayBeCreatedAsync(ClaimsPrincipal User, Photo photo) => (await authorizationService.AuthorizeAsync(User, photo, Policies.CreatePhoto)).Succeeded;
        public async Task<bool> ItemMayBeDeletedAsync(ClaimsPrincipal User, Photo photo) => (await authorizationService.AuthorizeAsync(User, photo, Policies.DeletePhoto)).Succeeded;
        public async Task<bool> ItemMayBeUpdatedAsync(ClaimsPrincipal User, Photo photo) => (await authorizationService.AuthorizeAsync(User, photo, Policies.EditPhoto)).Succeeded;
    }
}
```

Don't worry if it does not compile: we don't have the `Policies` yet. We will create them at a later step.

### CommentsAuthorizationService

- Add a new class. Name the class `CommentsAuthorizationService`
- Let the class implement the `IAuthorizationService<Comment>` interface
- Create a constructor with a `Microsoft.AspNetCore.Authorization.IAuthorizationService` parameter.  
Save the parameter in a readonly field.
- Implement our interface by defining its three methods
  - In the `ItemMayBeCreatedAsync`, invoke `AuthorizeAsync` method of our field, passing the user, the comment and a `Policies.CreateComment` constant (that we will define in our next step)
  - In the `ItemMayBeDeletedAsync`, invoke `AuthorizeAsync` method of our field, passing the user, the comment and a `Policies.DeleteComment` constant (that we will also define in our next step)
  - In the `ItemMayBeUpdatedAsync`, invoke `AuthorizeAsync` method of our field, passing the user, the comment and a `Policies.EditComment` constant (that we will also define in our next step)  
Return whether the authorization succeded

The code should look like this:

```cs
using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Shared.Authorization {
    public class CommentsAuthorizationService : IAuthorizationService<Comment> {
        private readonly IAuthorizationService authorizationService;
        public CommentsAuthorizationService(IAuthorizationService authorizationService) => this.authorizationService = authorizationService;
        public async Task<bool> ItemMayBeCreatedAsync(ClaimsPrincipal User, Comment comment) => (await authorizationService.AuthorizeAsync(User, comment, Policies.CreateComment)).Succeeded;
        public async Task<bool> ItemMayBeDeletedAsync(ClaimsPrincipal User, Comment comment) => (await authorizationService.AuthorizeAsync(User, comment, Policies.DeleteComment)).Succeeded;
        public async Task<bool> ItemMayBeUpdatedAsync(ClaimsPrincipal User, Comment comment) => (await authorizationService.AuthorizeAsync(User, comment, Policies.EditComment)).Succeeded;
    }
}
```

### Policies

- Add a new class. Name the class `Policies`
- Make the class `static`
- Add public constant strings to define the names of the policies:

```cs
public const string CreatePhoto = "CreatePhoto";
public const string EditPhoto = "EditPhoto";
public const string DeletePhoto = "DeletePhoto";
public const string CreateComment = "CreateComment";
public const string EditComment = "EditComment";
public const string DeleteComment = "DeleteComment";
```

Ok, so.... what is a Policy?  
According to Wikipedia 
> A policy is a deliberate system of principles to guide decisions and achieve rational outcomes. A policy is a statement of intent, and is implemented as a procedure or protocol. 

Basically it's a sort of contract, as in *if you want to do X you have to Y*.  
We give the policy a name, then we make a list of the requirements.  
For example  
To *Enter the Pub* (name), you have to
  - *Be 18 or older* (requirement)
  - *Have a valid Id* (requirement)

Each requirement needs to be checked.  
For example the *Have a valid Id* requirement could be checked by multiple conditions, such as
- Do you have a passport?
- Do you have an Identity Card?
- Do you have a driving licence?
- Do you have a library card?

Each condition is checked by a specific *handler*

So what we need to do is to create 
- [Policies](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-6.0) 
- [Requirements](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-6.0#requirements) 
- [Handlers](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-6.0#authorization-handlers)

We're going to use the approach explained in the article [Configuring Policy-based Authorization with Blazor](https://chrissainty.com/securing-your-blazor-apps-configuring-policy-based-authorization-with-blazor/) of the great Chris Sainty.

### Building the Policies

We can use an `AuthorizationPolicyBuilder` to add our requirements and build an `AuthorizationPolicy`.

- Open the `Policies` class
- Add the following methods

```cs
public static AuthorizationPolicy MayCreatePhotoPolicy() => new AuthorizationPolicyBuilder()
  .RequireAuthenticatedUser()
  .Build();

public static AuthorizationPolicy MayEditPhotoPolicy() => new AuthorizationPolicyBuilder()
  .RequireAuthenticatedUser()
  .AddRequirements(new SameAuthorRequirement())
  .Build();

public static AuthorizationPolicy MayDeletePhotoPolicy() => new AuthorizationPolicyBuilder()
  .RequireAuthenticatedUser()
  .AddRequirements(new SameAuthorRequirement())
  .Build();

public static AuthorizationPolicy MayCreateCommentPolicy() => new AuthorizationPolicyBuilder()
  .RequireAuthenticatedUser()
  .Build();

public static AuthorizationPolicy MayEditCommentPolicy() => new AuthorizationPolicyBuilder()
  .RequireAuthenticatedUser()
  .AddRequirements(new SameAuthorRequirement())
  .Build();

public static AuthorizationPolicy MayDeleteCommentPolicy() => new AuthorizationPolicyBuilder()
  .RequireAuthenticatedUser()
  .AddRequirements(new SameAuthorRequirement())
  .Build();
```

which require a
```cs
using Microsoft.AspNetCore.Authorization;
```

Don't worry if it doesn't compile, it's just that we haven't created the `SameAuthorRequirement` yet. 

### Requirement

An authorization requirement is a collection of data parameters that a policy can use to evaluate the current user principal. A requirement must implement `IAuthorizationRequirement`. This is an empty, marker interface. 
Our requirement is very simple, we don't need any additional configuration for that.

- In the `PhotoSharingApplication.Shared.Authorization` project, add a new `Class`
- Name the class `SameAuthorRequirement`
- Let the class implement the `IAuthorizationRequirement` interface

```cs
public class SameAuthorRequirement : IAuthorizationRequirement { }
```
which requires
```cs
using Microsoft.AspNetCore.Authorization;
```

### Authorization Handlers

An *authorization handler* is responsible for the evaluation of any properties of a requirement. The authorization handler must evaluate them against a provided `AuthorizationHandlerContext` to decide if authorization is allowed. To implement *Resource based authorization*, handlers must inherit `AuthorizationHandler<TRequirement, TResource>` where `TRequirement` is the requirement it handles, `TResource` is the resource to check for ownership.

In our handler, if the current User has a Name matching the UserName of the resource (photo or comment), the requirement will be considered fulfilled.  
We will indicate it by calling the `context.Succeed()` method, passing in the requirement that has been fulfilled.

Add a `PhotoEditDeleteAuthorizationHandler` class and replace its content with the following code:

```cs
using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Core.Entities;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Shared.Authorization {
  public class PhotoEditDeleteAuthorizationHandler : AuthorizationHandler<SameAuthorRequirement, Photo> {
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SameAuthorRequirement requirement, Photo photo) {
      if (context.User.Identity?.Name == photo.UserName) {
        context.Succeed(requirement);
      }

      return Task.CompletedTask;
    }
  }
}
```

Add a `CommentEditDeleteAuthorizationHandler` class and replace its content with the following code:
```cs
using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Core.Entities;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Shared.Authorization {
  public class CommentEditDeleteAuthorizationHandler : AuthorizationHandler<SameAuthorRequirement, Comment> {
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SameAuthorRequirement requirement, Comment comment) {
      if (context.User.Identity?.Name == comment.UserName) {
        context.Succeed(requirement);
      }

      return Task.CompletedTask;
    }
  }
}
```

## Handler registration

Handlers must be registered in the services collection during configuration. We have to configure both the two servers and the client.

Each handler is added to the services collection by using ```services.AddSingleton<IAuthorizationHandler, YourHandlerClass>();``` passing in your handler class.

### PhotoSharingApplication.WebServices.REST.Photos

- In the `PhotoSharingApplication.WebServices.REST.Photos` project, add a project reference to the `PhotoSharingApplication.Shared.Authorization` project
- Open the `Startup.cs` and add this line of code at the bottom of the `ConfigureServices` method:

```cs
services.AddSingleton<IAuthorizationHandler, PhotoEditDeleteAuthorizationHandler>();
```

which requires

```cs
using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Authorization;
```

### PhotoSharingApplication.WebServices.Grpc.Comments

- In the `PhotoSharingApplication.WebServices.Grpc.Comments` project, add a project reference to the `PhotoSharingApplication.Shared.Authorization` project
- Open the `Startup.cs` and add this line of code at the bottom of the `ConfigureServices` method:

```cs
services.AddSingleton<IAuthorizationHandler, CommentEditDeleteAuthorizationHandler>();
```

which requires

```cs
using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Authorization;
```

### PhotoSharingApplication.Frontend.BlazorWebAssembly

- In the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project, a dd a project reference to the `PhotoSharingApplication.Shared.Authorization` project
- Open the `Program.cs` and add this line of code in the `Main` method, before `await builder.Build().RunAsync();`:

```cs
builder.Services.AddSingleton<IAuthorizationHandler, PhotoEditDeleteAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, CommentEditDeleteAuthorizationHandler>();
```

which requires

```cs
using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Authorization;
```

## Authorization Policy Configuration

An *authorization policy* is registered at application startup as part of the Authorization service configuration. We have to configure both the two servers and the client. Let's first create extension methods to add the policies. We'll use them at a later step.

- Open the `Policies` class
- Add the following extension methods:

```cs
public static void AddPhotosPolicies(this AuthorizationOptions options) {
  options.AddPolicy(EditPhoto, MayEditPhotoPolicy());
  options.AddPolicy(DeletePhoto, MayDeletePhotoPolicy());
  options.AddPolicy(CreatePhoto, MayCreatePhotoPolicy());
}

public static void AddCommentsPolicies(this AuthorizationOptions options) {
  options.AddPolicy(CreateComment, MayCreateCommentPolicy());
  options.AddPolicy(EditComment, MayEditCommentPolicy());
  options.AddPolicy(DeleteComment, MayDeleteCommentPolicy());
}
```

Now let's add this policies on the backend and on the frontend.

### PhotoSharingApplication.WebServices.REST.Photos

- Open the `Startup.cs` of the `PhotoSharingApplication.WebServices.REST.Photos` project and replace this code of the `ConfigureServices` method:


```cs
services.AddAuthorization(options => {
  options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy => {
    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
    policy.RequireClaim(JwtClaimTypes.Name);
  });
});
```

with this code:

```cs
services.AddAuthorization(options => {
  options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy => {
    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
    policy.RequireClaim(JwtClaimTypes.Name);
  });
  
  options.AddPhotosPolicies();
});
```

### PhotoSharingApplication.WebServices.Grpc.Comments

- Open the `Startup.cs` and replace this code of the `ConfigureServices` method:

```cs
services.AddAuthorization(options => {
  options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy => {
    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
    policy.RequireClaim(JwtClaimTypes.Name);
  });
});
```
with this code:

```cs
services.AddAuthorization(options => {
  options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy => {
    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
    policy.RequireClaim(JwtClaimTypes.Name);
  });
  
  options.AddCommentsPolicies();
});
```

### PhotoSharingApplication.Frontend.BlazorWebAssembly

- In the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project, add a project reference to `PhotoSharingApplication.Shared.Authorization`
- Open the `Program.cs` and add this lines of code in the `Main` method

```cs
builder.Services.AddAuthorizationCore(options => {
  options.AddPhotosPolicies();
  options.AddCommentsPolicies();
});
```
which requires a 
```cs
using PhotoSharingApplication.Shared.Authorization;
```

## IAuthorizationService registration

Now let's register our `Authorization` as a service in our Blazor, Rest and gRpc projects.
- Blazor
  - Open the `Program` class of the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project
  - Add a call to `builder.Services.AddScoped<IAuthorizationService<Comment>, CommentsAuthorizationService>();`
  - Add a call to `builder.Services.AddScoped<IAuthorizationService<Photo>, PhotosAuthorizationService>();`  
  - Add a `using PhotoSharingApplication.Shared.Core.Entities;`
- REST
  - Open the `Startup` class of the `PhotoSharingApplication.WebServices.REST.Photos` project
  - Locate the `ConfigureServices` and add a call to the `services.AddScoped<IAuthorizationService<Comment>, CommentsAuthorizationService>();`
  - Add a `using PhotoSharingApplication.Shared.Core.Entities;`
-gRpc
  - Open the `Startup` class of the `PhotoSharingApplication.WebServices.Grpc.Comments` project
  - Locate the `ConfigureServices` and add a call to the `services.AddScoped<IAuthorizationService<Comment>, CommentsAuthorizationService>();`
  - Add a `using PhotoSharingApplication.Shared.Core.Entities;`

### Backend Infrastructure - PhotosRepository

To avoid exceptions, let's change our repositories server side to not keep track of the Photo (and Comment) when we read the data.

- Under the `EntityFramework` subfolder of the `Repositories` folder of the `PhotoSharingApplication.Backend.Infrastructure` project, open the `PhotosRepository` class
- Modify the `FindAsync` method as follows

```cs
public async Task<Photo> FindAsync(int id) => await context.Photos.AsNoTracking().SingleOrDefaultAsync(m => m.Id == id);
```

### Backend Infrastructure - CommentsRepository

- Under the `EntityFramework` subfolder of the `Repositories` folder of the `PhotoSharingApplication.Backend.Infrastructure` project, open the `CommentsRepository` class
- Modify the `FindAsync` method as follows

```cs
public async Task<Comment> FindAsync(int id) => await context.Comments.AsNoTracking().SingleOrDefaultAsync(m => m.Id == id);
```

If you run the application now, you should be able to create, edit and delete photos and comments only under the right conditions. You should get a `Forbidden` page whenever you try to perform and unauthorized operation.  
This means we achieved our goal to secure the update and delete using resource based authorization. 

## Cosmetics

We still see the buttons to delete and update, though, so let's take care of that.  
We can do that using the same `IUserService` and `IAuthorizationService<T>` that we used in the services.  

### PhotoDetailsComponent

- Under the `PhotoSharingApplication.Frontend.BlazorComponents` project, open the `PhotoDetailsComponent.razor` component.
- Ask for an `IUserService` and an `IAuthorizationService<Photo>`
- Add two boolean variables `mayEdit` and `mayDelete`
- During initialization, 
  - retrieve the user
  - invoke the `ItemMayBeUpdatedAsync` of the authorizationservice and set the `mayEdit` variable with the result
  - invoke the `ItemMayBeDeletedAsync` of the authorizationservice and set the `mayDelete` variable with the result
- Use those two variables to decide whether you should render the two corresponding buttons in the template


The code should look like the following:

```html
@using PhotoSharingApplication.Shared.Core.Entities
@using PhotoSharingApplication.Shared.Core.Interfaces;

@inject IUserService UserService
@inject IAuthorizationService<Photo> PhotosAuthorizationService

<MatCard>
  <div>
    <MatHeadline6>
      @Photo.Id - @Photo.Title
    </MatHeadline6>
  </div>
  <MatCardContent>
    <PhotoPictureComponent Photo="Photo"></PhotoPictureComponent>
    <MatBody2>
      @Photo.Description
    </MatBody2>
    <MatSubtitle2>
      Posted on @Photo.CreatedDate.ToShortDateString() by @Photo.UserName
    </MatSubtitle2>
  </MatCardContent>
  <MatCardActions>
    <MatCardActionButtons>
      @if (Details) {
        <MatButton Link="@($"photos/details/{Photo.Id}")">Details</MatButton>
      }
      @if (Edit && mayEdit) {
        <MatButton Link="@($"photos/update/{Photo.Id}")">Update</MatButton>
      }
      @if (Delete && mayDelete) {
        <MatButton Link="@($"photos/delete/{Photo.Id}")">Delete</MatButton>
      }
      @if (DeleteConfirm && mayDelete) {
        <MatButton OnClick="@(async()=> await OnDeleteConfirmed.InvokeAsync(Photo.Id))">Confirm Deletion</MatButton>
      }
    </MatCardActionButtons>
  </MatCardActions>
</MatCard>

@code {
  [Parameter]
  public Photo Photo { get; set; }

  [Parameter]
  public bool Details { get; set; }

  [Parameter]
  public bool Edit { get; set; }

  [Parameter]
  public bool Delete { get; set; }

  [Parameter]
  public bool DeleteConfirm { get; set; }

  [Parameter]
  public EventCallback<int> OnDeleteConfirmed { get; set; }

  bool mayEdit = false;
  bool mayDelete = false;

  protected override async Task OnInitializedAsync() {
    var User = await UserService.GetUserAsync();
    mayEdit = await PhotosAuthorizationService.ItemMayBeUpdatedAsync(User, Photo);
    mayDelete = await PhotosAuthorizationService.ItemMayBeDeletedAsync(User, Photo);
  }
}
```

### CommentReadComponent

- Under the `PhotoSharingApplication.Frontend.BlazorComponents` project, open the `CommentReadComponent.razor` component
- Ask for an `IUserService` and an `IAuthorizationService<Photo>`
- Add two boolean variables `mayEdit` and `mayDelete`
- During initialization, 
  - retrieve the user
  - invoke the `ItemMayBeUpdatedAsync` of the authorizationservice and set the `mayEdit` variable with the result
  - invoke the `ItemMayBeDeletedAsync` of the authorizationservice and set the `mayDelete` variable with the result
- Use those two variables to decide whether you should render the two corresponding buttons in the template


The code should look like the following:


```html
@using PhotoSharingApplication.Shared.Core.Entities
@using PhotoSharingApplication.Shared.Core.Interfaces;

@inject IUserService UserService
@inject IAuthorizationService<Comment> CommentsAuthorizationService
<MatCardContent>
  <em>On @CommentItem.SubmittedOn.ToShortDateString() At @CommentItem.SubmittedOn.ToShortTimeString(), @CommentItem.UserName said:</em>
  <MatH5>@CommentItem.Subject</MatH5>
  <p>@CommentItem.Body</p>
</MatCardContent>
<MatCardActions>
  @if (mayEdit) {
      <MatButton OnClick="RaiseEdit">Edit</MatButton>
  }
  @if (mayDelete) {
      <MatButton OnClick="RaiseDelete">Delete</MatButton>
  }
</MatCardActions>

@code {
  [Parameter]
  public Comment CommentItem { get; set; }

  [Parameter]
  public EventCallback<Comment> OnEdit { get; set; }

  [Parameter]
  public EventCallback<Comment> OnDelete { get; set; }

  bool mayEdit = false;
  bool mayDelete = false;

  protected override async Task OnInitializedAsync() {
    var User = await UserService.GetUserAsync();
    mayEdit = await CommentsAuthorizationService.ItemMayBeUpdatedAsync(User, CommentItem);
    mayDelete = await CommentsAuthorizationService.ItemMayBeDeletedAsync(User, CommentItem);
  }

  async Task RaiseEdit(MouseEventArgs args) => await OnEdit.InvokeAsync(CommentItem);
  async Task RaiseDelete(MouseEventArgs args) => await OnDelete.InvokeAsync(CommentItem);
}
```

If you run the application, you should see the update and delete button only on photos and comments created by the logged on user.

We're done with the security bit.  
Next: some performance improvements.