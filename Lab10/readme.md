# Security: Authentication and Authorization

We have not implemented any security yet. In this lab we are going to setup and configure a new project that will act as an *Authentication Server*. We will then protect the **Create** operation and we will use the Authentication Server to authenticate the user and issue a *token*, then have the client gain access to the protected operation by using such token.
The Authentication Server that we're going to use implements [OAuth 2.0 and OpenIdConnect](https://www.oauth.com/).

## The Authentication Server

We are going to use [Identity Server 4](https://identityserver4.readthedocs.io/en/latest/index.html)

We are going to use the [Templates](https://github.com/IdentityServer/IdentityServer4.Templates)

**NOTE: At the time of the writing the templates have not yet been updated to .NET core 6, so we're going to use .NET Core 3.1 instead. For more information, check the [GitHub repo](https://github.com/IdentityServer/IdentityServer4.Templates/tree/master/src/IdentityServer4AspNetIdentity)**

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

Right now, both the `IdentityServer` and the `BlazorWebAssembly` projects try to run from port 5001, so let's configure the IdentityServer project to run on a specific port of our choice.

### Identity Server

- In the `Solution Explorer`, right click the `PhotoSharingApplication.IdentityServer` project, select `Properties`
- In the `Properties` window of your project, click on `Debug`
- In the `Profile`, select `SelfHost`
- In the `App Url`, ensure that the value is `https://localhost:5007`
- Save
- Right click the `PhotoSharingApplication.IdentityServer` project, select `Set as Startup Project`
- Click on the green arrow (or press F5) and verify that the project starts from port 5007
- Stop the application

### Multiple Startup Projects
- In the `Solution Explorer`, right click the solution, select `Set Startup Projects`
- On the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project, select `Start`
- On the `PhotoSharingApplication.WebServices.REST.Photos` project, select `Start`
- On the `PhotoSharingApplication.WebServices.Grpc.Comments` project, select `Start`
- On the `PhotoSharingApplication.IdentityServer` project, select `Start`

## Identity Provider Configuration

Now that we have an `IdentityServer` project, we need to cofigure it for our own purposes. 
- We already have two users: *alice* with password *alice* and *bob* with password *bob* (you can find them in the the `QuickStart\TestUsers.cs` file).
- Our [IdentityResource](https://identityserver4.readthedocs.io/en/latest/topics/resources.html) has already been configured, so we don't need to change that in the `Config.cs` file.
- We need to configure two [API Scopes](https://identityserver4.readthedocs.io/en/latest/topics/resources.html#scopes) (the access to the REST Service for the Photos and the gRPC Service for the Comments)
- We need to configure one [client](https://identityserver4.readthedocs.io/en/latest/reference/client.html) (our Blazor application)

### ApiScopes

Open the `Config.cs` file located in the root of your `PhotoSharingApplication.IdentityServer` project.

- Configure the `Photos` ApiScope. 
  - Name it `photosrest`
  - Describe it as `Photos REST Service`
  - Include the `Name` of the user in the access token. We will use the name in a future lab to allow photos update and deletion only to the photo owner.
- Configure the `Comments` ApiScope. 
  - Name it `commentsgrpc`
  - Describe it as `Comments gRPC Service`
  - Include the `Name` of the user in the access token. We will use the name in a future lab to allow comments update and deletion only to the comment owner.

```cs
public static IEnumerable<ApiScope> ApiScopes =>
new ApiScope[]
{
    new ApiScope("commentsgrpc", "Comments gRpc Service") {UserClaims = new string[] { JwtClaimTypes.Name } },
    new ApiScope("photosrest", "Photos REST Service") {UserClaims = new string[] { JwtClaimTypes.Name } },
};
```

which requires 

```cs
using IdentityModel;
```

### ApiResources

We are also going to need two [ApiResources](https://identityserver4.readthedocs.io/en/latest/topics/resources.html#api-resources).  
 - Configure the `Photos` ApiResource. 
  - Name it `photosrest`
  - Describe it as `Photos REST Service`
  - Include the `Name` of the user in the access token. We will use the name in a future lab to allow photos update and deletion only to the photo owner.
  - Add the `photosrest` scope in the `Scopes` property
- Configure the `Comments` ApiResource. 
  - Name it `commentsgrpc`
  - Describe it as `Comments gRPC Service`
  - Include the `Name` of the user in the access token. We will use the name in a future lab to allow comments update and deletion only to the comment owner.
  - Add the `commentsgrpc` scope in the `Scopes` property

```cs
public static IEnumerable<ApiResource> ApiResources =>
new ApiResource[]
{
    new ApiResource("commentsgrpc", "Comments gRpc Service") {UserClaims = new string[] { JwtClaimTypes.Name },Scopes = {"commentsgrpc" } },
    new ApiResource("photosrest", "Photos REST Service") {UserClaims = new string[] { JwtClaimTypes.Name }, Scopes = { "photosrest"} }
};
```

### Client

The third thing we need to configure is the [Blazor Client](https://identityserver4.readthedocs.io/en/latest/topics/clients.html)
- Locate the `Clients` static property.
- Remove every client
- Add a new `Client`
- Set the `ClientId` to `blazorclient`
- Set the `ClientName` will be `Blazor Client`
- Set the `ClientUri` to `http://localhost:5001` 
- Set the `AllowedGrantTypes` to `GrantTypes.Code`
- Set the `RequirePkce` to `true`
- Set the `RequireClientSecret` to `false`
- Set the `RedirectUris` to the [custom app routes used in Blazor](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/additional-scenarios?view=aspnetcore-6.0#customize-app-routes)

```cs
{
    "https://localhost:5001/",
    "https://localhost:5001/authentication/login-callback",
    "https://localhost:5001/authentication/silent",
    "https://localhost:5001/authentication/popup",
},
```

- Set the `PostLogoutRedirectUris` to `https://localhost:5001`
- Set the `AllowedCorsOrigins` to `https://localhost:5001`
- Set the `AllowedScopes` to `{ "openid", "profile", "photosrest", "commentsgrpc" }`

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

### SeedData

The template we chose to generate our `PhotoSharingApplication.IdentityServer` project uses the `Config.cs` just to seed a database. Such db is further used to read the configuration when the server is running.   
Before we can create the db, we need to change the `SeedData.cs` file to include the `ApiResources` and not just the `ApiScopes`.  

Open the `SeedData.cs` file in the root of the IdentityServer project.  
Modify the following code

```cs
if (!context.ApiResources.Any())
{
    Log.Debug("ApiScopes being populated");
    foreach (var resource in Config.ApiScopes.ToList())
    {
        context.ApiScopes.Add(resource.ToEntity());
    }
    context.SaveChanges();
}
else
{
    Log.Debug("ApiScopes already populated");
}
```

as follows:

```cs
if (!context.ApiResources.Any())
{
    Log.Debug("ApiScopes being populated");
    foreach (var resource in Config.ApiScopes.ToList())
    {
        context.ApiScopes.Add(resource.ToEntity());
    }
    context.SaveChanges();
    Log.Debug("ApiResources being populated");
    foreach (var resource in Config.ApiResources.ToList()) {
        context.ApiResources.Add(resource.ToEntity());
    }
    context.SaveChanges();
}
else
{
    Log.Debug("ApiScopes already populated");
}
```

## Create the DataBase

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

In Visual Studio, run the application and test a user login, by navigating to `https://localhost:5007/account/login` and using `alice` / `alice` or `bob` / `bob` as username / password. You should see the user correctly logged on.

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
      options.Authority = "https://localhost:5007";
      options.RequireHttpsMetadata = false;
      options.TokenValidationParameters.NameClaimType = JwtClaimTypes.Name;
      options.Audience = "photosrest";
  });
```

which requires a

```cs
using IdentityModel
```

Locate the `Configure` method in the `Startup` class and add the following code right **BEFORE** the `app.UseEndPoints` line:

```cs
app.UseAuthentication();
app.UseAuthorization();
```


The last step is to protect the `Create` action of our `PhotosController` by using the [Authorize](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/simple?view=aspnetcore-6.0) attribute.

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

If you use the Swagger to invoke the Create action you should get a 401 status code in return. This means your API requires a credential.

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
      options.Authority = "https://localhost:5007";
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

The last step is to protect the `Create` action of our `CommentsService` by using the [Authorize](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/simple?view=aspnetcore-6.0) attribute.

Open your `CommentsService` class, locate the `Create` method and add the `[Authorize]` attribute right before the definition of the method:

```cs
[Authorize]
public override async Task<CreateReply> Create(CreateRequest request, ServerCallContext context) {
    try {
        Comment c = await commentsService.CreateAsync(new Comment { PhotoId = request.PhotoId, Subject = request.Subject, Body = request.Body });
        return new CreateReply() { Id = c.Id, PhotoId = c.PhotoId, Body = c.Body, Subject = c.Subject, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()), UserName = c.UserName };
    } catch (Exception ex){
        throw new RpcException(new Status(StatusCode.Internal, ex.Message));
    }
}
```

which requires a

```cs
using Microsoft.AspNetCore.Authorization;
```

The API is now protected by IdentityServer.

## Configuring the Blazor Client

The last part requires the configuration of our client project.

When we started, we used a Blazor templates with no authentication. This means that we have to add some parts that would already been there if we had chosen the [Standalone with Authentication Library](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/standalone-with-authentication-library?view=aspnetcore-6.0&tabs=visual-studio) template.

My Advice is to create a new project in a completely different folder and grab the code from there, because the template that I'm using at the time of this writing could be different from the one that you would get. Otherwise you could grab the code from my tutorial here. I will assume that you create a new project. We will throw away this new project as soon as we're done transfering the code we need.

### Create a new temporary project

To create a new Blazor WebAssembly project with an authentication mechanism:  
- After choosing the Blazor WebAssembly App template in the Create a new ASP.NET Core Web Application dialog, select Change under Authentication.
- Select *Individual User Accounts* to use ASP.NET Core's Identity system. This selection adds authentication support and doesn't result in storing users in a database. 

Let's start by copying all the differences and tweeking some of the files.

### Index.html

Our starting point is the [Index Page](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/standalone-with-authentication-library?view=aspnetcore-6.0&tabs=visual-studio#index-page). You will notice that the new project has an additional JavaScript file, which we need to add to our `index.html`

```html
<script src="_content/Microsoft.AspNetCore.Components.WebAssembly.Authentication/AuthenticationService.js"></script>
```

This file is included in an [Authentication Package](https://docs.microsoft.com/en-us/aspnet/core/security/blazor/webassembly/standalone-with-authentication-library?view=aspnetcore-5.0#authentication-package), which we also need to add as a NuGet package

- Add the NuGetPackage `Microsoft.AspNetCore.Components.WebAssembly.Authentication`

### Program.cs

We can now proceed to open `Program.cs`. Notice the folowing code for the [Authentication Support](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/standalone-with-authentication-library?view=aspnetcore-6.0&tabs=visual-studio#authentication-service-support) that we're missing:

```cs
builder.Services.AddOidcAuthentication(options =>
{
    // Configure your authentication provider options here.
    // For more information, see https://aka.ms/blazor-standalone-auth
    builder.Configuration.Bind("Local", options.ProviderOptions);
});
```

We will also add the [Access Token Scopes](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/standalone-with-authentication-library?view=aspnetcore-6.0&tabs=visual-studio#access-token-scopes) that we need

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
    "Authority": "https://localhost:5007/",
    "ClientId": "blazorclient",
    "ResponseType": "code",
  }
}
```

** NOTE: `"ResponseType" : "code"` is necessary because Identity Server uses [the authorization code flow designed for interactive clients](https://identityserver4.readthedocs.io/en/latest/topics/grant_types.html#interactive-clients)**

### Imports file

We need to add a new namespace to the [Imports file](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/standalone-with-authentication-library?view=aspnetcore-6.0&tabs=visual-studio#imports-file)

- Open `_Imports.razor` and add

```cs
@using Microsoft.AspNetCore.Components.Authorization
```

### App Component

Now open `App.razor` and notice the [App Component](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/standalone-with-authentication-library?view=aspnetcore-6.0&tabs=visual-studio#app-component) contains new components.

> - The `CascadingAuthenticationState` component manages exposing the `AuthenticationState` to the rest of the app.
> - The `AuthorizeRouteView` component makes sure that the current user is authorized to access a given page or otherwise renders the `RedirectToLogin` component.
> - The `RedirectToLogin` component manages redirecting unauthorized users to the login page.

Our `App.razor` becomes

```html
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(Program).Assembly" PreferExactMatches="@true">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
                <NotAuthorized>
                    @if (!context.User.Identity.IsAuthenticated)
                    {
                        <RedirectToLogin />
                    }
                    else
                    {
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

The new project has a [RedirectToLogin Component](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/standalone-with-authentication-library?view=aspnetcore-6.0&tabs=visual-studio#redirecttologin-component) which we don't have, so let's go tou our `Shared` folder and add a `RedirectToLogin.razor` file with the following code:

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

The new project also has a [LoginDisplay component](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/standalone-with-authentication-library?view=aspnetcore-6.0&tabs=visual-studio#logindisplay-component) which we also need to add to our `Shared` folder. This is the code in the new project, which uses `bootstrap` to style the html:

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
    private async Task BeginSignOut(MouseEventArgs args)
    {
        await SignOutManager.SetSignOutState();
        Navigation.NavigateTo("authentication/logout");
    }
}
```

But we're going to use `MatBlazor`, so our component becomes

```html
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.AspNetCore.Components.WebAssembly.Authentication

@inject NavigationManager Navigation
@inject SignOutSessionStateManager SignOutManager

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

The new project also has an [Authentication Page](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/standalone-with-authentication-library?view=aspnetcore-6.0&tabs=visual-studio#authentication-component) which we need to add to our project. Under the `Pages` folder, add a new `Authentication.razor` with the following code:

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

We can use the [Authorize attribute](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-6.0#authorize-attribute)

- Add to `_Imports.razor`

```cs 
@using Microsoft.AspNetCore.Authorization
```

Adding the [Authorize Attribute](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-6.0#authorize-attribute) to our `UploadPhoto`page will take care of redirecting the user to the login page of our Identity Server if the user is not logged on yet.

- Open the `UploadPhoto.razor` component in the `Pages` folder of the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project
- Add the `Authorize` attribute by typing the following code:

```cs
@attribute [Authorize]
```

If you run the application now, you should already see the login page whenever you try to upload a photo. The Upload itself doesn't work yet, though.

## Retrieving the Access Token

For the Rest client we're going to use the following strategy.  
First of all, we're going to configure two different [Typed HttpClient](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/additional-scenarios?view=aspnetcore-6.0#typed-httpclient)s.  
One, to be used for the read actions, will not attach the token to the requests, since we don't need authorization for those.  
Another one, to be used for the create, update and delete, will attach the token to the request after we will have [configured](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/additional-scenarios?view=aspnetcore-6.0#configure-the-httpclient-handler) it accordingly.    
The `Repository` will in turn use these two typed httpclients.

We first need to add the package `Microsoft.Extensions.Http` to the BlazorWebAssembly project.  
Now we can change the `Program.cs` class.  
You can remove the following code:

```cs
builder.Services.AddScoped(sp => new HttpClient() { BaseAddress = new Uri("https://localhost:44303/") });
```

and replace it with this:

```cs
builder.Services.AddHttpClient<PublicPhotosClient>(client => client.BaseAddress = new Uri("https://localhost:44303/"));

builder.Services.AddHttpClient<ProtectedPhotosClient>(client => client.BaseAddress = new Uri("https://localhost:44303/"))
    .AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
    .ConfigureHandler(authorizedUrls: new[] { "https://localhost:44303/" }, scopes: new[] { "photosrest" }));
```

Both `PublicPhotosClient` and `ProtectedPhotosClient` are custom classes that we need to create.  We will do this in the `Infrastructure` project.

### Typed HttpClients

- In the `PhotoSharingApplication.Frontend.Infrastructure` project, add a reference to the `Microsoft.Extensions.Http` NuGet Package.
- Add a new `TypedHttpClients` folder.  
- In the `TypedHttpClients` folder, add a new `PublicPhotosClient` class  
- In the constructor, add a dependency on the `HttpClient` and save the dependency in a private field
- Copy the code from the `Repository` class necessary to invoke the read actions:

```cs
using PhotoSharingApplication.Shared.Core.Entities;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Infrastructure.TypedHttpClients {
    public class PublicPhotosClient {
        private readonly HttpClient http;
        public PublicPhotosClient(HttpClient http) {
            this.http = http;
        }

        public async Task<Photo> FindAsync(int id) => await http.GetFromJsonAsync<Photo>($"/photos/{id}");

        public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await http.GetFromJsonAsync<List<Photo>>($"/photos");
    }
}
```

- In the `TypedHttpClients` folder, add a new `ProtectedPhotosClient` class  
- In the constructor, add a dependency on the `HttpClient` and save the dependency in a private field
- Copy the code from the `Repository` class necessary to invoke the create, update and delete actions:

```cs
using PhotoSharingApplication.Shared.Core.Entities;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Infrastructure.TypedHttpClients {
    public class ProtectedPhotosClient {
        private readonly HttpClient http;
        public ProtectedPhotosClient(HttpClient http) {
            this.http = http;
        }
        public async Task<Photo> CreateAsync(Photo photo) {
            HttpResponseMessage response = await http.PostAsJsonAsync("/photos", photo);
            return await response.Content.ReadFromJsonAsync<Photo>();
        }
        public async Task<Photo> RemoveAsync(int id) {
            HttpResponseMessage response = await http.DeleteAsync($"/photos/{id}");
            return await response.Content.ReadFromJsonAsync<Photo>();
        }

        public async Task<Photo> UpdateAsync(Photo photo) {
            HttpResponseMessage response = await http.PutAsJsonAsync($"/photos/{photo.Id}", photo);
            return await response.Content.ReadFromJsonAsync<Photo>();
        }
    }
}
```

Now we can use these two classes in the `PhotosRepository` class.  
- Open the `PhotosRepository` located under the `Repositories/Rest` folder of the `PhotoSharingApplication.Frontend.Infrastructure` project
- Modify the constructor to accept a `PublicPhotosClient` and a `ProtectedPhotosClient` instead of `HttpClient`
- Use the `PublicPhotosClient` in the methods to read the photos
- Use the `ProtectedPhotosClient` in the methods to create, update and delete photos.

```cs
using PhotoSharingApplication.Frontend.Infrastructure.TypedHttpClients;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Infrastructure.Repositories.Rest {
  public class PhotosRepository : IPhotosRepository {
    private readonly PublicPhotosClient publicPhotosClient;
    private readonly ProtectedPhotosClient protectedPhotosClient;

    public PhotosRepository(PublicPhotosClient publicPhotosClient, ProtectedPhotosClient protectedPhotosClient) {
      this.publicPhotosClient = publicPhotosClient;
      this.protectedPhotosClient = protectedPhotosClient;
    }
    public async Task<Photo> CreateAsync(Photo photo) => await protectedPhotosClient.CreateAsync(photo);

    public async Task<Photo> FindAsync(int id) => await publicPhotosClient.FindAsync(id);

    public async Task<List<Photo>> GetPhotosAsync(int amount = 10) => await publicPhotosClient.GetPhotosAsync(amount);

    public async Task<Photo> RemoveAsync(int id) => await protectedPhotosClient.RemoveAsync(id);

    public async Task<Photo> UpdateAsync(Photo photo) => await protectedPhotosClient.UpdateAsync(photo);
  }
}
```

### Try it

If you run the application now, you should be able to get the photos and upload a new photo (after you log in using `alice` `alice` as user name and password).

## Configuring the gRpc client

For the gRpc client, we're going to use more or less the same strategy.  
The `Repository` is going to use two different classes, just like we did for the Rest client. The difference will be in how we configure those classes. We will follow the [documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/additional-scenarios?view=aspnetcore-6.0#configure-and-use-grpc-in-components).

Find the configuration of the `Commenter.CommenterClient` in the `Program.cs` of the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project

```cs
builder.Services.AddSingleton(services => {
   var backendUrl = "https://localhost:5005"; // Local debug URL
   var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
   var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions { HttpClient = httpClient });
   return new Commenter.CommenterClient(channel);
});
```

and modify it to return an instance of a `PublicCommenterClient`, which we will create in a later step:

```cs
builder.Services.AddSingleton(services => {
    var backendUrl = "https://localhost:5005"; // Local debug URL
    var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
    var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions { HttpClient = httpClient });
    return new PublicCommentsClient(new Commenter.CommenterClient(channel));
});
```
Then also add a new `ProtectedCommenterClient` class, configured as follows.

```cs
builder.Services.AddScoped(sp => {
    var backendUrl = "https://localhost:5005"; // Local debug URL

    var commentsAuthorizationMessageHandler = sp.GetRequiredService<CommentsAuthorizationMessageHandler>();
    commentsAuthorizationMessageHandler.InnerHandler = new HttpClientHandler();
    var grpcWebHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, commentsAuthorizationMessageHandler);

    var httpClient = new HttpClient(grpcWebHandler);

    var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions { HttpClient = httpClient });

    return new ProtectedCommentsClient(new Commenter.CommenterClient(channel));
});
```

What is the `CommentsAuthorizationMessageHandler` that we're passing to this new configuration? It's a [Custom AuthorizationHandler](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/additional-scenarios?view=aspnetcore-6.0#custom-authorizationmessagehandler-class) that we're going to build.

### Custom Authorization Handler

Create a custom class that extends `AuthorizationMessageHandler` for use as the DelegatingHandler for our HttpClient. In the constructor, invoke the `ConfigureHandler` to configure it to authorize outbound HTTP requests using an access token. The access token will  be only attached if at least one of the authorized URLs is a base of the request URI (`HttpRequestMessage.RequestUri`).

- In the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project, add a new folder `AuthorizationMessageHandlers`
- In the new folder, add a new `CommentsAuthorizationMessageHandler` class
- Extend `AuthorizationMessageHandler`
- In the constructor, invoke the `ConfigureHandler` to configure it to authorize outbound HTTP requests using an access token.

```cs
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace PhotoSharingApplication.Frontend.BlazorWebAssembly.AuthorizationMessageHandlers {
    public class CommentsAuthorizationMessageHandler : AuthorizationMessageHandler {
        public CommentsAuthorizationMessageHandler(IAccessTokenProvider provider,
            NavigationManager navigationManager)
            : base(provider, navigationManager) {
            ConfigureHandler(
                authorizedUrls: new[] { "https://localhost:5005/photos" },
                scopes: new[] { "commentsgrpc" });
        }
    }
}
```
- Register the handler in the `Program.cs` file

```cs
builder.Services.AddScoped<CommentsAuthorizationMessageHandler>();
```

### PublicCommentsClient

- In the `PhotoSharingApplication.Frontend.Infrastructure` project, under the `TypedHttpClients` folder, add a new `PublicCommentsClient` class
- In the constructor, accept an instance of `HttpClient` and save it in a private field
- Copy the methods of the `CommentsRepository` necessary to read the comments

```cs
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.WebServices.Grpc.Comments;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Infrastructure.TypedHttpClients {
    public class PublicCommentsClient {
        private readonly Commenter.CommenterClient gRpcClient;

        public PublicCommentsClient(Commenter.CommenterClient gRpcClient) {
            this.gRpcClient = gRpcClient;
        }

        public async Task<Comment> FindAsync(int id) {
            FindReply c = await gRpcClient.FindAsync(new FindRequest() { Id = id });
            return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
        }

        public async Task<List<Comment>> GetCommentsForPhotoAsync(int photoId) {
            GetCommentsForPhotosReply resp = await gRpcClient.GetCommentsForPhotoAsync(new GetCommentsForPhotosRequest() { PhotoId = photoId });
            return resp.Comments.Select(c => new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() }).ToList();
        }
    }
}
```

### ProtectedCommentsClient

- In the `PhotoSharingApplication.Frontend.Infrastructure` project, under the `TypedHttpClients` folder, add a new `ProtectedCommentsClient` class
- In the constructor, accept an instance of `HttpClient` and save it in a private field
- Copy the methods of the `CommentsRepository` necessary to create, update and delete the comments

```cs
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.WebServices.Grpc.Comments;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Infrastructure.TypedHttpClients {
    public class ProtectedCommentsClient {
        private readonly Commenter.CommenterClient gRpcClient;

        public ProtectedCommentsClient(Commenter.CommenterClient gRpcClient) {
            this.gRpcClient = gRpcClient;
        }

        public async Task<Comment> CreateAsync(Comment comment) {
            CreateRequest createRequest = new CreateRequest() { PhotoId = comment.PhotoId, Subject = comment.Subject, Body = comment.Body };
            CreateReply c = await gRpcClient.CreateAsync(createRequest);
            return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
        }
        public async Task<Comment> RemoveAsync(int id) {
            RemoveReply c = await gRpcClient.RemoveAsync(new RemoveRequest() { Id = id });
            return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
        }

        public async Task<Comment> UpdateAsync(Comment comment) {
            UpdateReply c = await gRpcClient.UpdateAsync(new UpdateRequest { Id = comment.Id, Subject = comment.Subject, Body = comment.Body });
            return new Comment { Id = c.Id, PhotoId = c.PhotoId, UserName = c.UserName, Subject = c.Subject, Body = c.Body, SubmittedOn = c.SubmittedOn.ToDateTime() };
        }
    }
}
```

### The Repository

Now we can modify the Repository to let it use the new classes we just created.
- Open the `CommentsRepository` class located in the `Repositories/Grpc` folder of the `PhotoSharingApplication.Frontend.Infrastructure` project
- Modify the constructor to accept an instance of `PublicCommentsClient` and one of `ProtectedCommentsClient`, saving both in two private fields
- Modify the methods to make use of those two fields instead of the `HttpClient`

```cs
using PhotoSharingApplication.Frontend.Infrastructure.TypedHttpClients;
using PhotoSharingApplication.Shared.Core.Entities;
using PhotoSharingApplication.Shared.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoSharingApplication.Frontend.Infrastructure.Repositories.Grpc {
    public class CommentsRepository : ICommentsRepository {
        private readonly PublicCommentsClient publicCommentsClient;
        private readonly ProtectedCommentsClient protectedCommentsClient;

        public CommentsRepository(PublicCommentsClient publicCommentsClient, ProtectedCommentsClient protectedCommentsClient) {
            this.publicCommentsClient = publicCommentsClient;
            this.protectedCommentsClient = protectedCommentsClient;
        }
        public async Task<Comment> CreateAsync(Comment comment) => await protectedCommentsClient.CreateAsync(comment);

        public async Task<Comment> FindAsync(int id) => await publicCommentsClient.FindAsync(id);

        public async Task<List<Comment>> GetCommentsForPhotoAsync(int photoId) => await publicCommentsClient.GetCommentsForPhotoAsync(photoId);

        public async Task<Comment> RemoveAsync(int id) => await protectedCommentsClient.RemoveAsync(id);

        public async Task<Comment> UpdateAsync(Comment comment) => await protectedCommentsClient.UpdateAsync(comment);
    }
}
```

## Try it

- Start your 4 projects
- Login to IdentityServer
- Go to the Details of a Photo
- Add a new Comment

You should see that the new comment appears under the details of the Photo.
If you don't log on first, you should see an error.

## UI

Let's show the `Upload Photo` buttons only if the user is allowed to. We could hide the button or hint that the user needs to logon otherwise.

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

We also want to conditionally display the html to add a comment, just like we did for the `Upload` of the Photo.

Once again,  we're going to use an `<AuthorizedView>` component.

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

## Adding the User.Name to Photos and Comments

So now we need to get the user name on the server, on both the Rest API, so that we can add it to a new Photo, and the gRpc service, so that we can add it to a new comment.

### The REST API

A Web Api Controller can find the User Name in the `User.Identity.Name` property of the Controller itself, so let's use it to complete the information of the uploaded Photo:

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
    <PhotoPictureComponent Photo="Photo"></PhotoPictureComponent>
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

Run the application and post a comment after logging in. You should see that the new comment includes the user name that posted it.

--- 

What we need to do next is to allow updates and deletes only to the photo (or comment) owner.

This is what we're going to do in the next lab, where we're going to learn about `Resource Based Authorization`.

Go to `Labs/Lab11`, open the `readme.md` and follow the instructions thereby contained.   
