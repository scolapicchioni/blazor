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
namespace PhotoSharingApplication.Shared.Core.Entities {
    public class PhotoImage {
        public int Id { get; set; }
        public byte[] PhotoFile { get; set; }
        public string ImageMimeType { get; set; }
    }
}
```

- Open the `Photo` class
- Delete the  `PhotoFile` and the `ImageMimeType` properties 
- Add a `ImageUrl` property of type `string`
- Add a `PhotoImage` property of type `PhotoImage`

The code should look like this:

```cs
using System;
using System.Collections.Generic;

namespace PhotoSharingApplication.Shared.Core.Entities {
    public class Photo {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UserName { get; set; }
        public string ImageUrl { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public PhotoImage PhotoImage { get; set; }
    }
}
```

## The DbContext

We have to configure the relationship between the `Photo` and the `PhotoImage` entities. We're going to use [Table Splitting](https://docs.microsoft.com/en-us/ef/core/modeling/table-splitting).

- In the `Solution Explorer`, under the `Data` folder of the `PhotoSharingApplication.Backend.Infrastructure` project, open the `PhotoSharingApplicationContext` class
- Add a new `PhotoImages` property of type `DbSet<PhotoImage>`
- Add a new `ConfigurePhotoImage` method to
    - Map the property to the `Photos` table
    - Require both the `PhotoFile` and the `ImageMimeType` fields
- Invoke the `ConfigurePhotoImage` from the `OnModelCreating`
- Change the `ConfigurePhoto` to [exclude](https://docs.microsoft.com/en-us/ef/core/modeling/entity-properties?tabs=fluent-api%2Cwithout-nrt#included-and-excluded-properties) the `ImageUrl` field, so that it's not saved on the database

The code should look like this:

```cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhotoSharingApplication.Shared.Core.Entities;

namespace PhotoSharingApplication.Backend.Infrastructure.Data {
    public class PhotoSharingApplicationContext : DbContext {
        public PhotoSharingApplicationContext(DbContextOptions<PhotoSharingApplicationContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Photo>(ConfigurePhoto);
            modelBuilder.Entity<Comment>(ConfigureComment);
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

            builder.Property(p => p.PhotoFile)
                .IsRequired(true);

            builder.Property(p => p.ImageMimeType)
                .IsRequired(true)
                .HasMaxLength(255);
        }
        private void ConfigureComment(EntityTypeBuilder<Comment> builder) {
            builder.ToTable("Comments");

            builder.Property(comment => comment.Subject)
                .IsRequired()
                .HasMaxLength(250);
        }

