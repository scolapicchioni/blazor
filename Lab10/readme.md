# Security: Authentication and Authorization

We have not implemented any security yet. In this lab we are going to setup and configure a new project that will act as an *Authentication Server*. We will then protect the **Create** operation and we will use the Authentication Server to authenticate the user and issue a *token*, then have the client gain access to the protected operation by using such token.
The Authentication Server that we're going to use implements [OAuth 2.0 and IDConnect](https://www.oauth.com/).

## The Authentication Server

We are going to use [Identity Server 4](https://identityserver4.readthedocs.io/en/latest/index.html)

We are going to use the [Templates](https://github.com/IdentityServer/IdentityServer4.Templates)

**NOTE: At the time of the writing the templates have not yet been updated to .NET core 5, so we're going to use .NET Core 3.1 instead. For more information, check the [GitHub repo](https://github.com/IdentityServer/IdentityServer4.Templates/tree/master/src/IdentityServer4AspNetIdentity)**

- Open a command prompt and navigate to your solution folder (or `Labs\Lab10\Start\PhotoSharingApplication` if did not follow the previous labs)
- If you haven't installed the IdentityServer templates yet, do it by typing the following command:

```
dotnet new -i identityserver4.templates
```
- Create a new folder named `PhotoSharingApplication.IdentityServer` by typing the following command

```
md PhotoSharingApplication.IdentityServer
```
- Navigate to the `PhotoSharingApplication.IdentityServer` folder by typing the following command:

```
cd PhotoSharingApplication.IdentityServer
```

- Create an IdentityServer project that uses EntityFramework by typing the following command: 

```
dotnet new is4ef
```

- When asked `Do you want to run this action (Y|N)?` type `N` and press `Enter`. We will create the DB after we configure everything correctly.
- Open `Lab10\Start\PhotoSharingApplication\PhotoSharingApplication.sln` in Visual Studio 2019 Preview
- In the `Solution Explorer`, right click on the solution, select `Add` -> `Existing Project`
- Select `Lab10\Start\PhotoSharingApplication\PhotoSharingApplication.IdentityServer\PhotoSharingApplication.IdentityServer.csproj`

## Configuration of the http ports

Right now, both the `IdentityServer` and the `gRPC` projects try to run from port 5001, so let's configure each web project to run on a specific port of our choice. This way even the configuration will be easier.

### Blazor Web Assembly

- In the `Solution Explorer`, right click the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project, select `Properties`
- In the `Properties` window of your project, click on `Debug`
- In the `Profile`, select `PhotoSharingApplication.Frontend.BlazorWebAssembly`
- In the `App Url`, ensure that the value is `https://localhost:5001;http://localhost:5000`
- Save
- Right click the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project, select `Set as Startup Project`
- On the Visual Studio Toolbar on top, next to the green arrow, instead of `IISExpress` select `PhotoSharingApplication.Frontend.BlazorWebAssembly`
- Click on the green arrow (or press F5) and verify that the project starts from port 5001
- Stop the application

### Comments gRPC API

- In the `Solution Explorer`, right click the `PhotoSharingApplication.WebServices.Grpc.Comments` project, select `Properties`
- In the `Properties` window of your project, click on `Debug`
- In the `Profile`, select `PhotoSharingApplication.WebServices.Grpc.Comments`
- In the `App Url`, ensure that the value is `http://localhost:5020;https://localhost:5021`
- Save
- Right click the `PhotoSharingApplication.WebServices.Grpc.Comments` project, select `Set as Startup Project`
- On the Visual Studio Toolbar on top, next to the green arrow, instead of `IISExpress` select `PhotoSharingApplication.WebServices.Grpc.Comments`
- Click on the green arrow (or press F5) and verify that the project starts from port 5021
- Stop the application

### Identity Server

- In the `Solution Explorer`, right click the `PhotoSharingApplication.IdentityServer` project, select `Properties`
- In the `Properties` window of your project, click on `Debug`
- In the `Profile`, select `SelfHost`
- In the `App Url`, ensure that the value is `https://localhost:5031`
- Save
- Right click the `PhotoSharingApplication.IdentityServer` project, select `Set as Startup Project`
- Click on the green arrow (or press F5) and verify that the project starts from port 5031
- Stop the application

### Multiple Startup Projects
- In the `Solution Explorer`, right click the solution, select `Set Startup Projects`
- On the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project, select `Start`
- On the `PhotoSharingApplication.WebServices.REST.Photos` project, select `Start`
- On the `PhotoSharingApplication.WebServices.Grpc.Comments` project, select `Start`
- On the `PhotoSharingApplication.IdentityServer` project, select `Start`

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
  var backendUrl = "https://localhost:5021";
  var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
  var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions { HttpClient = httpClient });
  return new CommentsBaseService.CommentsBaseServiceClient(channel);
});
```

## Identity Provider Configuration

Now that we have an `IdentityServer` project, we need to cofigure it for our own purposes. 
- We already have two users: *alice* with password *alice* and *bob* with password *bob* (you can find them in the the `QuickStart\TestUsers.cs` file).
- Our [IdentityResource](https://identityserver4.readthedocs.io/en/latest/topics/resources.html) has already been configured, so we don't need to change that in the `Config.cs` file.
- We need to configure two [API Resource](https://identityserver4.readthedocs.io/en/latest/reference/api_resource.html) (the REST Service for the Photos and the gRPC Service for the Comments)
- We need to configure one [client](https://identityserver4.readthedocs.io/en/latest/reference/client.html) (our Blazor application)

### ApiResource

Open the `Config.cs` file located in the root of your `PhotoSharingApplication.IdentityServer` project.

- Configure the `Photos` ApiResource. 
  - Name it `photosrest`
  - Describe it as `Photos REST Service`
  - Include the `Name` of the user in the access token. We will use the name in a future lab to allow photos update and deletion only to the photo owner.
- Configure the `Comments` ApiResource. 
  - Name it `commentsgrpc`
  - Describe it as `Comments gRPC Service`
  - Include the `Name` of the user in the access token. We will use the name in a future lab to allow comments update and deletion only to the comment owner.

```cs
public static IEnumerable<ApiResource> Apis =>
  new ApiResource[] {
      new ApiResource("commentsgrpc", "Comments gRpc Service") {UserClaims = new string[] { JwtClaimTypes.Name } }, 
      new ApiResource("photosrest", "Photos REST Service") {UserClaims = new string[] { JwtClaimTypes.Name } }
  };
