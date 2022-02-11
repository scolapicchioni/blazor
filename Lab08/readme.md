# Backend: gRpc with ASP.NET 6 and Visual Studio 2022

In this lab we're going to take care of our Backend.

We're going to stick to the same [CLEAN architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) that we already have:

- The *Core* defines the business logic. There's going to be a `CommentsService` 
- The *Infrastructure* will contain the `CommentsRepository` where read and save the data with [Entity Framework Core](https://docs.microsoft.com/en-gb/ef/core/) on a SQL Server DataBase.
- An *Application* project, which in this case consists of a [gRpc](https://grpc.io/) service using [ASP.NET Core 6.0 gRpc](https://docs.microsoft.com/en-gb/aspnet/core/grpc/?view=aspnetcore-6.0).

Both the `Service` and the `Repository` will implement the interfaces and make use of the `Comment` entity that we have already defined on the `Shared` project

### Create the project

- On the `Solution Explorer`, right click your solution, then select `Add` -> `New Project`.
- In the `Create a new project` dialog, select `ASP.NET Core gRPC Service` and select `Next`
- Name the project `PhotoSharingApplication.WebServices.Grpc.Comments`. It's important to name the project `PhotoSharingApplication.WebServices.Grpc.Comments` so the namespaces will match when you copy and paste code.
- Leave the `Enable Docker` checkbox unchecked
- Ensure to select the latest .NET Core version (6.0) and select `Create`
- Add a project reference to the `PhotoSharingApplication.Shared` project.

## The Backend Core

- On the ` PhotoSharingApplication.WebServices.Grpc.Comments` prject, add a new `Core` folder.
- Under the `Core` folder, add a new `Services` folder
- Under the `Services` folder, add the following `CommentsService` class:

```cs
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.WebServices.Grpc.Comments.Core.Services;

public class CommentsService : ICommentsService {
    private readonly ICommentsRepository repository;

    public CommentsService(ICommentsRepository repository) => this.repository = repository;
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

Just like for the `PhotosService`: it's true that this class looks like the one we have for the frontend, so we may be tempted to share this as well, but we could also argue that the logic server side may very well be more convoluted than the one on the frontend (you may not want to share your *secrets* with the client), so we're going to keep them separated even if in our case they do the same thing.

## The Infrastructure

In the `PhotoSharingApplication.WebServices.Grpc.Comments` project we need to
- Add a `DbContext` class
- Add a `DbSet<Comment>` to the `DbContext`
- Configure it 
- Register it as a service
- Add a Connection string in the configuration file
- Add a `Migration` and update the database
- Create the `CommentsRepository` class

### The DbContext

- On the `Solution Explorer`, create a new `Infrastructure` folder
- Add the following NuGet packages (make sure to install the latest prerelease version):
    - `Microsoft.EntityFrameworkCore.Sqlite`
    - `Microsoft.EntityFrameworkCore.Design`
    - `Microsoft.EntityFrameworkCore.Tools`

### The DbContext

Now we can add the `DbContext`

- Under the `Infrastructure` folder, create a new folder `Data`
- Add a new class `CommentsDbContext`
- Let the class derive from `DbContext`

```cs
using Microsoft.EntityFrameworkCore;
namespace PhotoSharingApplication.WebServices.Grpc.Comments.Infrastructure.Data;

public class CommentsDbContext : DbContext {
}

```

Because we're going to use this `DbContext` from an ASP.NET Core project, we are going to use the [constructor accepting the DbOptions](https://docs.microsoft.com/en-gb/ef/core/miscellaneous/connection-strings#aspnet-core)

```cs
public CommentsDbContext(DbContextOptions<CommentsDbContext> options)  : base(options) {}
```

We want to give our model some configurations and restrictions, so we're going to use [Fluent API](https://docs.microsoft.com/en-gb/ef/core/modeling/#use-fluent-api-to-configure-a-model) to do that:

```cs
protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.Entity<Comment>(ConfigureComments);  
}

private void ConfigurePhoto(EntityTypeBuilder<Photo> builder) {
    builder.ToTable("Comments");

    builder.Property(comment => comment.Subject)
        .IsRequired()
        .HasMaxLength(250);
}
```

Lastly, we're going to add a `DbSet` for the `Comment`:

```cs
public DbSet<Comment> Comments { get; set; }
```

Don't forget to add the necessary `using`:

```cs
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhotoSharingApplication.Shared.Entities;
```

### Configuring the DbContext

The context has to be configured and added as a Service using the [Dependency Injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0) features of `ASP.NET Core`.

Open the [`Program.cs`](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup?view=aspnetcore-6.0) file, and add the configuration for the `DbContext` before building the app:

```cs
builder.Services.AddDbContext<CommentsDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("CommentsDbContext")));

