# FrontEnd: Additional Views

If you followed Lab01, you can continue from your own project. Otherwise, open the `Lab02/Start` solution and continue from there.

---

Now that we have a Blazor project with our first AllPhotos, we will proceed to create four additional pages:
- Details
- Create
- Update
- Delete

We will also create a first C# Data Layer with methods to 
- get a list of all the photos
- get one photo given its id
- create a given photo
- update a given photo
- delete a photo given its id 

For now, our data layer will be a prototype that works with a List in memory, since we want to focus on the UI.
We will replace it with one that can communicate with a REST service in a later lab, when we start thinking about the backend.

Because we already know that we're going to replace some code with something else, let's use an architecture that can ease the transition. It's going to take a bit more work at the beginning, but our efforts will be repayed at the end.

The architecture I'm talking about is something like the [CLEAN architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html).

If you want to see something like that in action (although this project uses it server side, but that's not the point), take a look at [eShop on Web](https://github.com/dotnet-architecture/eShopOnWeb/tree/master/src/Web)

Anyway, my idea is to create 
- A *Core* project where we define the business logic. There's going to be a Service that knows what to do (for example it validates the data before passing it to the infrastructure) 
- An *Infrastructure* project where we define how to actually read and save the data. For now we have a simple Repository class that uses a List. Later we'll talk to a REST service

Let's start with the Core project, the one that knows the business logic.

## The Frontend Core

- On the `Solution Explorer`, right click you solution, then select `Add` -> `New Project`.
- Select `Class Library`. Click `Next`
- In the  `Project Name` field, type `PhotoSharingApplication.Frontend.Core`
- Be sure to select the latest version of .Net (6.0 Preview)
- Click `Create`

We are going to define two interfaces: one for an IPhotosService and one for an IPhotosRepository. Our interfaces will look very similar and will contain the definitions for the method to Create, Read, Update and Delete photos. Both are going to use Photo entities, that we also have to define in this project. 

So let's create a folder `Entities` and let's create a `Photo` class in this new folder:

```cs
public class Photo {
  public int Id { get; set; }
  public string Title { get; set; }
  public byte[] PhotoFile { get; set; }
  public string ImageMimeType { get; set; }
  public string Description { get; set; }
  public DateTime CreatedDate { get; set; }
  public string UserName { get; set; }
}
```

Now let's add an `Interfaces` folder and create an interface for the `IPhotosService`:

```cs
public interface IPhotosService {
  Task<Photo> UploadAsync(Photo photo);
  Task<Photo> UpdateAsync(Photo photo);
  Task<Photo> FindAsync(int id);
  Task<List<Photo>> GetPhotosAsync(int amount = 10);
  Task<Photo> RemoveAsync(int id);
}
```

Don't forget to add the correct `using`:

```cs
using PhotoSharingApplication.Frontend.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
```

In the same folder, let's also create an `IPhotosRepository` interface:

```cs
public interface IPhotosRepository  {
  Task<List<Photo>> GetPhotosAsync(int amount = 10);
  Task<Photo> FindAsync(int id);
  Task<Photo> CreateAsync(Photo photo);
  Task<Photo> UpdateAsync(Photo photo);
  Task<Photo> RemoveAsync(int id);
}
```

Again, don't forget the `using`:

```cs
using PhotoSharingApplication.Frontend.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
```

Now we can implement our service, which for now will just pass the data to the repository and return the results, without any additional logic (we will replace it later). We are going to use the [Dependency Injection pattern](https://martinfowler.com/articles/injection.html?) to request for a repository.
In a new folder `Services`, add the following class:

```cs
public class PhotosService : IPhotosService {
  private readonly IPhotosRepository repository;
  public PhotosService(IPhotosRepository repository) => this.repository = repository;
  public async Task<Photo> FindAsync(int id) => await repository.FindAsync(id);
  public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await repository.GetPhotosAsync(amount);
  public async Task<Photo> RemoveAsync(int id) => await repository.RemoveAsync(id);
  public async Task<Photo> UpdateAsync(Photo photo) => await repository.UpdateAsync(photo);
  public async Task<Photo> UploadAsync(Photo photo) {
    photo.CreatedDate = DateTime.Now;
    return await repository.CreateAsync(photo);
  }
}
```

Once again, don't forget the `using`:

```cs
using PhotoSharingApplication.Frontend.Core.Entities;
using PhotoSharingApplication.Frontend.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
```

Of course nothing is actually *working*, but we can already start plugging our service to our UI.

- On the `Solution Explorer`, right click on the `Dependencies` folder of the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project
- Select `Add Project Reference`
- Check the checkbox next to `PhotoSharingApplication.Frontend.Core`
- Click `Ok`

To use our service in the `AllPhotos` page, we need to perform a couple of steps, also described in the [Blazor Dependency Injection documentation](https://docs.microsoft.com/en-gb/aspnet/core/blazor/fundamentals/dependency-injection?view=aspnetcore-6.0&pivots=webassembly)

1. Register the service
2. Inject the service in the page
3. Use the service from within the page

### Step 1: Add the service to the app

[In the docs](https://docs.microsoft.com/en-gb/aspnet/core/blazor/fundamentals/dependency-injection?view=aspnetcore-6.0&pivots=webassembly#add-services-to-an-app) they tell us what to do: 

- Open the `Program.cs` file of the `PhotoSharingApplication.Frontend.BlazorWebAssembly`project
- Add the following code, before the `await builder.Build().RunAsync();`

```cs
builder.Services.AddScoped<IPhotosService, PhotosService>();
```

Of course, also add the correct `using`:

```cs
using PhotoSharingApplication.Frontend.Core.Interfaces;
using PhotoSharingApplication.Frontend.Core.Services;
```

### Step 2: Request  the service from our component

- Open the  `Pages`-> `AllPhotos.razor` file in the `PhotoSharingApplication.Frontend.BlazorWebAssembly`
- Add the following lines (after the `@page "photos/all")

```cs
@using PhotoSharingApplication.Frontend.Core.Interfaces
@inject IPhotosService photosService
```

### Step 3: Use the service to request the photos

We want to fill our `photos` list with data coming from our photosService.
This means that we don't need our own `Photo` class, because we want the one defined in the `PhotoSharingApplication.Frontend.Core.Entities` namespace.  
We need to add a `using PhotoSharingApplication.Frontend.Core.Entities` and we can remove the `Photo` class from the `code` section.

Because the photosService has an asynchronous method, we need to replace our old `OnInitialized` with a new `OnInitializedAsync` (remember the [Lifecycle?](https://docs.microsoft.com/en-gb/aspnet/core/blazor/components/lifecycle?view=aspnetcore-6.0))

So in the end, our code should look something like this:

```cs
@page "/photos/all"

@using PhotoSharingApplication.Frontend.Core.Interfaces
@using PhotoSharingApplication.Frontend.Core.Entities
@inject IPhotosService photosService

<h3>AllPhotos</h3>

@if (photos == null) {
    <p>...Loading...</p>
} else {
    @foreach (var photo in photos) {
      <article>
          <p>@photo.Id</p>
          <p>@photo.Title</p>
          <p>@photo.Description</p>
          <p>@photo.CreatedDate.ToShortDateString()</p>
      </article>
    }
}

@code {
    List<Photo> photos;

    protected override async Task OnInitializedAsync() {
        photos = await photosService.GetPhotosAsync();
    }
}
```

Our page is ready, but we're missing the actual infrastructure, so let's think about that.

## The Frontend Infrastructure

- On the `Solution Explorer`, right click you solution, then select `Add` -> `New Project`.
- Select `Class Library`. Click `Next`
- In the  `Project Name` field, type `PhotoSharingApplication.Frontend.Infrastructure`
- Make sure to select the latest .Net Core version (6.0 Preview)
- Click `Create`
- On the `Solution Explorer`, right click on the `Dependencies` folder of the `PhotoSharingApplication.Frontend.Infrastructure` project
- Select `Add Project Reference`
- Check the checkbox next to `PhotoSharingApplication.Frontend.Core`
- Click `Ok`
- Create a new folder `Repositories` and inside that, create a subfolder `Memory`
- In this `Memory` folder, add a new class `PhotosRepository` with the following code

```cs
using PhotoSharingApplication.Frontend.Core.Entities;
using PhotoSharingApplication.Frontend.Core.Interfaces;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Infrastructure.Repositories.Memory {
  public class PhotosRepository : IPhotosRepository {
    private List<Photo> photos;
    public PhotosRepository() {
      photos = new List<Photo> { 
        new Photo {Id=1, Title = "One photo", Description = "Lorem ipsum dolor sit amen", CreatedDate = DateTime.Now.AddDays(-2) },
        new Photo {Id=2, Title = "Another photo", Description = "Some description" ,CreatedDate= DateTime.Now.AddDays(-1)},
        new Photo {Id=3, Title = "Yet another photo", Description = "More description here", CreatedDate= DateTime.Now }
      };
    }
    public Task<Photo> CreateAsync(Photo photo) {
      photo.Id = photos.Max(p=>p.Id) + 1;
      photos.Add(photo);
      return Task.FromResult(photo);
  }

  public Task<Photo> FindAsync(int id) => Task.FromResult(photos.FirstOrDefault(p => p.Id == id));

  public Task<List<Photo>> GetPhotosAsync(int amount = 10) => Task.FromResult(photos.OrderByDescending(p => p.CreatedDate).ThenBy(p => p.Title).Take(amount).ToList());

  public Task<Photo> RemoveAsync(int id) {
      Photo photo = photos.FirstOrDefault(p => p.Id == id);
      if(photo!=null) photos.Remove(photo);
      return Task.FromResult(photo);
  }

  public Task<Photo> UpdateAsync(Photo photo) {
      Photo oldPhoto = photos.FirstOrDefault(p => p.Id == photo.Id);
      if (oldPhoto!= null) {
        oldPhoto.Title = photo.Title;
        oldPhoto.PhotoFile = photo.PhotoFile;
        oldPhoto.ImageMimeType = photo.ImageMimeType;
        oldPhoto.Description = photo.Description;
        oldPhoto.CreatedDate = photo.CreatedDate;
        oldPhoto.UserName = photo.UserName;
      }
      return Task.FromResult(oldPhoto);
    }
  }
}
```

I know, I know, it's a very naive implementation, but it's just to have something working so that we can see some action in the UI, we're going to replace it with something better later anyway.

Our last step is to plug this implementation in our application, so that the PhotoService can use it. We do this simply by registering this class as a service during startup.

- On the `Solution Explorer`, right click on the `Dependencies` folder of the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project
- Select `Add Project Reference`
- Check the checkbox next to `PhotoSharingApplication.Frontend.Infrastructure`
- Click `Ok`
- Open the `Program.cs` file of the `PhotoSharingApplication.Frontend.BlazorWebAssembly`project
- Add the following code, before the `await builder.Build().RunAsync();`

```cs
builder.Services.AddScoped<IPhotosRepository, PhotoSharingApplication.Frontend.Infrastructure.Repositories.Memory.PhotosRepository>();
```

If you run the application now and navigate to `/photos/all` you should see the data coming from our service and infrastructure.

Let's proceed with the *Upload* page.

## The Upload Page

In this page we're going learn how to work with [Forms and validation in Blazor](https://docs.microsoft.com/en-gb/aspnet/core/blazor/forms-validation?view=aspnetcore-6.0), since we need to give the user the chance to write some data and submit it to our service. As usual, we're going to start quick and dirty, refining it along the way.

You should by now know how to add a new page, but just in case:

- In the `Solution Explorer`, right click on the `Pages` folder of the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project
- Select `Add` -> `Razor Component`
- Name the component `UploadPhoto.razor`

Let's link this page to the `photos/upload` route by adding this line at the top of the page:

```cs
@page "/photos/upload"
```

We know we're going to talk to the `PhotosService` eventually, so let's add the  necessary `using` and `inject`, just like we did in the `AllPhotos`:

```cs
@using PhotoSharingApplication.Frontend.Core.Interfaces
@using PhotoSharingApplication.Frontend.Core.Entities
@inject IPhotosService photosService
```

As usual, we're going to take care of two sections: the UI and the code.

For the UI we need an `EditForm` component, to which we have to specify a `Model` (the data to bind to the form) and a `HandleValidSubmit` (the method to invoke if the data is correct). We also need some input field that we can bind to the different properties of our photo and a submit button.

```html
<EditForm Model="@photo" OnValidSubmit="HandleValidSubmit">
    <p>
        <label>
            Title:
            <InputText @bind-Value="photo.Title" />
        </label>
    </p>
    <p>
        <label>
            Description (optional):
            <InputTextArea @bind-Value="photo.Description" />
        </label>
    </p>
    <button type="submit">Submit</button>
</EditForm>
```

In the code, we want to define a photo of type Photo (the model that our form is bound to) and send it to our service when the user clicks on the submit button and the data has been validated:

```cs
@code {
    Photo photo;

    protected override void OnInitialized() {
        photo = new Core.Entities.Photo();
    }

    private async Task HandleValidSubmit() {
        await photosService.UploadAsync(photo);
    }
}
```

If you run the code now, it should work but we don't see much happening. Also, if you type `/photos/all` in the address bar of your browser after you click on submit, you don't see the new photo in the list. Why is that?

Well, when you *navigate* to a new address, you're actually talking to the server, which serves you a completely new `index.html`. The browser gets the new response and initializes the application all over again, so we lose the previous memory state. As soon as we switch our repository to a class that calls the server to retrieve the data, we won't see this problem anymore so we're not going to do anything about this. If you want to know how to fix this, take a look at how to use the browser local storage instead of memory  by either using the [Protected Browser Storage](https://docs.microsoft.com/en-gb/aspnet/core/blazor/state-management?view=aspnetcore-6.0&pivots=webassembly#browser-storage-wasm) or any third party library, such as the [Blazored Local Storage](https://github.com/Blazored/LocalStorage).

We're not going to fix the navigation problem, but we are going to use a neat little trick to make it *look like* we're navigating to the `photos/all` while we're actually just replacing the address and rerendering a section of our page. Introducing: the [Navigation Manager](https://docs.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-6.0#uri-and-navigation-state-helpers).

To use the NavigationManager, we can just ask for it as a dependency:

```cs
@inject NavigationManager navigationManager
```

Then we can use it like this:

```cs
private async Task HandleValidSubmit() {
  await photosService.UploadAsync(photo);
  navigationManager.NavigateTo("/photos/all");
}
```

If you run the application now, go to `/photos/upload`, type some title and description and click on `submit`, you should see the `/photos/all` with the new photo. TA-DAAA!

Well, actually, there's a title and a description, but we still have no real photo... So let's get onto that.

## Upload of an image

We want to give the user the opportunity to select a file from her own device. Blazor has a very helpful [File Upload Compoment](https://docs.microsoft.com/en-us/aspnet/core/blazor/file-uploads?view=aspnetcore-6.0) that we can use.

We need to add it to our form, like so:

```html
<p>
  <label>
    File:
    <InputFile OnChange="HandleFileSelected" />
  </label>
</p>
```

Then we need to handle the change event in order to read the selected file and fill the corresponding photo properties, like so:

```cs
private async Task HandleFileSelected(InputFileChangeEventArgs args) {
  photo.ImageMimeType = args.File.ContentType;

  using (var streamReader = new System.IO.MemoryStream()) {
    await args.File.OpenReadStream().CopyToAsync(streamReader);
    photo.PhotoFile = streamReader.ToArray();
  }
}
```

If you run the application you can see that you can, indeed, select a file from your device. When we are redirected to the `/photos/all` we still don't see the image and that's obvious, since we are not rendering the correct markup yet. Let's fix that.

## Showing the picture

We now need to add an `<img>` tag to our `AllPhotos.razor` page. The question is: what shall we write in the `src` attribute?

We don't have a separate url where we can find the image, so we can't use the usual `http://theaddress/whereyourfileis.jpg` if you know what I mean. The file is already on the client, as an array of bytes, so how can we use that as a source for an image tag? 

It turns out that html supports the concept of [Data URLs](https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/Data_URIs), which is a format we can use to render the array of byte as a Base64 encoded string. It looks like this:

```
data:[<mediatype>][;base64],<data>
```

The `<mediatype>` is the information we saved in the `photo.ImageMimeType`, while the `<data>` is our `photo.PhotoFile` converted as Base64 (we can achieve this conversion by using the `ToBase64String` static method of the `Convert` class).

```html
<p><img src="@(photo.PhotoFile == null ? "" : $"data:{photo.ImageMimeType};base64,{Convert.ToBase64String(photo.PhotoFile)}")" /></p>
```

There we go, now we can run the application, upload a picture and we should see it in the list.

Not only, we can use the exact same code in the `UploadPhoto.razor` to show a preview of the file before it gets uploaded. Nice, uh?

```html
<p><img src="@(photo.PhotoFile == null ? "" : $"data:{photo.ImageMimeType};base64,{Convert.ToBase64String(photo.PhotoFile)}")" /></p>
```

Let's move on to a new page.

## The Details Page

The details page should show all the information about one particular photo, which means we need to know which photo, first. What we can do is to append the unique id of the photo to the url as a parameter. That way the details page can retrieve it from there and pass it to a photosService to retrieve the data to show. 

We will make use of [Route Parameters](https://docs.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-6.0#route-parameters)

Add the new `PhotoDetails.razor` page to the `Pages` folder of the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project, then insert the following line:

```
@page "/photos/details/{id:int}"
```

This means that an address such as `/photos/details/123` will be mapped by the routing engine to an `id` parameter equal to `123`

In the `@code` section, add the following parameter:

```cs
@code{
  [Parameter]
  public int Id { get; set; }
}
```

It's important that the name matches the one we used in the route, although not case sensitive.

The rest is very similar to the `AllPhotos`: we work with the photosService to get the photo and we display in the html. De difference is that instead of a list of photos, we now only have one, so we don't even need to loop.

```cs
@page "/photos/details/{id:int}"

@using PhotoSharingApplication.Frontend.Core.Interfaces
@using PhotoSharingApplication.Frontend.Core.Entities
@inject IPhotosService photosService

<h3>Details</h3>

@if (photo == null) {
    <p>...Loading...</p>
} else {
    <article>
        <p>@photo.Id</p>
        <p>@photo.Title</p>
        <p>@photo.Description</p>
        <p>@photo.CreatedDate.ToShortDateString()</p>
        <p><img src="@(photo.PhotoFile == null ? "" : $"data:{photo.ImageMimeType};base64,{Convert.ToBase64String(photo.PhotoFile)}")" /></p>
    </article>
}

@code {
    [Parameter]
    public int Id { get; set; }

    Photo photo;

    protected override async Task OnInitializedAsync() {
        photo = await photosService.FindAsync(Id);
    }
}
```

Save and check that the details view updates correctly when you enter an address such as `photos/details/1` and `photos/details/2`.

# The Delete Page 

We can repeat the same steps for the Delete Page as a start.
Add a new `DeletePhoto.razor` page to the `Pages` folder of the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project, then type the following code:

```cs
@page "/photos/delete/{id:int}"

@using PhotoSharingApplication.Frontend.Core.Interfaces
@using PhotoSharingApplication.Frontend.Core.Entities
@inject IPhotosService photosService

<h3>Delete</h3>

@if (photo == null) {
    <p>...Loading...</p>
} else {
    <article>
        <p>@photo.Id</p>
        <p>@photo.Title</p>
        <p>@photo.Description</p>
        <p>@photo.CreatedDate.ToShortDateString()</p>
        <p><img src="@(photo.PhotoFile == null ? "" : $"data:{photo.ImageMimeType};base64,{Convert.ToBase64String(photo.PhotoFile)}")" /></p>
    </article>
}

@code {
    [Parameter]
    public int Id { get; set; }

    Photo photo;

    protected override async Task OnInitializedAsync() {
        photo = await photosService.FindAsync(Id);
    }
}
```

As you can see, it's the third time that we copy / paste code. This is usually an indication that we should refactor our code into a separate component. I promise we *will* do that but not yet, let's first finish the Delete and Update pages.

Our Delete page doesn't actually delete, yet, so let's add a button to invoke the `photosService.RemoveAsync` method. 

We'll use [Event Handling](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/event-handling?view=aspnetcore-6.0) to handle the click of a button with a C# method that in turn will call the photosService.

Let's add the button:

```html
<div>
  <button @onclick="DeleteConfirm">Confirm Deletion</button>
</div>
```

Now the handler:

```cs
private async Task DeleteConfirm(MouseEventArgs e) {
  await photosService.RemoveAsync(Id);
  navigationManager.NavigateTo("/photos/all");
}
```

Also, don't forget to ask for the NavigationManager:

```cs
@inject NavigationManager navigationManager
```

This is the complete code of the Delete page:

```cs
@page "/photos/delete/{id:int}"

@using PhotoSharingApplication.Frontend.Core.Interfaces
@using PhotoSharingApplication.Frontend.Core.Entities
@inject IPhotosService photosService
@inject NavigationManager navigationManager

<h3>Delete</h3>

@if (photo == null) {
    <p>...Loading...</p>
} else {
    <article>
        <p>@photo.Id</p>
        <p>@photo.Title</p>
        <p>@photo.Description</p>
        <p>@photo.CreatedDate.ToShortDateString()</p>
        <p><img src="@(photo.PhotoFile == null ? "" : $"data:{photo.ImageMimeType};base64,{Convert.ToBase64String(photo.PhotoFile)}")" /></p>
        <div>
            <button @onclick="DeleteConfirm">Confirm Deletion</button>
        </div>
    </article>
}

@code {
    [Parameter]
    public int Id { get; set; }

    Photo photo;

    protected override async Task OnInitializedAsync() {
        photo = await photosService.FindAsync(Id);
    }
    private async Task DeleteConfirm(MouseEventArgs e) {
        await photosService.RemoveAsync(Id);
        navigationManager.NavigateTo("/photos/all");
    }
}
```

Start your application, navigate to `/photos/delete/2`, click on `Confirm Deletion` and see the photo disappear from the list.

## The Update Page

Last but not least, let's think about the update page, which is a mix between the Details and the Upload.

It is similar to the Details because it needs to know which photo to show; it's similar to the Upload because it contains a form bound to the photo.
The difference is that the save button invokes the `UpdateAsync` instead of the `UploadAsync`:

```cs
@page "/photos/update/{id:int}"

@using PhotoSharingApplication.Frontend.Core.Interfaces
@using PhotoSharingApplication.Frontend.Core.Entities
@inject IPhotosService photosService
@inject NavigationManager navigationManager

<h3>Update</h3>

@if (photo == null) {
  <p>...Loading...</p>
} else {
<EditForm Model="@photo" OnValidSubmit="HandleValidSubmit">
  <p>
    <label>
        Title:
        <InputText @bind-Value="photo.Title" />
    </label>
  </p>
  <p>
    <label>
        Description (optional):
        <InputTextArea @bind-Value="photo.Description" />
    </label>
  </p>
  <p>
    <label>
        File:
        <InputFile OnChange="HandleFileSelected" />
    </label>
  </p>
  <p><img src="@(photo.PhotoFile == null ? "" : $"data:{photo.ImageMimeType};base64,{Convert.ToBase64String(photo.PhotoFile)}")" /></p>
  <button type="submit">Submit</button>
</EditForm>
}

@code {
  [Parameter]
  public int Id { get; set; }

  Photo photo;

  protected override async Task OnInitializedAsync() {
    photo = await photosService.FindAsync(Id);
  }

  private async Task HandleValidSubmit() {
    await photosService.UpdateAsync(photo);
    navigationManager.NavigateTo("/photos/all");
  }

  private async Task HandleFileSelected(InputFileChangeEventArgs args) {
    photo.ImageMimeType = args.File.ContentType;

    using (var streamReader = new System.IO.MemoryStream()) {
      await args.File.OpenReadStream().CopyToAsync(streamReader);
      photo.PhotoFile = streamReader.ToArray();
    }
  }
}
```

Save, navigate to `/update/2`, change some values, click on the submit button and verify that the photo gets updated and that you are sent back to the root.

We did it! We have all the pages we need. 

## The NavLink component

The very last thing for this lab is to sprinkle some [navigation links](https://docs.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-5.0#navlink-and-navmenu-components) here and there, so that the user can go to the different pages more easily, without having to write the url by hand.

Let's start with the `AllPhotos.razor`.

We will insert a link to view the details, update and delete for each product (passing the specific id to the route) and one link to create a new product.

The last one is actually the easiest, because it's a static address:

```html
<NavLink href="photos/upload">Upload new Photo</NavLink>
```

The links for details, update and delete need to be constructed using an [Explicit Razor Expression](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-6.0#explicit-razor-expressions), like this:

```html
<div>
  <NavLink href="@($"photos/details/{photo.Id}")">Details</NavLink>
  <NavLink href="@($"photos/update/{photo.Id}")">Update</NavLink>
  <NavLink href="@($"photos/delete/{photo.Id}")">Delete</NavLink>
</div>
```

Save and verify that each photo in the `/photos/all` page now links to its own edit, update and delete page.

---

Our pages are functionally ready but their appearance could improve. We are going to take care of their styles in the next lab.

Go to `Labs/Lab03`, open the `readme.md` and follow the instructions thereby contained.