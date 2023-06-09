# Performance Optimization

So far things seem to be working, but the performances are not brilliant.  
The page with all the photos is particularly slow, since we download all the content in one go (both the metadata - such as Title, Description and so on - and the actual array of byte for the picture). Only once *everything* is on the client side we can start encoding each array of byte as a string to display the picture.   
If we we could dowload only the metadata without the ByteArray, then let the browser download each picture one by one in a separate request, we would:
- Be faster to show the metadata
- Download the picture in separate parallel asynchronous requests, showing them one by one as soon as they arrive, letting the usere *perceive* that we're fast 
- Take advantage of browser caching (which would speed up subsequent calls and lift the server)

So this is what we're going to do:

1. We're going to split our model in two:
    - `PhotoImage` will contain the `ImageMimeType` and `PhotoFile`
    - `Photo` will not contain `ImageMimeType` and `PhotoFile` anymore. In their place, we'll add the address of the controller action that the client will then use to download the file with the image.  
    In order to leave the database as is, we will have to configure the `DbContext` to map both those classes to the same old `Photo` table and establish a *One to one* relationship between the `Photo` and the `PhotoImage` models.
2. We will change the `IPhotosService` and `IPhotosRepository` shared interfaces to include 
    - A method that returns only the `Photo` (without the `PhotoImage`)
    - A method that returns only the `PhotoImage`
    - A method that returns the `Photo` including the `PhotoImage`
3. We will change the Repositories and the Services, both client side and server side, to implement the newly added methods
4. We will change the Rest Service to implement 
    - A method that returns only the `Photo` (without the `PhotoImage`)
    - A method that returns a `File` build from the `PhotoImage`
    - A method that returns the `Photo` including the `PhotoImage`
5. We will change the `PhotoEdit` and `PhotoDetails` Blazor Components to reflect the new shape of our models
6. We will change the `UpdatePhoto` and `UploadPhoto` Blazor Pages to reflect the new shape of our models

## The Model

- In the `Solution Explorer`, under the `Entities` folder of the `PhotoSharingApplication.Shared.Core` project, add a new class. Name the class `PhotoImage`
- Add an `Id` property of type `int`
- Move the `PhotoFile` and the `ImageMimeType` properties form the `Photo` class to this new `PhotoImage` class

The code should look like this

```cs
namespace PhotoSharingApplication.Shared.Entities;

public class PhotoImage {
    public int Id { get; set; }
    public byte[]? PhotoFile { get; set; }
    public string? ImageMimeType { get; set; }
}
```

- Open the `Photo` class
- Delete the  `PhotoFile` and the `ImageMimeType` properties 
- Add a `ImageUrl` property of type `string`
- Add a `PhotoImage` property of type `PhotoImage`

The code should look like this:

```cs
namespace PhotoSharingApplication.Shared.Entities;

public class Photo {
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? UserName { get; set; }
    public string? ImageUrl { get; set; }
    public PhotoImage? PhotoImage { get; set; }
}
```

## The DbContext

We have to configure the relationship between the `Photo` and the `PhotoImage` entities. We're going to use [Table Splitting](https://learn.microsoft.com/en-us/ef/core/modeling/table-splitting).

- In the `Solution Explorer`, under the `Infrastructure/Data` folder of the `PhotoSharingApplication.WebServices.Rest.Photos` project, open the `PhotosDbContext` class
- Add a new `PhotoImages` property of type `DbSet<PhotoImage>`
- Add a new `ConfigurePhotoImage` method to
    - Map the property to the `Photos` table
    - Require both the `PhotoFile` and the `ImageMimeType` fields
