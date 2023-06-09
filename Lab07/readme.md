# FrontEnd: Comments

If you followed Lab06, you can continue from your own project. Otherwise, open the `Lab07/Start` solution and continue from there.

---

Now that we took care of the Photos, we will proceed to implement the Comments functionality. Our `Details` Page will allow the user to 

- See the Comments related to the Photo
- Post a  new Comment
- Update an existing Comment
- Delete an existing Comment

We will also create a first C# Data Layer with methods to 
- get a list of all the comments for a given photo
- get one comment given its id
- create a given comment
- update a given comment
- delete a comment given its id 

For now, our data layer will be a prototype that works with a List in memory, since we want to focus on the UI.
We will replace it with one that can communicate with a gRpc service in a later lab, when we start thinking about the backend.

We're going to continue with the [CLEAN architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) that we already have.

- In The *Shared* project we will introduce the *Comment* entity and the interfaces for the *CommentsService* and the *CommentsRepository*
- In the *Core* we will define the business logic for the *CommentsService*
- In the *Infrastructure* we will define a *CommentsRepository*. For now we have a simple Repository class that uses a List. Later we'll talk to a gRpc service

## The Shared Code

We are going to define two interfaces: one for an `ICommentsService` and one for an `ICommentsRepository`. Our interfaces will look very similar and will contain the definitions for the method to Create, Read, Update and Delete photos. Both are going to use `Comment` entity, that we also have to define in this project. 

### The Comment Entity

- In the `Shared` project, under the `Entities` folder, add the following `Comment` class:

```cs
namespace PhotoSharingApplication.Shared.Entities;

public class Comment {
  public int Id { get; set; }
  public int PhotoId { get; set; }
  public string UserName { get; set; } = String.Empty;
  public string Subject { get; set; } = String.Empty;
  public string Body { get; set; } = String.Empty;
  public DateTime SubmittedOn { get; set; }
}
```

### The Interfaces

- Under the `Interfaces` folder, add the following `ICommentsService` interface:

```cs
using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.Shared.Interfaces;

public interface ICommentsService {
  Task<List<Comment>?> GetCommentsForPhotoAsync(int photoId);
  Task<Comment?> FindAsync(int id);
  Task<Comment?> CreateAsync(Comment comment);
  Task<Comment?> UpdateAsync(Comment comment);
  Task<Comment?> RemoveAsync(int id);
}
```

- Under the same folder, add the following `ICommentsRepository` interface

```cs
using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.Shared.Interfaces;

public interface ICommentsRepository {
  Task<List<Comment>?> GetCommentsForPhotoAsync(int photoId);
  Task<Comment?> FindAsync(int id);
  Task<Comment?> CreateAsync(Comment comment);
  Task<Comment?> UpdateAsync(Comment comment);
  Task<Comment?> RemoveAsync(int id);
}
```

## The Frontend Client