var app = builder.Build();
```

Which requires

```cs
using Microsoft.EntityFrameworkCore;
using PhotoSharingApplication.WebServices.Grpc.Comments.Infrastructure.Data;
using PhotoSharingApplication.WebServices.Grpc.Comments.Services;
```

Add a `CommentsDbContext` connection string to [configure](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0) it in the `appsettings.json` file, as per [Default](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#default-configuration):

```json
"ConnectionStrings": {
    "CommentsDbContext": "Data Source=Comments.db;"
  }
```

### Generate migrations and database

The database has not been created. We're going to use [Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=vs) to generate the DB and update the schema on a later Lab, using the [Entity Framework Core Tools in the Package Manager Console](https://docs.microsoft.com/en-us/ef/core/cli/powershell).

First, as per the [documentation](https://docs.microsoft.com/en-us/ef/core/cli/powershell):

> Before using the tools:
> 
> - Understand the difference between target and startup project.
> - Learn how to use the tools with .NET Standard class libraries.
> - For ASP.NET Core projects, set the environment.
>
> ### Target and startup project
> The commands refer to a project and a startup project.
>
> - The project is also known as the target project because it's where the commands add or remove files. By default, the Default project selected in Package Manager Console is the target project. You can specify a different project as target project by using the `--project` option.
> - The startup project is the one that the tools build and run. The tools have to execute application code at design time to get information about the project, such as the database connection string and the configuration of the model. By default, the Startup Project in Solution Explorer is the startup project. You can specify a different project as startup project by using the `--startup-project` option.

To add an initial migration, run the following command.

```
Add-Migration InitialCreate -Project PhotoSharingApplication.WebServices.Grpc.Comments -StartupProject PhotoSharingApplication.WebServices.Grpc.Comments
```

Three files are added to your project under the Migrations directory:

- XXXXXXXXXXXXXX_InitialCreate.cs--The main migrations file. Contains the operations necessary to apply the migration (in Up()) and to revert it (in Down()).
- XXXXXXXXXXXXXX_InitialCreate.Designer.cs--The migrations metadata file. Contains information used by EF.
- CommentsDbContextModelSnapshot.cs--A snapshot of your current model. Used to determine what changed when adding the next migration.

The timestamp in the filename helps keep them ordered chronologically so you can see the progression of changes.

### Update the database
Next, apply the migration to the database to create the schema.

```
Update-Database -Project PhotoSharingApplication.WebServices.Grpc.Comments -StartupProject PhotoSharingApplication.WebServices.Grpc.Comments
```

You should now have a new SQL Lite database called `Comments.db` with one empty `Comments` table.  
If you want to inspect its contents, you can use the `SQLite / Sql Server Compact Toolbox` extension for Visual Studio.

### The Repository

Now for the Repository that makes use of the `DbContext`.

- Under the `Infrastructure` folder of thhe gRpc project, create a new `Repositories` folder 
- Under the `Repositories` folder, create a new `EntityFramework` folder
- Under the `EntityFramework` folder, add a new class `CommentsRepository` 
- Let the class implement the `ICommentsRepository` interface by adding the following code:

```cs
using Microsoft.EntityFrameworkCore;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;
using PhotoSharingApplication.WebServices.Grpc.Comments.Infrastructure.Data;

