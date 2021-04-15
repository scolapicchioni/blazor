# FrontEnd: Connecting with the BackEnd

In this lab we're going connect our FrontEnd to the BackEnd.

Our client will issue http requests to our server and it will handle the results to update the model. Blazor already takes care of updating the UI.

Let's start by our `frontend` project.

We're going to replace our old Memory Repository with a new one that uses [`HttpClient`](https://docs.microsoft.com/en-us/aspnet/core/blazor/call-web-api?view=aspnetcore-6.0).

## Create a new Repository with HttpClient

- Reference the `System.Net.Http.Json` NuGet package in the `PhotoSharingApplication.Frontend.Infrastructure` project file (make sure to select the latest prerelease)
- In the `Solution Explorer` under `Repositories` folder of the `PhotoSharingApplication.Frontend.Infrastructure` project, add a new folder `Rest`
- In the `Rest` folder, add a `PhotosRepository` class
- Let the `PhotosRepository` class implement the `IPhotosRepository` interface

```cs
public class PhotosRepository : IPhotosRepository {
    public async Task<Photo> CreateAsync(Photo photo) {
        
    }

    public async Task<Photo> FindAsync(int id) {
        
    }

    public async Task<List<Photo>> GetPhotosAsync(int amount = 10) {
        
    }

    public async Task<Photo> RemoveAsync(int id) {
        
    }

    public async Task<Photo> UpdateAsync(Photo photo) {
        
    }
}
```

Let's require a dependency on an `HttpClient` object

```cs
private readonly HttpClient http;

public PhotosRepository(HttpClient http) {
    this.http = http;
}
```

which requires

```cs
using System.Net.Http;
```

Now let's implement the different actions

## The GetAll and Find

- The `FindAsync` becomes

```cs
public async Task<Photo> FindAsync(int id) => await http.GetFromJsonAsync<Photo>($"/photos/{id}");
```

which requires

```cs
using System.Net.Http.Json;
```

- The GetPhotosAsync becomes

```cs
public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await http.GetFromJsonAsync<List<Photo>>($"/photos");
```

## The Create

- The `CreateAsync` becomes

```cs
public async Task<Photo> CreateAsync(Photo photo) {
    HttpResponseMessage  response = await http.PostAsJsonAsync("/photos", photo);
    return await response.Content.ReadFromJsonAsync<Photo>();
}
```

## The  Update

- The `UpdateAsync` becomes

```cs
public async Task<Photo> UpdateAsync(Photo photo) {
    HttpResponseMessage response = await http.PutAsJsonAsync($"/photos/{photo.Id}", photo);
    return await response.Content.ReadFromJsonAsync<Photo>();
}
```

## The Delete

- The `RemoveAsync` becomes

```cs
public async Task<Photo> RemoveAsync(int id) {
    HttpResponseMessage response = await http.DeleteAsync($"/photos/{id}");
    return await response.Content.ReadFromJsonAsync<Photo>();
}
```

Now we need to inject the Repository and configure the HttpClient.

## Configuration

- Open the `Program.cs` file of the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project

- Replace

```cs
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
```

with

```cs
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:44379/") });
```

**NOTE: Your port may be different, make sure the number after localhost matches the one of your rest endpoint**

- Replace 

```cs
builder.Services.AddScoped<IPhotosRepository, PhotoSharingApplication.Frontend.Infrastructure.Repositories.Memory.PhotosRepository>();
```

with

```cs
builder.Services.AddScoped<IPhotosRepository, PhotoSharingApplication.Frontend.Infrastructure.Repositories.Rest.PhotosRepository>();
```

## Start both projects

In order to start both projects at the same time, we need to configure the Solution in Visual Studio

- In the `Solution Explorer`, right click on the Solution, select `Set Startup Projects`
- Click on `Multiple Startup Projects`
- Set `PhotoSharingApplication.Frontend.BlazorWebAssembly` on `Start`
- Set `PhotoSharingApplication.WebServices.REST.Photos` on `Start`
- Click `Ok`
- Start the two projects by pressing `F5`

You will notice an error in the browser console: 

```
Failed to fetch
```

This happens because our server does not allow [Cross Origin Requests (CORS)](https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-6.0). Let's proceed to modify our server project.

- Open `Startup.cs` of the `PhotoSharingApplication.WebServices.REST.Photos` project
- In the `ConfigureServices` method, add the following code:

```cs
services.AddCors(o => o.AddPolicy("AllowAll", builder =>
  {
      builder.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
  }));
```

- In the `Configure` method, **Between UseRouting and UseAuthorization**  

add the following code:

```cs
app.UseCors("AllowAll");
```

Save and verify that the client can send and receive data to and from the server.

The lab is complete, we successfully connected our frontend with the backend.

---

In the next 3 labs we're going to implement the *Comments* functionality.

- Lab07 will take care of the FrontEnd:
The `Details` Page will not only show the Photo details, it will also show the related comments and will allow the user to create, update and delete comments. For now we're going to use a Memory repository, with no real Backend side
- Lab08 will focus on the Backend:
The server will expose its functionalities through a gRpc Service
- Lab09 will connect the frontend to the backend


Go to `Labs/Lab07`, open the `readme.md` and follow the instructions to continue.   