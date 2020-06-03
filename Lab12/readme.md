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
    - `Photo` will not contain `ImageMimeType` and `PhotoFile` anymore. In their place, we'll add a link to the controller action that the client will then use to download the file with the image.  
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
    - Configure the generation of the `Id`, just like we did for the `Photo.Id`
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
        public PhotoSharingApplicationContext(DbContextOptions<PhotoSharingApplicationContext> options)
            : base(options) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Photo>(ConfigurePhoto);
            modelBuilder.Entity<PhotoImage>(ConfigurePhotoImage);
            modelBuilder.Entity<Comment>(ConfigureComment);
        }

        private void ConfigurePhoto(EntityTypeBuilder<Photo> builder) {
            builder.ToTable("Photos");

            builder.Property(p => p.Id)
                .UseHiLo("photos_hilo")
                .IsRequired();

            builder.Property(p => p.Title)
                .IsRequired(true)
                .HasMaxLength(255);

            builder.Ignore(p=>p.ImageUrl);

            builder.HasOne(p => p.PhotoImage)
                .WithOne()
                .HasForeignKey<PhotoImage>(p => p.Id);
        }
        private void ConfigurePhotoImage(EntityTypeBuilder<PhotoImage> builder) {
            builder.ToTable("Photos");

            builder.Property(p => p.Id)
                .UseHiLo("photos_hilo")
                .IsRequired();

            builder.Property(p => p.PhotoFile)
                .IsRequired(true);

            builder.Property(p => p.ImageMimeType)
                .IsRequired(true)
                .HasMaxLength(255);
        }

        private void ConfigureComment(EntityTypeBuilder<Comment> builder) {
            builder.ToTable("Comments");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .UseHiLo("comments_hilo")
               .IsRequired();

            builder.Property(c => c.Subject)
                .IsRequired()
                .HasMaxLength(250);

            builder.HasOne(c => c.Photo)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PhotoId);

            builder.Property(c => c.PhotoId).IsRequired();
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

        public PhotosService(IPhotosRepository repository, IAuthorizationService<Photo> photosAuthorizationService, IUserService userService) {
            this.repository = repository;
            this.photosAuthorizationService = photosAuthorizationService;
            this.userService = userService;
        }
        public async Task<Photo> FindWithImageAsync(int id) => await repository.FindWithImageAsync(id);
        public async Task<PhotoImage> GetImageAsync(int id) => await repository.GetImageAsync(id);
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

        public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await (from p in context.Photos
                                                                                 orderby p.CreatedDate descending
                                                                                 select p).Take(amount).ToListAsync();

        public async Task<Photo> RemoveAsync(int id) {
            var photo = await context.Photos.Include(nameof(PhotoImage)).SingleOrDefaultAsync(m => m.Id == id);
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
    ph.ImageUrl = Url.Link(nameof(GetImage), new { id = ph.Id });
    if (ph == null) return NotFound();
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

    - Add a `GetImage` that returns a `FileContentResult` using the `File` method

```cs
[HttpGet("image/{id:int}", Name = "GetImage")]
public async Task<IActionResult> GetImage(int id) {
    PhotoImage ph = await service.GetImageAsync(id);
    if (ph == null) return NotFound();
    return File(ph.PhotoFile, ph.ImageMimeType);
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
using System.Linq;
using System.Threading.Tasks;

namespace PhotoSharingApplication.WebServices.REST.Photos.Controllers {
    [Route("[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase {
        private readonly IPhotosService service;

        public PhotosController(IPhotosService service) {
            this.service = service;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Photo>> CreateAsync(Photo photo) {
            try {
                Photo p = await service.UploadAsync(photo);
                return CreatedAtRoute("Find", p, new { id = p.Id});
            } catch (UnauthorizedCreateAttemptException<Photo>) {
                return Forbid();
            }
        }

        [HttpGet("withimage/{id:int}", Name = "FindWithImage")]
        public async Task<ActionResult<Photo>> FindWithImage(int id) {
            Photo ph = await service.FindWithImageAsync(id);
            if (ph == null) return NotFound();
            return ph;
        }

        [HttpGet("{id:int}", Name = "Find")]
        public async Task<ActionResult<Photo>> Find(int id) {
            Photo ph = await service.FindAsync(id);
            ph.ImageUrl = Url.Link(nameof(GetImage), new { id = ph.Id });
            if (ph == null) return NotFound();
            return ph;
        }

        [HttpGet("image/{id:int}", Name = "GetImage")]
        public async Task<IActionResult> GetImage(int id) {
            PhotoImage ph = await service.GetImageAsync(id);
            if (ph == null) return NotFound();
            return File(ph.PhotoFile, ph.ImageMimeType);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos() {
            List<Photo> photos = (await service.GetPhotosAsync()).ToList();
            photos.ForEach(p => p.ImageUrl = Url.Link(nameof(GetImage), new { id = p.Id }));
            return photos;
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Photo>> Remove(int id) {
            Photo ph = await service.FindAsync(id);
            if (ph == null) return NotFound();

            try { 
                return await service.RemoveAsync(id);
            } catch(UnauthorizedDeleteAttemptException<Photo>) {
                return Forbid();
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<Photo>> Update(int id, Photo photo) {
            if (id != photo.Id)
                return BadRequest();
            Photo ph = await service.FindAsync(id);

            try { 
                return await service.UpdateAsync(photo);
            } catch(UnauthorizedEditAttemptException<Photo>) {
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
    - ```public async Task<Photo> FindWithImageAsync(int id) => await http.GetFromJsonAsync<Photo>($"/photos/withimage/{id}");```
    - ```public async Task<PhotoImage> GetImageAsync(int id) => await http.GetFromJsonAsync<PhotoImage>($"/photos/image/{id}");```

The code should look like this:

```cs
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Exceptions;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Infrastructure.Repositories.Rest {
    public class PhotosRepository : IPhotosRepository {
        private readonly HttpClient http;
        private readonly IAccessTokenProvider tokenProvider;

        public PhotosRepository(HttpClient http, IAccessTokenProvider tokenProvider) {
            this.http = http;
            this.tokenProvider = tokenProvider;
        }

        public async Task<Photo> CreateAsync(Photo photo) {
            var tokenResult = await tokenProvider.RequestAccessToken(new AccessTokenRequestOptions() { Scopes = new string[] { "photosrest" } });
            if (tokenResult.TryGetToken(out var token)) {
                var requestMessage = new HttpRequestMessage() {
                    Method = new HttpMethod("POST"),
                    RequestUri = new Uri(http.BaseAddress, "/photos"),
                    Content = JsonContent.Create(photo)
                };

                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);

                var response = await http.SendAsync(requestMessage);
                return await response.Content.ReadFromJsonAsync<Photo>();
            } else {
                throw new UnauthorizedCreateAttemptException<Photo>();
            }
        }

        public async Task<Photo> FindAsync(int id) => await http.GetFromJsonAsync<Photo>($"/photos/{id}");
        public async Task<Photo> FindWithImageAsync(int id) => await http.GetFromJsonAsync<Photo>($"/photos/withimage/{id}");
        public async Task<PhotoImage> GetImageAsync(int id) => await http.GetFromJsonAsync<PhotoImage>($"/photos/image/{id}");

        public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await http.GetFromJsonAsync<List<Photo>>("/photos");

        public async Task<Photo> RemoveAsync(int id) {
            var tokenResult = await tokenProvider.RequestAccessToken(new AccessTokenRequestOptions() { Scopes = new string[] { "photosrest" } });
            if (tokenResult.TryGetToken(out var token)) {
                var requestMessage = new HttpRequestMessage() {
                    Method = new HttpMethod("DELETE"),
                    RequestUri = new Uri(http.BaseAddress, $"/photos/{id}")
                };

                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);

                var response = await http.SendAsync(requestMessage);
                return await response.Content.ReadFromJsonAsync<Photo>();
            } else {
                throw new UnauthorizedDeleteAttemptException<Photo>();
            }
        }

        public async Task<Photo> UpdateAsync(Photo photo) {
            var tokenResult = await tokenProvider.RequestAccessToken(new AccessTokenRequestOptions() { Scopes = new string[] { "photosrest" } });
            if (tokenResult.TryGetToken(out var token)) {
                var requestMessage = new HttpRequestMessage() {
                    Method = new HttpMethod("PUT"),
                    RequestUri = new Uri(http.BaseAddress, $"/photos/{photo.Id}"),
                    Content = JsonContent.Create(photo)
                };

                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);

                var response = await http.SendAsync(requestMessage);
                return await response.Content.ReadFromJsonAsync<Photo>();
            } else {
                throw new UnauthorizedEditAttemptException<Photo>();
            }
        }
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
            photo.Id = photos.Max(p=>p.Id) + 1;
            photos.Add(photo);
            return Task.FromResult(photo);
        }

        public Task<Photo> FindAsync(int id) => Task.FromResult(photos.FirstOrDefault(p => p.Id == id));

        public Task<Photo> FindWithImageAsync(int id) => Task.FromResult(photos.FirstOrDefault(p => p.Id == id));

        public Task<PhotoImage> GetImageAsync(int id) => Task.FromResult(photoImages.FirstOrDefault(p => p.Id == id));

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

 - In the `Solution Explorer`, under the  `PhotoSharingApplication.Frontend.BlazorComponents` project, open the `PhotoDetailsComponent` razor component
 - Locate the `<MatMediaCard>` component and change its `ImageUrl` to `@Photo.ImageUrl`