namespace PhotoSharingApplication.WebServices.Grpc.Comments.Infrastructure.Repositories.EntityFramework;

public class CommentsRepository : ICommentsRepository {
    
    public async Task<Comment?> CreateAsync(Comment comment) {
        context.Add(comment);
        await context.SaveChangesAsync();
        return comment;
    }

    public async Task<Comment?> FindAsync(int id) => await context.Comments.SingleOrDefaultAsync(m => m.Id == id);

    public async Task<List<Comment>?> GetCommentsForPhotoAsync(int photoId) => await context.Comments.Where(c => c.PhotoId == photoId).ToListAsync();

    public async Task<Comment?> RemoveAsync(int id) {
        Comment comment = await context.Comments.SingleOrDefaultAsync(m => m.Id == id);
        context.Comments.Remove(comment);
        await context.SaveChangesAsync();
        return comment;
    }

    public async Task<Comment?> UpdateAsync(Comment comment) {
        context.Update(comment);
        await context.SaveChangesAsync();
        return comment;
    }
}
```

## The Application

It's time to create a [gRpc](https://grpc.io/) Service.

To create a [gRpc Service in .NET 6](https://docs.microsoft.com/en-us/aspnet/core/grpc/?view=aspnetcore-6.0) using the *proto first* approach, we're going to build an [ASP.NET Core site](https://docs.microsoft.com/en-us/aspnet/core/grpc/aspnetcore?view=aspnetcore-6.0&tabs=visual-studio) 

### The gRpc Service

Here is the API that you'll create:

| API                       | Description                | Request           | Reply     |
| ------------------------- | -------------------------- | ---------------------- | ----------------- |
| rpc Create	        | Adds a new Comment   	       | CreateRequest	                  | CreateReply |
| rpc Find    | Gets a Comment by ID        | FindRequest                   | FindReply           |
| rpc GetCommentsForPhoto        | Gets All Comments for a specific Photo          | GetCommentsForPhotoRequest                | GetCommentsForPhotoReply           |
| rpc Remove    | Deletes a Comment | RemoveRequest                | RemoveReply               |	
| rpc Update | Updates a Comment           | UpdateRequest | UpdateReply             |

These are the messages sent back and forth:

- *CreateRequest*
    - int PhotoId
    - string Subject
    - string Body
- *CreateReply*
    - int Id
    - int PhotoId
    - string UserName
    - string Subject
    - string Body
    - DateTime SubmittedOn
- *FindRequest*
   - int Id
- *FindReply*
    - int Id
    - int PhotoId
    - string UserName
    - string Subject
    - string Body
    - DateTime SubmittedOn
- *GetCommentsForPhotosRequest*
    - int photoId
- *GetCommentsForPhotosReply* 
    - List<GetCommentsForPhotosReplyItem> comments
- *GetCommentsForPhotosReplyItem 
    - int Id
    - int PhotoId
    - string UserName
    - string Subject
    - string Body
    - DateTime SubmittedOn
- *RemoveRequest*
    - int Id
- *RemoveReply*
    - int Id
    - int PhotoId
    - string UserName
    - string Subject
    - string Body
    - DateTime SubmittedOn
- *UpdateRequest*
    - int Id
    - string Subject
    - string Body
- *UpdateReply*
    - int Id
    - int PhotoId
    - string UserName
    - string Subject
    - string Body
    - DateTime SubmittedOn

Many messages look the same, but we want to keep the definitions separated so that future changes wouldn't break the interface.

### Examine the project files

GrpcGreeter project files:

- `greet.proto` – The Protos/greet.proto file defines the Greeter gRPC and is used to generate the gRPC server assets. For more information, see [Introduction to gRPC](https://docs.microsoft.com/en-us/aspnet/core/grpc/?view=aspnetcore-6.0).
- `Services` folder: Contains the implementation of the Greeter service.
- `appSettings.json` – Contains configuration data, such as protocol used by Kestrel. For more information, see [Configuration in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0).
- `Program.cs` – Contains the entry point for the gRPC service and code that configures app behavior.

### Add the .proto file

- In the `Protos` folder of the `PhotoSharingApplication.WebServices.Grpc.Comments` project, add a new `Protocol Buffer File` 
- Name the file `comments.proto`

As explained in the [Microsoft Documentation](https://docs.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/protobuf-data-types): 
- the C# `int` is translated into the `int32` type 
- the C# `DateTime` is translated into the `google.protobuf.Timestamp` type 
- the C# `List` is achieved through the [`repeated`](https://docs.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/protobuf-repeated) keyword

This means that the content of the `comments.proto` file becomes

```protobuf
syntax = "proto3";

