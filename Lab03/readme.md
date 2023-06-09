# FrontEnd: Styling the UI with MatBlazor

If you followed Lab01 and Lab02, you can continue from your own solution. Otherwise you can begin with the `Lab03/Start` code.

---

In order to give our UI a better look, we need to use [CSS](https://en.wikipedia.org/wiki/Cascading_Style_Sheets).

You could create your own css file, add that to your index.html page, define all your styles and use them on your component. This is probably the best approach if you are a web designer, because you can create a lean css file that focuses only on what you need, so that the client downloads a small file containing only what's necessary and nothing more.

If you want to create a nice website you *should* learn CSS, but chances are you're not a web designer, you have no time to look into it, you want to create a prototype or you just don't want to reinvent the wheel and solve all the alignment problems that have already been solved multiple times by other developers. 

There are many different [CSS frameworks](https://onaircode.com/top-css-frameworks-web-designer/) around that you could turn to in order to use predefined styles that can speed up your development.
[Bootstrap](https://getbootstrap.com/) is already included in the application we built.

Open `index.html` file under the `wwwroot` folder of your `PhotoSharingApplication.Frontend.Client` project.

You'll see a link to a `bootstrap.min.css` file:

```html
<link href="css/bootstrap/bootstrap.min.css" rel="stylesheet" />
```

This means that we can already use it to give a better look to our UI.

For example we could transform our `Pages/AllPhotos.razor` to use a [Card](https://getbootstrap.com/docs/5.3/components/card/) for each Photo, like this:

```html
<h3>AllPhotos</h3>

<NavLink href="photos/upload">Upload new Photo</NavLink>

@if (photos is null) {
    <p>...Loading...</p>
} else {
<div class="row">
    @foreach (var photo in photos) {
        <div class="col-4">
            <article class="card">
                <img class="card-img-top" src="@(photo.PhotoFile == null ? "" : $"data:{photo.ImageMimeType};base64,{Convert.ToBase64String(photo.PhotoFile)}")" />
                <div class="card-header">
                    <h5 class="card-title">@photo.Id - @photo.Title</h5>
                </div>
                <div class="card-body">
                    <p class="card-text">@photo.Description</p>
                    <p>@photo.CreatedDate.ToShortDateString()</p>
                </div>
                <div class="card-footer">
                    <NavLink class="card-link" href="@($"photos/details/{photo.Id}")">Details</NavLink>
                    <NavLink class="card-link" href="@($"photos/update/{photo.Id}")">Update</NavLink>
                    <NavLink class="card-link" href="@($"photos/delete/{photo.Id}")">Delete</NavLink>
                </div>
            </article>
        </div>
    }
</div>
}
```

It is just a matter of learning which css class to use and what html structure to give to your content. All I did was to go to the [Bootstrap](https://getbootstrap.com/docs/5.3/getting-started/introduction/) documentation site, take a look at some example for a [Card](https://getbootstrap.com/docs/5.3/components/card/), copy some of that code into my page and tweek it to my liking.

This is an approach you can use for all the rest of your site.

What some people in the community have done, is to create Blazor Components that already wrap those css classes for you, so another approach is to use those components instead of (or together with) the base css.

If you check the [Awesome Blazor Repo](https://github.com/AdrienTorris/awesome-blazor#libraries--extensions) (which contains tons of links to Blazor resources), you can see that there are many ongoing projects. One of the first link under the extensions points to [MudBlazor](https://github.com/MudBlazor/MudBlazor), which implements the [Google Material Design Guidelines](https://material.io/guidelines/).    
I am going to use MudBlazor, but feel free to try any framework you like.

So what I'm going to do in my project is to use the `MudBlazor` components instead of Bootstrap. I'm not saying that it's the best choice, but it is a valid choice (there are [many](https://www.sitepoint.com/free-material-design-css-frameworks-compared/) different css framework around).

According to its own [official git repo](https://github.com/MudBlazor/MudBlazor)

> MudBlazor is an ambitious Material Design component framework for Blazor with an emphasis on ease of use and clear structure. It is perfect for .NET developers who want to rapidly build web applications without having to struggle with CSS and Javascript. MudBlazor, being written entirely in C#, empowers you to adapt, fix or extend the framework. There are plenty of examples in the documentation, which makes understanding and learning MudBlazor very easy.

So let's install it following the [documentation](https://mudblazor.com/getting-started/installation#manual-install)

## Installation

- In your `Fontend.Client` project, in the Solution Explorer, right click the Dependencies and select `Manage NuGet Packages`
- On the `Browse` Tab, search for `MudBlazor`. 
- Install the `MudBlazor` package by Garderoben, Henon and Contributors
- Add `@using MudBlazor` to your `_Imports.razor`
- Replace the lisk to Boostrap with google fonts and the MudBlazor stylesheet: add the following code in the `head` section of `index.html` 

```html
<link href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap" rel="stylesheet" />
<link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
```

- Add a script reference at the end on `index.html`, after the `blazor.webassembly.js` script

```html
<script src="_content/MudBlazor/MudBlazor.min.js"></script>
```


## Break Everything

Let's remove all the styles we don't want to use from the `wwwroot/css/app.css`. I'm going to delete everything, leaving only these (I'll explain later what these are for):

```css
#blazor-error-ui {
    background: lightyellow;
    bottom: 0;
    box-shadow: 0 -1px 2px rgba(0, 0, 0, 0.2);
    display: none;
    left: 0;
    padding: 0.6rem 1.25rem 0.7rem 1.25rem;
    position: fixed;
    width: 100%;
    z-index: 1000;
}

    #blazor-error-ui .dismiss {
        cursor: pointer;
        position: absolute;
        right: 0.75rem;
        top: 0.5rem;
    }

.blazor-error-boundary {
    background: url(data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNTYiIGhlaWdodD0iNDkiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyIgeG1sbnM6eGxpbms9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsiIG92ZXJmbG93PSJoaWRkZW4iPjxkZWZzPjxjbGlwUGF0aCBpZD0iY2xpcDAiPjxyZWN0IHg9IjIzNSIgeT0iNTEiIHdpZHRoPSI1NiIgaGVpZ2h0PSI0OSIvPjwvY2xpcFBhdGg+PC9kZWZzPjxnIGNsaXAtcGF0aD0idXJsKCNjbGlwMCkiIHRyYW5zZm9ybT0idHJhbnNsYXRlKC0yMzUgLTUxKSI+PHBhdGggZD0iTTI2My41MDYgNTFDMjY0LjcxNyA1MSAyNjUuODEzIDUxLjQ4MzcgMjY2LjYwNiA1Mi4yNjU4TDI2Ny4wNTIgNTIuNzk4NyAyNjcuNTM5IDUzLjYyODMgMjkwLjE4NSA5Mi4xODMxIDI5MC41NDUgOTIuNzk1IDI5MC42NTYgOTIuOTk2QzI5MC44NzcgOTMuNTEzIDI5MSA5NC4wODE1IDI5MSA5NC42NzgyIDI5MSA5Ny4wNjUxIDI4OS4wMzggOTkgMjg2LjYxNyA5OUwyNDAuMzgzIDk5QzIzNy45NjMgOTkgMjM2IDk3LjA2NTEgMjM2IDk0LjY3ODIgMjM2IDk0LjM3OTkgMjM2LjAzMSA5NC4wODg2IDIzNi4wODkgOTMuODA3MkwyMzYuMzM4IDkzLjAxNjIgMjM2Ljg1OCA5Mi4xMzE0IDI1OS40NzMgNTMuNjI5NCAyNTkuOTYxIDUyLjc5ODUgMjYwLjQwNyA1Mi4yNjU4QzI2MS4yIDUxLjQ4MzcgMjYyLjI5NiA1MSAyNjMuNTA2IDUxWk0yNjMuNTg2IDY2LjAxODNDMjYwLjczNyA2Ni4wMTgzIDI1OS4zMTMgNjcuMTI0NSAyNTkuMzEzIDY5LjMzNyAyNTkuMzEzIDY5LjYxMDIgMjU5LjMzMiA2OS44NjA4IDI1OS4zNzEgNzAuMDg4N0wyNjEuNzk1IDg0LjAxNjEgMjY1LjM4IDg0LjAxNjEgMjY3LjgyMSA2OS43NDc1QzI2Ny44NiA2OS43MzA5IDI2Ny44NzkgNjkuNTg3NyAyNjcuODc5IDY5LjMxNzkgMjY3Ljg3OSA2Ny4xMTgyIDI2Ni40NDggNjYuMDE4MyAyNjMuNTg2IDY2LjAxODNaTTI2My41NzYgODYuMDU0N0MyNjEuMDQ5IDg2LjA1NDcgMjU5Ljc4NiA4Ny4zMDA1IDI1OS43ODYgODkuNzkyMSAyNTkuNzg2IDkyLjI4MzcgMjYxLjA0OSA5My41Mjk1IDI2My41NzYgOTMuNTI5NSAyNjYuMTE2IDkzLjUyOTUgMjY3LjM4NyA5Mi4yODM3IDI2Ny4zODcgODkuNzkyMSAyNjcuMzg3IDg3LjMwMDUgMjY2LjExNiA4Ni4wNTQ3IDI2My41NzYgODYuMDU0N1oiIGZpbGw9IiNGRkU1MDAiIGZpbGwtcnVsZT0iZXZlbm9kZCIvPjwvZz48L3N2Zz4=) no-repeat 1rem/1.8rem, #b32121;
    padding: 1rem 1rem 1rem 3.7rem;
    color: white;
}

    .blazor-error-boundary::after {
        content: "An error has occurred."
    }

.loading-progress {
    position: relative;
    display: block;
    width: 8rem;
    height: 8rem;
    margin: 20vh auto 1rem auto;
}

    .loading-progress circle {
        fill: none;
        stroke: #e0e0e0;
        stroke-width: 0.6rem;
        transform-origin: 50% 50%;
        transform: rotate(-90deg);
    }

        .loading-progress circle:last-child {
            stroke: #1b6ec2;
            stroke-dasharray: calc(3.141 * var(--blazor-load-percentage, 0%) * 0.8), 500%;
            transition: stroke-dasharray 0.05s ease-in-out;
        }

.loading-progress-text {
    position: absolute;
    text-align: center;
    font-weight: bold;
    inset: calc(20vh + 3.25rem) 0 auto 0.2rem;
}

    .loading-progress-text:after {
        content: var(--blazor-load-percentage-text, "Loading");
    }

```

- Register the services in `Program.cs`

```cs
using MudBlazor.Services;
builder.Services.AddMudServices();
```

- Add the following components to your `PhotoSharingApplication.Frontend.Client/Shared/MainLayout.razor`.

```
<MudThemeProvider />
```

Right now, if you run your site you'll see a partial disaster (**if it still looks good, you either did not remove the link to bootstrap or you need to refresh your browser cache by reloading the page on your browser with `CTRL` + `F5`**)

## Let's fix it

There are multiple places where we need to update our html.

Let's start with our `Pages/AllPhotos.razor`.

Our goal is to use a [Card](https://mudblazor.com/components/card) for each Photo. 

I'm going to go for this html, but feel free to use a layout you like, by looking at the documentation of MudBlazor and applying it to your component.

```html
@page "/photos/all"

@using PhotoSharingApplication.Shared.Entities
@using PhotoSharingApplication.Shared.Interfaces
@inject IPhotosService photosService

<PageTitle>All Photos</PageTitle>

<MudText Typo="Typo.h1">All Photos</MudText>

<MudIconButton Icon="@Icons.Material.Filled.AddAPhoto" Color="Color.Default" Href="photos/upload" />

<MudGrid Spacing="2" Justify="Justify.FlexStart">
    
@if (photos == null) {
    <p>...Loading...</p>
} else {
    @foreach (var photo in photos) {
    <MudItem xs="12" sm="4">
        <MudCard>
            <MudCardHeader>
                <CardHeaderContent>
                    <MudText Typo="Typo.body1">@photo.Id</MudText>
                    <MudText Typo="Typo.body2">@photo.Title</MudText>
                </CardHeaderContent>
            </MudCardHeader>
            <MudCardMedia Image="@(photo.PhotoFile == null ? "" : $"data:{photo.ImageMimeType};base64,{Convert.ToBase64String(photo.PhotoFile)}")" Height="250" />
            <MudCardContent>
                <MudText Typo="Typo.body2">@photo.Description</MudText>
                <MudText Typo="Typo.subtitle1">@photo.CreatedDate.ToShortDateString()</MudText>
            </MudCardContent>
            <MudCardActions>
                <MudIconButton Icon="@Icons.Material.Filled.Photo" Color="Color.Default" Href="@($"photos/details/{photo.Id}")" />
                <MudIconButton Icon="@Icons.Material.Filled.PhotoSizeSelectLarge" Color="Color.Default" Href="@($"photos/update/{photo.Id}")" />
                <MudIconButton Icon="@Icons.Material.Filled.Delete" Color="Color.Warning" Href="@($"photos/delete/{photo.Id}")" />
            </MudCardActions>
        </MudCard>
    </MudItem>
    }
}
</MudGrid>
@code {
    List<Photo>? photos;

    protected override async Task OnInitializedAsync() {
        photos = await photosService.GetPhotosAsync();
    }
}
```

## The Application Bar

Ok, our AllPhotos looks better (again, press CTRL+F5 in your browser if it doesn't), but what happened to the navigation?? And most of all, where can we fix it?

In order to find where the navigation is defined, we need to start from the very beginning and take a look at one attribute that I did not explain yet.

Open `App.razor`.

```html
<Router AppAssembly="@typeof(Program).Assembly">
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
```

You may have already noticed that `DefaultLayout="@typeof(MainLayout)"` attribute in the `<RouteView>` component.

That is a reference to our application [Layout](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/layouts?view=aspnetcore-7.0)

The basic idea is that our `Page` focuses on the content, while the `Layout` takes care of all the fluff that you want to have on every page but you don't want to have to rewrite over and over and over.

Since the navigation is indeed something we want to have on each page, that is included in the Layout. Our `RouteView` says that the Layout is called `MainLayout`, which means we have to look for a component called `MainLayout.razor`. We can find it under the `Shared` folder.

```html
@inherits LayoutComponentBase

<MudThemeProvider />

<div class="page">
    <div class="sidebar">
        <NavMenu />
    </div>

    <main>
        <div class="top-row px-4">
            <a href="https://docs.microsoft.com/aspnet/" target="_blank">About</a>
        </div>

        <article class="content px-4">
            @Body
        </article>
    </main>
</div>
```

As you can see, it contains some html that make use of the Bootstrap classes (which of course don't work anymore because we removed the link to `bootstrap.css` from our `index.html`).  
What you may have not noticed is that in the Solution explorer, you can see a small arrow on the left of the MainLayout.razor filename and that if you click on it you can see an additional `MainLayout.razor.css`.
That file is using [CSS Isolation](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/css-isolation?view=aspnetcore-7.0), a technique to define component-specific styles by creating a `.razor.css` file matching the name of the `.razor` file for the component in the same folder. The `.razor.css` file is a `scoped CSS file`.  
We can remove the `MainLayout.razor.css` file, since all the styles we need are already in MudBlazor.

Now we can replace the html of `MainLayout.razor` with some `MudBlazor` components.

I'm going to use 
- [MudLayout](https://mudblazor.com/getting-started/layouts) 
- [MudAppBar](https://mudblazor.com/components/appbar#regular-app-bar) 
- [MudDrawer](https://mudblazor.com/components/drawer#variants-responsive)
- [MudMainContent and MudContainer](https://mudblazor.com/getting-started/layouts#content-&-containers)

as shown in the [documentation](https://mudblazor.com/getting-started/layouts#appbar-&-drawer)


```html
@inherits LayoutComponentBase

<MudThemeProvider />

<MudLayout>
    <MudAppBar Elevation="1">
        <MudIconButton Icon="@Icons.Material.Filled.Menu" Color="Color.Inherit" Edge="Edge.Start" OnClick="@ToggleDrawer" />
    </MudAppBar>
    <MudDrawer @bind-Open="@open" Elevation="1">
        <MudDrawerHeader>
            <MudText Typo="Typo.h6">Photo Sharing Application</MudText>
        </MudDrawerHeader>
        <NavMenu/>
    </MudDrawer>
    <MudMainContent Class="pt-16 px-16">
        <MudContainer Class="mt-6">
            @Body
        </MudContainer>
    </MudMainContent>
</MudLayout>

@code {
    bool open = false;

    void ToggleDrawer() {
        open = !open;
    }
}
```

Running the application now, you'll see that the UI looks different, although it's not complete yet. At least you should see the drawer opening and closing when you click on the menu icon.

## The Navigation Bar

What is `<NavMenu>`? It's yet another component of ours, also to be found in the `Shared` folder, that contains the navigation bar that once was on the left of the page.

```html
<div class="top-row ps-3 navbar navbar-dark">
    <div class="container-fluid">
        <a class="navbar-brand" href="">PhotoSharingApplication.Frontend.Client</a>
        <button title="Navifation menu" class="navbar-toggler" @onclick="ToggleNavMenu">
            <span class="navbar-toggler-icon"></span>
        </button>
    </div>
</div>

<div class="@NavMenuCssClass" @onclick="ToggleNavMenu">
    <ul class="nav flex-column">
        <li class="nav-item px-3">
            <NavLink class="nav-link" href="" Match="NavLinkMatch.All">
                <span class="oi oi-home" aria-hidden="true"></span> Home
            </NavLink>
        </li>
        <li class="nav-item px-3">
            <NavLink class="nav-link" href="counter">
                <span class="oi oi-plus" aria-hidden="true"></span> Counter
            </NavLink>
        </li>
        <li class="nav-item px-3">
            <NavLink class="nav-link" href="fetchdata">
                <span class="oi oi-list-rich" aria-hidden="true"></span> Fetch data
            </NavLink>
        </li>
    </ul>
</div>

@code {
    private bool collapseNavMenu = true;

    private string NavMenuCssClass => collapseNavMenu ? "collapse" : null;

    private void ToggleNavMenu()
    {
        collapseNavMenu = !collapseNavMenu;
    }
}
```

We already have the logic to open or close our drawer, so we can delete the `code` section and the first `div` with the `button`.

Which means that all what's left is simply

```html
<MudNavMenu>
    <MudNavLink Href="/photos/all" Icon="@Icons.Material.Filled.List">All Photos</MudNavLink>
    <MudNavLink Href="/photos/upload" Icon="@Icons.Material.Filled.AddAPhoto">Add Photo</MudNavLink>
</MudNavMenu>
```

If you navigate to your home page, you should already see a better layout than before. Clicking on the links of the navigation bar should take you to the correct pages and style the menu items accordingly.

## Changing the Theme

We can also apply our own [Theme](https://mudblazor.com/customization/overview#theme-provider) to our site, by creating and assigning properties to a `MudTheme` instance, as shown in the [docs](https://mudblazor.com/customization/overview#custom-themes):

Our `MainLayout.razor` becomes:

```html
@inherits LayoutComponentBase

<MudThemeProvider Theme="MyCustomTheme" />

<MudLayout>
    <MudAppBar Elevation="1">
        <MudIconButton Icon="@Icons.Material.Filled.Menu" Color="Color.Inherit" Edge="Edge.Start" OnClick="@ToggleDrawer" />
    </MudAppBar>
    <MudDrawer @bind-Open="@open" Elevation="1">
        <MudDrawerHeader>
            <MudText Typo="Typo.h6">Photo Sharing Application</MudText>
        </MudDrawerHeader>
        <NavMenu/>
    </MudDrawer>
    <MudMainContent Class="pt-16 px-16">
        <MudContainer Class="mt-6">
            @Body
        </MudContainer>
    </MudMainContent>
</MudLayout>

@code {
    bool open = false;

    void ToggleDrawer() {
        open = !open;
    }


    MudTheme MyCustomTheme = new MudTheme() {
        Palette = new PaletteLight() {
            Primary = new MudBlazor.Utilities.MudColor("#BADA55"),
            Secondary = Colors.DeepOrange.Accent1,
            AppbarBackground = new MudBlazor.Utilities.MudColor("#BADA55"),
        },
        PaletteDark = new PaletteDark() {
            Primary = Colors.Blue.Lighten1
        },

        LayoutProperties = new LayoutProperties() {
            DrawerWidthLeft = "260px",
            DrawerWidthRight = "300px"
        }
    };
}
```

Save and verify that your color scheme is changed.

## The Details Page
The html section of the `PhotoDetails.razor` page will look pretty much like the `AllPhotos.razor` page.


```html
<PageTitle>Photo Details - @photo?.Title</PageTitle>

@if (photo is null) {
    <MudText Typo="Typo.body1">...Loading...</MudText>
} else {
    <MudCard >
        <MudCardHeader>
            <CardHeaderContent>
                <MudText Typo="Typo.body1">@photo.Id</MudText>
                <MudText Typo="Typo.body2">@photo.Title</MudText>
            </CardHeaderContent>
        </MudCardHeader>
        <MudCardMedia Image="@(photo.PhotoFile == null ? "" : $"data:{photo.ImageMimeType};base64,{Convert.ToBase64String(photo.PhotoFile)}")" />
        <MudCardContent>
            <MudText Typo="Typo.body2">@photo.Description</MudText>
            <MudText Typo="Typo.subtitle1">@photo.CreatedDate.ToShortDateString()</MudText>
        </MudCardContent>
        <MudCardActions>
            <MudIconButton Icon="@Icons.Material.Filled.Photo" Color="Color.Default" Href="@($"photos/details/{photo.Id}")" />
            <MudIconButton Icon="@Icons.Material.Filled.PhotoSizeSelectLarge" Color="Color.Default" Href="@($"photos/update/{photo.Id}")" />
            <MudIconButton Icon="@Icons.Material.Filled.Delete" Color="Color.Warning" Href="@($"photos/delete/{photo.Id}")" />
        </MudCardActions>
    </MudCard>
}
```

## The Upload Page

Let's tackle the `UploadPhoto` Page.

We still need our [EditForm](https://learn.microsoft.com/en-us/aspnet/core/blazor/forms-and-input-components?view=aspnetcore-7.0). 
Inside the form we will need [TextFields](https://mudblazor.com/components/textfield) and [FileUpload](https://mudblazor.com/components/fileupload).  
We also need to change the code to handle the selection of the file, because MudBlazor passes different parameters than the ones we had with the native Blazor `FileUpload` component.

```html
@page "/photos/upload"
@using PhotoSharingApplication.Shared.Entities;
@using PhotoSharingApplication.Shared.Interfaces;
@inject IPhotosService photosService
@inject NavigationManager navigationManager

<MudCard>
    <MudForm Model="photo">
        <MudCardContent>
            <MudTextField @bind-Value="photo.Title"
                          For="@(() => photo.Title)"
                          Label="Title" />
            <MudTextField @bind-Value="photo.Description"
                          Lines="3"
                          For="@(() => photo.Description)"
                          Label="Description" />
            <MudFileUpload T="IBrowserFile" FilesChanged="HandleFileSelected">
                <ButtonTemplate>
                    <MudFab HtmlTag="label"
                            Color="Color.Secondary"
                            StartIcon="@Icons.Material.Filled.Image"
                            Label="Load picture"
                            for="@context" />
                </ButtonTemplate>
            </MudFileUpload>
            <MudImage Fluid Src="@(photo.PhotoFile == null ? "" : $"data:{photo.ImageMimeType};base64,{Convert.ToBase64String(photo.PhotoFile)}")" Elevation="25" Class="rounded-lg" />
        </MudCardContent>
    </MudForm>
    <MudCardActions>
        <MudIconButton Color="Color.Primary" Icon="@Icons.Material.Filled.FileUpload" OnClick="HandleValidSubmit">Upload</MudIconButton>
    </MudCardActions>
</MudCard>


@code {
    Photo photo = new Photo();

    private async Task HandleValidSubmit() {
        await photosService.UploadAsync(photo);
        navigationManager.NavigateTo("/photos/all");
    }
    private async Task HandleFileSelected(IBrowserFile args) {
        photo.ImageMimeType = args.ContentType;

        using (var streamReader = new System.IO.MemoryStream()) {
            await args.OpenReadStream().CopyToAsync(streamReader);
            photo.PhotoFile = streamReader.ToArray();
        }
    }
}
```

## The Update Page

The `UpdatePhoto.razor` will look more or less the same, we just need to modify the button into update instead of add (which, again, calls for a refactor in order to apply the DRY principle, but we'll do it later).

```html
@page "/photos/update/{id:int}"

@using PhotoSharingApplication.Shared.Interfaces
@using PhotoSharingApplication.Shared.Entities
@inject IPhotosService photosService
@inject NavigationManager navigationManager

<PageTitle>Update Photo @photo?.Title</PageTitle>

@if (photo is null) {
    <MudText Typo="Typo.caption">...Loading...</MudText>
} else {
    <MudCard>
        <MudForm Model="photo">
            <MudCardContent>
                <MudTextField @bind-Value="photo.Title"
                              For="@(() => photo.Title)"
                              Label="Title" />
                <MudTextField @bind-Value="photo.Description"
                              Lines="3"
                              For="@(() => photo.Description)"
                              Label="Description" />
                <MudFileUpload T="IBrowserFile" FilesChanged="HandleFileSelected">
                    <ButtonTemplate>
                        <MudFab HtmlTag="label"
                                Color="Color.Secondary"
                                StartIcon="@Icons.Material.Filled.Image"
                                Label="Load picture"
                                for="@context"/>
                    </ButtonTemplate>
                </MudFileUpload>
                <MudImage Fluid Src="@(photo.PhotoFile == null ? "" : $"data:{photo.ImageMimeType};base64,{Convert.ToBase64String(photo.PhotoFile)}")" Elevation="25" Class="rounded-lg" />
            </MudCardContent>
        </MudForm>
        <MudCardActions>
            <MudIconButton Color="Color.Primary" Icon="@Icons.Material.Filled.FileUpload" OnClick="HandleValidSubmit">Upload</MudIconButton>
        </MudCardActions>
    </MudCard>
}

@code {
    [Parameter]
    public int Id { get; set; }

    Photo? photo;

    protected override async Task OnInitializedAsync() {
        photo = await photosService.FindAsync(Id);
    }

    private async Task HandleValidSubmit() {
        await photosService.UpdateAsync(photo);
        navigationManager.NavigateTo("/photos/all");
    }

    private async Task HandleFileSelected(IBrowserFile args) {
        photo.ImageMimeType = args.ContentType;

        using (var streamReader = new System.IO.MemoryStream()) {
            await args.OpenReadStream().CopyToAsync(streamReader);
            photo.PhotoFile = streamReader.ToArray();
        }
    }
}
```

## The Delete Page

The last template we have to change is the one of the `DeletePhoto.razor` Page, which will look similar to the `PhotoDetails` page.

```html
@page "/photos/delete/{id:int}"

@using PhotoSharingApplication.Shared.Interfaces
@using PhotoSharingApplication.Shared.Entities
@inject IPhotosService photosService
@inject NavigationManager navigationManager

<PageTitle>Delete Photo @photo?.Title</PageTitle>

@if (photo is null) {
    <MudText Typo="Typo.body1">...Loading...</MudText>
} else {
    <MudCard >
        <MudCardHeader>
            <CardHeaderContent>
                <MudText Typo="Typo.body1">@photo.Id</MudText>
                <MudText Typo="Typo.body2">@photo.Title</MudText>
            </CardHeaderContent>
        </MudCardHeader>
        <MudCardMedia Image="@(photo.PhotoFile == null ? "" : $"data:{photo.ImageMimeType};base64,{Convert.ToBase64String(photo.PhotoFile)}")" />
        <MudCardContent>
            <MudText Typo="Typo.body2">@photo.Description</MudText>
            <MudText Typo="Typo.subtitle1">@photo.CreatedDate.ToShortDateString()</MudText>
        </MudCardContent>
        <MudCardActions>
            <MudIconButton Icon="@Icons.Material.Filled.ArrowBack" Color="Color.Default" Href="photos/all" />
            <MudIconButton Icon="@Icons.Material.Filled.DeleteForever" Color="Color.Error" OnClick="DeleteConfirm" />
        </MudCardActions>
    </MudCard>
 }

@code {
    [Parameter]
    public int Id { get; set; }

    Photo? photo;

    protected override async Task OnInitializedAsync() {
        photo = await photosService.FindAsync(Id);
    }
    private async Task DeleteConfirm(MouseEventArgs e) {
        await photosService.RemoveAsync(Id);
        navigationManager.NavigateTo("/photos/all");
    }
}
```

Our styling is complete.  Our next lab will focus on some refactoring: we will create a `Razor Class Library` with two new components:
- A `PhotoDisplayComponent` that we will use from the `AllPhotos`, `Details` and `Delete` pages
- A `PhotoEditComponent` that we will use from `Upload` and `Update` pages

Go to `Labs/Lab04`, open the `readme.md` and follow those instructions to continue.   
