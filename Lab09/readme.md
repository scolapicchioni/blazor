# FrontEnd: Connecting with the BackEnd

In this lab we're going connect our FrontEnd to the BackEnd.  

Once again, since we have a [BFF](https://docs.microsoft.com/en-us/azure/architecture/patterns/backends-for-frontends), our client will just call our own *home*. YARP will forward the call to the gRpc service in the backend return the results to the client.

Our client will issue requests to our server and it will handle the results to update the model. Blazor already takes care of updating the UI.

Let's start by our `Frontend.Server` project, where we need to configure YARP to forward the calls to the backend.  

- Open appsettings.json and replace

```json
"ReverseProxy": {
    "Routes": {
      "photosrestroute": {
        "ClusterId": "photosrestcluster",
        "Match": {
          "Path": "/photos/{*any}"
        }
      }
    },
    "Clusters": {
      "photosrestcluster": {
        "Destinations": {
          "photosrestdestination": {
            "Address": "https://localhost:5003/"
          }
        }
      }
    }
  }
```

with

```json
"ReverseProxy": {
    "Routes": {
      "photosrestroute": {
        "ClusterId": "photosrestcluster",
        "Match": {
          "Path": "/photos/{*any}"
        }
      },
      "commentsgrpcroute": {
        "ClusterId": "commentsgrpccluster",
        "Match": {
          "Path": "/comments.Commenter/{*any}"
        }
      }
    },
    "Clusters": {
      "photosrestcluster": {
        "Destinations": {
          "photosrestdestination": {
            "Address": "https://localhost:5003/"
          }
        }
      },
      "commentsgrpccluster": {
        "Destinations": {
          "commentsgrpdestination": {
            "Address": "https://localhost:5005/"
          }
        }
      }
    }
  }
```

## The Frontend.Client

On the client, we need to replace our in memory repository with one that talks to the gRpc service. By adding the proto file to our client project, we can let the gpRpc tools generate a client for us. We will then use that client in our repository.

### Client generation

To use gRPC-Client in the `PhotoSharingApplication.Frontend.Client` project: 
- Add a reference to the following NuGet Packages:
    - `Google.Protobuf`
    - `Grpc.Net.Client`
    - `Grpc.Net.Client.Web`
    - `Grpc.Tools`
- In the `Solution Explorer` under `Repositories` folder, add a new folder `Grpc`
- Copy the `Protos` folder (and its content) of the `PhotoSharingApplication.WebServices.Grpc.Comments` under the `Grpc` folder
- In the `Solution Explorer`, right click the `comments.proto` file of the `PhotoSharingApplication.Frontend.Client` project, Select `Properties`
    - In the `Build Action` select `Protobuf Compiler`
    - In the `gRPC Stub Classes` select `Client Only`
- Build the application

## The Repository 
We're going create a Repository that uses `gRPC Client` as described in [the Microsoft Documentation](https://docs.microsoft.com/en-us/aspnet/core/grpc/client?view=aspnetcore-6.0).

- In the `Grpc` folder, add a `CommentsRepository` class
- Let the `CommentsRepository` class implement the `ICommentsRepository` interface

```cs
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Grpc;

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

- In the `PhotoSharingApplication.Frontend.Client`project:
- Open the `Program.cs` file and replace

```cs
builder.Services.AddScoped<IPhotosRepository, PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Memory.PhotosRepository>();
builder.Services.AddScoped<ICommentsRepository, PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Memory.CommentsRepository>();
```

with

```cs
builder.Services.AddScoped<IPhotosRepository, PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Rest.PhotosRepository>();
builder.Services.AddScoped<ICommentsRepository, PhotoSharingApplication.Frontend.Client.Infrastructure.Repositories.Grpc.CommentsRepository>();
builder.Services.AddSingleton(services => {
    var backendUrl = new Uri(builder.HostEnvironment.BaseAddress);
    var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions {
        HttpHandler = new GrpcWebHandler(new HttpClientHandler())
    });
    return new Commenter.CommenterClient(channel);
});
```

which requires

```cs
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using PhotoSharingApplication.Frontend.Client;
using PhotoSharingApplication.Frontend.Client.Core.Services;
using PhotoSharingApplication.WebServices.Grpc.Comments;
```

**NOTE: Your port may be different, make sure the number after localhost matches the one of your gRPC endpoint**

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

Save and run the application. You will get yet another error: `Content-Type 'application/grpc-web' is not supported.`. 
This is due to the fact that our gRpc service is not configured to accept gRpc-Web requests. Let's fix that:  

- Add the `Grpc.AspNetCore.Web` package to the `PhotoSharingApplication.WebServices.Grpc.Comments` project
- Open `Program.cs` file of the `PhotoSharingApplication.WebServices.Grpc.Comments` project and replace the following code:

```cs
app.MapGrpcService<CommentsGrpcService>().RequireCors("AllowAll"); 
```

with
```cs
app.UseGrpcWeb();
app.MapGrpcService<CommentsGrpcService>().RequireCors("AllowAll").EnableGrpcWeb();  
```


## Reconfiguring ports and startup projects

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

- Open `appsettings.json` of the `PhotoSharingApplication.Frontend.Server` project
- Update the `Address` entry under the `photosrestdestination` to `https://localhost:5003/`
- Update the `Address` entry under the `commentsgrpdestination` to `https://localhost:5005/`

### Try the application

Run all the projects by pressing F5 and verify that they all start and that they can communicate with each other

The lab is complete, we successfully connected our frontend with the backend.

Our Photos and Comments have an empty UserName, so we need to introduce the concept of Identity, which is what we're going to cover in the next labs. 

---

Go to `Labs/Lab10`, open the `readme.md` and follow the instructions thereby contained.   
