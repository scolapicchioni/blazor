# FrontEnd: Connecting with the BackEnd

In this lab we're going connect our FrontEnd to the BackEnd.  

Once again, since we have a [BFF](https://docs.microsoft.com/en-us/azure/architecture/patterns/backends-for-frontends), our client will just call our own *home* REST service, being unaware that back there we actually have a gRpc service. It will be the job of the BFF to call the gRpc service and return the results to the client.

Our client will issue requests to our server and it will handle the results to update the model. Blazor already takes care of updating the UI.

Let's start by our `Frontend.Server` project, where we need to build:
- A Controller
- A Service
- A Repository

## The Controller 

We need to build a REST service, which will be the endpoint that the client will use. Just like we did to talk to the Photos Rest service, we will create a CommentsController API Controller, deriving from `ControllerBase`.

- Under the `Controllers` folder, add a new API Controller called `CommentsController`.
- Add a dependency on an `ICommentsService` interface.
- Add the REST actions to
    - Get Comments For a specific photo given its id
    - Find a comment given its id
    - Add a comment
    - Update a comment
    - Delete a comment

```cs
using Microsoft.AspNetCore.Mvc;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.Frontend.Server.Controllers;

[Route("[controller]")]
[ApiController]
public class CommentsController : ControllerBase {
    private readonly ICommentsService service;

    public CommentsController(ICommentsService service) {
        this.service = service;
    }
    [HttpGet("/photos/{photoId:int}/comments")]
    public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsForPhoto(int photoId) => await service.GetCommentsForPhotoAsync(photoId);

    [HttpGet("{id:int}", Name = "FindComment")]
    public async Task<ActionResult<Comment>> Find(int id) {
        Comment? cm = await service.FindAsync(id);
        if (cm is null) return NotFound();
        return cm;
    }

    [HttpPost]
    public async Task<ActionResult<Comment>> CreateAsync(Comment comment) {
        Comment? c = await service.CreateAsync(comment);
        return CreatedAtRoute("FindComment", new { id = c.Id }, c);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Comment>> Update(int id, Comment comment) {
        if (id != comment.Id)
            return BadRequest();
        return await service.UpdateAsync(comment);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Comment>> Remove(int id) {
        Comment? cm = await service.FindAsync(id);
        if (cm is null) return NotFound();
        return await service.RemoveAsync(id);
    }
}
```

## The Service

As per our usual pattern, the `CommentsService` will make use of the `CommentsRepository`.
- Under the `Core/Services` folder of the `PhotoSharingApplication.Frontend.Server` project, add a new `CommentsService` class. Let the class implement the `ICommentsService` interface.
- Add a dependency on the `ICommentsRepository` interface.
- Implement the interface to use the `CommentsRepository` to get the data.

```cs
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.Frontend.Server.Core.Services;

public class CommentsService : ICommentsService {
    private readonly ICommentsRepository repository;

    public CommentsService(ICommentsRepository repository) {
        this.repository = repository;
    }
    public async Task<Comment?> CreateAsync(Comment comment) {
        comment.SubmittedOn = DateTime.Now;
        comment.UserName ??= "";
        return await repository.CreateAsync(comment);
    }

    public async Task<Comment?> FindAsync(int id) => await repository.FindAsync(id);

    public async Task<List<Comment>?> GetCommentsForPhotoAsync(int photoId) => await repository.GetCommentsForPhotoAsync(photoId);

    public async Task<Comment?> RemoveAsync(int id) => await repository.RemoveAsync(id);

    public async Task<Comment?> UpdateAsync(Comment comment) {
        Comment oldComment = await repository.FindAsync(comment.Id);
        oldComment.Subject = comment.Subject;
        oldComment.Body = comment.Body;
        oldComment.SubmittedOn = DateTime.Now;
        oldComment.UserName ??= "";
        return await repository.UpdateAsync(oldComment);
    }
}
```

## The Repository 
We're going create a Repository that uses `gRPC Client` as described in [the Microsoft Documentation](https://docs.microsoft.com/en-us/aspnet/core/grpc/client?view=aspnetcore-6.0).


To use gRPC-Client in the `PhotoSharingApplication.Frontend.Server` project: 
- Add a reference to the following NuGet Packages:
    - `Google.Protobuf`
    - `Grpc.Net.Client`
    - `Grpc.Tools`
- In the `Solution Explorer` under `Repositories` folder, add a new folder `Grpc`
- Copy the `Protos` folder (and its content) of the `PhotoSharingApplication.WebServices.Grpc.Comments` under the `Grpc` folder
- In the `Solution Explorer`, right click the `comments.proto` file of the `PhotoSharingApplication.Frontend.Server` project, Select `Properties`
    - In the `Build Action` select `Protobuf Compiler`
    - In the `gRPC Stub Classes` select `Client Only`
- Build the application
- In the `Grpc` folder, add a `CommentsRepository` class
- Let the `CommentsRepository` class implement the `ICommentsRepository` interface

```cs
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.Frontend.Server.Infrastructure.Repositories.Grpc;

public class CommentsRepository : ICommentsRepository {
    public Task<Comment?> CreateAsync(Comment comment) {
        throw new NotImplementedException();
    }

    public Task<Comment?> FindAsync(int id) {
        throw new NotImplementedException();
    }

    public Task<List<Comment>?> GetCommentsForPhotoAsync(int photoId) {
        throw new NotImplementedException();
    }

    public Task<Comment?> RemoveAsync(int id) {
        throw new NotImplementedException();
    }

    public Task<Comment?> UpdateAsync(Comment comment) {
        throw new NotImplementedException();
    }
}
```

Let's require a dependency on a `Commenter.CommenterClient` object

```cs
private readonly Commenter.CommenterClient gRpcClient;

public CommentsRepository(Commenter.CommenterClient gRpcClient) => this.gRpcClient = gRpcClient;
```

which requires a

```cs
using PhotoSharingApplication.WebServices.Grpc.Comments;
```

Now let's implement the different actions. Each action will need to translate the different `***Request` / `***Reply` to and from `Comment`.

## The GetAll and Find

- The `FindAsync` becomes

```cs
public async Task<Comment?> FindAsync(int id) {
    FindReply c = await gRpcClient.FindAsync(new FindRequest() { Id = id });
    return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
}
```

- The `GetCommentsForPhotoAsync` becomes

```cs
public async Task<List<Comment>?> GetCommentsForPhotoAsync(int photoId) {
    GetCommentsForPhotosReply resp = await gRpcClient.GetCommentsForPhotoAsync(new GetCommentsForPhotosRequest() { PhotoId = photoId });
    return resp.Comments.Select(c => new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() }).ToList();
}
```

## The Create

- The `CreateAsync` becomes

```cs
public async Task<Comment?> CreateAsync(Comment comment) {
    CreateRequest createRequest = new CreateRequest() { PhotoId = comment.PhotoId, Subject = comment.Subject, Body = comment.Body };
    CreateReply c = await gRpcClient.CreateAsync(createRequest);
    return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
}
```

## The Update

- The `UpdateAsync` becomes

```cs
public async Task<Comment?> UpdateAsync(Comment comment) {
    UpdateReply c = await gRpcClient.UpdateAsync(new UpdateRequest { Id = comment.Id, Subject = comment.Subject, Body = comment.Body });
    return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
}
```

## The Delete

- The `RemoveAsync` becomes

```cs
public async Task<Comment?> RemoveAsync(int id) {
    RemoveReply c = await gRpcClient.RemoveAsync(new RemoveRequest() { Id = id });
    return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
}
```

Now we need to inject the Repository and configure the gRpcClient.

## Configuration

- In the `PhotoSharingApplication.Frontend.Server`project:
- Open the `Program.cs` file and add the following code

```cs
builder.Services.AddScoped<ICommentsService, CommentsService>();
builder.Services.AddScoped<ICommentsRepository, PhotoSharingApplication.Frontend.Server.Infrastructure.Repositories.Grpc.CommentsRepository>();
builder.Services.AddSingleton(services => {
    var backendUrl = "https://localhost:5005"; // Local debug URL
    var channel = GrpcChannel.ForAddress(backendUrl);
    return new Commenter.CommenterClient(channel);
});
```

which requires

```cs
using Grpc.Net.Client;
using PhotoSharingApplication.Frontend.Server.Core.Services;
using PhotoSharingApplication.Shared.Interfaces;
using PhotoSharingApplication.WebServices.Grpc.Comments;
```

**NOTE: Your port may be different, make sure the number after localhost matches the one of your gRPC endpoint**

## The Client

Now that the BFF is ready, we need the client to talk to the BFF.  
We need to replace the fake MemoryRepository we made in the previous lab with a new Repository that uses an HttpClient to talk to our Rest service.

- Under the `Infrastructure/Repositories/Rest` folder of the `PhotoSharingApplication.Frontend.Client` project, add a new `CommentsRepository` class
    - Add a dependency on a `HttpClient` object
    - Implement the `ICommentsRepository` interface and make use of the httpclient to talk to the BFF

```cs
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;
using System.Net.Http.Json;

namespace PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Rest;

public class CommentsRepository : ICommentsRepository {
    private readonly HttpClient http;

    public CommentsRepository(HttpClient http) {
        this.http = http;
    }

    public async Task<Comment?> CreateAsync(Comment comment) {
        HttpResponseMessage response = await http.PostAsJsonAsync("/comments", comment);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<Comment>();
        else throw new Exception(response.ReasonPhrase);
    }

    public async Task<List<Comment>?> GetCommentsForPhotoAsync(int photoId) => await http.GetFromJsonAsync<List<Comment>>($"/photos/{photoId}/comments");

    public async Task<Comment?> UpdateAsync(Comment comment) {
        HttpResponseMessage response = await http.PutAsJsonAsync($"/comments/{comment.Id}", comment);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<Comment>();
        else throw new Exception(response.ReasonPhrase);
    }

    public async Task<Comment?> FindAsync(int id) => await http.GetFromJsonAsync<Comment>($"/comments/{id}");

    public async Task<Comment?> RemoveAsync(int id) {
        HttpResponseMessage response = await http.DeleteAsync($"/comments/{id}");
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<Comment>();
        else throw new Exception(response.ReasonPhrase);
    }
}
```

### Register the repository

In the `Program.cs` of the `PhotoSharingApplication.Frontend.Client` project, replace 

```cs
builder.Services.AddScoped<IPhotosRepository, PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Memory.PhotosRepository>();
builder.Services.AddScoped<ICommentsRepository, PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Memory.CommentsRepository>();
```
with

```cs
builder.Services.AddScoped<IPhotosRepository, PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Rest.PhotosRepository>();
builder.Services.AddScoped<ICommentsRepository, PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Rest.CommentsRepository>();
```

## Start 3 projects

In order to start both projects at the same time, we need to configure the Solution in Visual Studio

- In the `Solution Explorer`, right click on the Solution, select `Set Startup Projects`
- Click on `Multiple Startup Projects`
- Set `PhotoSharingApplication.Frontend.Server` on `Start`
- Set `PhotoSharingApplication.WebServices.REST.Photos` on `Start`
- Set `PhotoSharingApplication.WebServices.Grpc.Comments` on `Start`
- Click `Ok`
- Start the three projects by pressing `F5`

Navigate to `/photos/`.

You will notice an error in the browser console: 

```
Failed to fetch
```

This happens because our server does not allow [Cross Origin Requests (CORS)](https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-6.0). Let's proceed to modify our server project, as explained in the [documentation](https://docs.microsoft.com/en-us/aspnet/core/grpc/browser?view=aspnetcore-5.0#configure-grpc-web-in-aspnet-core).

### Configure CORS
In the ``PhotoSharingApplication.WebServices.Grpc.Comments`` project:

As explained in the [documentation](https://docs.microsoft.com/en-us/aspnet/core/grpc/browser?view=aspnetcore-6.0#grpc-web-and-cors)

- In `Program.cs`, before the building of the app, add the following code:

```cs
builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder => {
    builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
}));

//add the previous statement before this line:
var app = builder.Build();
```

- Before the `MapGrpcService`, add the following code:

```cs
app.UseCors();
```

- Replace 

```cs
endpoints.MapGrpcService<Services.CommentsService>();
```

with

```cs
app.MapGrpcService<CommentsGrpcService>().RequireCors("AllowAll"); 
```

Save and verify that the client can send data to the server.

## Reconfiguring ports ans startup projects

Let's reconfigure our projects to listen on ports that have no conflict with the other projects
- The `Frontend.Server` project will use `http://localhost:5000` and `https://localhost:5001`
- The `Rest.Photos` project will use `http://localhost:5002` and `https://localhost:5003`
- The `gRpc.Comments` project will use `http://localhost:5004` and `https://localhost:5005`
- The Blazor Server project will invoke the REST and gRpc services on the new ports

### Frontend Server

- In the `Solution Explorer`, right click the `PhotoSharingApplication.Frontend.Server` project, select `Properties`
- In the `Properties` window of your project, click on `Debug`
- In the `Profile`, select `PhotoSharingApplication.Frontend.Server`
- In the `App Url`, ensure that the value is `https://localhost:5001;http://localhost:5000`
- Save
- Right click the `PhotoSharingApplication.Frontend.Server` project, select `Set as Startup Project`
- Click on the green arrow (or press F5) and verify that the project starts from port 5001
- Stop the application

### Photos REST API

- In the `Solution Explorer`, right click the `PhotoSharingApplication.WebServices.Rest.Photos` project, select `Properties`
- In the `Properties` window of your project, click on `Debug`
- In the `App Url`, ensure that the value is `https://localhost:5003;http://localhost:5002`
- Save
- Click on the green arrow (or press F5) and verify that the project starts from port 5003
- Stop the application

### Comments gRPC API

- In the `Solution Explorer`, right click the `PhotoSharingApplication.WebServices.Grpc.Comments` project, select `Properties`
- In the `Properties` window of your project, click on `Debug`
- In the `Profile`, select `PhotoSharingApplication.WebServices.Grpc.Comments`
- In the `App Url`, ensure that the value is `https://localhost:5005;http://localhost:5004`
- Save
- Click on the green arrow (or press F5) and verify that the project starts from port 5005
- Stop the application

### Multiple Startup Projects
- In the `Solution Explorer`, right click the solution, select `Set Startup Projects`
- On the `PhotoSharingApplication.Frontend.Server` project, select `Start`
- On the `PhotoSharingApplication.WebServices.REST.Photos` project, select `Start`
- On the `PhotoSharingApplication.WebServices.Grpc.Comments` project, select `Start`

### Connect Blazor BFF to the new REST and gRPC ports

We need to reconfigure the `HttpClient` and the `gRPC Client` of the `Frontend.Server` project with the new ports.

- Open `Program.cs` of the `PhotoSharingApplication.Frontend.Server` project
- Update the port of the GrpcChannel

```cs
builder.Services.AddSingleton(services => {
    var backendUrl = "https://localhost:5005"; // Local debug URL
    var channel = GrpcChannel.ForAddress(backendUrl);
    return new Commenter.CommenterClient(channel);
});
```

Also update the connection to the REST service:

```cs
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5003/") });
```

### Try the application

Run all the projects by pressing F5 and verify that they all start and that they can communicate with each other

The lab is complete, we successfully connected our frontend with the backend.

Our Photos and Comments have an empty UserName, so we need to introduce the concept of Identity, which is what we're going to cover in the next labs. 

---

Go to `Labs/Lab10`, open the `readme.md` and follow the instructions thereby contained.   
