# Security: Authentication and Authorization

We have not implemented any security yet. In this lab we are going to setup and configure a new project that will act as an *Authentication Server*. We will then protect the **Create** operation and we will use the Authentication Server to authenticate the user and issue a *token*, then have the client gain access to the protected operation by using such token.
The Authentication Server that we're going to use implements [OAuth 2.0 and OpenIdConnect](https://www.oauth.com/).

[Duende](https://docs.duendesoftware.com/identityserver/v6/overview/) is going to be our Authentication Server.

We are going to use its [Templates](https://docs.duendesoftware.com/identityserver/v6/quickstarts/0_overview/#preparation).  
Specifically, the [ASP.NET Core Identity](https://docs.duendesoftware.com/identityserver/v6/quickstarts/5_aspnetid/#new-project-for-aspnet-core-identity) template, which contains the UI to login and logout and a couple of example users stored in a Sqlite database. This template stores the configuration in memory, which for our purpose is more than enough. Feel free to explore the other templates and eventually [use EntityFramework Core for configuration and operational data](https://docs.duendesoftware.com/identityserver/v6/quickstarts/4_ef/) instead.   

- We will protect our [resources](https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/) (both our Photos REST Api and our Comments gRpc Service) by
  - [Defining API Scopes](https://docs.duendesoftware.com/identityserver/v6/quickstarts/1_client_credentials/#defining-an-api-scope)
  - [Adding JWT Bearer Authentication](https://docs.duendesoftware.com/identityserver/v6/quickstarts/1_client_credentials/#add-jwt-bearer-authentication)
  - [Using the Authorize attribute](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/simple?view=aspnetcore-7.0)
- We will configure our client (the Backend For Frontend) by:
  - [Adding an interactive client](https://docs.duendesoftware.com/identityserver/v6/quickstarts/2_interactive/)
- We will protect our Frontend by
  - [Using the Duende.BFF security framework](https://docs.duendesoftware.com/identityserver/v6/quickstarts/7_blazor/), specifically integrating it with [YARP](https://docs.duendesoftware.com/identityserver/v6/bff/apis/yarp/)

## The Authentication Server

- Open a command prompt and navigate to your solution folder (or `Labs\Lab10\Start\PhotoSharingApplication` if did not follow the previous labs)
- If you haven't installed the Duende templates yet, do it by typing the following command:

```
dotnet new install Duende.IdentityServer.Templates
```

- Create a Duende project that uses ASPNET Core Identity by typing the following command, as described in the [tutorial](https://docs.duendesoftware.com/identityserver/v6/quickstarts/5_aspnetid/): 

```
dotnet new isaspid -n PhotoSharingApplication.IdentityProvider
```

- When asked `Do you want to run this action (Y|N)?` type `Y` and press `Enter`. This will create a db and seed it with the example users.
- Open your solution in Visual Studio
- In the `Solution Explorer`, right click on the solution, select `Add` -> `Existing Project`
- Select the `PhotoSharingApplication\PhotoSharingApplication.IdentityProvider\PhotoSharingApplication.IdentityProvider.csproj` file.

 If you want to browse the SqlLite db created by the template from within Visual Studio, you will need a Visual Studio Extension. 
- In Visual Studio, select `Tools -> Extension And Updates`, 
- Select `Online` 
- Search for `SqLite`. 
- Select the `SQLite / SQL Server Compact Toolbox` 
- Click on `Download`. 
- When the download completes, close Visual Studio to start the installation. 
- When the installation complets, go back to Visual Studio
- Click on `View -> Other Windows -> SQLite / SQL Compact Toolbox`, 
- Click on the `Add SQLite / SQL Compact from current solution` button. 
- You should see a `AspIdUsers.db` Database.
  - Feel free to explore its structure and content. 

## Configuration of the http ports

Right now, both the `IdentityProvider` and the `Frontend.Server` projects try to run from port 5001, so let's configure the IdentityProvider project to run on a specific port of our choice.

### Identity Provider

- In the `Solution Explorer`, right click the `PhotoSharingApplication.IdentityProvider` project, select `Properties`
- In the `Properties` window of your project, click on `Debug`
- In the `Profile`, select `SelfHost`
- In the `App Url`, ensure that the value is `https://localhost:5007`
- Save
- Right click the `PhotoSharingApplication.IdentityProvider` project, select `Set as Startup Project`
- Click on the green arrow (or press F5) and verify that the project starts from port 5007
- Stop the application

### Multiple Startup Projects
- In the `Solution Explorer`, right click the solution, select `Set Startup Projects`
- On the `PhotoSharingApplication.Frontend.Server` project, select `Start`
- On the `PhotoSharingApplication.WebServices.Rest.Photos` project, select `Start`
- On the `PhotoSharingApplication.WebServices.Grpc.Comments` project, select `Start`
- On the `PhotoSharingApplication.IdentityProvider` project, select `Start`

## Identity Provider Configuration

Now that we have an `IdentityProvider` project, we need to cofigure it for our own purposes. 
- We already have two users: *alice* and *bob* with password *Pass123$* (you can find them in the the `SeedData.cs` file).
- Our [IdentityResource](https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/identity/) has already been configured, so we don't need to change that in the `Config.cs` file.
- We need to configure two [API Scopes](https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes/) (the access to the REST Service for the Photos and the gRPC Service for the Comments)
- We need to configure one [client](https://docs.duendesoftware.com/identityserver/v6/fundamentals/clients/) (our Blazor BFF application)

### ApiScopes

Open the `Config.cs` file located in the root of your `PhotoSharingApplication.IdentityProvider` project.

- Configure the `Photos` ApiScope. 
  - Name it `photosrest`
  - Describe it as `Photos REST Service`
  - Include the `Name` of the user in the access token. We will use the name in a future lab to allow photos update and deletion only to the photo owner.
- Configure the `Comments` ApiScope. 
  - Name it `commentsgrpc`
  - Describe it as `Comments gRPC Service`
  - Include the `Name` of the user in the access token. We will use the name in a future lab to allow comments update and deletion only to the comment owner.

So replace this code
```cs
public static IEnumerable<ApiScope> ApiScopes =>
    new ApiScope[]
    {
        new ApiScope("scope1"),
        new ApiScope("scope2"),
    };
```

with this code

```cs
public static IEnumerable<ApiScope> ApiScopes =>
    new ApiScope[]
    {
        new ApiScope("photosrest") { UserClaims = new string[] { JwtClaimTypes.Name }},
        new ApiScope("commentsgrpc") { UserClaims = new string[] { JwtClaimTypes.Name }},
    };
```

### Client

The third thing we need to configure is the [ASP.NET Interactive Client](https://docs.duendesoftware.com/identityserver/v6/quickstarts/2_interactive/#register-an-oidc-client)
- Locate the `Clients` static property.
- Remove every client
- Add a new `Client`
- Set the `ClientId` to `photosharing.bff`
- Set the `ClientSecret` to a New Guid (you can generate one in Visual Studio by selecting the Tools -> Create GUID menu item)
- Set the `AllowedGrantTypes` to `GrantTypes.Code`
- Set the `RedirectUris` to `https://localhost:5001/signin-oidc`
- Set the `FrontChannelLogoutUri` to `https://localhost:5001/signout-oidc`
- Set the `PostLogoutRedirectUris` to `https://localhost:5001/signout-callback-oidc`
- Set the `AllowOfflineAccess` to `true`
- Set the `AllowedScopes` to `{ "openid", "profile", "photosrest", "commentsgrpc" }`

The `Clients` property should look like the following:

```cs
public static IEnumerable<Client> Clients =>
  new Client[]
  {
      // interactive client using code flow + pkce
      new Client
      {
          ClientId = "photosharing.bff",
          ClientSecrets = { new Secret("A9B27D26-E71C-4C53-89A8-3DAB53CE1854".Sha256()) }, //generate your own GUID

          AllowedGrantTypes = GrantTypes.Code,

          RedirectUris = { "https://localhost:5001/signin-oidc" },
          FrontChannelLogoutUri = "https://localhost:5001/signout-oidc",
          PostLogoutRedirectUris = { "https://localhost:5001/signout-callback-oidc" },

          AllowOfflineAccess = true,
          AllowedScopes = { "openid", "profile", "photosrest", "commentsgrpc" }
      },
  };
```

This project is also configured to use Google Authentication. We can remove it because we're not going to use it.

Open the `HostingExtensions.cs` and remove the `AddGoogle` call. This code

```cs
builder.Services.AddAuthentication()
  .AddGoogle(options =>
  {
      options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

      // register your IdentityServer with Google at https://console.developers.google.com
      // enable the Google+ API
      // set the redirect URI to https://localhost:5001/signin-google
      options.ClientId = "copy client ID from Google here";
      options.ClientSecret = "copy client secret from Google here";
  });
```

simply becomes

```cs
services.AddAuthentication();
```

In Visual Studio, run the application and test a user login, by navigating to `https://localhost:5007/account/login` and using `alice` / `Pass123$` or `bob` / `Pass123$` as username / password. You should see the user correctly logged on.

## Configuring the REST Service

We can now switch to our Web Api project. We need to:
- Configure it to use Duende
- Protect the access to the `Create` action to allow only authenticated users

As explained in the [Protecting an API](https://docs.duendesoftware.com/identityserver/v6/quickstarts/1_client_credentials/#create-an-api-project) tutorial, we need to configure the API.

- Add the following package to the `PhotoSharingApplication.WebServices.Rest.Photos` project:
  - `Microsoft.AspNetCore.Authentication.JwtBearer`

We need to add the authentication services to DI and the authentication middleware to the pipeline. These will:

- validate the incoming token to make sure it is coming from a trusted issuer
- validate that the token is valid to be used with this api (aka scope)

We need to add the authentication services to DI and configure "Bearer" as the default scheme. We can do that thanks to the `AddAuthentication` extension method. We then also have to add the JwtBearer access token validation handler into DI for use by the authentication services, throught the invocation of the `AddJwtBearer` extension method, to which we have to configure the `Authority` (which is the http address of our Identity Provider) .

Open your `Program.cs`, add the following code before building the app:

```cs
builder.Services.AddAuthentication("Bearer")
.AddJwtBearer("Bearer", options => {
    options.Authority = "https://localhost:5007";

    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateAudience = false,
        NameClaimType = "name"
    };
});
```

which requires a

```cs
using Microsoft.IdentityModel.Tokens;
```

Add the following code right **BEFORE** the `app.UseAuthorization` line:

```cs
app.UseAuthentication();
```


The last step is to protect the `Create` action of our `PhotosController` by using the [Authorize](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/simple?view=aspnetcore-7.0) attribute.

Open your `PhotosController` class, locate the `CreateAsync` method and add the `[Authorize]` attribute right before the definition of the method:

```cs
[Authorize]
[HttpPost]
public async Task<ActionResult<Photo>> CreateAsync(Photo photo) {
    Photo? p = await service.UploadAsync(photo);
    return CreatedAtRoute("Find", new { id = p.Id }, p);
}
```

which requires a

```cs
using Microsoft.AspNetCore.Authorization;
```

If you use the Swagger to invoke the Create action you should get a 401 status code in return. This means your API requires a credential.

The API is now protected by Duende.

## Configuring the gRPC Service

The steps to configure our gRPC Service are nearly identical to the ones we took for the REST Service, with just minor tweeks here and there. 

We need to:
- Configure it to use our Identity Provider
- Protect the access to the `Create` action to allow only authenticated users

- Add the following package to the `PhotoSharingApplication.WebServices.Grpc.Comments` project:
  - `Microsoft.AspNetCore.Authentication.JwtBearer`

Open your `Program.cs` and add the following code, before the building of the app:

```cs
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options => {
        options.Authority = "https://localhost:5007";

        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateAudience = false,
            NameClaimType = "name"
        };
    });
builder.Services.AddAuthorization();
```

which requires

```cs
using Microsoft.IdentityModel.Tokens;
```
- Add the following code right **BEFORE** the `app.MapGrpcService` line:

```cs
app.UseAuthentication();
app.UseAuthorization();
```

The last step is to protect the `Create` action of our `CommentsGrpcService` by using the [Authorize](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/simple?view=aspnetcore-7.0) attribute.

Open your `CommentsGrpcService` class, locate the `Create` method and add the `[Authorize]` attribute right before the definition of the method:

```cs
[Authorize]
public override async Task<CreateReply> Create(CreateRequest request, ServerCallContext context) {
    try {
        Comment c = await commentsService.CreateAsync(new Comment { PhotoId = request.PhotoId, Subject = request.Subject, Body = request.Body });
        return new CreateReply() { Id = c.Id, PhotoId = c.PhotoId, Body = c.Body, Subject = c.Subject, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()), UserName = c.UserName };
    } catch (Exception ex) {
        throw new RpcException(new Status(StatusCode.Internal, ex.Message));
    }
}
```

which requires a

```cs
using Microsoft.AspNetCore.Authorization;
```

The API is now protected by Duende.

## Configuring the Blazor BFF

The last part requires the configuration of our client project, which is split in two: the ASP.NET Server Host (the BFF) and the Blazor WebAssembly project (the Frontend client).

When we started, we used a Blazor templates with no authentication. This means that we have to add some parts that would already been there if we had chosen the [Standalone with Authentication Library](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/standalone-with-authentication-library?view=aspnetcore-7.0&tabs=visual-studio) template.

Luckily for us, we can steal the code described in the [Building Blazor WASM client applications Tutorial](https://docs.duendesoftware.com/identityserver/v6/quickstarts/7_blazor/) or even directly from the [github repo of the BFF Blazor Wasm Sample](https://github.com/DuendeSoftware/Samples/tree/main/IdentityServer/v6/BFF/BlazorWasm)


### The Frontend.Server Project
- Add the following packages:
    - Microsoft.AspNetCore.Authentication.OpenIdConnect 
    - Duende.BFF

As described in the documentation:

> Next, we will add OpenID Connect and OAuth support to the backend. For this we are adding the Microsoft OpenID Connect authentication handler for the protocol interactions with the token service, and the cookie authentication handler for managing the resulting authentication session. See here for more background information.
>  
> The BFF services provide the logic to invoke the authentication plumbing from the frontend (more about this later).
>  
> Add the following snippet to your Program.cs above the call to builder.Build();

```cs
builder.Services.AddBff();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "cookie";
    options.DefaultChallengeScheme = "oidc";
    options.DefaultSignOutScheme = "oidc";
})
.AddCookie("cookie", options => {
    options.Cookie.Name = "__Host-blazor";
    options.Cookie.SameSite = SameSiteMode.Strict;
})
.AddOpenIdConnect("oidc", options => {
    options.Authority = "https://localhost:5007";

    options.ClientId = "photosharing.bff";
    options.ClientSecret = "A9B27D26-E71C-4C53-89A8-3DAB53CE1854"; // replace this with the GUID you generated as `ClientSecrets` in the Config.cs file of the IdentityProvider project
    options.ResponseType = "code";
    options.ResponseMode = "query";

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("photosrest");
    options.Scope.Add("commentsgrpc");
    options.Scope.Add("offline_access");

    options.MapInboundClaims = false;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.SaveTokens = true;
});
```

- Add the required middleware for authentication, authorization and BFF session management. Add the following snippet after the call to UseRouting:

```cs
app.UseAuthentication();
app.UseBff();
app.UseAuthorization();
app.MapBffManagementEndpoints();
```

- Since we already use YARP, we're going to follow the [documentation](https://docs.duendesoftware.com/identityserver/v6/bff/apis/yarp/) to configure Duende with YARP.

- To enable our YARP integration, add a reference to the `Duende.BFF.Yarp` Nuget package and add the YARP and our service to DI by replacing this code:

```cs
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
```

with this code:

```cs
builder
    .Services
    .AddReverseProxy()
    .AddTransforms<AccessTokenTransformProvider>()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddBffExtensions();
```

which requires

```cs
using Duende.Bff.Yarp;
```

- Add the YARP endpoint to the routing table by replacing this code

```cs
app.MapReverseProxy();
```

with this code

```cs
app.MapBffReverseProxy();
```

### The Frontend.Client

Now let's add authentication and authorization to our WebAssembly project, the `Fontend.Client`

- Add the `Microsoft.AspNetCore.Components.WebAssembly.Authentication` authentication/authorization related package to both the `Frontend.Client` and the `BlazorComponents` projects
- Add a using statement to `_Imports.razor` to bring the above package in scope in both the `Frontend.Client` and the `BlazorComponents` projects

```cs
@using Microsoft.AspNetCore.Components.Authorization
```
- To propagate the current authentication state to all pages in your Blazor client, you add a special component called `CascadingAuthenticationState` to your application. This is done by wrapping the Blazor router with that component in `App.razor`:

```html
<CascadingAuthenticationState>
<Router AppAssembly="@typeof(App).Assembly"  AdditionalAssemblies="new[] { typeof(MainLayout).Assembly }">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
        <FocusOnNavigate RouteData="@routeData" Selector="h1" />
    </Found>
    <NotFound>
        <PageTitle>Not found</PageTitle>
        <LayoutView Layout="@typeof(MainLayout)">
            <p role="alert">Sorry, there's nothing at this address.</p>
        </LayoutView>
    </NotFound>
</Router>
</CascadingAuthenticationState>
```
- Add conditional rendering to the navigation menu to be able to trigger login/logout as well as displaying the current user name when logged in. This is achieved by using the `AuthorizeView` component.  Open the `NavMenu.razor` component located under the `Shared` folder of the `BlazorComponents` project and replace its content with

```html
<MudNavMenu>
    <MudNavLink Href="/photos/all" Icon="@Icons.Material.Filled.List">All Photos</MudNavLink>
    <AuthorizeView>
        <Authorized>
            <MudNavLink Href="/photos/upload" Icon="@Icons.Material.Filled.AddAPhoto">Add Photo</MudNavLink>
            <MudText Typo="Typo.button">Hello, @context.User.Identity.Name!</MudText>
            <MudNavLink Icon="@Icons.Material.Filled.Logout" Href="@context.User.FindFirst("bff:logout_url")?.Value" />
        </Authorized>
        <NotAuthorized>
            <MudSpacer />
            <MudNavLink Icon="@Icons.Material.Filled.Login" Href="bff/login" />
        </NotAuthorized>
    </AuthorizeView>
</MudNavMenu>
```
If you want, you can also personalize the AppBar in `MainLayou.razor` like so:

```html
<MudAppBar Elevation="1">
    <MudIconButton Icon="@Icons.Material.Filled.Menu" Color="Color.Inherit" Edge="Edge.Start" OnClick="@ToggleDrawer" />
    <MudSpacer />
    <AuthorizeView>
        <Authorized>
            <MudIconButton Color="Color.Inherit" Icon="@Icons.Material.Filled.Logout" Href="@context.User.FindFirst("bff:logout_url")?.Value" />
        </Authorized>
        <NotAuthorized>
            <MudIconButton Color="Color.Inherit" Icon="@Icons.Material.Filled.Login" Href="bff/login" />
        </NotAuthorized>
    </AuthorizeView>
</MudAppBar>
```

`CascadingAuthenticationState` is an abstraction over an arbitrary authentication system. It internally relies on a service called `AuthenticationStateProvider` to return the required information about the current authentication state and the information about the currently logged on user.

This component needs to be implemented, and that’s what we’ll do next.

The Duende BFF library has a server-side component that allows querying the current authentication session and state (see [here](https://docs.duendesoftware.com/identityserver/v6/bff/session/management/#user)). We will now add a Blazor `AuthenticationStateProvider` that will internally use this endpoint. You can find the source code on the [official github Duende Blazor Wasm BFF sample](https://github.com/DuendeSoftware/Samples/blob/main/IdentityServer/v6/BFF/BlazorWasm/Client/BFF/BffAuthenticationStateProvider.cs)

- In the `Frontend.Client` project, add a new `DuendeAuth` folder.  
- In the new folder, add a `BffAuthenticationStateProvider` class  with the following content:

```cs
using Microsoft.AspNetCore.Components.Authorization;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;

namespace PhotoSharingApplication.Frontend.Client.DuendeAuth;

public class BffAuthenticationStateProvider : AuthenticationStateProvider {
    private static readonly TimeSpan UserCacheRefreshInterval
        = TimeSpan.FromSeconds(60);

    private readonly HttpClient _client;
    private readonly ILogger<BffAuthenticationStateProvider> _logger;

    private DateTimeOffset _userLastCheck
        = DateTimeOffset.FromUnixTimeSeconds(0);
    private ClaimsPrincipal _cachedUser
        = new ClaimsPrincipal(new ClaimsIdentity());

    public BffAuthenticationStateProvider(
        HttpClient client,
        ILogger<BffAuthenticationStateProvider> logger) {
        _client = client;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync() {
        return new AuthenticationState(await GetUser());
    }

    private async ValueTask<ClaimsPrincipal> GetUser(bool useCache = true) {
        var now = DateTimeOffset.Now;
        if (useCache && now < _userLastCheck + UserCacheRefreshInterval) {
            _logger.LogDebug("Taking user from cache");
            return _cachedUser;
        }

        _logger.LogDebug("Fetching user");
        _cachedUser = await FetchUser();
        _userLastCheck = now;

        return _cachedUser;
    }

    record ClaimRecord(string Type, object Value);

    private async Task<ClaimsPrincipal> FetchUser() {
        try {
            _logger.LogInformation("Fetching user information.");
            var response = await _client.GetAsync("bff/user?slide=false");

            if (response.StatusCode == HttpStatusCode.OK) {
                var claims = await response.Content.ReadFromJsonAsync<List<ClaimRecord>>();

                var identity = new ClaimsIdentity(
                    nameof(BffAuthenticationStateProvider),
                    "name",
                    "role");

                foreach (var claim in claims) {
                    identity.AddClaim(new Claim(claim.Type, claim.Value.ToString()));
                }

                return new ClaimsPrincipal(identity);
            }
        } catch (Exception ex) {
            _logger.LogWarning(ex, "Fetching user failed.");
        }

        return new ClaimsPrincipal(new ClaimsIdentity());
    }
}
```

and register it in the client's `Program.cs` file:

```cs
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, BffAuthenticationStateProvider>();
```

which requires:

```cs
using Microsoft.AspNetCore.Components.Authorization;
using PhotoSharingApplication.Frontend.Client.DuendeAuth;
```

To properly secure the call, you need to add a static X-CSRF header to the call.

This can be easily accomplished by a delegating handler that can be plugged into the default HTTP client used by the Blazor frontend. Again, the source code can be found on the [github repo](https://github.com/DuendeSoftware/Samples/blob/main/IdentityServer/v6/BFF/BlazorWasm/Client/BFF/AntiforgeryHandler.cs). Let's first add the handler:

- In the `DuendeAuth` folder, add a new `AntiForgeryHandler` class with the following content:

```cs
namespace PhotoSharingApplication.Frontend.Client.DuendeAuth;

public class AntiforgeryHandler : DelegatingHandler {
    public AntiforgeryHandler() { }
    public AntiforgeryHandler(HttpClientHandler innerHandler) : base(innerHandler) { }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
        request.Headers.Add("X-CSRF", "1");
        return base.SendAsync(request, cancellationToken);
    }
}
```

and register it in the client's `Program.cs` by replacing this line:

```cs
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
```
with

```cs
builder.Services.AddTransient<AntiforgeryHandler>();

builder.Services.AddHttpClient("backend", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<AntiforgeryHandler>();
builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("backend"));
```

and replacing this code:

```cs
builder.Services.AddSingleton(services => {
    var backendUrl = new Uri(builder.HostEnvironment.BaseAddress);
    var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions {
        HttpHandler = new GrpcWebHandler(new HttpClientHandler())
    });
    return new Commenter.CommenterClient(channel);
});
```

with this:

```cs
builder.Services.AddSingleton(services => {
    var backendUrl = new Uri(builder.HostEnvironment.BaseAddress);
    var channel = GrpcChannel.ForAddress(backendUrl, new GrpcChannelOptions {
        HttpHandler = new GrpcWebHandler(new AntiforgeryHandler(new HttpClientHandler())),
    });
    return new Commenter.CommenterClient(channel);
});
```

Which requires you to install the `Microsoft.Extensions.Http` NuGet Package.

## Retrieving the Access Token

As explained in the [Duende](https://docs.duendesoftware.com/identityserver/v6/bff/apis/yarp/#token-management) tutorial, we need to add an entry to YARP’s metadata dictionary to instruct our plumbing to forward the current user access token for the route:
- Open the `appsettings.json` file in the `PhotoSharingApplication.Frontend.Server` project
- Replace this code

```json
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
}
```

with this:

```json
"Routes": {
    "photosrestroute": {
        "ClusterId": "photosrestcluster",
        "Match": {
            "Path": "/photos/{*any}"
        },
        "Metadata": {
            "Duende.Bff.Yarp.TokenType": "User"
        }
    },
    "commentsgrpcroute": {
        "ClusterId": "commentsgrpccluster",
        "Match": {
            "Path": "/comments.Commenter/{*any}"
        },
        "Metadata": {
            "Duende.Bff.Yarp.TokenType": "User"
        }
    }
}
```

The configuration is done.

Let's begin by testing if it all still works. Run all four projects.
You should still be able to get the list of all photos and also to get the details, modify and delete a specific photo, but not to post a new photo or a new comment, even if you navigate to `photos/upload`.
If you login, though, using `alice` or `bob` and `Pass123$`, you should be able to add a photo and / or a comment.

The user experience is less than ideal, though, since they don't get warned upfront that they are not authorized to perform specific activities unless they log in first. Let's proceed to give them a better experience.  

## Deny access to the Create Page for Unauthorized users

We can use the [Authorize attribute](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-7.0#authorize-attribute)

- Add to `_Imports.razor`

```cs 
@using Microsoft.AspNetCore.Authorization
```

- Open the `UploadPhoto.razor` component in the `Pages` folder of the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project
- Add the `Authorize` attribute by typing the following code:

```cs
@attribute [Authorize]
```
Then we can wrap the ui in an `AuthorizeView` component, showing a login button if the user is not authorized to upload a photo:

```html
<PageTitle>Upload Photo</PageTitle>

<AuthorizeView>
    <Authorized>
        <PhotoEditComponent Photo="photo" OnSave="Upload"></PhotoEditComponent>
    </Authorized>
    <NotAuthorized>
        <MudButton Variant="Variant.Filled" EndIcon="@Icons.Material.Filled.Login" Color="Color.Error" Href="bff/login">You are not authorized. Log in to upload a picture</MudButton>
    </NotAuthorized>
</AuthorizeView>
```

## gRPC

We also want to conditionally display the html to add a comment, just like we did for the `Upload` of the Photo.

Once again,  we're going to use an `<AuthorizedView>` component.

- Open the `CommentsComponent.razor`
- Change the `html` to look like this (leave the `code` section as it is)

```html
<MudText Typo="Typo.h3">Comments</MudText>

@if (comments is null) {
    <MudText Typo="Typo.body1">No comments for this photo yet</MudText>
} else {
    @foreach (var comment in comments) {
        <CommentComponent CommentItem="comment" ViewMode="CommentComponent.ViewModes.Read" OnUpdate="UpdateComment" OnDelete="DeleteComment"/>
    }
    <AuthorizeView>
        <Authorized>
            <CommentComponent CommentItem="new Comment() {PhotoId = PhotoId}" ViewMode="CommentComponent.ViewModes.Create" OnCreate="CreateComment"/>
        </Authorized>
        <NotAuthorized>
            <MudButton Variant="Variant.Filled" EndIcon="@Icons.Material.Filled.Login" Color="Color.Error" Href="bff/login">Log in to add a comment</MudButton>
        </NotAuthorized>
    </AuthorizeView>
}
```

Now if you run the application you'll see the form to post a comment only if you're authenticated.

## Adding the User.Name to Photos and Comments

So now we need to get the user name on the server, on both the Rest API and the gRpc service, so that we can add it to a new Photo and Comment.

### The REST API

A Web Api Controller can find the User Name in the `User.Identity.Name` property of the Controller itself, so let's use it to complete the information of the uploaded Photo:

- Open the `PhotosController` on the `Controllers` folder of the `PhotoSharingApplication.WebServices.REST.Photos` project
- Modify the `CreateAsync` action as follows:

```cs
[Authorize]
[HttpPost]
public async Task<ActionResult<Photo>> CreateAsync(Photo photo) {
    photo.UserName = User?.Identity?.Name;
    Photo? p = await service.UploadAsync(photo);
    return CreatedAtRoute("Find", photo, new { id = photo.Id });
}
```

In order to display the name of the user on the `PhotoDetailsComponent.razor` component, let's update the user interface by replacing this code 
```html
<MudText Typo="Typo.subtitle1">@Photo.CreatedDate.ToShortDateString()</MudText>
```

with this

```html
<MudText Typo="Typo.subtitle1">Uploaded on @Photo.CreatedDate.ToShortDateString() by @Photo.UserName</MudText>
```

Upload a Photo and verify that the user name is correctly added to the Photo information and shown on the UI.

### gRPC Backend Service

The `Create` method that we override on our `CommentsService` has a `ServerCallContext` parameter. That's where we can find our UserName.

- Open the `CommentsService.cs` under the `Services` folder of the `PhotoSharingApplication.WebServices.Grpc.Comments` project
- Replace the `Create` method with the following code.

```cs
[Authorize]
public override async Task<CreateReply> Create(CreateRequest request, ServerCallContext context) {
    try {
        var user = context.GetHttpContext().User;
        Comment c = await commentsService.CreateAsync(new Comment { PhotoId = request.PhotoId, Subject = request.Subject, Body = request.Body, UserName = user.Identity.Name });
        return new CreateReply() { Id = c.Id, PhotoId = c.PhotoId, Body = c.Body, Subject = c.Subject, SubmittedOn = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(c.SubmittedOn.ToUniversalTime()), UserName = c.UserName };
    } catch (Exception ex) {
        throw new RpcException(new Status(StatusCode.Internal, ex.Message));
    }
}
```

Should you have problems with the tokens, you can learn how to troubleshoot and eventually fix the errors in these two excellent articles:
- https://nestenius.se/2023/03/28/missing-openid-connect-claims-in-asp-net-core/
- https://nestenius.se/2023/06/02/debugging-jwtbearer-claim-problems-in-asp-net-core/

## Next Steps

What we need to do next is to allow updates and deletes only to the photo (or comment) owner.

This is what we're going to do in the next lab, where we're going to learn about `Resource Based Authorization`.

Go to `Labs/Lab11`, open the `readme.md` and follow the instructions thereby contained.   
