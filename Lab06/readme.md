# FrontEnd: Connecting with the BackEnd

In this lab we're going connect everything together.  

We're going to use a design pattern called [Backends For Frontends](https://learn.microsoft.com/en-us/azure/architecture/patterns/backends-for-frontends), where the client calls our own server (*home* so to speak) and the server calls the Rest Service, using a reverse proxy.

Our client will issue http requests to our server and it will handle the results to update the model. Blazor already takes care of updating the UI.  
Our Server will forward the calls to the REST service and return the results to the client. Since there is no point in reinventing the wheel, we're going to use [YARP](https://microsoft.github.io/reverse-proxy/index.html), a library that will forward the calls for us so that we don't have to write any service for it.

Let's start by our `Frontend` project.

We're going to replace our old Memory Repository with a new one that uses [`HttpClient`](https://learn.microsoft.com/en-us/aspnet/core/blazor/call-web-api?view=aspnetcore-7.0&pivots=webassembly). 

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
public async Task<Photo?> FindAsync(int id) => await http.GetFromJsonAsync<Photo>($"/api/photos/{id}");
```

which requires

```cs
using System.Net.Http.Json;
```

- The GetPhotosAsync becomes

```cs
public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await http.GetFromJsonAsync<List<Photo>>($"/api/photos");
```

## The Create

- The `CreateAsync` becomes

```cs
public async Task<Photo?> CreateAsync(Photo photo) {
    HttpResponseMessage response = await http.PostAsJsonAsync("/api/photos", photo);
    return await response.Content.ReadFromJsonAsync<Photo>();
}
```

## The  Update

- The `UpdateAsync` becomes

```cs
public async Task<Photo?> UpdateAsync(Photo photo) {
    HttpResponseMessage response = await http.PutAsJsonAsync($"/api/photos/{photo.Id}", photo);
    return await response.Content.ReadFromJsonAsync<Photo>();
}
```

## The Delete

- The `RemoveAsync` becomes

```cs
public async Task<Photo?> RemoveAsync(int id) {
    HttpResponseMessage response = await http.DeleteAsync($"/api/photos/{id}");
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

## The Backend for Frontend

The Client calls the Server and the Server now needs to forward the call to the REST service. We need to:  
- Add the YARP package, as described in the [Getting Started](https://microsoft.github.io/reverse-proxy/articles/getting-started.html)
- Add the services and middleware to the pipeline
- Configure YARP to forward the calls to the correct backend address

- In the `PhotoSharingApplication.Frontend.Server` project, add a `Yarp.ReverseProxy` nuGet package
- [Add the YARP Middleware](https://microsoft.github.io/reverse-proxy/articles/getting-started.html#add-the-yarp-middleware) by opening `Program.cs` and adding the following lines:

```cs
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
```

Then replace 

```cs
app.MapControllers();
```

with 

```cs
app.MapReverseProxy();
```

- Add the [Configuration](https://microsoft.github.io/reverse-proxy/articles/getting-started.html#configuration) to the `appsettings.json` file:

```json
"ReverseProxy": {
    "Routes": {
        "photosrestroute": {
            "ClusterId": "photosrestcluster",
            "Match": {
                "Path": "/api/photos/{*any}"
            }
        }
    },
    "Clusters": {
        "photosrestcluster": {
            "Destinations": {
                "photosrestdestination": {
                "Address": "https://localhost:5003"
                }
            }
        }
    }
}
```

**NOTE: My `Address` section uses `https://localhost:5003` because my REST service is running on that port. Yours may be running on a different one. If you want to run on the same port, right click on the PhotoSharingApplication.WebServices.REST.Photos project name in the Visual Studio Solution Explorer, select the `Debug` section, click ok `Open debug lunch profile UI` then change the `App URL` from what you have to `https://localhost:5003;http://localhost:5004`**

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

This happens because our Rest service does not allow [Cross Origin Requests (CORS)](https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-7.0). Let's proceed to modify our Rest project.

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

NOTE: If you still cannot receive a response, you may want to check the logs. To do that follow the [documentation](https://microsoft.github.io/reverse-proxy/articles/diagnosing-yarp-issues.html) and add this keys to the `Logging` section of the `appsettings.json` file on your server project:  

```json
"Microsoft": "Information",
"Yarp": "Information"
```

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