import "google/protobuf/timestamp.proto";

option csharp_namespace = "PhotoSharingApplication.WebServices.Grpc.Comments";

package comments;

service Commenter {
  rpc Create (CreateRequest) returns (CreateReply);
  rpc Find(FindRequest) returns (FindReply);
  rpc GetCommentsForPhoto(GetCommentsForPhotosRequest) returns (GetCommentsForPhotosReply);
  rpc Remove(RemoveRequest) returns (RemoveReply);
  rpc Update(UpdateRequest) returns (UpdateReply);
}

message CreateRequest {
    int32 PhotoId =  1;
    string Subject = 2;
    string Body = 3;
}

message CreateReply {
    int32 Id = 1;
    int32 PhotoId =  2;
    string UserName = 3;
    string Subject = 4;
    string Body = 5;
    google.protobuf.Timestamp SubmittedOn = 6;
}

message FindRequest{
   int32 Id = 1; 
}

message FindReply {
    int32 Id = 1;
    int32 PhotoId =  2;
    string UserName = 3;
    string Subject = 4;
    string Body = 5;
    google.protobuf.Timestamp SubmittedOn = 6;
}

message GetCommentsForPhotosRequest{
    int32 photoId = 1;
}
message GetCommentsForPhotosReply {
  repeated GetCommentsForPhotosReplyItem comments = 1;
}

message GetCommentsForPhotosReplyItem {
    int32 Id = 1;
    int32 PhotoId =  2;
    string UserName = 3;
    string Subject = 4;
    string Body = 5;
    google.protobuf.Timestamp SubmittedOn = 6;
}

message RemoveRequest{
    int32 Id = 1;
}

message RemoveReply {
    int32 Id = 1;
    int32 PhotoId =  2;
    string UserName = 3;
    string Subject = 4;
    string Body = 5;
    google.protobuf.Timestamp SubmittedOn = 6;
}

message UpdateRequest {
    int32 Id = 1;
    string Subject = 2;
    string Body = 3;
}

message UpdateReply {
    int32 Id = 1;
    int32 PhotoId =  2;
    string UserName = 3;
    string Subject = 4;
    string Body = 5;
    google.protobuf.Timestamp SubmittedOn = 6;
}
```
- Save the file.
- In the `Solution Explorer`, right click on `comments.proto`, select `Properties`
- In The `Properties` window
    - In the `Build Action` select `Protobuf compiler`
    - In the `gRPC Stub Classes` select `Server Only`
- Build the application

### Add the Service

- In the `Solution Explorer`, right click the `Services` folder, select `Add` -> `class`
- Name the class `CommentsGrpcService`
- Let the class derive from `Commenter.CommenterBase`

```cs
namespace PhotoSharingApplication.WebServices.Grpc.Comments.Services ; 

public class CommentsGrpcService : Commenter.CommenterBase {
}
```

We want to use the `CommentsService` of out *Backend.Core*, so let's make use of the DI container by explicitly declaring the dependency on the `ICommentsService` in the GrpcService constructor:

- Add a `Project Reference` to `PhotoSharingApplication.Backend.Core`
- Add a constructor that accepts a `ICommentsService` parameter
- Save the parameter into a private readonly field
- Add a `using PhotoSharingApplication.Shared.Core.Interfaces;`

```cs
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.WebServices.Grpc.Comments.Services;