        public DbSet<Photo> Photos { get; set; }
        public DbSet<PhotoImage> PhotoImages { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }
}
```

## The Interfaces

- In the `Solution Explorer`, under the `Interfaces` folder of the `PhotoSharingApplication.Shared.Core` project, open the `IPhotosService` interface, then add
    - ```Task<Photo> FindWithImageAsync(int id);```
    - ```Task<PhotoImage> GetImageAsync(int id);```

The code should look like this:

```cs
using PhotoSharingApplication.Shared.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Shared.Core.Interfaces {
    public interface IPhotosService {
        Task<Photo> UploadAsync(Photo photo);
        Task<Photo> UpdateAsync(Photo photo);
        Task<Photo> FindAsync(int id);
        Task<Photo> FindWithImageAsync(int id);
        Task<PhotoImage> GetImageAsync(int id);
        Task<List<Photo>> GetPhotosAsync(int amount = 10);
        Task<Photo> RemoveAsync(int id);
    }
}
```

- In the `Solution Explorer`, under the `Interfaces` folder of the `PhotoSharingApplication.Shared.Core` project, open the `IPhotosRepository` interface, then add
    - ```Task<Photo> FindWithImageAsync(int id);```
    - ```Task<PhotoImage> GetImageAsync(int id);```

The code should look like this:

```cs
using PhotoSharingApplication.Shared.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Shared.Core.Interfaces {
    public interface IPhotosRepository {
        Task<List<Photo>> GetPhotosAsync(int amount = 10);
        Task<Photo> FindAsync(int id);
        Task<Photo> FindWithImageAsync(int id);
        Task<PhotoImage> GetImageAsync(int id);
        Task<Photo> CreateAsync(Photo photo);
        Task<Photo> UpdateAsync(Photo photo);
        Task<Photo> RemoveAsync(int id);
    }
}
```

## Backend

### The Service

- In the `Solution Explorer`, under the `Services` folder of the `PhotoSharingApplication.Backend.Core` project, open the `PhotosService` class, then add
    - ```public async Task<Photo> FindWithImageAsync(int id) => await repository.FindWithImageAsync(id);```
    - ```public async Task<PhotoImage> GetImageAsync(int id) => await repository.GetImageAsync(id);```

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
        public async Task<Photo> FindWithImageAsync(int id) => await repository.FindWithImageAsync(id);
        public async Task<PhotoImage> GetImageAsync(int id) => await repository.GetImageAsync(id);
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

### The Repository

- In the `Solution Explorer`, under the `EntityFramework` folder of the `Repositories` folder of the  `PhotoSharingApplication.Backend.Infrastructure` project, open the `PhotosService` class, then add
    - ```public async Task<Photo> FindWithImageAsync(int id) => await context.Photos.Include(nameof(PhotoImage)).AsNoTracking().SingleOrDefaultAsync(m => m.Id == id);```
    - ```public async Task<PhotoImage> GetImageAsync(int id) => await context.PhotoImages.AsNoTracking().SingleOrDefaultAsync(prop => prop.Id == id);```
- Change the `RemoveAsync` to *include* the `PhotoImage` entity

The code should look like this:

```cs
using Microsoft.EntityFrameworkCore;
using PhotoSharingApplication.Backend.Infrastructure.Data;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Backend.Infrastructure.Repositories.EntityFramework {
    public class PhotosRepository : IPhotosRepository {
        private readonly PhotoSharingApplicationContext context;

        public PhotosRepository(PhotoSharingApplicationContext context) {
            this.context = context;
        }
        public async Task<Photo> CreateAsync(Photo photo) {
            context.Add(photo);
            await context.SaveChangesAsync();
            return photo;
        }

        public async Task<Photo> FindAsync(int id) => await context.Photos.AsNoTracking().SingleOrDefaultAsync(m => m.Id == id);
        public async Task<Photo> FindWithImageAsync(int id) => await context.Photos.Include(nameof(PhotoImage)).AsNoTracking().SingleOrDefaultAsync(m => m.Id == id);
        public async Task<PhotoImage> GetImageAsync(int id) => await context.PhotoImages.AsNoTracking().SingleOrDefaultAsync(prop => prop.Id == id);

        public async Task<List<Photo>> GetPhotosAsync(int amount = 10) =>
            await (from p in context.Photos
                   orderby p.CreatedDate descending
                   select p).Take(amount).ToListAsync();

        public async Task<Photo> RemoveAsync(int id) {
            var photo = await context.Photos.SingleOrDefaultAsync(m => m.Id == id);
            context.Photos.Remove(photo);
            await context.SaveChangesAsync();
            return photo;
        }

        public async Task<Photo> UpdateAsync(Photo photo) {
            context.Update(photo);
            await context.SaveChangesAsync();
            return photo;
        }
    }
}
```

### The REST Controller

- In the `Solution Explorer`, under the `Controllers` folder of the  `PhotoSharingApplication.WebServices.REST.Photos` project, open the `PhotosControllers` class, then add
    -  The `FindWithImage` method

```cs
[HttpGet("withimage/{id:int}", Name = "FindWithImage")]
public async Task<ActionResult<Photo>> FindWithImage(int id) {
    Photo ph = await service.FindWithImageAsync(id);
    if (ph == null) return NotFound();
    return ph;
}
```
    - The `GetImage` method

```cs
[HttpGet("image/{id:int}", Name = "GetImage")]
public async Task<IActionResult> GetImage(int id) {
    PhotoImage ph = await service.GetImageAsync(id);
    if (ph == null) return NotFound();
    return File(ph.PhotoFile, ph.ImageMimeType);
}
```

    - Change the `FindImage` to include the `ImageUrl` address, as explained in the [article](https://odetocode.com/blogs/scott/archive/2013/03/27/webapi-tip-5-generating-links.aspx) of the late Scott Allen:

```cs
[HttpGet("{id:int}", Name = "Find")]
public async Task<ActionResult<Photo>> Find(int id) {
    Photo ph = await service.FindAsync(id);
    if (ph == null) return NotFound();
    ph.ImageUrl = Url.Link(nameof(GetImage), new { id = ph.Id });
    return ph;
}
```

    - Use the same strategy for the `GetPhotos`

```cs
[HttpGet]
public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos() {
    List<Photo> photos = (await service.GetPhotosAsync()).ToList();
    photos.ForEach(p => p.ImageUrl = Url.Link(nameof(GetImage), new { id = p.Id }));
    return photos;
}
```

The final code should look like this:

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
        public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos() {
            List<Photo> photos = await service.GetPhotosAsync();
            photos.ForEach(p => p.ImageUrl = Url.Link(nameof(GetImage), new { id = p.Id }));
            return photos;
        }

        [HttpGet("{id:int}", Name = "Find")]
        public async Task<ActionResult<Photo>> Find(int id) {
            Photo ph = await service.FindAsync(id);
            if (ph == null) return NotFound();
            ph.ImageUrl = Url.Link(nameof(GetImage), new { id = ph.Id });
            return ph;
        }

        [HttpGet("withimage/{id:int}", Name = "FindWithImage")]
        public async Task<ActionResult<Photo>> FindWithImage(int id) {
            Photo ph = await service.FindWithImageAsync(id);
            if (ph == null) return NotFound();
            return ph;
        }

        [HttpGet("image/{id:int}", Name = "GetImage")]
        public async Task<IActionResult> GetImage(int id) {
            PhotoImage ph = await service.GetImageAsync(id);
            if (ph == null) return NotFound();
            return File(ph.PhotoFile, ph.ImageMimeType);
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

## The Frontend

### The Service

- In the `Solution Explorer`, under the `Services` folder of the `PhotoSharingApplication.Frontend.Core` project, open the `PhotosService` class, then add
    - ```public async Task<Photo> FindWithImageAsync(int id) => await repository.FindWithImageAsync(id);```
    - ```public async Task<PhotoImage> GetImageAsync(int id) => await repository.GetImageAsync(id);```

The code should look like this:

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

        public PhotosService(IPhotosRepository repository, IAuthorizationService<Photo> photosAuthorizationService, IUserService userService) {
            this.repository = repository;
            this.photosAuthorizationService = photosAuthorizationService;
            this.userService = userService;
        }
        public async Task<Photo> FindAsync(int id) => await repository.FindAsync(id);
        public async Task<Photo> FindWithImageAsync(int id) => await repository.FindWithImageAsync(id);
        public async Task<PhotoImage> GetImageAsync(int id) => await repository.GetImageAsync(id);
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
            if (await photosAuthorizationService.ItemMayBeUpdatedAsync(user,photo))
                return await repository.UpdateAsync(photo);
            else throw new UnauthorizedEditAttemptException<Photo>($"Unauthorized Edit Attempt of Photo {photo.Id}");
        }
        public async Task<Photo> UploadAsync(Photo photo) {
            var user = await userService.GetUserAsync();
            if (await photosAuthorizationService.ItemMayBeCreatedAsync(user,photo)) {
                photo.CreatedDate = DateTime.Now;
                photo.UserName = user.Identity.Name;
                return await repository.CreateAsync(photo);
            }else throw new UnauthorizedCreateAttemptException<Photo>($"Unauthorized Create Attempt of Photo {photo.Id}");
        }
    }
}
```

### The Repository

- In the `Solution Explorer`, under the `Rest` folder of the `Repositories` folder of the  `PhotoSharingApplication.Frontend.Infrastructure` project, open the `PhotosService` class, then add
    - ```public async Task<Photo> FindWithImageAsync(int id) => await publicPhotosClient.FindWithImageAsync(id);```
    - ```public async Task<PhotoImage> GetImageAsync(int id) => await publicPhotosClient.GetImageAsync(id);```

The code should look like this:

```cs
using PhotoSharingApplication.Frontend.Infrastructure.TypedHttpClients;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Infrastructure.Repositories.Rest {
    public class PhotosRepository : IPhotosRepository {
        private readonly PublicPhotosClient publicPhotosClient;
        private readonly ProtectedPhotosClient protectedPhotosClient;

        public PhotosRepository(PublicPhotosClient publicPhotosClient, ProtectedPhotosClient protectedPhotosClient) {
            this.publicPhotosClient = publicPhotosClient;
            this.protectedPhotosClient = protectedPhotosClient;
        }
        public async Task<Photo> CreateAsync(Photo photo) => await protectedPhotosClient.CreateAsync(photo);

        public async Task<Photo> FindAsync(int id) => await publicPhotosClient.FindAsync(id);

        public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await publicPhotosClient.GetPhotosAsync(amount);
        public async Task<Photo> FindWithImageAsync(int id) => await publicPhotosClient.FindWithImageAsync(id);
        public async Task<PhotoImage> GetImageAsync(int id) => await publicPhotosClient.GetImageAsync(id);
        public async Task<Photo> RemoveAsync(int id) => await protectedPhotosClient.RemoveAsync(id);
        public async Task<Photo> UpdateAsync(Photo photo) => await protectedPhotosClient.UpdateAsync(photo);
    }
}
```
### PublicPhotosClient

- In the `PublicPhotosClient` file located under the `TypedHttpClients` folder of the `PhotoSharingApplication.Frontend.Infrastructure` project, add
    - ```public async Task<Photo> FindWithImageAsync(int id) => await http.GetFromJsonAsync<Photo>($"/photos/withimage/{id}");```
    - ```public async Task<PhotoImage> GetImageAsync(int id) => await http.GetFromJsonAsync<PhotoImage>($"/photos/image/{id}");```

The code should look like this

```cs
using PhotoSharingApplication.Shared.Core.Entities;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Infrastructure.TypedHttpClients {
    public class PublicPhotosClient {
        private readonly HttpClient http;
        public PublicPhotosClient(HttpClient http) => this.http = http;

        public async Task<Photo> FindAsync(int id) => await http.GetFromJsonAsync<Photo>($"/photos/{id}");
        public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await http.GetFromJsonAsync<List<Photo>>($"/photos");
        public async Task<Photo> FindWithImageAsync(int id) => await http.GetFromJsonAsync<Photo>($"/photos/withimage/{id}");
        public async Task<PhotoImage> GetImageAsync(int id) => await http.GetFromJsonAsync<PhotoImage>($"/photos/image/{id}");
    }
}
```

### Memory Repository