Now we can implement our service, which for now will just pass the data to the repository and return the results, without any additional logic (we will replace it later). We are going to use the [Dependency Injection pattern](https://martinfowler.com/articles/injection.html?) to request for a repository.

In the `PhotoSharingApplication.Frontend.Client`, under the `Core/Services` folder, add a new `CommentsServiceRepository` class.

```cs
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.Frontend.Client.Core.Services {
  public class CommentsService : ICommentsService {
    private readonly ICommentsRepository repository;
    public CommentsService(ICommentsRepository repository) => this.repository = repository;

    public async Task<Comment?> CreateAsync(Comment comment) => await repository.CreateAsync(comment);

    public async Task<Comment?> FindAsync(int id) => await repository.FindAsync(id);

    public async Task<List<Comment>?> GetCommentsForPhotoAsync(int photoId) => await repository.GetCommentsForPhotoAsync(photoId);

    public async Task<Comment?> RemoveAsync(int id) => await repository.RemoveAsync(id);

    public async Task<Comment?> UpdateAsync(Comment comment) {
      comment.SubmittedOn = DateTime.Now;
      return await repository.UpdateAsync(comment);
    }
  }
}
```

Of course nothing is actually *working*, but we can already start plugging our service to our UI.

To use our service in the `Details` page, we need to perform a couple of steps, also described in the [Blazor Dependency Injection documentation](https://learn.microsoft.com/en-gb/aspnet/core/blazor/fundamentals/dependency-injection?view=aspnetcore-7.0&pivots=webassembly)

### Add the service to the app

[In the docs](https://learn.microsoft.com/en-gb/aspnet/core/blazor/fundamentals/dependency-injection?view=aspnetcore-7.0&pivots=webassembly#add-services-to-a-blazor-webassembly-app) they tell us what to do: 

- Open the `Program.cs` file of the `PhotoSharingApplication.Frontend.Client`project
- Add the following code, before the `await builder.Build().RunAsync();`

```cs
builder.Services.AddScoped<ICommentsService, CommentsService>();
```

## The Frontend Infrastructure

- In the `Infrastructure/Repositories/Memory` folder, of the `PhotoSharingApplication.Frontend.Client` project, add a new class `CommentsRepository` with the following code

```cs
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Memory;
public class CommentsRepository : ICommentsRepository {
  private List<Comment> comments;
  public CommentsRepository() {
    comments = new() {
      new() { Id = 1, Subject = "A Comment", Body = "The Body of the comment", SubmittedOn = DateTime.Now.AddDays(-1), PhotoId = 1 },
      new() { Id = 2, Subject = "Another Comment", Body = "Another Body of the comment", SubmittedOn = DateTime.Now.AddDays(-2), PhotoId = 1 },
      new() { Id = 3, Subject = "Yet another Comment", Body = "Yet Another Body of the comment", SubmittedOn = DateTime.Now, PhotoId = 2 },
      new() { Id = 4, Subject = "More Comment", Body = "More Body of the comment", SubmittedOn = DateTime.Now.AddDays(-3), PhotoId = 2 }
    };
  }
  public Task<Comment?> CreateAsync(Comment comment) {
    comment.Id = comments.Max(p => p.Id) + 1;
    comments.Add(comment);
    return Task.FromResult(comment);
  }

  public Task<Comment?> FindAsync(int id) => Task.FromResult(comments.FirstOrDefault(p => p.Id == id));

  public Task<List<Comment>?> GetCommentsForPhotoAsync(int photoId) => Task.FromResult(comments.Where(c => c.PhotoId == photoId).OrderByDescending(c => c.SubmittedOn).ThenBy(c => c.Subject).ToList());

  public Task<Comment?> RemoveAsync(int id) {
    Comment? comment = comments.FirstOrDefault(c => c.Id == id);
    if (comment is not null) comments.Remove(comment);
    return Task.FromResult(comment);
  }

  public Task<Comment?> UpdateAsync(Comment comment) {
    Comment? oldComment = comments.FirstOrDefault(c => c.Id == comment.Id);
    if (oldComment is not null) {
      oldComment.Subject = comment.Subject;
      oldComment.Body = comment.Body;
    }
    return Task.FromResult(oldComment);
  }
}

```

I know, I know, it's a very naive implementation, but it's just to have something working so that we can see some action in the UI, we're going to replace it with something better later anyway.

Our last step is to plug this implementation in our application, so that the CommentsService can use it. We do this simply by registering this class as a service during startup.

- Open the `Program.cs` file of the `PhotoSharingApplication.Frontend.BlazorWebAssembly`project
- Add the following code, before the `await builder.Build().RunAsync();`

```cs
builder.Services.AddScoped<ICommentsRepository, PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Memory.CommentsRepository>();
```

## The Comments UI

We want to enrich the `PhotoDetails` Page by 

- Retreiving the comments of a `Photo`, by talking to the `CommentsService` 
- Displaying the details of each comment 
- Displaying a form to submit a new comment

We could do this directly on the `PhotoDetails` Page, but it would start getting a bit too crowded and hard to maintain.
We shouldn't give too many responsibilities to the `PhotoDetails` page.

We can split the functionalities by creating a `CommentsComponent` and referring to it from within the `PhotoDetails` page.

The `CommentsComponent` will receive the Id of the Photo and take care of the rest. The only thing we need to add to the `PhotoDetails.razor` page of the `PhotoSharingApplication.Frontend.Client` project is the `CommentsComponent` tag, passing the `PhotoId` as a property, which we will use to retrieve the comments.

```html
@if (photo is null) {
    <MudText Typo="Typo.body1">...Loading...</MudText>
} else {
    <PhotoDetailsComponent Photo="photo" Edit Delete />
    <CommentsComponent PhotoId="Id"></CommentsComponent>
}
```

## The CommentsComponent

Let's put the new `CommentsComponent.razor` in the `Components` folder of the `PhotoSharingApplication.Frontend.BlazorComponents` project.

Here, we will provide a `[Parameter]` to get the `PhotoId`.

```cs
@code {
  [Parameter, EditorRequired]
  public int PhotoId { get; set; }
}
```

We will also get the dependency on the `CommentsService` and initialize a `List` of comments.

```cs
@using PhotoSharingApplication.Shared.Entities;
@using PhotoSharingApplication.Shared.Interfaces;
@inject ICommentsService CommentsService

<MudText Typo="Typo.h3">Comments</MudText>

@code {
    [Parameter, EditorRequired]
    public int PhotoId { get; set; }

    private List<Comment>? comments = default!;

    protected override async Task OnInitializedAsync() {
        comments = await CommentsService.GetCommentsForPhotoAsync(PhotoId);
    }
}
```

If there are comments, we will scroll through the list and render the details of each comment. We will also display a form to submit a new comment.

Once again, we don't want to have this bit too crowded, so let's think about a `Comment` component capable of changing its own appearance in place.

Our `CommentComponent` will have a `Property` *`ViewMode`* with four different possible states:

- Read
- Edit
- Delete
- Create

Depending on the value of this property, the component will render specific HTML for specific functionalities. In each mode we will have buttons to switch to a different mode if necessary. 

Let's first use it from our `CommentsComponent`, which becomes:

```html
<MudText Typo="Typo.h3">Comments</MudText>

@if (comments is null) {
    <MudText Typo="Typo.body1">No comments for this photo yet</MudText>
} else {
    @foreach (var comment in comments) {
        <CommentComponent CommentItem="comment" ViewMode="CommentComponent.ViewModes.Read"></CommentComponent>
    }
    <CommentComponent CommentItem="new Comment() {PhotoId = PhotoId}" ViewMode="CommentComponent.ViewModes.Create"></CommentComponent>
}
```

## The CommentComponent

We can create the `CommentComponent` in the `PhotoSharingApplication.Frontend.BlazorComponents` project.

We will accept a `[Parameter]` for the `Comment` and a `[Parameter]` for the *ViewMode*. The `ViewMode` parameter will be of a new `enum` type that we will define in the component.

```cs
<MatH4>Comment</MatH4>

@code {
  [Parameter, EditorRequired]
  public Comment CommentItem { get; set; } = default!;

  [Parameter]
  public ViewModes ViewMode { get; set; } = ViewModes.Read;

  public enum ViewModes {
    Read, Edit, Delete, Create
  }
}
```

When the component is initialized, we will make our own copy of the CommentItem that we got as a parameter
Our `CommentComponent` will render a different subcomponent depending on the `ViewMode`:

```html
@if (ViewMode == ViewModes.Read) {
  <CommentReadComponent CommentItem="CommentItem" />
} else if (ViewMode == ViewModes.Edit) {
  <CommentEditComponent CommentItem="CommentItem" />
} else if (ViewMode == ViewModes.Delete) {
  <CommentDeleteComponent CommentItem="CommentItem" />
} else if (ViewMode == ViewModes.Create) {
  <CommentCreateComponent CommentItem="CommentItem" />
}
```

Each of these component will render the correct HTML and  it will  raise events that we will handle here, either to switch view or to let the page know that it's time to talk to the service to create / update / delete a comment.

```html
@if (ViewMode == ViewModes.Read) {
  <CommentReadComponent CommentItem="CommentItem" OnEdit="SwitchToEditMode" OnDelete="SwitchToDeleteMode"/>
} else if (ViewMode == ViewModes.Edit) {
    <CommentEditComponent CommentItem="CommentItem" OnSave="ConfirmUpdate" OnCancel="SwitchToReadMode" />
} else if (ViewMode == ViewModes.Delete) {
  <CommentDeleteComponent CommentItem="CommentItem" OnDelete="ConfirmDelete" OnCancel="SwitchToReadMode"/>
} else if (ViewMode == ViewModes.Create) {
  <CommentCreateComponent CommentItem="CommentItem" OnSave="ConfirmCreate"/>
}
```

The code to handle these events will switch ViewMode eventually after notifying the parent component. We will also need some `EventCallback` to notify the parent component.

The code to switch view mode is fairly simple:

```cs
private void SwitchToReadMode() => ViewMode = ViewModes.Read;
private void SwitchToEditMode() => ViewMode = ViewModes.Edit;
private void SwitchToDeleteMode() => ViewMode = ViewModes.Delete;
```

The `ConfirmUpdate` will notify the parent component then switch to read mode:

```cs
[Parameter]
public EventCallback<Comment> OnUpdate { get; set; }

private async Task ConfirmUpdate(Comment comment) {
    await OnUpdate.InvokeAsync(comment);
    SwitchToReadMode();
}
```

The `ConfirmDelete` will notify the parent component then switch to read mode: 

```cs
[Parameter]
public EventCallback<Comment> OnDelete { get; set; }

private async Task ConfirmDelete(Comment comment) {
    await OnDelete.InvokeAsync(comment);
    SwitchToReadMode();
}
```

The `ConfirmCreate` will notify the parent component then create a new comment to reset the fields.

```cs
[Parameter]
public EventCallback<Comment> OnCreate { get; set; }

private async Task ConfirmCreate(Comment comment) {
    await OnCreate.InvokeAsync(comment);
}
```

That's all for this component, so let's create the four sub components.

## The CommentReadComponent

In the `PhotoSharingApplication.Frontend.BlazorComponents` project, create a new `CommentReadComponent.razor` Razor  Component.  
This component shows a `Card` with the details of the comment and two buttons to switch to Edit and Delete mode.  
Its logic only notifies its parent component.  

```cs
<MudCard>
    <MudCardContent>
        <MudText Typo="Typo.subtitle2">On @CommentItem.SubmittedOn.ToShortDateString() At @CommentItem.SubmittedOn.ToShortTimeString(), @CommentItem.UserName said:</MudText>
        <MudText Typo="Typo.body1">@CommentItem.Subject</MudText>
        <MudText Typo="Typo.body2">@CommentItem.Body</MudText>
    </MudCardContent>
    <MudCardActions>
        <MudIconButton Icon="@Icons.Material.Filled.Edit" OnClick="RaiseEdit" />
        <MudIconButton Icon="@Icons.Material.Filled.Delete" Color="Color.Warning" OnClick="RaiseDelete" />
    </MudCardActions>
</MudCard>

@code {
    [Parameter, EditorRequired]
    public Comment CommentItem { get; set; } = default!;

    [Parameter]
    public EventCallback<Comment> OnEdit { get; set; }
    [Parameter]
    public EventCallback<Comment> OnDelete { get; set; }

    async Task RaiseEdit(MouseEventArgs args) => await OnEdit.InvokeAsync(CommentItem);
    async Task RaiseDelete(MouseEventArgs args) => await OnDelete.InvokeAsync(CommentItem);
}
```

## The CommentEditComponent

In the `PhotoSharingApplication.Frontend.BlazorComponents` project, create a `CommentEditComponent.razor` Razor Component.  
This component will have a `MudForm` with two TextFields (for `Subject` and `Body`) and two buttons (for `Update` and `Cancel`).  
The logic will notify the parent component through the use of event callbacks.  
The most important thing is that this component will make its own copy of the comment it receives from the parent and binds the textfields to its own copy.

```cs
<MudCard>
    <MudForm Model="originalComment">
        <MudCardContent>
            <MudTextField @bind-Value="originalComment.Subject"
                          For="@(() => originalComment.Subject)"
                          Label="Title" />
            <MudTextField @bind-Value="originalComment.Body"
                          Lines="3"
                          For="@(() => originalComment.Body)"
                          Label="Description" />
        </MudCardContent>
    </MudForm>
    <MudCardActions>
        <MudIconButton Color="Color.Primary" Icon="@Icons.Material.Filled.NavigateBefore" OnClick="@(async ()=> await OnCancel.InvokeAsync(originalComment))">Cancel</MudIconButton>
        <MudIconButton Color="Color.Primary" Icon="@Icons.Material.Filled.Check" OnClick="@(async ()=> await OnSave.InvokeAsync(originalComment))">Update</MudIconButton>
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

    protected override void OnInitialized() {
        originalComment = new Comment { Id = CommentItem.Id, PhotoId = CommentItem.PhotoId, Subject = CommentItem.Subject, Body = CommentItem.Body, SubmittedOn = CommentItem.SubmittedOn, UserName = CommentItem.UserName };
    }
}
```
## The CommentDeleteComponent

In the `PhotoSharingApplication.Frontend.BlazorComponents` project, create a `CommentDeleteComponent.razor` Razor Component.  
The `CommentDeleteComponent` will show the details of the comment and it will provide two buttons to confirm deletion and to cancel.  
Its logic will notify the parent component through `EventCallback`s.  

```cs
<MudCard>
    <MudCardContent>
        <MudText Typo="Typo.subtitle2">On @CommentItem.SubmittedOn.ToShortDateString() At @CommentItem.SubmittedOn.ToShortTimeString(), @CommentItem.UserName said:</MudText>
        <MudText Typo="Typo.body2">@CommentItem.Subject</MudText>
    </MudCardContent>
    <MudCardActions>
        <MudIconButton Icon="@Icons.Material.Filled.NavigateBefore" OnClick="RaiseCancel" />
        <MudIconButton Icon="@Icons.Material.Filled.DeleteForever" Color="Color.Error" OnClick="RaiseDelete" />
    </MudCardActions>
</MudCard>

@code {
    [Parameter, EditorRequired]
    public Comment CommentItem { get; set; } = default!;

    [Parameter]
    public EventCallback<Comment> OnCancel { get; set; }
    [Parameter]
    public EventCallback<Comment> OnDelete { get; set; }

    async Task RaiseCancel(MouseEventArgs args) => await OnCancel.InvokeAsync(CommentItem);
    async Task RaiseDelete(MouseEventArgs args) => await OnDelete.InvokeAsync(CommentItem);
}
```
## The CommentCreateComponent

In the `PhotoSharingApplication.Frontend.BlazorComponents` project, create a `CommentCreateComponent.razor` Razor Component.  
The `CommentCreateComponent` will have an `MudForm` with two TextFields (for `Subject` and `Body`) and one button for `Save`.  
The logic will notify the parent component through the use of an `EventCallback`.  
The most important thing is that this component will make its own copy of the comment it receives from the parent and binds the textfields to its own copy.  

```cs
<MudCard>
    <MudForm Model="originalComment">
        <MudCardContent>
            <MudTextField @bind-Value="originalComment.Subject"
                          For="@(() => originalComment.Subject)"
                          Label="Title" />
            <MudTextField @bind-Value="originalComment.Body"
                          Lines="3"
                          For="@(() => originalComment.Body)"
                          Label="Description" />
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

    protected override void OnInitialized() {
        originalComment = new Comment { Id = CommentItem.Id, PhotoId = CommentItem.PhotoId, Subject = CommentItem.Subject, Body = CommentItem.Body, SubmittedOn = CommentItem.SubmittedOn, UserName = CommentItem.UserName };
    }

    async Task RaiseCreate(MouseEventArgs args) {
        await OnSave.InvokeAsync(originalComment);
        originalComment = new Comment { Id = CommentItem.Id, PhotoId = CommentItem.PhotoId, Subject = CommentItem.Subject, Body = CommentItem.Body, SubmittedOn = CommentItem.SubmittedOn, UserName = CommentItem.UserName };
    }
}
```

The last thing we need to do is to handle the actual `OnCreate`, `OnUpdate` and `OnDelete` events in the `CommentsComponent` to actually invoke the methods of the `CommentsService`.

Open the `CommentsComponent` and replace its content with the following code:


```cs
@using PhotoSharingApplication.Shared.Entities;
@using PhotoSharingApplication.Shared.Interfaces;
@inject ICommentsService CommentsService

<MudText Typo="Typo.h3">Comments</MudText>

@if (comments is null) {
    <MudText Typo="Typo.body1">No comments for this photo yet</MudText>
} else {
    @foreach (var comment in comments) {
        <CommentComponent CommentItem="comment" ViewMode="CommentComponent.ViewModes.Read" OnUpdate="UpdateComment" OnDelete="DeleteComment"/>
    }
    <CommentComponent CommentItem="new Comment() {PhotoId = PhotoId}" ViewMode="CommentComponent.ViewModes.Create" OnCreate="CreateComment"/>
}

@code {
    [Parameter, EditorRequired]
    public int PhotoId { get; set; }

    private List<Comment>? comments = default!;

    protected override async Task OnInitializedAsync() {
        comments = await CommentsService.GetCommentsForPhotoAsync(PhotoId);
    }
    async Task CreateComment(Comment comment) {
        comments.Add(await CommentsService.CreateAsync(comment));
    }

    async Task UpdateComment(Comment comment) {
        comment = await CommentsService.UpdateAsync(comment);
    }

    async Task DeleteComment(Comment comment) {
        await CommentsService.RemoveAsync(comment.Id);
        comments.Remove(comment);
    }
}
```

This way we stay client side and we get a list of photos that have comments.

- Open `Program.cs` of the `PhotoSharingApplication.Frontend.Client` project 
- Comment the following line

```cs
// builder.Services.AddScoped<IPhotosRepository, PhotoSharingApplication.Frontend.Infrastructure.Repositories.Rest.PhotosRepository>();
```
- Add the following line
```cs
builder.Services.AddScoped<IPhotosRepository, PhotoSharingApplication.Frontend.Infrastructure.Repositories.Memory.PhotosRepository>();
```

If you run the application now and navigate to `/photos/details/1` you should see the comments under the Photo details.

Clicking on the different buttons should switch view and perform the relative actions.

That's it for this lab, our frontend is ready for now.

---

In the following lab, we're taking care of the backend. We will build the functionalities to save the comments in the database and we will serve them through a gRpc Service.

Go to `Labs/Lab08`, open the `readme.md` and follow the instructions thereby contained.