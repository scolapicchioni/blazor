# FrontEnd: Connecting with the BackEnd

In this lab we're going connect our FrontEnd to the BackEnd.

Our client will issue requests to our server using [gRPC Web](https://github.com/grpc/grpc/blob/master/doc/PROTOCOL-WEB.md) and it will handle the results to update the model. Blazor already takes care of updating the UI.

Let's start by our `frontend` project.

We're going to replace our old Memory Repository with a new one that uses `gRPC Client` as described in [the Microsoft Documentation](https://docs.microsoft.com/en-us/aspnet/core/grpc/browser?view=aspnetcore-6.0#configure-grpc-web-with-the-net-grpc-client).

## Create a new Repository with HttpClient

To use gRPC-Web in the `PhotoSharingApplication.Frontend.Infrastructure` project: 
- Add a reference to the following NuGet Packages:
    - `Google.Protobuf`
    - `Grpc.Net.Client`
    - `Grpc.Net.Client.Web`
    - `Grpc.Tools`
- In the `Solution Explorer` under `Repositories` folder, add a new folder `Grpc`
- Add a `Protos` folder under the `Grpc` folder
- Copy the `comments.proto` file from the `Protos` folder of the `PhotoSharingApplication.WebServices.Grpc.Comments` to the `Protos` folder of the `PhotoSharingApplication.Frontend.Infrastructure` project
- In the `Solution Explorer`, right click the `comments.proto` file of the `PhotoSharingApplication.Frontend.Infrastructure` project, Select `Properties`
    - In the `Build Action` select `Protobuf Compiler`
    - In the `gRPC Stub Classes` select `Client Only`
- Build the application
- In the `Grpc` folder, add a `CommentsRepository` class
- Let the `CommentsRepository` class implement the `ICommentsRepository` interface

```cs
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;

namespace PhotoSharingApplication.Frontend.Infrastructure.Repositories.Grpc;

public class CommentsRepository : ICommentsRepository {
    public async Task<Comment?> CreateAsync(Comment comment) {
        
    }

    public async Task<Comment?> FindAsync(int id) {
        
    }

    public async Task<List<Comment>?> GetCommentsForPhotoAsync(int photoId) {
        
    }

    public async Task<Comment?> RemoveAsync(int id) {
        
    }

    public async Task<Comment?> UpdateAsync(Comment comment) {
        
    }
}
```

Let's require a dependency on a `Commenter.CommenterClient` object

```cs
private readonly Commenter.CommenterClient gRpcClient;

public CommentsRepository(Commenter.CommenterClient gRpcClient) {
    this.gRpcClient = gRpcClient;
}
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

- In the `PhotoSharingApplication.Frontend.BlazorWebAssembly`project:
- Open the `Program.cs` file and add the following code

```cs
builder.Services.AddSingleton(services => {
    var backendUrl = "https://localhost:5001"; // Local debug URL
    var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
    var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions { HttpClient = httpClient });
    return new Commenter.CommenterClient(channel);
});
```

which require

```cs
using Grpc.Net.Client.Web;
using Grpc.Net.Client;
using PhotoSharingApplication.WebServices.Grpc.Comments;
```

**NOTE: Your port may be different, make sure the number after localhost matches the one of your gRPC endpoint**

- Replace 

```cs
builder.Services.AddScoped<IPhotosRepository, PhotoSharingApplication.Frontend.Infrastructure.Repositories.Memory.PhotosRepository>();
builder.Services.AddScoped<ICommentsRepository, PhotoSharingApplication.Frontend.Infrastructure.Repositories.Memory.CommentsRepository>();
```

with

```cs
builder.Services.AddScoped<IPhotosRepository, PhotoSharingApplication.Frontend.Infrastructure.Repositories.Rest.PhotosRepository>();
builder.Services.AddScoped<ICommentsRepository, PhotoSharingApplication.Frontend.Infrastructure.Repositories.Grpc.CommentsRepository>();
```

## Start 3 projects

In order to start both projects at the same time, we need to configure the Solution in Visual Studio

- In the `Solution Explorer`, right click on the Solution, select `Set Startup Projects`
- Click on `Multiple Startup Projects`
- Set `PhotoSharingApplication.Frontend.BlazorWebAssembly` on `Start`
- Set `PhotoSharingApplication.WebServices.REST.Photos` on `Start`
- Set `PhotoSharingApplication.WebServices.Grpc.Comments` on `Start`
- Click `Ok`
- Start the three projects by pressing `F5`

Navigate to `/photos/`.

You will notice an error in the browser console: 

```
Failed to fetch
```

This happens because our server does not allow gRPC Web and [Cross Origin Requests (CORS)](https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-6.0). Let's proceed to modify our server project, as explained in the [documentation](https://docs.microsoft.com/en-us/aspnet/core/grpc/browser?view=aspnetcore-5.0#configure-grpc-web-in-aspnet-core).

### Configure gRPC Web
In the ``PhotoSharingApplication.WebServices.Grpc.Comments`` project:

- Add the `Grpc.AspNetCore.Web` NuGet Package
- Open `Startup.cs`
- In the `Configure` method, between UseRouting and UseEndPoint, add

```cs
app.UseGrpcWeb();
```

- Add the following code:

```cs
endpoints.MapGrpcService<CommentsGrpcService>().EnableGrpcWeb();
```         

### Configure CORS

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

- Under `UseGrpcWeb`, add the following code:

```cs
app.UseCors();
```

- In the `UseEndpoint`, change

```cs
endpoints.MapGrpcService<Services.CommentsService>().EnableGrpcWeb();
```

into

```cs
endpoints.MapGrpcService<Services.CommentsService>().EnableGrpcWeb().RequireCors("AllowAll"); 
```

Save and verify that the client can send data to the server.

## Reconfiguring ports ans startup projects

Let's reconfigure our projects to listen on ports that have no conflict with the other projects
- The `BlazorWebAssembly` project will use `http://localhost:5000` and `https://localhost:5001`
- The `Rest` project will use `http://localhost:5002` and `https://localhost:5003`
- The `gRpc` project will use `http://localhost:5004` and `https://localhost:5005`
- The Blazor project will invoke the REST and gRpc services on the new ports