 - In the `Solution Explorer`, under the `Memory` folder of the `Repositories` folder of the  `PhotoSharingApplication.Frontend.Infrastructure` project, open the `PhotosService` class, then complete the class so that it compiles

 The code should look like this:

 ```cs
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Infrastructure.Repositories.Memory {
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
        public Task<Photo> CreateAsync(Photo photo) {
            photo.Id = photos.Max(p => p.Id) + 1;
            photos.Add(photo);
            return Task.FromResult(photo);
        }

        public Task<Photo> FindAsync(int id) => Task.FromResult(photos.FirstOrDefault(p => p.Id == id));
        public Task<List<Photo>> GetPhotosAsync(int amount = 10) => Task.FromResult(photos.OrderByDescending(p => p.CreatedDate).ThenBy(p => p.Title).Take(amount).ToList());
        public Task<Photo> FindWithImageAsync(int id) => Task.FromResult(photos.FirstOrDefault(p => p.Id == id));
        public Task<PhotoImage> GetImageAsync(int id) => Task.FromResult(photoImages.FirstOrDefault(p => p.Id == id));

        public Task<Photo> RemoveAsync(int id) {
            Photo photo = photos.FirstOrDefault(p => p.Id == id);
            if (photo != null) photos.Remove(photo);
            return Task.FromResult(photo);
        }

        public Task<Photo> UpdateAsync(Photo photo) {
            Photo oldPhoto = photos.FirstOrDefault(p => p.Id == photo.Id);
            if (oldPhoto != null) {
                oldPhoto.Title = photo.Title;
                oldPhoto.Description = photo.Description;
                oldPhoto.CreatedDate = photo.CreatedDate;
                oldPhoto.UserName = photo.UserName;
                if (oldPhoto.PhotoImage == null)
                    oldPhoto.PhotoImage = new PhotoImage();
                oldPhoto.PhotoImage.PhotoFile = photo.PhotoImage?.PhotoFile;
                oldPhoto.PhotoImage.ImageMimeType = photo.PhotoImage?.ImageMimeType;
            }
            return Task.FromResult(oldPhoto);
        }
    }
}
```

### Blazor Component

In one of the previous labs we created a separate `PhotoPictureComponent.razor` in the `PhotoSharingApplication.Frontend.BlazorComponents`.  
We then used it in the `AllPhotos`, in the `PhotosDetails` and in the `UploadPhoto`.  
Now we need to make a distinction whether the image is to be downloaded from the server or if it's a file that the user selected from her device to upload it.

- In the `Solution Explorer`, under the  `PhotoSharingApplication.Frontend.BlazorComponents` project, open the `PhotoPictureComponent.razor` component
- Add a `bool` property `IsLocal`
- Render the `<MatMediaCard>` as it was if `IsLocal` is true
- Render the `<MatMediaCard>` component with `ImageUrl` to `@Photo.ImageUrl` if `IsLocal` is `false`

The code should look like this:

```html
@if (IsLocal) {
<MatCardMedia Wide="true" ImageUrl="@(Photo.PhotoImage?.PhotoFile is null ? "" : $"data:{Photo.PhotoImage.ImageMimeType};base64,{Convert.ToBase64String(Photo.PhotoImage.PhotoFile)}")"></MatCardMedia>
} else {
<MatCardMedia Wide="true" ImageUrl="@Photo.ImageUrl"></MatCardMedia>
}

@code {
    [Parameter]
    public Photo Photo { get; set; }

    [Parameter]
    public bool IsLocal { get; set; }
}
```

- In the `Solution Explorer`, under the  `PhotoSharingApplication.Frontend.BlazorComponents` project, open the `PhotoEditComponent` razor component
- Locate the `HandleMatFileSelected` method and change it to make use of the new model structure 
- Locate the `<PhotoPictureComponent>` tag and add a `IsLocal` property setting it to `true`

The code should look like this:

```html
<MatCard>
    <MatH3>Upload Photo</MatH3>
    <MatCardContent>
        <EditForm Model="@Photo" OnValidSubmit="@(async ()=> await OnSave.InvokeAsync(Photo))">
            <p>
                <MatTextField @bind-Value="@Photo.Title" Label="Title" FullWidth></MatTextField>
            </p>
            <p>
                <MatTextField @bind-Value="@Photo.Description" Label="Description" TextArea FullWidth></MatTextField>
            </p>
            <p>
                <MatFileUpload OnChange="@HandleMatFileSelected"></MatFileUpload>
            </p>
            <p>
                <MatButton Type="submit">Upload</MatButton>
            </p>
        </EditForm>
        <PhotoPictureComponent Photo="Photo" IsLocal="true"></PhotoPictureComponent>
    </MatCardContent>
</MatCard>

@code {
    [Parameter]
    public Photo Photo { get; set; }

    [Parameter]
    public EventCallback<Photo> OnSave { get; set; }

    async Task HandleMatFileSelected(IMatFileUploadEntry[] files) {
        IMatFileUploadEntry file = files.FirstOrDefault();
        if (file == null) {
            return;
        }
        if (Photo.PhotoImage is null) Photo.PhotoImage = new PhotoImage();
        Photo.PhotoImage.ImageMimeType = file.Type;
        using (var stream = new System.IO.MemoryStream()) {
            await file.WriteToStreamAsync(stream);
            Photo.PhotoImage.PhotoFile = stream.ToArray();
        }
    }
}
```

### Blazor Web Assembly

