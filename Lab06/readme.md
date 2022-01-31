# FrontEnd: Connecting with the BackEnd

In this lab we're going connect everything together.  

We're going to use a design pattern called [Backends For Frontends](https://docs.microsoft.com/en-us/azure/architecture/patterns/backends-for-frontends), where the client calls our own server (*home* so to speak) and the server calls the Rest Service, basically acting like a reverse proxy.

Our client will issue http requests to our server and it will handle the results to update the model. Blazor already takes care of updating the UI.  
Our Server will forward the calls to the REST service and return the results to the client. Although we could use libraries to forward the call (such as [YARP](https://microsoft.github.io/reverse-proxy/index.html) for example), we're going to do it manually. It's a bit more work, but in our case it is going to be a copy/paste of code that we have to write anyway. Feel free to learn and use YARP if you prefer.

Let's start by our `Frontend` project.

We're going to replace our old Memory Repository with a new one that uses [`HttpClient`](https://docs.microsoft.com/en-us/aspnet/core/blazor/call-web-api?view=aspnetcore-6.0).

## Create a new Repository with HttpClient

- In the `Solution Explorer` under `Infrastructure\Repositories` folder of the `PhotoSharingApplication.Frontend.Client` project, add a new `Rest` folder
- In the `Rest` folder, add a `PhotosRepository` class
- Let the `PhotosRepository` class implement the `IPhotosRepository` interface

```cs
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Rest;

public class PhotosRepository : IPhotosRepository {
    public Task<Photo?> CreateAsync(Photo photo) {
        throw new NotImplementedException();
    }

    public Task<Photo?> FindAsync(int id) {
        throw new NotImplementedException();
    }

    public Task<List<Photo>> GetPhotosAsync(int amount = 10) {
        throw new NotImplementedException();
    }

    public Task<Photo?> RemoveAsync(int id) {
        throw new NotImplementedException();
    }

    public Task<Photo?> UpdateAsync(Photo photo) {
        throw new NotImplementedException();
    }
}
```

Let's require a dependency on an `HttpClient` object

```cs
private readonly HttpClient http;

public PhotosRepository(HttpClient http) => this.http = http;
```

Now let's implement the different actions

## The GetAll and Find

- The `FindAsync` becomes

```cs
public async Task<Photo?> FindAsync(int id) => await http.GetFromJsonAsync<Photo>($"/photos/{id}");
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
public async Task<Photo?> CreateAsync(Photo photo) {
    HttpResponseMessage response = await http.PostAsJsonAsync("/photos", photo);
    return await response.Content.ReadFromJsonAsync<Photo>();
}
```

## The  Update

- The `UpdateAsync` becomes

```cs
public async Task<Photo?> UpdateAsync(Photo photo) {
    HttpResponseMessage response = await http.PutAsJsonAsync($"/photos/{photo.Id}", photo);
    return await response.Content.ReadFromJsonAsync<Photo>();
}
```

## The Delete

- The `RemoveAsync` becomes

```cs
public async Task<Photo?> RemoveAsync(int id) {
    HttpResponseMessage response = await http.DeleteAsync($"/photos/{id}");
    return await response.Content.ReadFromJsonAsync<Photo>();
}
```

Now we need to inject the Repository and configure the HttpClient.

## Configuration

- Open the `Program.cs` file of the `PhotoSharingApplication.Frontend.Client` project

- Replace 

```cs
builder.Services.AddScoped<IPhotosRepository, PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Memory.PhotosRepository>();
```

with

```cs
builder.Services.AddScoped<IPhotosRepository, PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Rest.PhotosRepository>();
```

## The Backend

The Client calls the Server and the Server now needs a REST service to call the REST Service.  
- We're going to create a `Controller` just like the one we made for the `REST` project
- The `Controller` will use a `Service`, again like the one we made for the `REST` project
- The `Service` will use a `Repository`, which will look like the `PhotosRepository` we created in the `Client` project

### The Controller

Copy the `PhotosController` class from the `Controllers` folder of the `REST` project and paste it under the `Controllers` folder of the `Frontend.Server` project.  
Change the namespace to match the folder structure.

```cs
namespace PhotoSharingApplication.Frontend.Server.Controllers;
```

### The Service

Copy the `Core` folder of the `REST` project and paste it into the `Frontend.Server` project, with all its content.  
Change the namespace of the `PhotosService` class to match the folder structure.

```cs
namespace PhotoSharingApplication.Frontend.Server.Core.Services;
```

### The Repository

Copy the `Infrastructure` folder of the `Frontend.Client` project and paste it into the `Frontend.Server` project, with all its content.  
You can remove the `Memory` folder if you want to.
Change the namespace of the `PhotosRepository` class to match the folder structure.

```cs
namespace PhotoSharingApplication.Frontend.Server.Infrastructure.Repositories.Rest;
```

### Dependency Injection

Open the `Program.cs` file of the `Frontend.Server` project.  Add the following lines right before the building of the app:

```cs
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5003") });
builder.Services.AddScoped<IPhotosService, PhotosService>();
builder.Services.AddScoped<IPhotosRepository, PhotoSharingApplication.Frontend.Server.Infrastructure.Repositories.Rest.PhotosRepository>();

//add those previous lines before this one:
var app = builder.Build();
```

**NOTE: Your port may be diffferent. Ensure that the address you setup for the HttpClient corresponds to the one where your Rest Service is running**

## Start both projects

In order to start both projects at the same time, we need to configure the Solution in Visual Studio

- In the `Solution Explorer`, right click on the Solution, select `Set Startup Projects`
- Click on `Multiple Startup Projects`
- Set `PhotoSharingApplication.Frontend.Client` on `Start`
- Set `PhotoSharingApplication.WebServices.Rest.Photos` on `Start`
- Click `Ok`
- Start the two projects by pressing `F5`

You will notice an error in the browser console: 

```
Failed to fetch
```

This happens because our Rest service does not allow [Cross Origin Requests (CORS)](https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-6.0). Let's proceed to modify our Rest project.

- Open `Program.cs` of the `PhotoSharingApplication.WebServices.Rest.Photos` project and add the following code right before the building of the app

```cs
builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder =>
{
    builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
}));
//add the previous statement before this line:
var app = builder.Build();
```

- Also in Program.cs, add the following code **BEFORE app.UseAuthorization**  

```cs
app.UseCors("AllowAll");
```

Save and verify that the client can send and receive data to and from the server.

The lab is complete, we successfully connected our frontend with the backend.

By the way, this could be a good moment to get rid of all the files, folders and projects that we don't need, just to clean up the whole solution.  

---

In the next 3 labs we're going to implement the *Comments* functionality.

- Lab07 will take care of the FrontEnd:
The `Details` Page will not only show the Photo details, it will also show the related comments and will allow the user to create, update and delete comments. For now we're going to use a Memory repository, with no real Backend side
- Lab08 will focus on the Backend:
The server will expose its functionalities through a gRpc Service
- Lab09 will connect the frontend to the backend


Go to `Labs/Lab07`, open the `readme.md` and follow the instructions to continue.   