### Blazor Web Assembly

- In the `Solution Explorer`, right click the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project, select `Properties`
- In the `Properties` window of your project, click on `Debug`
- In the `Profile`, select `PhotoSharingApplication.Frontend.BlazorWebAssembly`
- In the `App Url`, ensure that the value is `https://localhost:5001;http://localhost:5000`
- Save
- Right click the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project, select `Set as Startup Project`
- Click on the green arrow (or press F5) and verify that the project starts from port 5001
- Stop the application

### Photos REST API

- In the `Solution Explorer`, right click the `PhotoSharingApplication.WebServices.REST.Photos` project, select `Properties`
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
- On the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project, select `Start`
- On the `PhotoSharingApplication.WebServices.REST.Photos` project, select `Start`
- On the `PhotoSharingApplication.WebServices.Grpc.Comments` project, select `Start`

### Connect Blazor to the new REST and gRPC ports

We need to reconfigure the `HttpClient` and the `gRPC Client` with the new ports.

- Open `Program.cs` of the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project
- Change this code

```cs
builder.Services.AddSingleton(services => {
  var backendUrl = "https://localhost:5001"; // Local debug URL
  var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
  var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions { HttpClient = httpClient });
  return new CommentsBaseService.CommentsBaseServiceClient(channel);
});
```

into this code

```cs
builder.Services.AddSingleton(services => {
  var backendUrl = "https://localhost:5005";
  var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
  var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions { HttpClient = httpClient });
  return new CommentsBaseService.CommentsBaseServiceClient(channel);
});
```

Also update the connection to the REST service as follows:

```cs
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5003/") });
```

### Try the application

Run all the projects by pressing F5 and verify that they all start and that they can communicate with each other

The lab is complete, we successfully connected our frontend with the backend.

Our Photos and Comments have an empty UserName, so we need to introduce the concept of Identity, which is what we're going to cover in the next labs. 

---

Go to `Labs/Lab10`, open the `readme.md` and follow the instructions thereby contained.   
