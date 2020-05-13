# Backend: Web API with ASP.NET 5 and Visual Studio 2019 Preview

In this lab we're going to take care of our Backend.

We're going to use the same [CLEAN architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) that we have on the frontend:

- A *Core* project where we define the business logic. There's going to be a Service that knows what to do (for example it validates the data before passing it to the infrastructure) 
- An *Infrastructure* project where we define how to actually read and save the data. We're going to use [Entity Framework](https://docs.microsoft.com/en-gb/ef/core/)  to talk to a SQL Server DataBase.
- An *Application* project, which in this case consists of a [REST](https://www.restapitutorial.com/lessons/whatisrest.html#) service using [ASP.NET 5 Web API](https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-5.0&tabs=visual-studio).

Both the `Service` and the `Repository` will implement the interfaces and make use of the `Photo` entity that we have already defined on the fontend, so before we start, let's factor those out, into a new `Shared` project.

## The Shared Core

- On the `Solution Explorer`, right click you solution, then select `Add` -> `New Project`.
- Select `Class Library (.NET Standard)`. Click `Next`
- In the  `Project Name` field, type `PhotoSharingApplication.Shared.Core`
- Open the `PhotoSharingApplication.Frontend.Core`
- Cut the following folders with their content:
    - `Interfaces`
    - `Entities`
- Paste them into the `PhotoSharingApplication.Shared.Core` project
- Rename the namespaces of the classes and interfaces to match project / folder name
    - Change the namespace of the `Photo` class to `PhotoSharingApplication.Shared.Core.Entities`
    - Change the namespace of `IPhotosRepository` and `IPhotosService` to `PhotoSharingApplication.Shared.Core.Interfaces`
- In the `PhotoSharingApplication.Frontend.Core`, 
    - Add a Project Reference to the `PhotoSharingApplication.Shared.Core`
    - Open the `Service` and change the `using` to match the new namespaces
- In the `PhotoSharingApplication.Frontend.Infrastructure`
    - Open the `Repository` and change the `using` to match the new namespaces
- In the `PhotoSharingApplication.Frontend.BlazorWebAssembly`
    - Open `Program.cs` and change the `using` to match the new namespaces
    - Open every Page and change the `using` (or just delete them from each page and add the correct ones to the _`Imports.razor`)
- In the `PhotoSharingApplication.Frontend.BlazorComponents`
    - Open `PhotoEditComponent` and `PhotoDetailsComponent` and change the `using` to match the new namespaces

Run the application and verify that everything works as before

## The Backend Core

- On the `Solution Explorer`, right click your solution, then select `Add` -> `New Project`.
- Select `Class Library (.NET Standard)`. Click `Next`
- In the  `Project Name` field, type `PhotoSharingApplication.Backend.Core`
- Add a project reference to `PhotoSharingApplication.Shared.Core`

Now we can implement our service, which for now will just pass the data to the repository and return the results, with little or no additional logic (we will replace it later). We are going to use the [Dependency Injection pattern](https://martinfowler.com/articles/injection.html?) to request for a repository.

```cs
public class PhotosService : IPhotosService {
  private readonly IPhotosRepository repository;
  public PhotosService(IPhotosRepository repository) => this.repository = repository;
  public async Task<Photo> FindAsync(int id) => await repository.FindAsync(id);
  public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await repository.GetPhotosAsync(amount);
  public async Task<Photo> RemoveAsync(int id) => await repository.RemoveAsync(id);
  public async Task<Photo> UpdateAsync(Photo photo) => await repository.UpdateAsync(photo);
  public async Task<Photo> UploadAsync(Photo photo) {
    photo.CreatedDate = DateTime.Now;
    return await repository.CreateAsync(photo);
  }
}
```

Don't forget the `using`:

```cs
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
```

Now, it's true that this class looks like the one we have for the frontend, so we may be tempted to share this as well, but we could also argue that the logic server side may very well be more convoluted than the one on the frontend (you may not want to share your *secrets* with the client), sp we're going to keep them separated even if in our case they do the same thing.

## The Backend Infrastructure

- On the `Solution Explorer`, right click you solution, then select `Add` -> `New Project`.
- Select `Class Library (.NET Standard)`. Click `Next`
- In the  `Project Name` field, type `PhotoSharingApplication.Backend.Infrastructure`
- On the `Solution Explorer`, right click on the `Dependencies` folder of the `PhotoSharingApplication.Frontend.Infrastructure` project
- Select `Add Project Reference`
- Check the checkbox next to `PhotoSharingApplication.Shared.Core`
- Click `Ok`

In order to use EntityFrameworkCore 5, you need to update the target framework of your library to 2.1:

- Double click on the project name on the Solution Explorer and replace

```xml
<PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
</PropertyGroup> 
```  
  with 
```xml
<PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
</PropertyGroup>
```

- Add the following NuGet packages:
    - `Microsoft.EntityFrameworkCore`
    - `Microsoft.EntityFrameworkCore.SqlServer`
    - `Microsoft.EntityFrameworkCore.Tools`
    - `Microsoft.EntityFrameworkCore.Design`

### The DbContext

Now we can add the `DbContext`

- Crate a new folder `Data`
- Add a new class `PhotoSharingApplicationContext`
- Let the class derive from `DbContext`

```cs
using Microsoft.EntityFrameworkCore;

namespace PhotoSharingApplication.Backend.Infrastructure.Data {
  public class PhotoSharingApplicationContext : DbContext {
  }
}
```

Because we're going to use this `DbContext` from an ASP.NET Core project, we are going to use the [constructor accepting the DbOptions](https://docs.microsoft.com/en-gb/ef/core/miscellaneous/connection-strings#aspnet-core)

```cs
public PhotoSharingApplicationContext(DbContextOptions<PhotoSharingApplicationContext> options)
  : base(options) {

}
```

We want to give our model some configurations and restrictions, so we're going to use [Fluent API](https://docs.microsoft.com/en-gb/ef/core/modeling/#use-fluent-api-to-configure-a-model) to do that:

```cs
protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.Entity<Photo>(ConfigurePhoto);    
}

private void ConfigurePhoto(EntityTypeBuilder<Photo> builder) {
    builder.ToTable("Photos");

    builder.Property(ci => ci.Id)
        .UseHiLo("photos_hilo")
        .IsRequired();

    builder.Property(ci => ci.Title)
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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhotoSharingApplication.Shared.Core.Entities;
```

The `DbContext` is ready (at least in this project, we still need to configure it further but that's the job of the ASP.NET Core project that we will build later).

### The Repository

Now for the Repository that makes use of the DbContext.

- Create a new folder `Repositories` and inside that, create a subfolder `EntityFramework`
- In this `EntityFramework` folder, add a new class `PhotosRepository` 
- Let the class implement the `IPhotosRepository` interface by adding the following code:

```cs
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Backend.Infrastructure.Repositories.EntityFramework {
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
}
```

To make use of the `DbContext`, we're going to resort to Dependecy Injection, so we need a constructor and a field where to store the DbContext so that we can use it from the methods we have to implement:

```cs
private readonly PhotoSharingApplicationContext context;

public PhotosRepository(PhotoSharingApplicationContext context) {
    this.context = context;
}
```
Now we're going to use [Asynchronous saving](https://docs.microsoft.com/en-gb/ef/core/saving/async) to Add, Update and Delete data.


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

Which requires

```cs
using Microsoft.EntityFrameworkCore;
```

- The code to [Delete](https://docs.microsoft.com/en-gb/ef/core/saving/basic#deleting-data) a Photo from the Database becomes:


```cs
public async Task<Photo> RemoveAsync(int id) {
    var photo = await context.Photos.SingleOrDefaultAsync(m => m.Id == id);
    context.Photos.Remove(photo);
    await context.SaveChangesAsync();
    return photo;
}
```

To read the data, we're going to use [Asynchronous Queries](https://docs.microsoft.com/en-gb/ef/core/querying/async)

- The code to Read all the photos becomes

```cs
    public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => 
    await (from p in context.Photos
            orderby p.CreatedDate descending
            select p).Take(amount).ToListAsync();
```

Which requires

```cs
using System.Linq;
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

The **client** is whatever consumes the web API (browser, mobile app, and so forth). We aren't writing a client in this tutorial. We'll use [Postman](https://www.getpostman.com/apps) to test the app. We will write the client in the following lab.

A **model** is an object that represents the data in your application. In this case, the only model is a Photo item. Models are represented as simple C# classes (POCOs).

A **controller** is an object that handles HTTP requests and creates the HTTP response. This app will have a single controller.

### Create the project

- On the `Solution Explorer`, right click your solution, then select `Add` -> `New Project`.
- Select the `ASP.NET Core Web Application` project template. 
    - Name the Project `PhotoSharingApplication.WebServices.REST.Photos`
    - Select `Add`
- In the `Create a new ASP.NET Core Web Application` window:
    - Select `.NET Core`
    - Select `ASP .NET 5`
    - Select the `API` template
    - Leave `No Authentication`. 
    - Ensure that the `Configure for Https` checkbox is selected
    - Do not check `Enable Docker Support`.
    - Click `Create`

### Add the Controller

- In the `Solution Explorer`, right click the `Controllers` folder, select `Add` -> `Controller`
- Select `API Empty`
- Name the controller `PhotosController`

The wizard took care of the [Controller](https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-3.0) by generating a class that derives from [ControllerBase](https://docs.microsoft.com/dotnet/api/microsoft.aspnetcore.mvc.controllerbase), which provides many properties and methods that are useful for handling HTTP requests.

The `Microsoft.AspNetCore.Mvc` namespace provides attributes that can be used to configure the behavior of web API controllers and action methods.

The [ApiController](https://docs.microsoft.com/dotnet/api/microsoft.aspnetcore.mvc.apicontrollerattribute) attribute was applied to the controller class to enable the following API-specific behaviors:
- [Attribute routing requirement](https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-3.0#attribute-routing-requirement)
- [Automatic HTTP 400 responses](https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-3.0#automatic-http-400-responses)
- [Binding source parameter inference](https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-3.0#binding-source-parameter-inference)
- [Multipart/form-data request inference](https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-3.0#multipartform-data-request-inference)
- [Problem details for error status codes](https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-3.0#problem-details-for-error-status-codes) 



```cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PhotoSharingApplication.WebServices.REST.Photos.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase {
    }
}
```

We want to use the `Service` of out *Backend.Core*, so let's make use of the DI container by explicitly declaring the dependency on the `IPhotosService` in the Controller constructor:

- Add a `Project Reference` to `PhotoSharingApplication.Backend.Core`
- Add a constructor that accepts a `IPhotosService` parameter
- Save the parameter into a private readonly field

```cs
private readonly IPhotosService service;
public PhotosController(IPhotosService service) {
    this.service = service;
}
```

Now we can start implementing our CRUD methods.

The controller and every action should be mapped to a route through the use of [routing](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-3.0) system, in particular [attribute routing](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-3.0#attribute-routing) and [attribute routing using http verbs attributes](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-3.0#attribute-routing-with-httpverb-attributes).

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

It returns an `Task<ActionResult<IEnumerable<Photo>>>`. MVC automatically serializes the list of `Photo` to JSON and writes the JSON into the body of the response message. The response code for this method is 200, assuming there are no unhandled exceptions. (Unhandled exceptions are translated into 5xx errors.)

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
[HttpGet("{id:int}", Name="Find")]
public async Task<ActionResult<Photo>> Find(int id) {
    Photo ph = await service.FindAsync(id);
    if (ph == null) return NotFound();
    return ph;
}
```

### Create Action

As for REST standards, the action that adds a product to the database is bound to the `POST http verb`.

```cs
[HttpPost]
public async Task<ActionResult<Photo>> CreateAsync(Photo photo) {
    Photo p = await service.UploadAsync(photo);
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


## Registering the services and configuring the DbContext

The context has to be configured and added as a Service using the [Dependency Injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-5.0) features of `ASP.NET Core`.

- Add a Project reference to your `PhotoSharingApplication.Backend.Infrastructure` 

Open the [`Startup.cs`](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup?view=aspnetcore-5.0) file, find the `ConfigureServices` method and add the configuration for the DbContext:

```cs
public void ConfigureServices(IServiceCollection services) {
    services.AddControllers();
    services.AddDbContext<PhotoSharingApplicationContext>(options =>
        options.UseSqlServer(Configuration.GetConnectionString("PhotoSharingApplicationContext")));
}
```

Which requires

```cs
using Microsoft.EntityFrameworkCore;
using PhotoSharingApplication.Backend.Infrastructure.Data;
```

Add `PhotoSharingApplicationContext` the connection string to [configure](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0) it in the `appsettings.json` file, as per [Default](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0#default-configuration):

```json
"ConnectionStrings": {
    "PhotoSharingApplicationContext": "Server=(localdb)\\mssqllocaldb;Database=PhotoSharingApplicationContextBlazorLabs;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
```

Now, the two interfaces and implementations.

To use our service in the `PhotosController` controller, we need to perform a couple of steps, also described in the [Dependency Injection documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-5.0)


[In the docs](https://docs.microsoft.com/en-gb/aspnet/core/blazor/dependency-injection?view=aspnetcore-5.0#add-services-to-an-app) they tell us what to do: 

- Open the `Startup.cs` file of the `PhotoSharingApplication.Backend.REST.PhotosServices`project
- Replace the current `COnfigureServices` method with the following

```cs
public void ConfigureServices(IServiceCollection services) {
    services.AddControllers();
    services.AddDbContext<PhotoSharingApplicationContext>(options =>
        options.UseSqlServer(Configuration.GetConnectionString("PhotoSharingApplicationContext")));
    services.AddScoped<IPhotosService, PhotosService>();
    services.AddScoped<IPhotosRepository, PhotosRepository>();
}
```

Of course, also add the correct `using`:

```cs
using PhotoSharingApplication.Backend.Infrastructure.Data;
using PhotoSharingApplication.Backend.Infrastructure.Repositories.EntityFramework;
using PhotoSharingApplication.Frontend.Core.Services;
using PhotoSharingApplication.Shared.Core.Interfaces;
```


### Generate migrations and database

The database has not been created. We're going to use [Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/) to generate the DB and update the schema on a later Lab, using the [Entity Framework Core Tools in the Package Manager Console](https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/powershell).

First, we need to add the tools by adding the following NuGet Packages to our `PhotoSharingApplication.WebServices.REST.Photos` project:

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`

Then, as per the documentation:

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
Add-Migration InitialCreate -Project PhotoSharingApplication.Backend.Infrastructure -StartupProject PhotoSharingApplication.WebServices.REST.Photos
```

Three files are added to your project under the Migrations directory:

- XXXXXXXXXXXXXX_InitialCreate.cs--The main migrations file. Contains the operations necessary to apply the migration (in Up()) and to revert it (in Down()).
- XXXXXXXXXXXXXX_InitialCreate.Designer.cs--The migrations metadata file. Contains information used by EF.
- BackEndContextModelSnapshot.cs--A snapshot of your current model. Used to determine what changed when adding the next migration.

The timestamp in the filename helps keep them ordered chronologically so you can see the progression of changes.

### Update the database
Next, apply the migration to the database to create the schema.

```
Update-Database -Project PhotoSharingApplication.Backend.Infrastructure -StartupProject PhotoSharingApplication.WebServices.REST.Photos
```

You should now have a new SQL Server database called `PhotoSharingApplicationContextBlazorLabs` with one empty `Photos` table.

### Try the Get Actions of the controller

In Visual Studio, set the `PhotoSharingApplication.WebServices.REST.Photos` project as startup project, then press `CTRL+F5` to launch the app. Visual Studio launches a browser and navigates to http://localhost:port/weatherforecast, where port is a randomly chosen port number. Navigate to the Photos controller at `http://localhost:port/products`.

You should see an empty JSON array. 

### Install Postman

This tutorial uses Postman to test the web API.

- Install [Postman](https://www.getpostman.com/downloads/)
- Start the web app.
- Start Postman.
- Disable SSL certificate verification
- From File > Settings (*General tab), disable SSL certificate verification.

### Use Postman to send a Create request

- Set the HTTP method to POST
- Select the Body radio button
- Select the raw radio button
- Set the type to JSON
- In the key-value editor, enter a photo item such as

```json
{
	"id" : 0,
	"title" : "A New Photo",
	"description" : "The Description of the new Photo",
	"userName" : "alice"
}
```

Select `Send`

Select the `Headers` tab in the lower pane and copy the `Location` header.

You can use the `Location` header URI to access the resource you just created.

If you add multiple photos and try to navigate to the photos again, you should see an array with all the photos you added.


Navigate to `http://localhost:port/photos/1`

You should see this response:

```
HTTP/1.1 200 OK
Transfer-Encoding: chunked
Content-Type: application/json; charset=utf-8

{
    "id": 1,
    "title": "A New Photo",
    "photoFile": null,
    "imageMimeType": null,
    "description": "The Description of the new Photo",
    "createdDate": "2020-05-13T11:52:55.7818333",
    "userName": "alice"
}
```

Navigate to `http://localhost:port/photos/99`

You should see this response header:

```
HTTP/1.1 404 Not Found
Content-Length: 0
```

### Try the Update Action

You can use POSTMAN to test the Update action.

- Set the HTTP method to PUT
- Set the address to `http://localhost:port/photos/1`
- Select the Body radio button
- Select the raw radio button
- Set the type to JSON
- In the key-value editor, enter a photo item such as

```json
{
    "id": 1,
    "title": "Updated Photo Title",
    "photoFile": null,
    "imageMimeType": null,
    "description": "The Updated Description of the updated Photo",
    "createdDate": "2020-05-13T11:52:55.7818333",
    "userName": "bob"
}
```

Select `Send`

Verify that the respons is a 200 and that its body contains the updated photo object.

### Try the Delete Action

You can use POSTMAN to test the Delete action.

- Set the HTTP method to DELETE
- Set the address to `http://localhost:port/photos/1`

Select `Send`

Check that the response contains the first photo.
If you call the action to get all the photos, you should not see the photo with id 1 anymore.

Our service is ready. In the next lab we will setup the client side. 

Go to `Labs/Lab05`, open the `readme.md` and follow the instructions thereby contained.   