```

which requires 

```cs
using IdentityModel;
```

### Client

The second thing we need to configure is the [Blazor Client](https://identityserver4.readthedocs.io/en/latest/topics/clients.html)
- Locate the `Clients` static property.
- Remove every client except for the last one (`spa`)
- Change the `ClientId` to `blazorclient`
- Change the `ClientName` will be `Blazor Client`
- Change the `ClientUri` to `http://localhost:5001` 
- Change the `RedirectUris` to the [custom app routes used in Blazor](https://docs.microsoft.com/en-us/aspnet/core/security/blazor/webassembly/additional-scenarios?view=aspnetcore-5.0#customize-app-routes)
```cs
{
    "https://localhost:5001/",
    "https://localhost:5001/authentication/login-callback",
    "https://localhost:5001/authentication/silent",
    "https://localhost:5001/authentication/popup",
},
```

- Change the `PostLogoutRedirectUris` to `https://localhost:5001`
- Change the `AllowedCorsOrigins` to `https://localhost:5001`
- Change the `AllowedScopes` to `{ "openid", "profile", "photosrest", "commentsgrpc" }`

The `Clients` property should look like the following:

```cs
public static IEnumerable<Client> Clients =>
  new Client[]
  { 
    // Blazor client using code flow + pkce
    new Client
    {
      ClientId = "blazorclient",
      ClientName = "Blazor Client",
      ClientUri = "https://localhost:5001",

      AllowedGrantTypes = GrantTypes.Code,
      RequirePkce = true,
      RequireClientSecret = false,

      RedirectUris =
      {
        "https://localhost:5001/",
        "https://localhost:5001/authentication/login-callback",
        "https://localhost:5001/authentication/silent",
        "https://localhost:5001/authentication/popup",
      },

      PostLogoutRedirectUris = { "https://localhost:5001/" },
      AllowedCorsOrigins = { "https://localhost:5001" },

      AllowedScopes = { "openid", "profile", "photosrest", "commentsgrpc" }
    }
  };
```

The template we chose to generate our `PhotoSharingApplication.IdentityServer` project uses the `Config.cs` just to seed a database. Such db is further used to read the configuration when the server is running. 

**It is important that you understand that if you ever change the `Config.cs`, you need to delete the DB and recreate it again.**

So let's create and seed the database. 

In order to do that we need to run the `PhotoSharingApplication.IdentityServer` project with an argument `/seed`

- Ensure that the project compiles
- Open a command prompt under the `PhotoSharingApplication.IdentityServer` project folder and type the following command

```
dotnet run /seed
```

The command we ran created a SQLite database. If you want to browse it from within Visual Studio, you will need a Visual Studio Extension. 
- In Visual Studio, select `Tools -> Extension And Updates`, 
- Select `Online` 
- Search for `SqLite`. 
- Select the `SQLite / SQL Server Compact Toolbox` 
- Click on `Download`. 
- When the download completes, close Visual Studio to start the installation. 
- When the installation complets, go back to Visual Studio
- Click on `View -> Other Windows -> SQLite / SQL Compact Toolbox`, 
- Click on the `Add SQLite / SQL Compact from current solution` button. 
- You should see a `IdentityServer.db` Database.
  - Feel free to explore its structure and content. 
  - You will notice the configuration tables used by IdentityServer.  


This project is also configured to use Google Authentication. We can remove it because we're not going to use it.

Open the `Startup.cs`, locate the `ConfigureServices` method and remove the `AddGoogle` call. This code

```cs
services.AddAuthentication()
  .AddGoogle(options =>
  {
      // register your IdentityServer with Google at https://console.developers.google.com
      // enable the Google+ API
      // set the redirect URI to http://localhost:5000/signin-google
      options.ClientId = "copy client ID from Google here";
      options.ClientSecret = "copy client secret from Google here";
  });
```

simply becomes

```cs
services.AddAuthentication();
```

In Visual Studio, run the application and test a user login, by navigating to `https://localhost:5031/account/login` and using `alice` / `alice` or `bob` / `bob` as username / password. You should see the user correctly logged on.

## Configuring the REST Service

We can now switch to our Web Api project. We need to:
- Configure it to use Identity Server
- Protect the access to the `Create` action to allow only authenticated users

As explained it the [Adding an API](https://identityserver4.readthedocs.io/en/latest/quickstarts/1_client_credentials.html#configuration) tutorial, we need to configure the API.

- Add the following packages to the `PhotoSharingApplication.WebServices.REST.Photos` project:
  - `IdentityModel`
  - `Microsoft.AspNetCore.Authentication.JwtBearer`


We need to add the authentication services to DI and the authentication middleware to the pipeline. These will:

- validate the incoming token to make sure it is coming from a trusted issuer
- validate that the token is valid to be used with this api (aka scope)

We need to add the authentication services to DI and configure "Bearer" as the default scheme. We can do that thanks to the `AddAuthentication` extension method. We then also have to add the JwtBearer access token validation handler into DI for use by the authentication services, throught the invocation of the `AddJwtBearer` extension method, to which we have to configure the `Authority` (which is the http address of our Identity Server) and the `Audience` (which we set in the previous project as `photosrest`). The Metadata Address or Authority must use HTTPS unless disabled for development by setting `RequireHttpsMetadata=false`.

Open your `Startup` class, locate the `ConfigureServices` method and add the following code:

```cs
 services.AddAuthentication("Bearer")
  .AddJwtBearer("Bearer", options => {
      options.Authority = "https://localhost:5031";
      options.RequireHttpsMetadata = false;
      options.TokenValidationParameters.NameClaimType = JwtClaimTypes.Name;
      options.Audience = "photosrest";
  });
```

which requires a

```cs
using IdentityModel
```

We also need to add the authentication middleware to the pipeline so authentication will be performed automatically on every call into the host, by invoking the `UseAuthorization` extension method **BEFORE** the `UseEndPoints` in the `Configure` method of our `Startup` class.

Locate the `Configure` method and add the following code right before the `app.UseEndPoints()` line:

```cs
app.UseAuthorization();
```

The last step is to protect the `Create` action of our `PhotosController` by using the [Authorize](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/simple?view=aspnetcore-5.0) attribute.

Open your `PhotosController` class, locate the `CreateAsync` method and add the `[Authorize]` attribute right before the definition of the method:

```cs
[Authorize]
[HttpPost]
public async Task<ActionResult<Photo>> CreateAsync(Photo photo) {
  Photo p = await service.UploadAsync(photo);
  return CreatedAtRoute("Find", photo, new { id = photo.Id});
}
```

which requires a

```cs
using Microsoft.AspNetCore.Authorization;
```

If you use the POSTMAN to invoke the Create action you should get a 401 status code in return. This means your API requires a credential.

The API is now protected by IdentityServer.

## Configuring the gRPC Service

The steps to configure our gRPC Service are nearly identical to the ones we took for the REST Service, with just minor tweeks here and there. 

We need to:
- Configure it to use Identity Server
- Protect the access to the `Create` action to allow only authenticated users

- Add the following packages to the `PhotoSharingApplication.WebServices.Grpc.Comments` project:
  - `IdentityModel`
  - `Microsoft.AspNetCore.Authentication.JwtBearer`

Open your `Startup` class, locate the `ConfigureServices` method and add the following code:

```cs
services.AddAuthentication("Bearer")
  .AddJwtBearer("Bearer", options => {
      options.Authority = "https://localhost:5031";
      options.RequireHttpsMetadata = false;
      options.TokenValidationParameters.NameClaimType = JwtClaimTypes.Name;
      options.Audience = "commentsgrpc";
  });
services.AddAuthorization(options => {
    options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy => {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireClaim(JwtClaimTypes.Name);
    });
});
```

which requires

```cs
using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
```
- Locate the `Configure` method and add the following code right **BEFORE** the `app.UseEndPoints` line:

```cs
app.UseAuthentication();
app.UseAuthorization();
```

The last step is to protect the `Create` action of our `CommentsService` by using the [Authorize](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/simple?view=aspnetcore-5.0) attribute.

Open your `CommentsService` class, locate the `Create` method and add the `[Authorize]` attribute right before the definition of the method:

```cs
[Authorize]
public override async Task<CreateReply> Create(CreateRequest request, ServerCallContext context) {
  Comment c = await commentsService.CreateAsync(new Comment { PhotoId = request.PhotoId, Subject = request.Subject, Body = request.Body });
  return new CreateReply() { Id = c.Id, PhotoId = c.PhotoId, Body = c.Body, Subject = c.Subject, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()), UserName = c.UserName };
}
```

which requires a

```cs
using Microsoft.AspNetCore.Authorization;
```

The API is now protected by IdentityServer.

## Configuring the Blazor Client

The last part requires the configuration of our client project.

When we started, we used a Blazor templates with no authentication. This means that we have to add some parts that would already been there if we had chosen the [Standalone with Authentication Library](https://docs.microsoft.com/en-us/aspnet/core/security/blazor/webassembly/standalone-with-authentication-library?view=aspnetcore-5.0) template.

My Advice is to create a new project in a completely different folder and grab the code from there, because the template that I'm using at the time of this writing could be different from the one that you would get. Otherwise you could grab the code from my tutorial here. I will assume that you create a new project. We will throw away this new project as soon as we're done transfering the code we need.

### Create a new temporary project

- Open a command propmpt
- Create a temporary folder with the same name as your BlazorWebAssembly project somewhere on your hard disk (for example `C:\temp\PhotoSharingApplication.Frontend.BlazorWebAssembly` or something like that)
- Navigate to that folder by typing `cd C:\temp\PhotoSharingApplication.Frontend.BlazorWebAssembly`
- Create a new Blazor Client project with Authentication by typing the following command

```cs
dotnet new blazorwasm -au Individual
```
- Open the project in a new Instance of Visual Studio

Let's start by copying all the differences and tweeking some of the files.

### Index.html

Our starting point is the [Index Page](https://docs.microsoft.com/en-us/aspnet/core/security/blazor/webassembly/standalone-with-authentication-library?view=aspnetcore-5.0#index-page). You will notice that the new project has an additional JavaScript file, which we need to add to our `index.html`

```html
<script src="_content/Microsoft.AspNetCore.Components.WebAssembly.Authentication/AuthenticationService.js"></script>
```

This file is included in an [Authentication Package](https://docs.microsoft.com/en-us/aspnet/core/security/blazor/webassembly/standalone-with-authentication-library?view=aspnetcore-5.0#authentication-package), which we also need to add as a NuGet package

- Add the NuGetPackage `Microsoft.AspNetCore.Components.WebAssembly.Authentication`

### Program.cs

We can now proceed to open `Program.cs`. Notice the folowing code for the [Authentication Support](https://docs.microsoft.com/en-us/aspnet/core/security/blazor/webassembly/standalone-with-authentication-library?view=aspnetcore-5.0#authentication-service-support) that we're missing:

```cs
builder.Services.AddOidcAuthentication(options =>
{
    // Configure your authentication provider options here.
    // For more information, see https://aka.ms/blazor-standalone-auth
    builder.Configuration.Bind("Local", options.ProviderOptions);
});
```

We will also add the [Access Token Scopes](https://docs.microsoft.com/en-us/aspnet/core/security/blazor/webassembly/standalone-with-authentication-library?view=aspnetcore-5.0#access-token-scopes) that we need

```cs
builder.Services.AddOidcAuthentication(options => {
    // Configure your authentication provider options here.
    // For more information, see https://aka.ms/blazor-standalone-auth
    builder.Configuration.Bind("Local", options.ProviderOptions);

    options.ProviderOptions.DefaultScopes.Add("photosrest");
    options.ProviderOptions.DefaultScopes.Add("commentsgrpc"); 
});
```

which requires a

```cs
using Microsoft.Extensions.Configuration;
```

### appsettings.json

Configuration is supplied by the `wwwroot/appsettings.json` file, which we need to add to our `wwwroot` folder. The one on the new project looks like this:

```json
{
  "Local": {
    "Authority": "https:login.microsoftonline.com/",
    "ClientId": "33333333-3333-3333-33333333333333333"
  }
}
```

but we're going to configure our application to talk to our own Identity Server:

```json
{
  "Local": {
    "Authority": "https:localhost:5031/",
    "ClientId": "blazorclient",
    "ResponseType": "code",
  }
}
```

** NOTE: `"ResponseType" : "code"` is necessary because Identity Server uses [the authorization code flow designed for interactive clients](https://identityserver4.readthedocs.io/en/latest/topics/grant_types.html#interactive-clients)**

### Imports file

We need to add a new namespace to the [Imports file](https://docs.microsoft.com/en-us/aspnet/core/security/blazor/webassembly/standalone-with-authentication-library?view=aspnetcore-5.0#imports-file)

- Open `_Imports.razor` and add

```cs
@using Microsoft.AspNetCore.Components.Authorization
```

### App Component

Now open `App.razor` and notice the [App Component](https://docs.microsoft.com/en-us/aspnet/core/security/blazor/webassembly/standalone-with-authentication-library?view=aspnetcore-5.0#app-component) contains new components.

> - The `CascadingAuthenticationState` component manages exposing the `AuthenticationState` to the rest of the app.
> - The `AuthorizeRouteView` component makes sure that the current user is authorized to access a given page or otherwise renders the `RedirectToLogin` component.
> - The `RedirectToLogin` component manages redirecting unauthorized users to the login page.

Our `App.razor` becomes

```html
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(Program).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
                <NotAuthorized>
                    @if (!context.User.Identity.IsAuthenticated) {
                        <RedirectToLogin />
                    } else {
                        <p>You are not authorized to access this resource.</p>
                    }
                </NotAuthorized>
            </AuthorizeRouteView>
        </Found>
        <NotFound>
            <LayoutView Layout="@typeof(MainLayout)">
                <p>Sorry, there's nothing at this address.</p>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>
```

### RedirectToLogin Component

The new project has a [RedirectToLogin Component](https://docs.microsoft.com/en-us/aspnet/core/security/blazor/webassembly/standalone-with-authentication-library?view=aspnetcore-5.0#redirecttologin-component) which we don't have, so let's go tou our `Shared` folder and add a `RedirectToLogin.razor` file with the following code:

```cs
@inject NavigationManager Navigation
@using Microsoft.AspNetCore.Components.WebAssembly.Authentication
@code {
  protected override void OnInitialized()
  {
    Navigation.NavigateTo($"authentication/login?returnUrl={Uri.EscapeDataString(Navigation.Uri)}");
  }
}
```

### LoginDisplay Component

The new project also has a [LoginDisplay component](https://docs.microsoft.com/en-us/aspnet/core/security/blazor/webassembly/standalone-with-authentication-library?view=aspnetcore-5.0#logindisplay-component) which we also need to add to our `Shared` folder. This is the code in the new project, which uses `bootstrap` to style the html:

```html
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.AspNetCore.Components.WebAssembly.Authentication

@inject NavigationManager Navigation
@inject SignOutSessionStateManager SignOutManager

<AuthorizeView>
    <Authorized>
        Hello, @context.User.Identity.Name!
        <button class="nav-link btn btn-link" @onclick="BeginSignOut">Log out</button>
    </Authorized>
    <NotAuthorized>
        <a href="authentication/login">Log in</a>
    </NotAuthorized>
</AuthorizeView>

@code{
    private async Task BeginSignOut(MouseEventArgs args) {
        await SignOutManager.SetSignOutState();
        Navigation.NavigateTo("authentication/logout");
    }
}
```

But we're going to use `MatBlazor`, so our component becomes

```html
<AuthorizeView>
    <Authorized>
        <span class="mat"> Hello, @context.User.Identity.Name!</span>
        <MatButton Unelevated OnClick="BeginSignOut">Log out</MatButton>
    </Authorized>
    <NotAuthorized>
        <MatButton Unelevated Link="authentication/login">Log In</MatButton>
    </NotAuthorized>
</AuthorizeView>

@code{
    private async Task BeginSignOut(MouseEventArgs args) {
        await SignOutManager.SetSignOutState();
        Navigation.NavigateTo("authentication/logout");
    }
}
```

### MainLayout

The `LoginDisplay` component is rendered in the `MainLayout` component of the new project, so let's render it from our `MainLayout` as well, which becomes:

```html
@inherits LayoutComponentBase
<MatThemeProvider Theme="@theme">
  <MatDrawerContainer Style="width: 100vw; height: 100vh;">
    <MatDrawer @bind-Opened="@Opened">
      <NavMenu />
    </MatDrawer>
    <MatDrawerContent>
      <MatAppBarContainer>
        <MatAppBar Fixed="true">
          <MatAppBarRow>
            <MatAppBarSection>
              <MatIconButton Icon="menu" OnClick="ButtonClicked"></MatIconButton>
              <MatAppBarTitle>Photo Sharing Application</MatAppBarTitle>
            </MatAppBarSection>
            <MatAppBarSection Align="@MatAppBarSectionAlign.End">
              <LoginDisplay />
            </MatAppBarSection>
          </MatAppBarRow>
        </MatAppBar>
        <MatAppBarContent>
          <div class="mat">
            @Body
          </div>
        </MatAppBarContent>
      </MatAppBarContainer>
    </MatDrawerContent>
  </MatDrawerContainer>
</MatThemeProvider>
@code
{
  bool Opened = true;

  void ButtonClicked() {
    Opened = !Opened;
  }

  MatTheme theme = new MatTheme() {
    Primary = MatThemeColors.Orange._500.Value,
    Secondary = MatThemeColors.BlueGrey._500.Value
  };
}
```

### Authentication Page

The new project also has an [Authentication Page](https://docs.microsoft.com/en-us/aspnet/core/security/blazor/webassembly/standalone-with-authentication-library?view=aspnetcore-5.0#authentication-component) which we need to add to our project. Under the `Pages` folder, add a new `Authentication.razor` with the following code:

```html
@page "/authentication/{action}"
@using Microsoft.AspNetCore.Components.WebAssembly.Authentication
<RemoteAuthenticatorView Action="@Action" />

@code{
  [Parameter] public string Action { get; set; }
}
```

The configuration is done.

Let's begin by testing if it all still works. Run all four projects.
You should still be able to get the list of all photos and also to get the details, modify and delete a specific photo, but not to post a new photo or a new comment.
This happens because we need to retrieve the access token from IdentityServer and pass it to the services.


## Deny access to the Create Page for Unauthorized users

We can use the [Authorize attribute](https://docs.microsoft.com/en-us/aspnet/core/security/blazor/?view=aspnetcore-5.0#authorize-attribute)

- Add to `_Imports.razor`

```cs 
@using Microsoft.AspNetCore.Authorization
```

Adding the [Authorize Attribute](https://docs.microsoft.com/en-us/aspnet/core/security/blazor/?view=aspnetcore-5.0#authorize-attribute) to our `UploadPhoto`page will take care of redirecting the user to the login page of our Identity Server if the user is not logged on yet.

- Open the `UploadPhoto.razor` component in the `Pages` folder of the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project
- Add the `Authorize` attribute by typing the following code:

```cs
@attribute [Authorize]
```

If you run the application now, you should already see the login page whenever you try to upload a photo. The Upload itself doesn't work yet, though.

## Retrieving the Access Token

We first need to configure our `HttpClient` to [attach the token to the outgoing request](https://docs.microsoft.com/en-us/aspnet/core/security/blazor/webassembly/additional-scenarios?view=aspnetcore-5.0#attach-tokens-to-outgoing-requests).

## Passing the Token to the Service

We're going to follow the instructions described in the [documentation](https://docs.microsoft.com/en-us/aspnet/core/security/blazor/webassembly/additional-scenarios?view=aspnetcore-5.0#httpclient-and-httprequestmessage-with-fetch-api-request-options) and manually add the Authentication header with the Bearer Token retrieved by asking it to the TokenProvider.

### Changing the Infrastructure

We can get the `TokenProvider` using dependency injection.

- Open the `PhotosRepository` in the `Repositories` -> `Rest` folder of the `PhotoSharingApplication.Frontend.Infrastructure` project
- Change the constructor to require an `IAccessTokenProvider` and save the parameter in a readonly field:

```cs
private readonly HttpClient http;
private readonly IAccessTokenProvider tokenProvider;

public PhotosRepository(HttpClient http, IAccessTokenProvider tokenProvider) {
  this.http = http;
  this.tokenProvider = tokenProvider;
}
```

### Rewrite the CreateAsync

The `PostAsJsonAsync` of the `httpClient` object doesn't give us the possibility to add the [authorization header](https://tools.ietf.org/html/rfc6750) that our REST API is expecting. That's why we're going to use the `SendAsync` method instead, where we can pass a `RequestMessage` that we have to buid ourself. We can configure the `RequestMessage` by adding the `Authorization` header with the necessary `Bearer` token.

To get the token, we're going to make use of the `tokenProvider`.

1. Invoke the `RequestAccessToken` method of the `tokenProvider` for the `photosrest` scope
2. Get the token by invoking the `TryGetToken`
3. Create a new `HttpRequestMessage` passing
  - the method (POST)
  - the `URI`
  - the content (our photo, serialized as a JSON object)
4. Set the `Authorization` header of the `requestMessage` to `Bearer`, followed by the value of our token
5. Send the `requestMessage`
6. Return the response deserialized from JSON

```cs
public async Task<Photo> CreateAsync(Photo photo) { 
  var tokenResult = await tokenProvider.RequestAccessToken(new AccessTokenRequestOptions() { Scopes = new string[] { "photosrest" } });
  if (tokenResult.TryGetToken(out var token)) {
    var requestMessage = new HttpRequestMessage() {
      Method = new HttpMethod("POST"),
      RequestUri = new Uri(http.BaseAddress, "/photos"),
      Content = JsonContent.Create(photo)
    };

    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);

    var response = await http.SendAsync(requestMessage);
    return await response.Content.ReadFromJsonAsync<Photo>();
  }
  return null;
}
```

Run the application and try to upload a picture. You should be redirected to the login page of `Identity Server`.
Type `alice` as a User name and `alice` as password.

You should get the `consent` screen, where you're asked to allow access to both the `REST Api` and the `gRPC Server` and to give away your `profile` and `User Identifier` information. Click on `Yes, Allow`.

You should be back to the `Upload` page. Proceed with uploading a picture and verify that the picture shows up in the `AllPhotos` page.

We did it! Our REST service is protected and the Blazor client can get access to it if the user logs in and gives consent. All thanks to Identity Server.

That's great, isn't it?

Now one last thing we may want to do is to show the `Upload Photo` buttons only if the user is allowed to. We could hide the button or hint that the user needs to logon otherwise.

Of course this is not really a security measure, but it gives the user a better experience by clarifying the requirements.

### Modify the Upload NavItem

We can take a look at the `LoginDisplay.razor` component to understand how to conditionally display the html depending on the authorization status.

Let's use an `<AuthorizeView>` component and display 
  - The Link to the `Upload` route if the user is authorized
  - The link to the `Login` route if the user is not authorized

### NavMenu.razor 

- Open `NavMenu.razor` under the `Shared` folder and modify the `html` to look like this:

```html
<MatNavMenu>
    <MatNavItem Href="/photos/all">All Photos <MatIcon Icon="@MatIconNames.List"></MatIcon></MatNavItem>
    <AuthorizeView>
        <Authorized>
            <MatNavItem Href="/photos/upload">Upload Photo <MatIcon Icon="@MatIconNames.Add"></MatIcon></MatNavItem>
        </Authorized>
        <NotAuthorized>
            <MatNavItem Href="/authentication/login">Login to Upload  a Photo <MatIcon Icon="@MatIconNames.Account_circle"></MatIcon></MatNavItem>
        </NotAuthorized>
    </AuthorizeView>
</MatNavMenu>
```

If you run the application you should see the navigation menu item differ depending on the login status.

We're going to use the same trick in the `AllPhotos` page:

```html
<AuthorizeView>
    <Authorized>
        <MatButton Link="photos/upload">Upload new Photo</MatButton>
    </Authorized>
    <NotAuthorized>
        <MatButton Link="authentication/login">Log In to Upload a Photo</MatButton>
    </NotAuthorized>
</AuthorizeView>
```

We have successfully managed to protect the `Upload` of a `Photo`.

## gRPC

Our `gRPC` client is still not sending the token to the service, which means that no new comments can be posted, no matter the login status. 

These are our next steps:

- Change the repository to get the token and add it to the headers of the request
- Change the UI to show the Post button only for authorized users

### The CommentsRepository

The `CreateAsync` of the `serviceClient` object give us the possibility to add the [authorization header](https://tools.ietf.org/html/rfc6750) that our gRPC API is expecting. 

To get the token, we're going to make use of the `tokenProvider`.

We can get the `TokenProvider` using dependency injection.

- Open the `CommentsRepository` in the `Repositories` -> `Grpc` folder of the `PhotoSharingApplication.Frontend.Infrastructure` project
- Change the constructor to require an `IAccessTokenProvider` and save the parameter in a readonly field:

```cs
private readonly CommentsBaseService.CommentsBaseServiceClient serviceClient;
private readonly IAccessTokenProvider tokenProvider;

public CommentsRepository(CommentsBaseService.CommentsBaseServiceClient serviceClient, IAccessTokenProvider tokenProvider) {
  this.serviceClient = serviceClient;
  this.tokenProvider = tokenProvider;
}
```
which requires a 
```cs
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
```

Now change the `CreateAsync` method:

1. Invoke the `RequestAccessToken` method of the `tokenProvider` for the `photosrest` scope
2. Get the token by invoking the `TryGetToken`
3. Create a `MetaData` header
4. Set the `Authorization` header of the `requestMessage` to `Bearer`, followed by the value of our token
5. Send the `createRequest` with the header
6. Return the  Comment with the values contained in the response 

```cs
 public async Task<Comment> CreateAsync(Comment comment) {
  var tokenResult = await tokenProvider.RequestAccessToken(new AccessTokenRequestOptions() { Scopes = new string[] { "photosrest" } });
  if (tokenResult.TryGetToken(out var token)) {
    GrpCore.Metadata headers = new GrpCore.Metadata();
    headers.Add("Authorization", $"Bearer {token.Value}");

    CreateRequest createRequest = new CreateRequest() { PhotoId = comment.PhotoId, Subject = comment.Subject, Body = comment.Body };
    CreateReply c = await serviceClient.CreateAsync(createRequest, headers);
    return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
  }
  return null;
}
```
which requires

```cs
using GrpCore = Grpc.Core;
```

Now we want to display the html conditionally, just like we did for the `Upload` of the Photo.

Just like we did on the `AllPhotos`, we're going to use an `<AuthorizedView>` component.

- Open the `CommentsComponent.razor`
- Change the `html` to look like this (leave the `code` section as it is)

```html
<MatH3>Comments</MatH3>

@if (comments == null) {
  <p><em>No Comments for this Photo</em></p>
} else {
  @foreach (var comment in comments) {
    <CommentComponent CommentItem="comment" ViewMode="CommentComponent.ViewModes.Read" OnUpdate="UpdateComment" OnDelete="DeleteComment"></CommentComponent>
  }
  <AuthorizeView>
    <Authorized>
        <CommentComponent CommentItem="new Comment() {PhotoId = PhotoId}" ViewMode="CommentComponent.ViewModes.Create" OnCreate="CreateComment"></CommentComponent>
    </Authorized>
    <NotAuthorized>
        <MatButton Link="authentication/login">Log In to Comment</MatButton>
    </NotAuthorized>
  </AuthorizeView>
}
```

Now if you run the application you'll see the form to post a comment only if you're authenticated.

If you do post a comment, you'll see the exact same error we had at the end of the prevoius lab. That's because the service on the backend is not writing the user name on the comment, which causes an exception when the service tries to serialize the comments back to the client. So now we need to get the user name on the server.

### gRPC Backend Service

The `Create` method that we override on our `CommentsService` has a `ServerCallContext` parameter. That's where we can find our UserName.

- Open the `CommentsService.cs` under the `Services` folder of the `PhotoSharingApplication.WebServices.Grpc.Comments` project
- Replace the `Create` method with the following code.

```cs
[Authorize]
public override async Task<CreateReply> Create(CreateRequest request, ServerCallContext context) {
  var user = context.GetHttpContext().User;
  Comment c = await commentsService.CreateAsync(new Comment { PhotoId = request.PhotoId, Subject = request.Subject, Body = request.Body, UserName = user.Identity.Name });
  return new CreateReply() { Id = c.Id, PhotoId = c.PhotoId, Body = c.Body, Subject = c.Subject, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()), UserName = c.UserName };
}
```

**NOTE: THIS ONLY WORKS BECAUSE:**
1. On the Identity Server Project Config we added the `JwtClaimTypes.Name` in the `UserClaims` of the `commentsgrpc ApiResource`, which made sure that the User Name is carried in the Access Token emitted by Identity Server
2. In the `startup` we added `options.TokenValidationParameters.NameClaimType = JwtClaimTypes.Name;` in the `AddJwtBearer("Bearer", options =>{ ... })`, which ensures that the `User.Identity.Name` is extracted from the `JwtClaimTypes.Name` instead of the default `ClaimTypes.Name` (the Microsoft type)

If you want to try this, I suggest you delete all the comments you have on your database, then run the applciation and post a comment after logging in. You should finally see the comment in the list of comments of the photo details.

### The REST API

Since we're adding the User Name to the Comments, let's do this also for the Photo. A Web Api Controller can find the User Name in the `User.Identity.Name` property of the Controller itself, so let's use it to complete the information of the uploaded Photo:

- Open the `PhotosController` on the `Controllers` folder of the `PhotoSharingApplication.WebServices.REST.Photos` project
- Modify the `CreateAsync` action as follows:

```cs
[Authorize]
[HttpPost]
public async Task<ActionResult<Photo>> CreateAsync(Photo photo) {
  photo.UserName = User.Identity.Name;
  Photo p = await service.UploadAsync(photo);
  return CreatedAtRoute("Find", p, new { id = p.Id});
}
```

In order to display the name of the user on the `PhotoDetails` component, let's update the user interface as following (leave the `code` section as is)

```html
<MatCard>
  <div>
    <MatHeadline6>
      @Photo.Id - @Photo.Title
    </MatHeadline6>
  </div>
  <MatCardContent>
    <MatCardMedia Wide="true" ImageUrl="@(Photo.PhotoFile == null ? "" : $"data:{Photo.ImageMimeType};base64,{Convert.ToBase64String(Photo.PhotoFile)}")"></MatCardMedia>
    <MatBody2>
      @Photo.Description
    </MatBody2>
    <MatSubtitle2>
      Posted on @Photo.CreatedDate.ToShortDateString() by @Photo.UserName
    </MatSubtitle2>
  </MatCardContent>
  <MatCardActions>
    <MatCardActionButtons>
      @if (Details) {
        <MatButton Link="@($"/photos/details/{Photo.Id}")">Details</MatButton>
      }
      @if (Edit) {
        <MatButton Link="@($"/photos/update/{Photo.Id}")">Update</MatButton>
      }
      @if (Delete) {
        <MatButton Link="@($"/photos/delete/{Photo.Id}")">Delete</MatButton>
      }
      @if (DeleteConfirm) {
        <MatButton OnClick="@(async()=> await OnDeleteConfirmed.InvokeAsync(Photo.Id))">Confirm Deletion</MatButton>
      }
    </MatCardActionButtons>
  </MatCardActions>
</MatCard>
```
Upload a Photo and verify that the user name is correctly added to the Photo information and shown on the UI.

--- 

What we need to do next is to allow updates and deletes only to the photo (or comment) owner.

This is what we're going to do in the next lab, where we're going to learn about `Resource Based Authorization`.

Go to `Labs/Lab11`, open the `readme.md` and follow the instructions thereby contained.   
