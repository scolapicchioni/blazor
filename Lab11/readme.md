# Security: Resource Based Authorization

We did not protect the update and delete operations, yet.

What we would like to have is an application where:
- Photos may be updated and deleted only by their respective owners
- Comments may be updated and deleted only by their respective owners

In order to achieve this, we have to update both our Backend and our FrontEnd.

## BackEnd
- We will configure the [Resource Based Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased?view=aspnetcore-5.0) by creating
    - A CanEditDeletePhoto[Policy](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-5.0)
    - A SameAuthor[Requirement](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-5.0#requirements)
    - A PhotoEditDelete[AuthorizationHandler](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-5.0#authorization-handlers). This handler will succeed only if the UserName property of the Photo being updated/deleted matches the value of the user name.
    - A CanEditDeleteComment[Policy](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-5.0)
    - A CommentEditDelete[AuthorizationHandler](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-5.0#authorization-handlers)
    This handler will succeed only if the UserName property of the Comment being updated/deleted matches the value of the user name.
- During the Update and Delete of a Photo, we will ask an `AuthorizationService` to check if the `CanEditDeletePhotoPolicy` is fulfilled, eventually denying the possibility to complete the action if the user is not the photo's owner
- During the Update and Delete of a Comment, we will ask an `AuthorizationService` to check if the `CanEditDeleteCommentPolicy` is fulfilled, eventually denying the possibility to complete the action if the user is not the comments' owner

## FrontEnd
- We will:
  - Configure the [Resource Based Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased?view=aspnetcore-5.0) by reusing the same Policies, Requirements and Handlers as the Backend side.
  - Pass the access token to our PhotosService during update and delete, just like we did during the Create
  - Pass the access token to our CommentsService during update and delete, just like we did during the Create
  - Show the Update and Delete buttons only if allowed by asking the `AuthorizationService` to check if the `CanEditDeletePhotoPolicy` is fulfilled.

Let's start by updating our BackEnd Service.

## Authorization

Let's proceed to enforce Authorization Policies by implementing [Resource Based Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased?view=aspnetcore-3.0).

We already know that we can use the same policies, requirements and handlers from both backend and frontend, so it seems logical to put the code in a shared project.

- On the `Solution Explorer` right click on the Solution, select `Add` -> `New Project`
- As `Project Template`, select `Class Library (.NET Standard)`
- In the `Project Name` field, type `PhotoSharingApplication.Shared.Authorization`
- Click `Create`
- Add a reference to the `Microsoft.AspNetCore.Authorization` NuGet package
- Add a project reference to the `PhotoSharingApplication.Shared.Core` project

We're going to use the approach explained in the article [Configuring Policy-based Authorization with Blazor](https://chrissainty.com/securing-your-blazor-apps-configuring-policy-based-authorization-with-blazor/) of the great Chris Sainty.

Role authorization and Claims authorization make use of 

- a requirement 
- a handler for the requirement 
- a pre-configured policy

These building blocks allow you to express authorization evaluations in code, allowing for a richer, reusable, and easily testable authorization structure.

### Requirement

An authorization requirement is a collection of data parameters that a policy can use to evaluate the current user principal. In our policies the requirement we have is a single parameter, the owner. A requirement must implement `IAuthorizationRequirement`. This is an empty, marker interface. 
Our requirement is very simple, we don't need any additional configuration for that.

- In the `PhotoSharingApplication.Shared.Authorization` project, add a new `Class`
- Name the class `SameAuthorRequirement`
- Let the class implement the `IAuthorizationRequirement` interface

```cs
public class SameAuthorRequirement : IAuthorizationRequirement { }
```
which requires
```cs
using Microsoft.AspNetCore.Authorization;
```

### Authorization Handlers

An *authorization handler* is responsible for the evaluation of any properties of a requirement. The authorization handler must evaluate them against a provided `AuthorizationHandlerContext` to decide if authorization is allowed. A requirement can have multiple handlers. Handlers must inherit `AuthorizationHandler<T>` where `T` is the requirement it handles.

In our handler, if the current User has a Name matching the UserName of the photo, authorization will be successful. We will indicate it by calling the `context.Succeed()` method, passing in the requirement that has been fulfilled.

Add a `PhotoEditDeleteAuthorizationHandler` class and replace its content with the following code:

```cs
using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Core.Entities;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Shared.Authorization {
  public class PhotoEditDeleteAuthorizationHandler : AuthorizationHandler<SameAuthorRequirement, Photo> {
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                    SameAuthorRequirement requirement,
                                                    Photo resource) {
      if (context.User.Identity?.Name == resource.UserName) {
          context.Succeed(requirement);
      }

      return Task.CompletedTask;
    }
  }
}
```

Add a `CommentEditDeleteAuthorizationHandler` class and replace its content with the following code:
```cs
using PhotoSharingApplication.Shared.Core.Entities;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Shared.Authorization {
  public class CommentEditDeleteAuthorizationHandler : AuthorizationHandler<SameAuthorRequirement, Comment> {
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                    SameAuthorRequirement requirement,
                                                    Comment resource) {
      if (context.User.Identity?.Name == resource.UserName) {
        context.Succeed(requirement);
      }

      return Task.CompletedTask;
    }
  }
}
```

### Building the Policies

We can use an `AuthorizationPolicyBuilder` to add our requirements and build an `AuthorizationPolicy`, which we will register both on the client and on the server on a later step.

- Add a `Policies` class
- Mark the class as `static`
- Add the following method

```cs
public static AuthorizationPolicy CanEditDeletePhotoPolicy() => new AuthorizationPolicyBuilder()
  .RequireAuthenticatedUser()
  .AddRequirements(new SameAuthorRequirement())
  .Build();
```

which requires a
```cs
using Microsoft.AspNetCore.Authorization;
```

In the same class, also add this method:

```cs
public static AuthorizationPolicy CanEditDeleteCommentPolicy() => new AuthorizationPolicyBuilder()
  .RequireAuthenticatedUser()
  .AddRequirements(new SameAuthorRequirement())
  .Build();
```

To register and check the policies, we will need to assign a name to the policies. Let's define two constants in the `Policies` static class:

```cs
public const string EditDeletePhoto = "EditDeletePhoto";
public const string EditDeleteComment = "EditDeleteComment";
```

## Handler registration

Handlers must be registered in the services collection during configuration. We have to configure both the two servers and the client.

Each handler is added to the services collection by using ```services.AddSingleton<IAuthorizationHandler, YourHandlerClass>();``` passing in your handler class.

### PhotoSharingExamples.RESTServices.WebApiPhotos

- Add a project reference to the `PhotoSharingApplication.Shared.Authorization` project
- Open the `Startup.cs` and add this line of code at the bottom of the `ConfigureServices` method:

```cs
services.AddSingleton<IAuthorizationHandler, PhotoEditDeleteAuthorizationHandler>();
```

which requires

```cs
using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Authorization;
```

### PhotoSharingApplication.WebServices.Grpc.Comments

- Add a project reference to the `PhotoSharingApplication.Shared.Authorization` project
- Open the `Startup.cs` and add this line of code at the bottom of the `ConfigureServices` method:

```cs
services.AddSingleton<IAuthorizationHandler, CommentEditDeleteAuthorizationHandler>();
```

which requires

```cs
using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Authorization;
```

### PhotoSharingExamples.Frontent.BlazorWebAssembly

- Add a project reference to the `PhotoSharingApplication.Shared.Authorization` project
- Open the `Program.cs` and add this line of code in the `Main` method, before `await builder.Build().RunAsync();`:

```cs
builder.Services.AddSingleton<IAuthorizationHandler, PhotoEditDeleteAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, CommentEditDeleteAuthorizationHandler>();
```

which requires

```cs
using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Shared.Authorization;
```

## Authorization Policy Configuration


An *authorization policy* is registered at application startup as part of the Authorization service configuration. We have to configure both the two servers and the client.

### PhotoSharingExamples.RESTServices.WebApiPhotos

- Open the `Startup.cs` and replace this code of the `ConfigureServices` method:


```cs
services.AddAuthorization(options => {
  options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy => {
    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
    policy.RequireClaim(JwtClaimTypes.Name);
  });
});
```

with this code:

```cs
services.AddAuthorization(options => {
  options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy => {
    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
    policy.RequireClaim(JwtClaimTypes.Name);
  });
  
  options.AddPolicy(Policies.EditDeletePhoto, Policies.CanEditDeletePhotoPolicy());
});
```

### PhotoSharingApplication.WebServices.Grpc.Comments

- Open the `Startup.cs` and replace this code of the `ConfigureServices` method:

```cs
services.AddAuthorization(options => {
  options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy => {
    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
    policy.RequireClaim(JwtClaimTypes.Name);
  });
});
```
with this code:

```cs
services.AddAuthorization(options => {
  options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy => {
    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
    policy.RequireClaim(JwtClaimTypes.Name);
  });
  
  options.AddPolicy(Policies.EditDeleteComment, Policies.CanEditDeleteCommentPolicy());
});
```

### PhotoSharingExamples.Frontent.BlazorWebAssembly

- Open the `Program.cs` and add this lines of code in the `Main` method

```cs
builder.Services.AddAuthorizationCore(options =>
{
  options.AddPolicy(Policies.EditDeletePhoto, Policies.CanEditDeletePhotoPolicy());
  options.AddPolicy(Policies.EditDeleteComment, Policies.CanEditDeleteCommentPolicy());
});
```

### Authorizing within your code

Policies can usually be applied using the `Authorize` attribute by specifying the policy name, but not in this case.
Our authorization depends upon the resource being accessed. A `Product` has a `UserName` property. Only the product owner is allowed to update it or delete it, so the resource must be loaded from the product repository before an authorization evaluation can be made. This cannot be done with an `[Authorize]` attribute, as attribute evaluation takes place before data binding and before your own code to load a resource runs inside an action. Instead of *declarative authorization*, the attribute method, we must use *imperative authorization*, where a developer calls an authorize function within their own code.

Authorization is implemented as a service, `IAuthorizationService`, registered in the service collection and available via dependency injection for any class to access.

### PhotoSharingExamples.RESTServices.WebApiPhotos

- Open the `PhotosController` in the `Controllers` folder of the `PhotoSharingExamples.RESTServices.WebApiPhotos` project and change the constructor to include an `IAuthorizationService` parameter, to be saved in a private readonly field, like so:

```cs
private readonly IPhotosService service;
private readonly IAuthorizationService authorizationService;

public PhotosController(IPhotosService service, IAuthorizationService authorizationService) {
  this.service = service;
  this.authorizationService = authorizationService;
}
```

The ```IAuthorizationService``` interface has two methods, one where you pass the resource and the policy name and the other where you pass the resource and a list of requirements to evaluate.
To call the service, load your photo within your action then call the `AuthorizeAsync`, returning a `ForbidResult` if the `Succeeded` property of the result is false. 
Also, add the `[Authorize]` attribute on top of the method in order to make sure that the user is at least authenticated before proceeding with the action.

### Update Action

This is how the `Update` becomes:

```cs
[Authorize]
[HttpPut("{id}")]
public async Task<ActionResult<Photo>> Update(int id, Photo photo) {
    if (id != photo.Id)
      return BadRequest();
    Photo ph = await service.FindAsync(id);
    var authorizationResult = await authorizationService.AuthorizeAsync(User, ph, Policies.EditDeletePhoto);

    if (authorizationResult.Succeeded) {
      return await service.UpdateAsync(photo);
    } else {
      return Forbid();
    }
}
```

which requires

```cs
using PhotoSharingApplication.Shared.Authorization;
```

### Remove Action

This is how the `Remove` becomes:

```cs
[Authorize]
[HttpDelete("{id}")]
public async Task<ActionResult<Photo>> Remove(int id) {
  Photo ph = await service.FindAsync(id);
  if (ph == null) return NotFound();
  
  var authorizationResult = await authorizationService.AuthorizeAsync(User, ph, Policies.EditDeletePhoto);

  if (authorizationResult.Succeeded) {
    return await service.RemoveAsync(id);
  } else {
    return Forbid();
  }
}
```

### PhotoSharingApplication.WebServices.Grpc.Comments

We can use pretty much the same idea on the gRPC service as well:

- Open the `CommentsService` in the `Services` folder of the `PhotoSharingApplication.WebServices.Grpc.Comments` project and change the constructor to include an `IAuthorizationService` parameter, to be saved in a private readonly field, like so:

```cs
private readonly ICommentsService commentsService;
private readonly IAuthorizationService authorizationService;

public CommentsService(ICommentsService commentsService, IAuthorizationService authorizationService) {
  this.commentsService = commentsService;
  this.authorizationService = authorizationService;
}
```

### Update method

The process is almost the same as the one we used in the Rest Service.

What changes is:
- We need to find the User in the `ServerCallContext`
- As exaplined in the [Error handling Documentation](https://docs.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/error-handling), if the authorization does not succeed, we need to throw an  `Rpcexception` containing the `PermissionDenied` Status Code

```cs
[Authorize]
public override async Task<UpdateReply> Update(UpdateRequest request, ServerCallContext context) {
  Comment co = await commentsService.FindAsync(request.Id);
  var user = context.GetHttpContext().User;
  var authorizationResult = await authorizationService.AuthorizeAsync(user, co, Policies.EditDeleteComment);

  if (authorizationResult.Succeeded) {
    Comment c = await commentsService.UpdateAsync(new Comment { Id = request.Id, Subject = request.Subject, Body = request.Body });
    return new UpdateReply() { Id = c.Id, PhotoId = c.PhotoId, Subject = c.Subject, UserName = c.UserName, Body = c.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()) };
  } else {
    var metadata = new Metadata { { "User", user.Identity.Name } };
    throw new RpcException(new Status(StatusCode.PermissionDenied, "Permission denied"), metadata);
  }
}
```

### Remove Method

Same here:

```cs
public override async Task<RemoveReply> Remove(RemoveRequest request, ServerCallContext context) {
    Comment co = await commentsService.FindAsync(request.Id);
    var user = context.GetHttpContext().User;
    var authorizationResult = await authorizationService.AuthorizeAsync(user, co, Policies.EditDeleteComment);

  if (authorizationResult.Succeeded) {
    Comment c = await commentsService.RemoveAsync(request.Id);
    return new RemoveReply() { Id = c.Id, PhotoId = c.PhotoId, Subject = c.Subject, UserName = c.UserName, Body = c.Body, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()) };
  } else {
    var metadata = new Metadata { { "User", user.Identity.Name } };
    throw new RpcException(new Status(StatusCode.PermissionDenied, "Permission denied"), metadata);
  }
}
```

## Update the Products Component, Details and Delete View

First of all, let's see if we can speed up our process a bit. We have three pages (Home, Details and Delete) that basically use the same layout: a card with the product information. The only thing that changes are which buttons to show. This means that we will have to update the UI and the logic of three components (Products, Details and Delete), writing  the same code three times. This situation is less than ideal, so let's refactor the `card` into its own component and let's make it so that we can configure which buttons to show depending on the view.

## The Product Component

In the `src/components` folder, create a new file `Product.vue`.
Scaffold the usual vue template with `<template>`, `<script>` and `<style>`.
Now cut the `card` of the `Products` component and paste it in the `Product` component.

```html
<template>
  <v-card elevation="10">
    <v-card-title>{{ product.id }} - {{ product.name }} </v-card-title>
    <v-card-text>
    <p>{{ product.description }}</p>
    <p>{{ product.price }}</p>
    </v-card-text>
    <v-card-actions>
      <v-btn icon :to="{name: 'details', params: {id: product.id}}"><v-icon>mdi-card-bulleted</v-icon></v-btn>
      <v-btn icon :to="{name: 'update', params: {id: product.id}}"><v-icon>mdi-pencil</v-icon></v-btn>
      <v-btn icon :to="{name: 'delete', params: {id: product.id}}"><v-icon>mdi-delete</v-icon></v-btn>
    </v-card-actions>
  </v-card>
</template>
```

This time, instead of loading the product by asking it to the datalayer, we will accept it as a [prop](https://vuejs.org/v2/guide/components-props.html). So the `<script>` becomes

```html
<script>
export default {
    props: {
        product : Object
    }
}
</script>
```

Now let's use the component from within the `Products` component.
- The `<template>` becomes

```html
<template>
  <v-row>
    <v-col
     v-for="product in products" :key="product.id"
     cols="12" md="4"
    >
      <product :product="product" />
    </v-col>
  </v-row>
</template>
```

-The `<script>` becomes

```html
<script>
import datalayer from '@/datalayer'
import Product from "@/components/Product.vue"

export default {
  components: {
    Product
  },
  data () {
    return {
      products: []
    }
  },
  async created () {
    this.products = await datalayer.getProducts()
  }
}
</script>
```

Save and verify that the home view still works as before.

Because we want to use the card from within the `Details` and `Delete` views as well, we need to be able to configure which buttons to show.
- The `Products` Component used in the `Home` View will configure the `Product` to show: 
  - A button to navigate to the `Details` View
  - A button to navigate to the `Update` View
  - A button to navigate to the `Delete` View
- The `Details` View  will configure the `Product` to show:
  - A button to navigate to the `Update` View
  - A button to navigate to the `Delete` View
- The `Delete` View  will configure the `Product` to show:
  - A button to actually delete the product

Let's create some `Boolean` `props` into the `Product` component:

```js
details: {type: Boolean, default: false},
update: {type: Boolean, default: false},
requestdelete: {type: Boolean, default: false},
confirmdelete: {type: Boolean, default: false}
```

Let's use the props to show the corresponding buttons.

```html
<v-card-actions>
  <v-btn icon :to="{name: 'details', params: {id: product.id}}" v-if="details===true"><v-icon>mdi-card-bulleted</v-icon></v-btn>
  <v-btn icon :to="{name: 'update', params: {id: product.id}}" v-if="update===true"><v-icon>mdi-pencil</v-icon></v-btn>
  <v-btn icon :to="{name: 'delete', params: {id: product.id}}" v-if="requestdelete===true"><v-icon>mdi-delete</v-icon></v-btn>
  <v-btn @click="deleteProduct" color="warning" v-if="confirmdelete===true">DELETE PRODUCT</v-btn>
</v-card-actions>
```

Let's also transfer the `deleteProduct` method from the `Delete` view to the `Product` component.

```js
async deleteProduct () {
    await datalayer.deleteProduct(+this.product.id)
    this.$router.push({name: 'home'})
}
```

Now let's have the `Products` component [pass the values](https://vuejs.org/v2/guide/components-props.html#Passing-a-Boolean) it needs to show. We don't need to pass the rest, as they are already `false`.

```html
<product :product="product" details update requestdelete />
```

Let's repeat for the `Details` and `Delete` Views.

The `<template>` section of the `src/views/Details.vue` view becomes:

```html
<template>
  <v-row>
    <v-col cols="sm">
      <product :product="product" update requestdelete />
    </v-col>
  </v-row>
</template>
```

while the `<script>` section becomes

```js
<script>
import datalayer from '@/datalayer'
import Product from "@/components/Product.vue"
export default {
  components: {
    Product
  },
  data () {
    return {
      product: {
        id: 0,
        name: '',
        description: '',
        price: 0
      }
    }
  },
  async created () {
    this.product = await datalayer.getProductById(+this.$route.params.id)
  }
}
</script>
```

The `<template>` section of the `src/views/Delete.vue` view becomes

```html
<template>
  <v-row>
    <v-col cols="sm">
      <product :product="product" confirmdelete />
    </v-col>
  </v-row>
</template>
```

The `<script>` section becomes

```js
<script>
import datalayer from '@/datalayer'
import Product from "@/components/Product.vue"
export default {
  components: {
    Product
  },
  data () {
    return {
      product: {
        id: 0,
        name: '',
        description: '',
        price: 0
      }
    }
  },
  async created () {
    this.product = await datalayer.getProductById(+this.$route.params.id)
  }
}
</script>
```

We're done refactoring. Everything should still work exactly as before, so we can (finally!) proceed to 
- Show the owner name on each product
- Show the delete and update buttons only if the user is authorized 

### The product owner name

Let's show the owner of each product. Open the `Product.vue` component under the `src\components` folder and replace this code

```html
<v-card-text>
  <p>{{ product.description }}</p>
  <p>{{ product.price }}</p>
</v-card-text>
```

with the following

```
<v-card-text>
  <p>{{ product.description }}</p>
  <p>{{ product.price }}</p>
  <p>{{ product.userName }}</p>
</v-card-text>
```

If you have updated the database content with user names for the products, you should see them on each view, now.

Now we want to show the buttons only if the product is owned by the current user. In order to do that we have to use the `authenticationManager` to get who the current user is. We will use this information to to compare it with each product's `userName` property in the view template. Luckily we have written our code in a mixin, so we can import that to already include most of the logic we need.

In the `Product` component we will test if the `product.userName` is equal to the `user.name`.

## Import the `checksec` mixin to retrieve the current user

Open the `HomeView.vue` component under your `src/components` folder, locate the `<script>` tag and import the applicationUserManager constant by adding the followin line:

```js
import checkSecMixin from "@/checksec"
```

Add the mixin to a new `mixins` array:

```
  mixins:[checkSecMixin],
```

We can now update the template. Locate the following code

```html
<v-card-actions>
  <v-btn icon :to="{name: 'details', params: {id: product.id}}" v-if="details===true"><v-icon>mdi-card-bulleted</v-icon></v-btn>
  <v-btn icon :to="{name: 'update', params: {id: product.id}}" v-if="update===true"><v-icon>mdi-pencil</v-icon></v-btn>
  <v-btn icon :to="{name: 'delete', params: {id: product.id}}" v-if="requestdelete===true"><v-icon>mdi-delete</v-icon></v-btn>
  <v-btn @click="deleteProduct" color="warning" v-if="confirmdelete===true">DELETE PRODUCT</v-btn>
</v-card-actions>
```

and update it like this:

```html
<v-card-actions>
  <v-btn icon :to="{name: 'details', params: {id: product.id}}" v-if="details===true"><v-icon>mdi-card-bulleted</v-icon></v-btn>
  <v-btn icon :to="{name: 'update', params: {id: product.id}}" v-if="update===true && user.name===product.userName"><v-icon>mdi-pencil</v-icon></v-btn>
  <v-btn icon :to="{name: 'delete', params: {id: product.id}}" v-if="requestdelete===true && user.name===product.userName"><v-icon>mdi-delete</v-icon></v-btn>
  <v-btn @click="deleteProduct" color="warning" v-if="confirmdelete===true && user.name===product.userName">DELETE PRODUCT</v-btn>
</v-card-actions>
```

If you run the application, you should see the update and delete button only on products created by the logged on user.

### Pass the credentials during Update and Delete

Now let's proceed to update our `datalayer`: we need to pass the credentials during the update and delete, to make sure that our service can recognize the user by extracting the user name from the token.

Modify the `updateProduct` method of your `datalayer.js` file as follows:

```js
async updateProduct (id, product) {
  const user = await applicationUserManager.getUser()
  return fetch(`${this.serviceUrl}/${id}`, {
    method: 'PUT',
    body: JSON.stringify(product),
    headers: new Headers({
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + (user ? user.access_token : '')
    })
  })
}
```

The `deleteProduct` method becomes:

```js
async deleteProduct (id) {
  const user = await applicationUserManager.getUser()
  return fetch(`${this.serviceUrl}/${id}`, {
    method: 'DELETE',
    headers: new Headers({
      'Authorization': 'Bearer ' + (user ? user.access_token : '')
    })
  })
}
```

This concludes the walkthrough.
I hope you enjoyed it!

In the next step I plan to use the Camera API to upload a picture of the product.

Stay tuned for updates!