The code should look like this:

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
        <MatCardMedia Wide="true" ImageUrl="@Photo.ImageUrl"></MatCardMedia>
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
                <MatButton Link="@($"/photos/details/{Photo.Id}")">Details</MatButton>
            }
            @if (Edit && mayEdit) {
                <MatButton Link="@($"/photos/update/{Photo.Id}")">Update</MatButton>
            }
            @if (Delete && mayDelete) {
                <MatButton Link="@($"/photos/delete/{Photo.Id}")">Delete</MatButton>
            }
            @if (DeleteConfirm) {
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

- In the `Solution Explorer`, under the  `PhotoSharingApplication.Frontend.BlazorComponents` project, open the `PhotoDetailsComponent` razor component
- Locate the `HandleMatFileSelected` method and change it to make use of the new model structure 

The code should look like this:

```html
@using PhotoSharingApplication.Shared.Core.Entities

<MatCard>
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
        <MatCardMedia Wide="true" ImageUrl="@(Photo.PhotoImage.PhotoFile == null ? "" : $"data:{Photo.PhotoImage.ImageMimeType};base64,{Convert.ToBase64String(Photo.PhotoImage.PhotoFile)}")"></MatCardMedia>
    </MatCardContent>
</MatCard>

@code {
    [Parameter]
    public Photo Photo { get; set; }

    [Parameter]
    public EventCallback<Photo> OnSave { get; set; }


    async Task HandleMatFileSelected(IMatFileUploadEntry[] files) {
        if (Photo.PhotoImage == null)
            Photo.PhotoImage = new PhotoImage();
        IMatFileUploadEntry file = files.FirstOrDefault();
        Photo.PhotoImage.ImageMimeType = file.Type;

        if (file == null) {
            return;
        }

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

We're done!

If you run the application, already the first time you navigate to `AllPhotos` you should see the images appear one by one, which is already an improvement. But the best improvement is if you navigate away and go back to the AllPhotos: you will see that the browser uses the cached pictures instead of downloading them again.