- Invoke the `ConfigurePhotoImage` from the `OnModelCreating`
- Change the `ConfigurePhoto` to [exclude](https://learn.microsoft.com/en-us/ef/core/modeling/entity-properties?tabs=fluent-api%2Cwithout-nrt#included-and-excluded-properties) the `ImageUrl` field, so that it's not saved on the database

The code should look like this:

```cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.WebServices.Rest.Photos.Infrastructure.Data;

public class PhotosDbContext : DbContext {
    public PhotosDbContext(DbContextOptions<PhotosDbContext> options): base(options) {    }
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Photo>(ConfigurePhoto);
        modelBuilder.Entity<PhotoImage>(ConfigurePhotoImage);
    }

    private void ConfigurePhoto(EntityTypeBuilder<Photo> builder) {
        builder.ToTable("Photos");

        builder.Property(photo => photo.Title)
            .IsRequired(true)
            .HasMaxLength(255);

        builder.Ignore(p => p.ImageUrl);

        builder.HasOne(p => p.PhotoImage)
            .WithOne()
            .HasForeignKey<PhotoImage>(p => p.Id);
    }
    private void ConfigurePhotoImage(EntityTypeBuilder<PhotoImage> builder) {
        builder.ToTable("Photos");
    }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<PhotoImage> PhotoImages { get; set; }
}
```

## The Interfaces

- In the `Solution Explorer`, under the `Interfaces` folder of the `PhotoSharingApplication.Shared` project, open the `IPhotosService` interface, then add
    - `Task<Photo> FindWithImageAsync(int id);`
    - `Task<PhotoImage> GetImageAsync(int id);`

The code should look like this:

```cs
using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.Shared.Interfaces;

public interface IPhotosService {
    Task<Photo?> UploadAsync(Photo photo);
    Task<Photo?> UpdateAsync(Photo photo);
    Task<Photo?> FindAsync(int id);
    Task<Photo?> FindWithImageAsync(int id);
    Task<PhotoImage?> GetImageAsync(int id);
    Task<List<Photo>> GetPhotosAsync(int amount = 10);
    Task<Photo?> RemoveAsync(int id);
}
```

- In the `Solution Explorer`, under the `Interfaces` folder of the `PhotoSharingApplication.Shared` project, open the `IPhotosRepository` interface, then add
    - `Task<Photo> FindWithImageAsync(int id);`
    - `Task<PhotoImage> GetImageAsync(int id);`

The code should look like this:

```cs
using PhotoSharingApplication.Shared.Entities;

namespace PhotoSharingApplication.Shared.Interfaces;

public interface IPhotosRepository {
    Task<Photo?> UpdateAsync(Photo photo);
    Task<Photo?> FindAsync(int id);
    Task<Photo?> CreateAsync(Photo photo);
    Task<List<Photo>> GetPhotosAsync(int amount = 10);
    Task<Photo?> RemoveAsync(int id);
    Task<Photo?> FindWithImageAsync(int id);
    Task<PhotoImage?> GetImageAsync(int id);
}
```

## Backend

### The Service

- In the `Solution Explorer`, under the `Core/Services` folder of the `PhotoSharingApplication.WebServices.Rest.Photos` project, open the `PhotosService` class, then add
    - `public async Task<Photo?> FindWithImageAsync(int id) => await repository.FindWithImageAsync(id);`
    - `public async Task<PhotoImage?> GetImageAsync(int id) => await repository.GetImageAsync(id);`

### The Repository

- In the `Solution Explorer`, under the `Infrastructure/Repositories/EntityFramework` folder of the  `PhotoSharingApplication.WebServices.Rest.Photos` project, open the `PhotosService` class, then add
    - `public async Task<Photo?> FindWithImageAsync(int id) => await context.Photos.Include(nameof(PhotoImage)).AsNoTracking().SingleOrDefaultAsync(m => m.Id == id);`
    - `public async Task<PhotoImage?> GetImageAsync(int id) => await context.PhotoImages.AsNoTracking().SingleOrDefaultAsync(prop => prop.Id == id);`

### The REST Controller

- In the `Solution Explorer`, under the `Controllers` folder of the  `PhotoSharingApplication.WebServices.Rest.Photos` project, open the `PhotosControllers` class, then add
    
```cs
[HttpGet("withimage/{id:int}", Name = "FindWithImage")]
public async Task<ActionResult<Photo>> FindWithImage(int id) {
    Photo? ph = await service.FindWithImageAsync(id);
    if (ph is null) return NotFound();
    return ph;
}

[HttpGet("image/{id:int}", Name = "GetImage")]
public async Task<IActionResult> GetImage(int id) {
    PhotoImage? ph = await service.GetImageAsync(id);
    if (ph is null || ph.PhotoFile is null || ph.ImageMimeType is null) return NotFound();
    return File(ph.PhotoFile, ph.ImageMimeType);
}
```

- Change the `Find` to include the `ImageUrl` address:

```cs
[HttpGet("{id:int}", Name = "Find")]
public async Task<ActionResult<Photo>> Find(int id) {
    Photo? ph = await service.FindAsync(id);
    if (ph is null) return NotFound();
    ph.ImageUrl = Url.Link(nameof(GetImage), new { id = ph.Id });
    return ph;
}
```

- Use the same strategy for the `GetPhotos`

```cs
[HttpGet]
public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos() => 
    (await service
        .GetPhotosAsync())
        .Select(p => new Photo { 
            Id = p.Id, 
            CreatedDate = p.CreatedDate, 
            Description = p.Description, 
            PhotoImage = p.PhotoImage, 
            Title = p.Title, 
            UserName = p.UserName, 
            ImageUrl = Url.Link(nameof(GetImage), new { id = p.Id })
        })
        .ToList();
```

## The Frontend Client

### The Service

- In the `Solution Explorer`, under the `Core/Services` folder of the `PhotoSharingApplication.Frontend.Client` project, open the `PhotosService` class, then add

```cs
public async Task<Photo?> FindWithImageAsync(int id) => await repository.FindWithImageAsync(id);
public async Task<PhotoImage?> GetImageAsync(int id) => await repository.GetImageAsync(id);
```

### The Repository

- In the `Solution Explorer`, under the `Rest` folder of the `Infrastructure/Repositories/Rest` folder of the  `PhotoSharingApplication.Frontend.Client` project, open the `PhotosService` class, then add

```cs
    public async Task<PhotoImage?> GetImageAsync(int id) => await http.GetFromJsonAsync<PhotoImage>($"/api/photos/image/{id}");
    public async Task<Photo> FindWithImageAsync(int id) => await http.GetFromJsonAsync<Photo>($"/api/photos/withimage/{id}");
```

### Memory Repository

 - In the `Solution Explorer`, under the `Memory` folder of the `Repositories` folder of the  `PhotoSharingApplication.Frontend.Infrastructure` project, open the `PhotosService` class, then complete the class so that it compiles

 The code should look like this:

 ```cs
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Memory;

public class PhotosRepository : IPhotosRepository {
    private List<Photo> photos;
    private List<PhotoImage> photoImages;
    public PhotosRepository() {
        photos = new List<Photo> {
            new Photo {Id=1, Title = "One photo", Description = "Lorem ipsum dolor sit amen", CreatedDate = DateTime.Now.AddDays(-2) },
            new Photo {Id=2, Title = "Another photo", Description = "Some description" ,CreatedDate= DateTime.Now.AddDays(-1)},
            new Photo {Id=3, Title = "Yet another photo", Description = "More description here", CreatedDate= DateTime.Now }
        };
        photoImages = new List<PhotoImage> {
            new PhotoImage {Id=1, ImageMimeType="jpg"},
            new PhotoImage {Id=2, ImageMimeType = "gif"},
            new PhotoImage {Id=3, ImageMimeType = "png"}
        };
    }

    public Task<Photo?> CreateAsync(Photo photo) {
        photo.Id = photos.Max(p => p.Id) + 1;
        photos.Add(photo);
        return Task.FromResult(photo)!;
    }

    public Task<Photo?> FindAsync(int id) => Task.FromResult(photos.FirstOrDefault(p => p.Id == id));

    public Task<Photo?> FindWithImageAsync(int id) => Task.FromResult(photos.FirstOrDefault(p => p.Id == id));

    public Task<PhotoImage?> GetImageAsync(int id) => Task.FromResult(photoImages.FirstOrDefault(p => p.Id == id));

    public Task<List<Photo>> GetPhotosAsync(int amount = 10) => Task.FromResult(photos.OrderByDescending(p => p.CreatedDate).ThenBy(p => p.Title).Take(amount).ToList());

    public Task<Photo?> RemoveAsync(int id) {
        Photo? photo = photos.FirstOrDefault(p => p.Id == id);
        if (photo is not null) photos.Remove(photo);
        return Task.FromResult(photo);
    }

    public Task<Photo?> UpdateAsync(Photo photo) {
        Photo? oldPhoto = photos.FirstOrDefault(p => p.Id == photo.Id);
        if (oldPhoto is not null) {
            oldPhoto.Title = photo.Title;
            oldPhoto.Description = photo.Description;
            oldPhoto.CreatedDate = photo.CreatedDate;
            oldPhoto.UserName = photo.UserName;
            if (oldPhoto.PhotoImage is null)
                oldPhoto.PhotoImage = new PhotoImage();
            oldPhoto.PhotoImage.PhotoFile = photo.PhotoImage?.PhotoFile;
            oldPhoto.PhotoImage.ImageMimeType = photo.PhotoImage?.ImageMimeType;
        }
        return Task.FromResult(oldPhoto);
    }
}
```

### Blazor Components

We have two components where we display the image: `PhotoDetailsComponent.razor` and `PhotoEditComponent.razor`.
Let's first tackle `PhotoDetailsComponent.razor`, where instead of displaying the ByteArray as encoded string, we will use the new `ImageUrl` property so that the browser can  download the image separately.  
This means that our previous code  

```html
<MudCardMedia Image="@(Photo.PhotoFile == null ? "" : $"data:{Photo.ImageMimeType};base64,{Convert.ToBase64String(Photo.PhotoFile)}")" Height="250" />
```

becomes instead

```html
<MudCardMedia Image="@Photo.ImageUrl" Height="250" />
```

The razor code of the `PhotoEditComponent.razor` should look like this:

```html
<MudImage Fluid Src="@(Photo.PhotoImage?.PhotoFile is null ? "" : $"data:{Photo.PhotoImage.ImageMimeType};base64,{Convert.ToBase64String(Photo.PhotoImage.PhotoFile)}")" Elevation="25" Class="rounded-lg" />
```

while the `code` section should now look like this:

```cs
@code {
    [Parameter, EditorRequired]
    public Photo Photo { get; set; } = default!;

    [Parameter]
    public EventCallback<Photo> OnSave { get; set; }

    private async Task HandleFileSelected(IBrowserFile args) {
        if (Photo.PhotoImage is null) Photo.PhotoImage = new PhotoImage();
        Photo.PhotoImage.ImageMimeType = args.ContentType;

        using (var streamReader = new System.IO.MemoryStream()) {
            await args.OpenReadStream().CopyToAsync(streamReader);
            Photo.PhotoImage.PhotoFile = streamReader.ToArray();
        }
    }
}
```


### UploadPhoto

In the `Solution Explorer`, under the `Pages` folder of the  `PhotoSharingApplication.Frontend.BlazorComponents` project, open the `UploadPhoto.razor` file

The razor code should look like this:

```html
@attribute [Authorize]

<PageTitle>Upload Photo</PageTitle>

<AuthorizeView>
    <Authorized>
        <PhotoEditComponent Photo="photo" OnSave="Upload"></PhotoEditComponent>
    </Authorized>
    <NotAuthorized>
        <MudButton Variant="Variant.Filled" EndIcon="@Icons.Material.Filled.Login" Color="Color.Error" Href="bff/login">You are not authorized. Log in to upload a picture</MudButton>
    </NotAuthorized>
</AuthorizeView>
```

### UpdatePhoto

- In the `Solution Explorer`, under the `Pages` folder `PhotoSharingApplication.Frontend.BlazorComponents` project, open the `UpdatePhoto.razor` file
- Locate the `OnInitialized` method and change its content to invoke the `FindWithImageAsync` method

The code should look like this:

```cs
protected override async Task OnInitializedAsync() {
    photo = await photosService.FindWithImageAsync(Id);
}
```

If you run the application, already the first time you navigate to `AllPhotos` you should see the images appear one by one, which is already an improvement. But the best improvement is if you navigate away and go back to the AllPhotos: you will see that the browser uses the cached pictures instead of downloading them again.

## Optional - Virtualize Component

This is a nice accomplishment, but we can do more to improve the perceived performance of our application by using a [Blazor component virtualization](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/virtualization?view=aspnetcore-7.0).

> Improve the perceived performance of component rendering using the Blazor framework's built-in virtualization support with the `Virtualize` component. Virtualization is a technique for limiting UI rendering to just the parts that are currently visible. For example, virtualization is helpful when the app must render a long list of items and only a subset of items is required to be visible at any given time.  
>  
> Use the `Virtualize` component when:  
>  
> - Rendering a set of data items in a loop.
> - Most of the items aren't visible due to scrolling.
> - The rendered items are the same size.  
>  
> When the user scrolls to an arbitrary point in the `Virtualize` component's list of items, the component calculates the visible items to show. Unseen items aren't rendered.  
> (...)  
> If you don't want to load all of the items into memory, you can specify an items provider delegate method to the component's `Virtualize<TItem>.ItemsProvider` parameter that asynchronously retrieves the requested items on demand.  
> The items provider receives an `ItemsProviderRequest`, which specifies the required number of items starting at a specific start index. The items provider then retrieves the requested items from a database or other service and returns them as an `ItemsProviderResult<TItem>` along with a count of the total items. The items provider can choose to retrieve the items with each request or cache them so that they're readily available.

Our `AllPhotos` page is now looping through a set of `Photo`s, but it's actually only receiving 10 photos in total, since every member of the chain has a limitation of 10 items. Both the `IPhotosService` and the `IPhotosRepository` have a default `amount` parameter set to 10.  
What we can do is to accept another parameter to also set the starting index where to retrieve the items, then use it in a query on the `Repository` on the Backend.  
The `Virtualize` component will take care of knowing the value to send as a start index.  
In order to know how many items to ask for, we will also need to know how many items are there in total, which means that we will need to add a new method to return the photos count.  

We need to change:
- Two interfaces in the `PhotoSharingApplication.Shared.Interfaces`
    - `IPhotosService.GetPhotosAsync` so that it accepts more parameters
    - `IPhotosService.GetPhotosCountAsync` to return an int
    - `IPhotosRepository.GetPhotosAsync` so that it accepts more parameters
    - `IPhotosRepository.GetPhotosCountAsync` to return an int
- Three classes in the `PhotoSharingApplication.Frontend.Client`
    - Modify `PhotosService.GetPhotosAsync`
    - Add `PhotosService.GetPhotosCountAsync`
    - Modify `Rest.PhotosRepository.GetPhotosAsync`
    - Add `Rest.PhotosRepository.GetPhotosCountAsync`
    - Modify `Memory.PhotosRepository.GetPhotosAsync`
    - Add `Memory.PhotosRepository.GetPhotosCountAsync`
- Two classes in the `PhotoSharingApplication.WebServices.REST.Photos`
    - Modify `PhotosService.GetPhotosAsync`
    - Add `PhotosService.GetPhotosCountAsync`
    - Modify `PhotosRepository.GetPhotosAsync`
    - Add `PhotosRepository.GetPhotosCountAsync`
- One class in the `PhotoSharingApplication.WebServices.REST.Photos.Controllers`
    - Modify `PhotosController.GetPhotos`
    - Add `PhotosController.GetPhotosCount`
- One razor component in the `PhotoSharingApplication.Frontend.BlazorComponents.Pages`
    - `AllPhotos.razor`

Let's get to it.

## Interfaces
- Open the `IPhotosService` interface located under the `Interfaces` folder of the `PhotoSharingApplication.Shared` project
- Replace this code

```cs
Task<List<Photo>> GetPhotosAsync(int amount = 10);
```

with this
```cs
Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken);

Task<int> GetPhotosCountAsync();
```

- Open the `IPhotosRepository` interface located under the `Interfaces` folder of the `PhotoSharingApplication.Core` project.
- Replace this code

```cs
Task<List<Photo>> GetPhotosAsync(int amount = 10);
```

with this
```cs
Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken);

Task<int> GetPhotosCountAsync();
```

## FrontEnd.Client

- Open the `PhotosService` class located under the `Core/Services` folder of the `PhotoSharingApplication.Frontend.Client` project 
- Replace this code 

```cs
public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await repository.GetPhotosAsync(amount);
```

with this:
```cs
public async Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken) => await repository.GetPhotosAsync(startIndex, amount, cancellationToken);

public async Task<int> GetPhotosCountAsync() => await repository.GetPhotosCountAsync();
```
- Open the `PhotosRepository` class located under the `Infrastructure/Repositories/Rest` folder of the `PhotoSharingApplication.Frontend.Client` project
- Replace this code
```cs
public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await publicPhotosClient.GetPhotosAsync(amount);
```
with this
```cs
public async Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken) => await http.GetFromJsonAsync<List<Photo>>($"/api/photos/{startIndex}/{amount}", cancellationToken);
public async Task<int> GetPhotosCountAsync() => int.Parse(await http.GetStringAsync($"/api/photos/count"));
``` 
- Open the `PhotosRepository` class located under the `Repositories/Memory` folder of the `PhotoSharingApplication.Frontend.Infrastructure` project
- Replace this code
```cs
public Task<List<Photo>> GetPhotosAsync(int amount = 10) => Task.FromResult(photos.OrderByDescending(p => p.CreatedDate).ThenBy(p => p.Title).Take(amount).ToList());
```
with this
```cs
public Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken) => Task.FromResult(photos.OrderByDescending(p => p.CreatedDate).ThenBy(p => p.Title).Skip(startIndex).Take(amount).ToList());

public Task<int> GetPhotosCountAsync() => Task.FromResult(photos.Count);
``` 

## BackEnd
- Open the `PhotosService` class located under the `Core/Services` folder of the `PhotoSharingApplication.WebServices.Rest.Photos` project
- Replace this code
```cs
public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await repository.GetPhotosAsync(amount);
```
with this
```cs
public async Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken) => await repository.GetPhotosAsync(startIndex, amount, cancellationToken);
        
public async Task<int> GetPhotosCountAsync() => await repository.GetPhotosCountAsync();
```
- Open the `PhotosRepository` class located under the `Infrastructure/Repositories/EntityFramework` folder of the `PhotoSharingApplication.WebServices.Rest.Photos` project
- Replace this code
```cs
public async Task<List<Photo>> GetPhotosAsync(int amount = 10) =>
    await (from p in context.Photos
            orderby p.CreatedDate descending
            select p).Take(amount).ToListAsync();
```
with this
```cs
public async Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken) =>
    await (from p in context.Photos
            orderby p.CreatedDate descending
            select p).Skip(startIndex).Take(amount).ToListAsync(cancellationToken);

public async Task<int> GetPhotosCountAsync() => await context.Photos.CountAsync();
```
- Open the `PhotosController` class located under the `Controllers` folder of the `PhotoSharingApplication.WebServices.Rest.Photos` project
- Replace this code
```cs
[HttpGet]
    public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos() => (await service.GetPhotosAsync()).Select(p=> new Photo { Id=p.Id, CreatedDate = p.CreatedDate, Description = p.Description, PhotoImage = p.PhotoImage, Title = p.Title, UserName = p.UserName, ImageUrl = p.ImageUrl = Url.Link(nameof(GetImage), new { id = p.Id })}).ToList();
```
with this
```cs
[HttpGet("{startIndex}/{amount}")]
public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos(int startIndex, int amount, CancellationToken cancellationToken) => (await service.GetPhotosAsync(startIndex,amount,cancellationToken)).Select(p=> new Photo { Id=p.Id, CreatedDate = p.CreatedDate, Description = p.Description, PhotoImage = p.PhotoImage, Title = p.Title, UserName = p.UserName, ImageUrl = p.ImageUrl = Url.Link(nameof(GetImage), new { id = p.Id })}).ToList();

[HttpGet("count")]
public async Task<ActionResult<int>> GetPhotosCount() => await service.GetPhotosCountAsync();
```

## Blazor UI
- Open the `AllPhotos.razor` component located under the in the `Pages` folder of the `PhotoSharingApplication.Frontend.BlazorComponents` project
- Replace the following code 
```html
@foreach (var photo in photos) {
<MudItem xs="12" sm="4">
    <PhotoDetailsComponent Photo="photo" Details Edit Delete />
</MudItem>
}
```

with this

```html
<MudGrid Spacing="2" Justify="Justify.FlexStart" class="object-contain overflow-scroll">
<Virtualize @ref="virtualizecomponent" Context="photo" ItemsProvider="@LoadPhotos">
    <MudItem xs="12" sm="4">
        <PhotoDetailsComponent Photo="photo" Details Edit Delete />
    </MudItem>
</Virtualize>
</MudGrid>
```

Modify the code like this:

```cs
@code {
    Virtualize<Photo> virtualizecomponent = default!;
    int totalNumberOfPhotos;

    protected override async Task OnInitializedAsync() {
        totalNumberOfPhotos = await photosService.GetPhotosCountAsync();
        await virtualizecomponent.RefreshDataAsync();
    }

    private async ValueTask<ItemsProviderResult<Photo>> LoadPhotos(ItemsProviderRequest request) {
        int numberOfPhotos = Math.Min(request.Count, totalNumberOfPhotos - request.StartIndex);
        List<Photo> photos = await photosService.GetPhotosAsync(request.StartIndex, numberOfPhotos, request.CancellationToken);

        return new ItemsProviderResult<Photo>(photos, totalNumberOfPhotos);
    }
}
```

## Try it

Start all your applications.  
Log on as `alice` or `bob` (password `Pass123$`) to upload at least 10 to 15 pictures.  
Go to `AllPhotos`. You should see that the scrollbar never changes, but you can keep scrolling.  
If you inspect the network you shold see the different calls made to the server.

---

We're done with optimizing.  
In the next lab we're going to add some validation.
