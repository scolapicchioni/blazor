# Backend: Web API with ASP.NET 6.0 and Visual Studio 2022

In this lab we're going to take care of our Backend.

We're going to use the same [CLEAN architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) that we have on the frontend:

- A *Core* folder where we define the business logic. There's going to be a *Service* that knows what to do (for example it validates the data before passing it to the infrastructure) 
- An *Infrastructure* folder where we define how to actually read and save the data. We're going to use [Entity Framework Core](https://docs.microsoft.com/en-gb/ef/core/)  to talk to a SQL Server DataBase.
- An *Application* project, which in this case consists of a [REST](https://www.restapitutorial.com/lessons/whatisrest.html#) service using [ASP.NET Core 6.0 Web API](https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-6.0&tabs=visual-studio).

Both the `Service` and the `Repository` will implement the interfaces and make use of the `Photo` entity that we have already defined on the `Shared` project.

## The Backend

### Create the project

- On the `Solution Explorer`, right click your solution, then select `Add` -> `New Project`.
- Select the `ASP.NET Core Web Api` project template. 
    - Name the Project `PhotoSharingApplication.WebServices.REST.Photos`
    - Select `.NET 6.0`
    - Leave the `Authentication Type` to `None`. 
    - Ensure that the `Configure for Https` checkbox is selected
    - Do not check `Enable Docker Support`.
    - Ensure that the `Use controllers` checkbox is selected.
    - Ensure that `Enable OpenAPI Support` is selected
    - Click `Create`
    - Add a project reference to the `PhotoSharingApplication.Shared` project.

### The Core Buisness logic

Now we can implement our service, which for now will just pass the data to the repository and return the results, with little or no additional logic (we will replace it later). We are going to use the [Dependency Injection pattern](https://martinfowler.com/articles/injection.html?) to request for a repository.

Create a new `Core` folder, then under it create a new folder named `Services`.
Add a `PhotosService.cs` class to the `Services` folder.

```cs
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;

namespace PhotoSharingApplication.Backend.Core.Services;

public class PhotosService : IPhotosService {
    private readonly IPhotosRepository repository;
    public PhotosService(IPhotosRepository repository) => this.repository = repository;
    public async Task<Photo?> FindAsync(int id) => await repository.FindAsync(id);
    public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await repository.GetPhotosAsync(amount);
    public async Task<Photo?> RemoveAsync(int id) => await repository.RemoveAsync(id);
    public async Task<Photo?> UpdateAsync(Photo photo) => await repository.UpdateAsync(photo);
    public async Task<Photo?> UploadAsync(Photo photo) {
        photo.CreatedDate = DateTime.Now;
        return await repository.CreateAsync(photo);
    }
}
```

Now, it's true that this class looks like the one we have for the frontend, so we may be tempted to share this as well, but we could also argue that the logic server side may very well be more convoluted than the one on the frontend (you may not want to share your *secrets* with the client), so we're going to keep them separated even if in our case they do the same thing.

## The Backend Infrastructure

- On the `Solution Explorer`, create a new `Infrastructure` folder
- Add the following NuGet packages (make sure to install the latest prerelease version):
    - `Microsoft.EntityFrameworkCore.Sqlite`
    - `Microsoft.EntityFrameworkCore.Tools`

### The DbContext

Now we can add the `DbContext`

- Under the `Infrastructure` folder, create a new folder `Data`
- Add a new class `PhotosDbContext`
- Let the class derive from `DbContext`

```cs
using Microsoft.EntityFrameworkCore;
namespace PhotoSharingApplication.WebServices.Rest.Photos.Infrastructure.Data;

public class PhotosDbContext : DbContext {
}

```

Because we're going to use this `DbContext` from an ASP.NET Core project, we are going to use the [constructor accepting the DbOptions](https://docs.microsoft.com/en-gb/ef/core/miscellaneous/connection-strings#aspnet-core)

```cs
public PhotosDbContext(DbContextOptions<PhotosDbContext> options)  : base(options) {}
```

We want to give our model some configurations and restrictions, so we're going to use [Fluent API](https://docs.microsoft.com/en-gb/ef/core/modeling/#use-fluent-api-to-configure-a-model) to do that:

```cs
protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.Entity<Photo>(ConfigurePhoto);    
}

private void ConfigurePhoto(EntityTypeBuilder<Photo> builder) {
    builder.ToTable("Photos");

    builder.Property(photo => photo.Title)
        .IsRequired(true)
        .HasMaxLength(255);
}
```

Lastly, we're going to add a `DbSet` for the `Photo`:

```cs
public DbSet<Photo> Photos { get; set; }
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
builder.Services.AddDbContext<PhotosDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("PhotosDbContext")));

var app = builder.Build();
```

Which requires

```cs
using Microsoft.EntityFrameworkCore;
using PhotoSharingApplication.WebServices.Rest.Photos.Infrastructure.Data;
```

Add a `PhotosDbContext` connection string to [configure](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0) it in the `appsettings.json` file, as per [Default](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#default-configuration):

```json
"ConnectionStrings": {
    "PhotosDbContext": "Data Source=Photos.db;"
  }
```

### Generate migrations and database

The database has not been created. We're going to use [Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=vs) to generate the DB and update the schema on a later Lab, using the [Entity Framework Core Tools in the Package Manager Console](https://docs.microsoft.com/en-us/ef/core/cli/powershell).

First, we need to add the tools by adding the following NuGet Packages to our `PhotoSharingApplication.WebServices.Rest.Photos` project:

- `Microsoft.EntityFrameworkCore.Design`

Then, as per the [documentation](https://docs.microsoft.com/en-us/ef/core/cli/powershell):

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
Add-Migration InitialCreate -Project PhotoSharingApplication.WebServices.Rest.Photos -StartupProject PhotoSharingApplication.WebServices.Rest.Photos
```

Three files are added to your project under the Migrations directory:

- XXXXXXXXXXXXXX_InitialCreate.cs--The main migrations file. Contains the operations necessary to apply the migration (in Up()) and to revert it (in Down()).
- XXXXXXXXXXXXXX_InitialCreate.Designer.cs--The migrations metadata file. Contains information used by EF.
- PhotoSharingApplicationContextModelSnapshot.cs--A snapshot of your current model. Used to determine what changed when adding the next migration.

The timestamp in the filename helps keep them ordered chronologically so you can see the progression of changes.

### Update the database
Next, apply the migration to the database to create the schema.

```
Update-Database -Project PhotoSharingApplication.WebServices.Rest.Photos -StartupProject PhotoSharingApplication.WebServices.Rest.Photos
```

You should now have a new SQL Lite database called `Photos.db` with one empty `Photos` table.  
The file should be located under the root of your project.  
If you want to see the schema, you can install the `SQLite/ Sql Server Compact Toolbox` Extension for Visual Studio and open the `Photos.db` file.  


## The Repository

Now for the *Repository* that makes use of the DbContext.

- In the `Infrastructure` folder, add a `Repositories` folder
- In the `Repositories` folder, add a `EntityFramework` folder
- In the `EntityFramework` folder, add a new class `PhotosRepository` 
- Let the class implement the `IPhotosRepository` interface by adding the following code:

```cs
using PhotoSharingApplication.Shared.Entities;
using PhotoSharingApplication.Shared.Interfaces;

namespace PhotoSharingApplication.WebServices.Rest.Photos.Infrastructure.Repositories.EntityFramework;

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

To make use of the `DbContext`, we're going to resort to Dependecy Injection, so we need a constructor and a field where to store the DbContext so that we can use it from the methods we have to implement:

```cs
private readonly PhotosDbContext context;

public PhotosRepository(PhotosDbContext context) => this.context = context;
```

which requires

```cs
using PhotoSharingApplication.WebServices.Rest.Photos.Infrastructure.Data;
```

Now we're going to use [Asynchronous operations](https://docs.microsoft.com/en-gb/ef/core/miscellaneous/async) to Create, Reade, Update and Delete data.


- The code to [Add](https://docs.microsoft.com/en-gb/ef/core/saving/basic#adding-data) the Photo to the DataBase becomes:

```cs
public async Task<Photo> CreateAsync(Photo photo) {
    context.Add(photo);
    await context.SaveChangesAsync();
    return photo;
}
```

- The code to [Update](https://docs.microsoft.com/en-gb/ef/core/saving/basic#updating-data) a Photo in the database becomes:

```cs
public async Task<Photo> UpdateAsync(Photo photo) {
    context.Update(photo);
    await context.SaveChangesAsync();
    return photo;
}
```

- The code to [Delete](https://docs.microsoft.com/en-gb/ef/core/saving/basic#deleting-data) a Photo from the Database becomes:


```cs
public async Task<Photo> RemoveAsync(int id) {
    var photo = await context.Photos.SingleOrDefaultAsync(m => m.Id == id);
    if (photo is not null) {
        context.Photos.Remove(photo);
        await context.SaveChangesAsync();
    }
    return photo;
}
```

which requires

```
using Microsoft.EntityFrameworkCore;
```

To read the data, we're going to use [Asynchronous Queries](https://docs.microsoft.com/en-gb/ef/core/querying/async)

- The code to Read all the photos becomes

```cs
    public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => 
    await (from p in context.Photos
            orderby p.CreatedDate descending
            select p).Take(amount).ToListAsync();
```

- The code to read one photo becomes

```cs
public async Task<Photo> FindAsync(int id) => await context.Photos.SingleOrDefaultAsync(m => m.Id == id);
```

## The Application

It's time to create a REST Service.

## Create a Web API with ASP.NET Core

Here is the API that you'll create:

| API                       | Description                | Request body           | Response body     |
| ------------------------- | -------------------------- | ---------------------- | ----------------- |
| GET /photos	        | Get all photos   	       | None	                  | Array of photos |
| GET /photos/{id}    | Get a photo by ID        | None                   | Photo           |
| POST /photos        | Add a new photo          | Photo                | Photo           |
| PUT /photos/{id}    | Update an existing photo | Photo                | Photo               |	
| DELETE /photos/{id} | Delete a photo           | None. No request body- | Photo             |

The client submits a request and receives a response from the application. Within the application we find the controller, which makes use of the Service we implemented in the *Backend.Core* project. The request comes into the application's controller, and read/write operations occur between the controller and the service. The model is serialized and returned to the client in the response.

The **client** is whatever consumes the web API (browser, mobile app, and so forth). We aren't writing a client in this tutorial. We'll use [Swagger / OpenApi](https://docs.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger?view=aspnetcore-6.0) to try the app. We will write the client in the following lab.

A **model** is an object that represents the data in your application. In this case, the only model is a Photo item. Models are represented as simple C# classes (POCOs).

A **controller** is an object that handles HTTP requests and creates the HTTP response. This app will have a single controller.

### Add the Controller

- In the `Solution Explorer`, right click the `Controllers` folder, select `Add` -> `Controller`
- Select `API Empty`
- Name the controller `PhotosController`

The wizard took care of the [Controller](https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-6.0) by generating a class that derives from [ControllerBase](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase?view=aspnetcore-5.0&viewFallbackFrom=aspnetcore-6.0), which provides many properties and methods that are useful for handling HTTP requests.

The `Microsoft.AspNetCore.Mvc` namespace provides attributes that can be used to configure the behavior of web API controllers and action methods.

The [ApiController](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.apicontrollerattribute?view=aspnetcore-5.0&viewFallbackFrom=aspnetcore-6.0) attribute was applied to the controller class to enable the following API-specific behaviors:
- [Attribute routing requirement](https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-6.0#attribute-routing-requirement)
- [Automatic HTTP 400 responses](https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-6.0#automatic-http-400-responses)
- [Binding source parameter inference](https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-6.0#binding-source-parameter-inference)
- [Multipart/form-data request inference](https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-6.0#multipartform-data-request-inference)
- [Problem details for error status codes](https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-6.0#problem-details-for-error-status-codes) 

```cs
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PhotoSharingApplication.WebServices.Rest.Photos.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PhotosController : ControllerBase {
}
```

We want to use the `Service` of out *Backend.Core*, so let's make use of the DI container by explicitly declaring the dependency on the `IPhotosService` in the Controller constructor:

- Add a constructor that accepts a `IPhotosService` parameter
- Save the parameter into a private readonly field

```cs
    private readonly IPhotosService service;

    public PhotosController(IPhotosService service) => this.service = service;
```
Which requires

```cs
using PhotoSharingApplication.Shared.Interfaces;
```

Now we can start implementing our CRUD methods.

The controller and every action should be mapped to a route through the use of [routing](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0) system, in particular [attribute routing](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0#attribute-routing-for-rest-apis) and [attribute routing using http verbs attributes](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0#http-verb-templates).

We want all the routes to start with `photos` and not with `api/photos`, so, let's change the `[Route]` attribute at the beginning of out controller:

```cs
[Route("[controller]")]
```
## Getting Photos

We need two methods to get the photos.

- The Method to get all the photos:

```cs
[HttpGet]
public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos() => await service.GetPhotosAsync();
```

Which requires

```cs
using PhotoSharingApplication.Shared.Entities;
```

It returns a `Task<ActionResult<IEnumerable<Photo>>>`. MVC automatically serializes the list of `Photo` to JSON and writes the JSON into the body of the response message. The response code for this method is 200, assuming there are no unhandled exceptions. (Unhandled exceptions are translated into 5xx errors.)

Here is an example HTTP response for the first `GetPhotos()` method:

```
HTTP/1.1 200 OK
   Content-Type: application/json; charset=utf-8
   Server: Microsoft-IIS/10.0
   Date: Thu, 18 Jun 2015 20:51:10 GMT
   Content-Length: 82

   [{"Id":1,"Name":"Photo 1","Description":"First Sample Photo"}]
```

The second `Find(int id)` method returns a `Task<ActionResult<Photo>>` type:

- A 404 status code is returned when the photo represented by id doesn't exist in the underlying data store. The `NotFound` convenience method is invoked as shorthand for return `new NotFoundResult();`.
- A 200 status code is returned with the Photo object when the photo does exist.`.

```cs
[HttpGet("{id:int}", Name = "Find")]
public async Task<ActionResult<Photo>> Find(int id) {
    Photo? ph = await service.FindAsync(id);
    if (ph is null) return NotFound();
    return ph;
}
```

### Create Action

As for REST standards, the action that adds a product to the database is bound to the `POST http verb`.

```cs
[HttpPost]
public async Task<ActionResult<Photo>> CreateAsync(Photo photo) {
    Photo? p = await service.UploadAsync(photo);
    return CreatedAtRoute("Find", photo, new { id = photo.Id});
}
```

The `CreateAsync()` is an `HTTP POST` method, indicated by the `[HttpPost]` attribute. The `[ApiController]` attribute at the top of the controller declaration tells MVC to get the value of the `Photo` item from the body of the HTTP request.

The `CreatedAtRoute` method:

- Returns a 201 response. HTTP 201 is the standard response for an HTTP POST method that creates a new resource on the server.
- Adds a Location header to the response. The Location header specifies the URI of the newly created Photo item. See [10.2.2 201 Created](https://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html).
- Uses the `Find` named route to create the URL. The `Find` named route is defined in `Find(int id)`

Thanks to the `[ApiController]` attribute, the request is checked against the validation engine and in case of a BadRequest the default response type for an HTTP 400 response is ValidationProblemDetails. The following request body is an example of the serialized type:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "|7fb5e16a-4c8f23bbfc974667.",
  "errors": {
    "": [
      "A non-empty request body is required."
    ]
  }
}
```

The ValidationProblemDetails type:

- Provides a machine-readable format for specifying errors in web API responses.
- Complies with the RFC 7807 specification.

### Update

Update is similar to Create, but uses `HTTP PUT`. 

```cs
[HttpPut("{id}")]
public async Task<ActionResult<Photo>> Update(int id, Photo photo) {
    if (id != photo.Id)
        return BadRequest();
    return await service.UpdateAsync(photo);
}
```


The response is a 200 with our updated Photo. According to the HTTP spec, a PUT request requires the client to send the entire updated entity, not just the deltas. To support partial updates, use HTTP PATCH.

### Delete

The Delete uses `HTTP DELETE` verb and expects an `id` in the address. 

```cs
[HttpDelete("{id}")]
public async Task<ActionResult<Photo>> Remove(int id) {
    Photo ph = await service.FindAsync(id);
    if (ph == null) return NotFound();
    return await service.RemoveAsync(id);
}
```

It returns 
- A 200 (Ok) with the deleted product if successful
- A 404 (Not Found) if the id is not found in the database


## Registering the service and repository

Now, the two interfaces and implementations.

To use our service in the `PhotosController` controller, we need to perform a couple of steps, also described in the [Dependency Injection documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0)

- Open the `Program.cs` file
- Add the following lines right before the building of the app :

```cs
builder.Services.AddScoped<IPhotosService, PhotosService>();
builder.Services.AddScoped<IPhotosRepository, PhotosRepository>();

//add the prevoius two lines before this one
var app = builder.Build();
```

Of course, also add the correct `using`:

```cs
using PhotoSharingApplication.Shared.Interfaces;
using PhotoSharingApplication.WebServices.Rest.Photos.Core.Services;
using PhotoSharingApplication.WebServices.Rest.Photos.Infrastructure.Data;
using PhotoSharingApplication.WebServices.Rest.Photos.Infrastructure.Repositories.EntityFramework;
```



### Try the Actions of the controller

In Visual Studio, set the `PhotoSharingApplication.WebServices.REST.Photos` project as startup project, then press `F5` to launch the app. Visual Studio launches a browser and navigates to `http://localhost:port/swagger`, where port is a randomly chosen port number. 

- Try the Get action of the Photos controller using the provided UI.
    - You should see an empty JSON array in the Response Body. 
- Try the POST action of the Photos controller passing the following Request Body

```json
{
  "id": 0,
  "title": "One Photo",
  "photoFile": "",
  "imageMimeType": "",
  "description": "One Nice Photo",
  "createdDate": "2022-01-24T08:44:43.223Z",
  "userName": "alice"
}
```
You should see the 201 and the response body. Also notice the `Location` between the Response Headers.

If you add multiple photos and try GET action again, you should see an array with all the photos you added.

Try the Get for `Photos/{id}` and pass `1` as an `ID`

You should see this in the Response Body:

```
{
  "id": 1,
  "title": "One Photo",
  "photoFile": "",
  "imageMimeType": "",
  "description": "One Nice Photo",
  "createdDate": "2022-01-24T09:46:52.825878",
  "userName": "alice"
}
```

Try the action again passing `99` as an `ID`

You should see a 404 with a response body similar to this one:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "traceId": "00-5547642c46e21ce1f9f84f172bea9924-126e589ed8633505-00"
}
```

Try the `PUT` method, passing `1` as an `ID` and the following request body:

```json
{
  "id": 1,
  "title": "One Photo But Modified",
  "photoFile": "",
  "imageMimeType": "",
  "description": "One Nice Photo But Modified",
  "createdDate": "2022-01-24T09:46:52.825878",
  "userName": "alice"
}
```


Verify that the response is a 200 and that its body contains the updated photo object.

Try the `DELETE` action passing `1` as an `ID`.

Check that the Response Status Code is 200 and that its Response Body contains the first photo.  
If you call the action to get all the photos, you should not see the photo with id 1 anymore.

Our service is ready. In the next lab we will setup the client side. 

Go to `Labs/Lab06`, open the `readme.md` and follow the instructions to continue.   