public class CommentsGrpcService : Commenter.CommenterBase {
    private readonly ICommentsService commentsService;

    public CommentsGrpcService(ICommentsService commentsService) {
        this.commentsService = commentsService;
    }
}
```

Now we can start implementing our methods.

## Getting all the Comments for a Photo

Our `CommentsService` has a `GetCommentsForPhotoAsync` method that returns a `List<Comment>`, so we will use that to start.

We cannot return the result as it is, because our method needs to return a `GetCommentsForPhotosReply`, so we need to create an instance of that first.

The definition of `GetCommentsForPhotosReply` in the `comments.proto` file states that the message contains a `repeated` field called `comments` of type `GetCommentsForPhotosReplyItem`

What we find in our C# class is that a `GetCommentsForPhotosReply` instance contains a `Comments` property to which we can add a collection using its `AddRange` method.

We cannot add a `List<Comment>`, though, because the `AddRange` accepts a collection of `GetCommentsForPhotosReplyItem`.

So we first need to project each `Comment` into a `GetCommentsForPhotosReplyItem`.
We can do that with a simple `Linq` query.

Also, the `SubmittedOn` type is `DateTime`, while `protobuf` wants a `TimeStamp`, so we need to translate that too, by using the `FromDateTime` static method of the `Google.Protobuf.WellKnownTypes.Timestamp` class.

Our code becomes:

```cs
public override async Task<GetCommentsForPhotosReply> GetCommentsForPhoto(GetCommentsForPhotosRequest request, ServerCallContext context) {
    List<Comment> comments = await commentsService.GetCommentsForPhotoAsync(request.PhotoId);
    GetCommentsForPhotosReply r = new GetCommentsForPhotosReply();
    IEnumerable<GetCommentsForPhotosReplyItem> replyItems = comments.Select(c => new GetCommentsForPhotosReplyItem() { Id = c.Id, PhotoId = c.PhotoId, Subject = c.Subject, UserName = c.UserName, Body = c.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()) });
    r.Comments.AddRange(replyItems);
    return r;
}
```
which require the following using:

```cs
using Grpc.Core;
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;
```

## Get One Comment

Our `CommentsService` has a `FindAsync` method that returns a `Comment`, so we will use that to start.

We cannot return the result as it is, because our method needs to return a `FindReply`, so we need to project our `Comment` into a `FindReply`.

Also, the `SubmittedOn` type is `DateTime`, while `protobuf` wants a `TimeStamp`, so we need to translate that too, by using the `FromDateTime` static method of the `Google.Protobuf.WellKnownTypes.Timestamp` class.

We are going to return an error if the comment has not been found.

Our code becomes:

```cs
public override async Task<FindReply> Find(FindRequest request, ServerCallContext context) {
    Comment c = await commentsService.FindAsync(request.Id);
    if (c is null) {
        throw new RpcException(new Status(StatusCode.NotFound, "Comment not found"));
    }
    return new FindReply() { Id = c.Id, PhotoId = c.PhotoId, Subject = c.Subject, UserName = c.UserName, Body = c.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()) };
}
```

### Create a Comment

Our `CommentsService` has a `CreateAsync` method that accepts a `Comment` and returns a `Comment`, so we will use that to start.

We cannot pass the comment as it is, because our method receives a `CreateRequest`, so we need to project that into an instance of a new `Comment`.

We cannot return the result as it is, because our method needs to return a `CreateReply`, so we need to project our `Comment` into a `CreateReply`.

Also, the `SubmittedOn` type is `DateTime`, while `protobuf` wants a `TimeStamp`, so we need to translate that too, by using the `FromDateTime` static method of the `Google.Protobuf.WellKnownTypes.Timestamp` class.

We are going to return an error if the Service or the Repository throw an Exception.

Our code becomes:

```cs
public override async Task<CreateReply> Create(CreateRequest request, ServerCallContext context) {
    try {
        Comment c = await commentsService.CreateAsync(new Comment { PhotoId = request.PhotoId, Subject = request.Subject, Body = request.Body });
        return new CreateReply() { Id = c.Id, PhotoId = c.PhotoId, Body = c.Body, Subject = c.Subject, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()), UserName = c.UserName };
    } catch (Exception ex){
        throw new RpcException(new Status(StatusCode.Internal, ex.Message));
    }
}
```

### Update a Comment

Update is similar to Create, but it uses the `UpdateAsync` method of the `CommentsService` and returns an `UpdateReply`. We are going to return an error if the Service or the Repository throw an Exception.

```cs
public override async Task<UpdateReply> Update(UpdateRequest request, ServerCallContext context) {
    try {
        Comment c = await commentsService.UpdateAsync(new Comment { Id = request.Id, Subject = request.Subject, Body = request.Body });
        return new UpdateReply() { Id = c.Id, PhotoId = c.PhotoId, Subject = c.Subject, UserName = c.UserName, Body = c.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()) };
    } catch (Exception ex) {
        throw new RpcException(new Status(StatusCode.Internal, ex.Message));
    }
}
```
### Delete a Comment

Delete is similar to Create too, but it uses the `RemoveAsync` method of the `CommentsService` and returns an `RemoveReply`. We are going to return an error if the Service or the Repository throw an Exception.

```cs
public override async Task<RemoveReply> Remove(RemoveRequest request, ServerCallContext context) {
    try { 
    Comment c = await commentsService.RemoveAsync(request.Id);
    return new RemoveReply() { Id = c.Id, PhotoId = c.PhotoId, Subject = c.Subject, UserName = c.UserName, Body = c.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()) };
    } catch (Exception ex) {
        throw new RpcException(new Status(StatusCode.Internal, ex.Message));
    }
}
```

## Registering the services and configuring the DbContext

To use our service in the `CommentsGrpcService` gRpc Service, we need to perform a couple of steps: 

- Open `Program.cs`
- Type the following code before the building of the app

```cs
builder.Services.AddScoped<ICommentsService, CommentsService>();
builder.Services.AddScoped<ICommentsRepository, CommentsRepository>();
//add those previous lines befor this one:
var app = builder.Build();
```

This requires the following `using`

```cs
using Microsoft.EntityFrameworkCore;
using PhotoSharingApplication.Shared.Interfaces;
using PhotoSharingApplication.WebServices.Grpc.Comments.Core.Services;
using PhotoSharingApplication.WebServices.Grpc.Comments.Infrastructure.Data;
using PhotoSharingApplication.WebServices.Grpc.Comments.Infrastructure.Repositories.EntityFramework;
using PhotoSharingApplication.WebServices.Grpc.Comments.Services;
```

## Mapping the service as an EndPoint

In the `Program.cs` class of the `PhotoSharingApplication.WebServices.Grpc.Comments` project, find the `app.MapGrpcService<GreeterService>();` and replace it with
- Inside the `UseEndPoints` method, add the following code:

```cs
app.MapGrpcService<CommentsGrpcService>();
```

Our service is ready. In the next lab we will setup the client side. 

## Optional

If you want to try your gRpc service without writing a client first, you can use different tools, one of which is [BloomRPC](https://github.com/uw-labs/bloomrpc).  
You can follow the [instructions](https://github.com/uw-labs/bloomrpc#installation) to install it (for example by installing [Chocolatey](https://chocolatey.org/install) first).  
Then start your project and check on which port number it runs.  
On BloomRPC, import the proto file and as an address type `localhost:{PORT NUMBER}` (replacing `{PORT NUMBER}` with your port, which may very well be 5000).  
Try the different actions. You should see the results in BloomRPC.

--- 

Go to `Labs/Lab09`, open the `readme.md` and follow the instructions to continue.   