 - In the `Solution Explorer`, under the  `PhotoSharingApplication.Frontend.BlazorWebAssembly` project, open the `UploadPhoto` razor component
 - Locate the `OnInitialized` method and change its content to instantiate both the `Photo` and the `PhotoImage` objects

The code should look like this:

```html
@page "/photos/upload"
@using PhotoSharingApplication.Shared.Core.Exceptions
 
@inject IPhotosService photosService
@inject NavigationManager navigationManager
@attribute [Authorize]

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
        photo.PhotoImage = new PhotoImage();
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

- In the `Solution Explorer`, under the  `PhotoSharingApplication.Frontend.BlazorWebAssembly` project, open the `UpdatePhoto` razor component
- Locate the `OnInitialized` method and change its content to invoke the `FindWithImageAsync` method

The code should look like this:

```html
@page "/photos/update/{id:int}"
@using PhotoSharingApplication.Shared.Core.Exceptions
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
        photo = await photosService.FindWithImageAsync(Id);
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

If you run the application, already the first time you navigate to `AllPhotos` you should see the images appear one by one, which is already an improvement. But the best improvement is if you navigate away and go back to the AllPhotos: you will see that the browser uses the cached pictures instead of downloading them again.

This is a nice accomplishment, but we can do more to imrove the perceived performance of our application by using a [Blazor component virtualization](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/virtualization?view=aspnetcore-6.0).

## Virtualize Component

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
- Two interfaces in the `PhotoSharingApplication.Shared.Core.Interfaces`
    - `IPhotosService.GetPhotosAsync` so that it accepts more parameters
    - `IPhotosService.GetPhotosCountAsync` to return an int
    - `IPhotosRepository.GetPhotosAsync` so that it accepts more parameters
    - `IPhotosRepository.GetPhotosCountAsync` to return an int
- One class in the `PhotoSharingApplication.Frontend.Core.Services`
    - Modify `PhotosService.GetPhotosAsync`
    - Add `PhotosService.GetPhotosCountAsync`
- Two classes in the `PhotoSharingApplication.Frontend.Infrastructure.Repositories`
    - Modify `Rest.PhotosRepository.GetPhotosAsync`
    - Add `Rest.PhotosRepository.GetPhotosCountAsync`
    - Modify `Memory.PhotosRepository.GetPhotosAsync`
    - Add `Memory.PhotosRepository.GetPhotosCountAsync`
- One class in the `PhotoSharingApplication.Frontend.Infrastructure.TypedHttpClients`
    - Modify `PublicPhotosClient.GetPhotosAsync`
    - Add `PublicPhotosClient.GetPhotosCountAsync`
- One class in the `PhotoSharingApplication.Backend.Core.Services`
    - Modify `PhotosService.GetPhotosAsync`
    - Add `PhotosService.GetPhotosCountAsync`
- One class in the `PhotoSharingApplication.Backend.Infrastructure.Repositories.EntityFramework`
    - Modify `PhotosRepository.GetPhotosAsync`
    - Add `PhotosRepository.GetPhotosCountAsync`
- One class in the `PhotoSharingApplication.WebServices.REST.Photos.Controllers`
    - Modify `GetPhotos`
    - Add `GetPhotosCount`
- One razor component in the `PhotoSharingApplication.Frontend.BlazorWebAssembly.Pages`
    - `AllPhotos.razor`

Let's get to it.

## Interfaces
- Open the `IPhotosService` interface located under the `Interfaces` folder of the `PhotoSharingApplication.Shared.Core` project
- Replace this code

```cs
Task<List<Photo>> GetPhotosAsync(int amount = 10);
```

with this
```cs
Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken);

Task<int> GetPhotosCountAsync();
```

- Open the `IPhotosRepository` interface located under the `Interfaces` folder of the `PhotoSharingApplication.Shared.Core` project.
- Replace this code

```cs
Task<List<Photo>> GetPhotosAsync(int amount = 10);
```

with this
```cs
Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken);

Task<int> GetPhotosCountAsync();
```

## FrontEnd

- Open the `PhotosService` class located under the `Services` folder of the `PhotoSharingApplication.Frontend.Core` project 
- Replace this code 

```cs
public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await repository.GetPhotosAsync(amount);
```

with this:
```cs
public async Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken) => await repository.GetPhotosAsync(startIndex, amount, cancellationToken);

public async Task<int> GetPhotosCountAsync() => await repository.GetPhotosCountAsync();
```
- Open the `PhotosRepository` class located under the `Repositories/Rest` folder of the `PhotoSharingApplication.Frontend.Infrastructure` project
- Replace this code
```cs
public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await publicPhotosClient.GetPhotosAsync(amount);
```
with this
```cs
public async Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken) => await publicPhotosClient.GetPhotosAsync(amount);

public async Task<int> GetPhotosCountAsync()=> await publicPhotosClient.GetPhotosCountAsync();
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

- Open the `PublicPhotosClient` located under the `TypedHttpClients` folder of the `PhotoSharingApplication.Frontend.Infrastructure` project.
- Replace this code
```cs
public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await http.GetFromJsonAsync<List<Photo>>($"/photos");
```
with this
```cs
public async Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken) => await http.GetFromJsonAsync<List<Photo>>($"/photos/{startIndex}/{amount}", cancellationToken);

public async Task<int> GetPhotosCountAsync() => int.Parse(await http.GetStringAsync($"/photos/count"));
```

## BackEnd
- Open the `PhotosService` class located under the `Services` folder of the `PhotoSharingApplication.Backend.Core` project
- Replace this code
```cs
public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await repository.GetPhotosAsync(amount);
```
with this
```cs
public async Task<List<Photo>> GetPhotosAsync(int startIndex, int amount, CancellationToken cancellationToken) => await repository.GetPhotosAsync(startIndex, amount, cancellationToken);
        
public async Task<int> GetPhotosCountAsync() => await repository.GetPhotosCountAsync();
```
- Open the `PhotosRepository` class located under the `Repositories/EntityFramework` folder of the `PhotoSharingApplication.Backend.Infrastructure` project
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
- Open the `PhotosController` class located under the `Controllers` folder of the `PhotoSharingApplication.WebServices.REST.Photos` project
- Replace this code
```cs
[HttpGet]
public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos() {
    List<Photo> photos = await service.GetPhotosAsync();
    photos.ForEach(p => p.ImageUrl = Url.Link(nameof(GetImage), new { id = p.Id }));
    return photos;
}
```
with this
```cs
[HttpGet("{startIndex}/{amount}")]
public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos(int startIndex, int amount, CancellationToken cancellationToken) {
    List<Photo> photos = await service.GetPhotosAsync(startIndex, amount, cancellationToken);
    photos.ForEach(p => p.ImageUrl = Url.Link(nameof(GetImage), new { id = p.Id }));
    return photos;
}

[HttpGet("count")]
public async Task<ActionResult<int>> GetPhotosCount() => await service.GetPhotosCountAsync();
```

## Blazor UI
- Open the `AllPhotos.razor` component located under the in the `Pages` folder of the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project
- Replace the following code 
```html
@foreach (var photo in photos) {
    <div class="mat-layout-grid-cell mat-layout-grid-cell-span-4">
        <PhotoDetailsComponent Photo="photo" Details Edit Delete></PhotoDetailsComponent>
    </div>
}
```

with this

```html
<Virtualize @ref="virtualizecomponent" Context="photo" ItemsProvider="@LoadPhotos">
    <div class="mat-layout-grid-cell mat-layout-grid-cell-span-4">
        <PhotoDetailsComponent Photo="photo" Details Edit Delete></PhotoDetailsComponent>
    </div>
</Virtualize>
```

Modify the code like this:

```cs
@code {
    Virtualize<Photo> virtualizecomponent;

    int totalNumberOfPhotos;
    protected override async Task OnInitializedAsync() {
        totalNumberOfPhotos = await photosService.GetPhotosCountAsync();
        await virtualizecomponent.RefreshDataAsync();
    }

    private async ValueTask<ItemsProviderResult<Photo>> LoadPhotos(
    ItemsProviderRequest request) {
        var numberOfPhotos = Math.Min(request.Count, totalNumberOfPhotos - request.StartIndex);
        var employees = await photosService.GetPhotosAsync(request.StartIndex,
            numberOfPhotos, request.CancellationToken);

        return new ItemsProviderResult<Photo>(employees, totalNumberOfPhotos);
    }
}
```

## Try it

Start all your applications.  
Log on as `alice` (password `alice`) or `bob` (password `bob`) to upload at least 10 to 15 pictures.  
Go to `AllPhotos`. You should see that the scrollbar never changes, but you can keep scrolling.  
If you inspect the network you shold see the different calls made to the server.

---

We're done with optimizing.  
In the next lab we're going to add some validation.
