# FrontEnd: Styling the UI with MatBlazor

If yo followed Lab01 and Lab02, you can continue from your own solution. Otherwise you can begin with the `Lab03/Start` code.

---

In order to give our UI a better look, we need to use [CSS](https://en.wikipedia.org/wiki/Cascading_Style_Sheets).

You could create your own css file, add that to your index.html page, define all your styles and use them on your component. This is probably the best approach if you are a web designer, because you can create a lean css file that focuses only on what you need, so that the client downloads a small file containing only what's necessary and nothing more.

If you want to create a nice website you *should* learn CSS, but chances are you're not a web designer, you have no time to look into it, you want to create a prototype or you just don't want to reinvent the wheel and solve all the alignment problems that have already been solved multiple times by other developers. 

There are many different [CSS frameworks](https://onaircode.com/top-css-frameworks-web-designer/) around that you could turn to in order to use predefined styles that can speed up your development.
[Bootstrap](https://getbootstrap.com/) is already included in the application we built.

Open `index.html` file under the `wwwroot` folder of your `PhotoSharingApplication.Frontend.BlazorWebAssembly` project.

You'll see a link to a `bootstrap.min.css` file:

```html
<link href="css/bootstrap/bootstrap.min.css" rel="stylesheet" />
```

This means that we can already use it to give a better look to our UI.

For example we could transform our `Pages/AllPhotos.razor` to use a [Card](https://getbootstrap.com/docs/4.4/components/card/) for each Photo, like this:

```html
<h3>AllPhotos</h3>

<NavLink href="photos/upload">Upload new Photo</NavLink>

@if (photos == null) {
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

It is just a matter of learning which css class to use and what html structure to give to your content. All I did was to go to the [Bootstrap](https://getbootstrap.com/docs/4.4/getting-started/introduction/) documentation site, take a look at some example for a [Card](https://getbootstrap.com/docs/4.4/components/card/), copy some of that code into my page and tweek it to my liking.

This is an approach you can use for all the rest of your site.

What some people in the community have done, is to create Blazor Components that already wrap those css classes for you, so another approach is to use those components instead of (or together with) the base css.

If you check the [Awesome Blazor Repo](https://github.com/AdrienTorris/awesome-blazor#libraries--extensions) (which contains tons of links to Blazor resources), you can see that there are many ongoing projects. When I first wrote this lab, the first link under the extensions was to [MatBlazor](https://github.com/SamProf/MatBlazor), which implements the [Google Material Design Guidelines](https://material.io/guidelines/).  
The first place one year later has been taken by [Ant Design Blazor](https://github.com/ant-design-blazor/ant-design-blazor) since it has more stars on GitHub.  
I am going to keep the labs with MatBlazor since it has more downloads than AntBlazor, but feel free to try any framework you like.

So what I'm going to do in my project is to use the `MatBlazor` components instead of Bootstrap. I'm not saying that it's the best choice, but it is a valid choice (there are [many](https://www.sitepoint.com/free-material-design-css-frameworks-compared/) different css framework around).

According to its own [official git repo](https://github.com/SamProf/MatBlazor)

> MatBlazor comprises a range of components which implement common interaction patterns according to the Material Design specification.

So let's install it following the [documentation](https://github.com/SamProf/MatBlazor#installation)

## Installation

- In your BlazorWebAssembly project, in the Solution Explorer, right click the Dependencies and select `Manage NuGet Packages`
- On the `Browse` Tab, search for `MatBlazor`. **Ensure to check the `Include Prerelease` option**
- Install the `MatBlazor` package by Vladimir Samoilenko
- Add the following code in the `head` section of `index.html` 

```html
<script src="_content/MatBlazor/dist/matBlazor.js"></script>
<link href="_content/MatBlazor/dist/matBlazor.css" rel="stylesheet" />
```

- Add `@using MatBlazor` to your `_Imports.razor`

## Break Everything

Let's remove all the stiles we don't want to use from the `wwwroot/css/app.css`. I'm going to delete everything, leaving only these two (I'll explain later what these are for):

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
```

Then I'm going to remove the link to bootstrap from `index.html`

```html
<link href="css/bootstrap/bootstrap.min.css" rel="stylesheet" />
```

Right now, if you play your app you'll see a partial disaster (**if it still looks good, you either did not remove the link to bootstrap or you need to refresh your browser cache by reloading the page on your browser with `CTRL` + `F5`**)

## Let's fix it

There are multiple places where we need to update our html.

Let's start with our `Pages/AllPhotos.razor`.

Our goal is to use a [Card](https://www.matblazor.com/Card) for each Photo. 

I'm going to go for this html, but feel free to use a layout you like, by looking at the documentation of MatBlazor and applying it to your component.

```html
@page "/photos/all"

@using PhotoSharingApplication.Frontend.Core.Interfaces
@using PhotoSharingApplication.Frontend.Core.Entities
@inject IPhotosService photosService

<MatH3>AllPhotos</MatH3>

<MatButton Link="photos/upload">Upload new Photo</MatButton>

@if (photos == null) {
    <p class="mat">...Loading...</p>
} else {
<div class="mat-layout-grid">
  <div class="mat-layout-grid-inner">
    @foreach (var photo in photos) {
      <div class="mat-layout-grid-cell mat-layout-grid-cell-span-4">
        <MatCard>
          <div>
            <MatHeadline6>
                @photo.Id - @photo.Title
            </MatHeadline6>
            <MatSubtitle2>
                @photo.CreatedDate.ToShortDateString()
            </MatSubtitle2>
          </div>
          <MatCardContent>
            <MatCardMedia Wide="true" ImageUrl="@(photo.PhotoFile == null ? "" : $"data:{photo.ImageMimeType};base64,{Convert.ToBase64String(photo.PhotoFile)}")"></MatCardMedia>
            <MatBody2>
                @photo.Description
            </MatBody2>
          </MatCardContent>
          <MatCardActions>
            <MatCardActionButtons>
              <MatButton Link="@($"photos/details/{photo.Id}")">Details</MatButton>
              <MatButton Link="@($"photos/update/{photo.Id}")">Update</MatButton>
              <MatButton Link="@($"photos/delete/{photo.Id}")">Delete</MatButton>
            </MatCardActionButtons>
          </MatCardActions>
        </MatCard>
      </div>
    }
  </div>
</div>
}

@code {
  List<Photo> photos;

  protected override async Task OnInitializedAsync() {
      photos = await photosService.GetPhotosAsync();
  }
}
```

## The Application Bar

Ok, our AllPhotos looks better, but what happened to the navigation?? And most of all, where can we fix it?

In order to find where the navigation is defined, we need to start from the very beginning and take a look at one attribute that I did not explain yet.

Open `App.razor`.

```html
<Router AppAssembly="@typeof(Program).Assembly" PreferExactMatches="@true">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
    </Found>
    <NotFound>
        <LayoutView Layout="@typeof(MainLayout)">
            <p>Sorry, there's nothing at this address.</p>
        </LayoutView>
    </NotFound>
</Router>
```

You may have already noticed that `DefaultLayout="@typeof(MainLayout)"` attribute in the `<RouteView>` component.

That is a reference to our application [Layout](https://docs.microsoft.com/en-us/aspnet/core/blazor/layouts?view=aspnetcore-6.0)

The basic idea is that our `Page` focuses on the content, while the `Layout` takes care of all the fluff that you want to have on every page but you don't want to have to rewrite over and over and over.

Since the navigation is indedd something that we want to have on each page, that is included in the Layout. Our `RouteView` says that the Layout is called `MainLayout`, which means we have to look for a component called `MainLayout.razor`. We can find it under the `Shared` folder.

```html
@inherits LayoutComponentBase

<div class="page">
    <div class="sidebar">
        <NavMenu />
    </div>

    <div class="main">
        <div class="top-row px-4">
            <a href="http://blazor.net" target="_blank" class="ml-md-auto">About</a>
        </div>

        <div class="content px-4">
            @Body
        </div>
    </div>
</div>
```

As you can see, it contains some html that make use of the Bootstrap classes (which of course don't work anymore because we removed the link to `bootstrap.css` from our `index.html`).  
What you may have not noticed is that in the Solution explorer, you can see a small arrow on the left of the MainLayout.razor filename and that if you click on it you can see an additional `MainLayout.razor.css`.
That file is using [CSS Isolation](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/css-isolation?view=aspnetcore-6.0), a technique to define component-specific styles by creating a `.razor.css` file matching the name of the `.razor` file for the component in the same folder. The `.razor.css` file is a `scoped CSS file`.  
We can remove the `MainLayout.razor.css` file, since all the styles we need are already in MatBlazor.

Now we can replace the html of `MainLayout.razor` with some `MatBlazor` components.

I Like the idea of having a sidebar that opens or closes at the click of a button, so I'm going to use a [Drawer](https://www.matblazor.com/Drawer).

We create a Drawer by using the `<MatDrawerContainer>` component, which is composed by two main sections:
- `MatDrawer`
- `MatDrawerContent`

The `MatDrawer` will contain our `<NavMenu>`, while the `MatDrawerContent` will have an [AppBar](https://www.matblazor.com/AppBar)

The AppBar also has a container (`<MatAppBarContainer>`) with the bar ( `<MatAppBar>`) and the content (`<MatAppBarContent>`).

The `MatDrawer` can appear or disappear depending on the value of an `Opened` property, which we can [bind](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/data-binding?view=aspnetcore-6.0) to a boolean variable.

We're going to change the value of our variable [at the click of a button](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/event-handling?view=aspnetcore-6.0)

```html
@inherits LayoutComponentBase

<MatDrawerContainer Style="width: 100vw; height: 100vh;">
  <MatDrawer @bind-Opened="@Opened">
    <NavMenu />
  </MatDrawer>
  <MatDrawerContent>
    <MatAppBarContainer>
      <MatAppBar Fixed="true">
        <MatAppBarRow>
          <MatAppBarSection>
            <MatIconButton Icon="menu" OnClick="@((e) => ButtonClicked())"></MatIconButton>
            <MatAppBarTitle>Photo Sharing Application</MatAppBarTitle>
          </MatAppBarSection>
          <MatAppBarSection Align="@MatAppBarSectionAlign.End">
            <MatIconButton Icon="favorite"></MatIconButton>
          </MatAppBarSection>
        </MatAppBarRow>
      </MatAppBar>
      <MatAppBarContent>
        @Body
      </MatAppBarContent>
    </MatAppBarContainer>
  </MatDrawerContent>
</MatDrawerContainer>

@code
{
    bool Opened = true;

    void ButtonClicked() {
        Opened = !Opened;
    }
}
```

Running the application now, you'll see that the UI looks different, although it's not complete yet. At least you should see the drawer opening and closing when you click on the menu icon.

## The Navigation Bar

What is `<NavMenu>`? It's yet another component of ours, also to be found in the `Shared` folder, that contains the navigation bar that once was on the left of the page.

```html
<div class="top-row pl-4 navbar navbar-dark">
    <a class="navbar-brand" href="">PhotoSharingApplication.Frontend.BlazorWebAssembly</a>
    <button class="navbar-toggler" @onclick="ToggleNavMenu">
        <span class="navbar-toggler-icon"></span>
    </button>
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
<MatNavMenu>
  <MatNavItem Href="/photos/all">All Photos <MatIcon Icon="@MatIconNames.List"></MatIcon></MatNavItem>
  <MatNavItem Href="/photos/upload">Upload Photo <MatIcon Icon="@MatIconNames.Add"></MatIcon></MatNavItem>
</MatNavMenu>
```

You can also delete the corresponding `NavMenu.razor.css` file from the Solution Explorer.  

If you navigate to your home page, you should already see a better layout than before. Clicking on the links of the navigation bar should take you to the correct pages and style the menu items accordingly.

## Changing the Theme

We can also apply our own [Theme](https://www.matblazor.com/Themes) to our site, by wrapping our `MainLayout` in a `<MatThemeProvider>`, then creating and assigning properties to a `ThemeProvider` instance:

Our `MainLayout.razor` becomes:

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
              <MatIconButton Icon="favorite"></MatIconButton>
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

Save and verify that your color scheme is changed.

## The Details Page
The html section of the `PhotoDetails.razor` page will look pretty much like the `AllPhotos.razor` page.


```html
<MatH3>Details</MatH3>

@if (photo == null) {
  <p>...Loading...</p>
} else {
<div class="mat-layout-grid">
  <div class="mat-layout-grid-inner">
    <div class="mat-layout-grid-cell mat-layout-grid-cell-span-12">
      <MatCard>
        <div>
          <MatHeadline6>
            @photo.Id - @photo.Title
          </MatHeadline6>
          <MatSubtitle2>
            @photo.CreatedDate.ToShortDateString()
          </MatSubtitle2>
        </div>
        <MatCardContent>
          <MatCardMedia Wide="true" ImageUrl="@(photo.PhotoFile == null ? "" : $"data:{photo.ImageMimeType};base64,{Convert.ToBase64String(photo.PhotoFile)}")"></MatCardMedia>
          <MatBody2>
            @photo.Description
          </MatBody2>
        </MatCardContent>
        <MatCardActions>
          <MatCardActionButtons>
            <MatButton Link="@($"photos/update/{photo.Id}")">Update</MatButton>
            <MatButton Link="@($"photos/delete/{photo.Id}")">Delete</MatButton>
          </MatCardActionButtons>
        </MatCardActions>
      </MatCard>
    </div>
  </div>
</div>
}
```

## The Upload Page

Let's tackle the `UploadPhoto` Page.

We still need our [EditForm](https://docs.microsoft.com/en-us/aspnet/core/blazor/forms-validation?view=aspnetcore-6.0). 
Inside the form we will need [TextFields](https://www.matblazor.com/TextField) and [FileUpload](https://www.matblazor.com/FileUpload).  
We also need to change the code to handle the selection of the file, because MatBlazor passes different parameters than the ones we had with the native Blazor `FileUpload` component.

```html
@page "/photos/upload"

@using PhotoSharingApplication.Frontend.Core.Entities
@using PhotoSharingApplication.Frontend.Core.Interfaces

@inject IPhotosService photosService
@inject NavigationManager navigationManager

<div class="mat-layout-grid">
  <div class="mat-layout-grid-inner">
    <div class="mat-layout-grid-cell mat-layout-grid-cell-span-12">
      <MatCard>
        <MatH3>Upload Photo</MatH3>
        <MatCardContent>
          <EditForm Model="@photo" OnValidSubmit="HandleValidSubmit">
            <p>
              <MatTextField @bind-Value="@photo.Title" Label="Title" FullWidth></MatTextField>
            </p>
            <p>
              <MatTextField @bind-Value="@photo.Description" Label="Description" TextArea FullWidth></MatTextField>
            </p>
            <p>
              <MatFileUpload OnChange="@HandleMatFileSelected"></MatFileUpload>
            </p>
            <p>
              <MatButton Type="submit">Upload</MatButton>
            </p>
          </EditForm>
          <MatCardMedia Wide="true" ImageUrl="@(photo.PhotoFile == null ? "" : $"data:{photo.ImageMimeType};base64,{Convert.ToBase64String(photo.PhotoFile)}")"></MatCardMedia>
        </MatCardContent>
      </MatCard>
    </div>
  </div>
</div>
@code {
  Photo photo;

  protected override void OnInitialized() {
    photo = new Core.Entities.Photo();
  }

  private async Task HandleValidSubmit() {
    await photosService.UploadAsync(photo);
    navigationManager.NavigateTo("/photos/all");
  }

  async Task HandleMatFileSelected(IMatFileUploadEntry[] files) {
    IMatFileUploadEntry file = files.FirstOrDefault();
    if (file == null) {
      return;
    }
    photo.ImageMimeType = file.Type;

    using (var stream = new System.IO.MemoryStream()) {
      await file.WriteToStreamAsync(stream);
      photo.PhotoFile = stream.ToArray();
    }
  }
}
```

## The Update Page

The `UpdatePhoto.razor` will look more or less the same, we just need to modify the button into update instead of add.

```html
@page "/photos/update/{id:int}"

@using PhotoSharingApplication.Frontend.Core.Interfaces
@using PhotoSharingApplication.Frontend.Core.Entities
@inject IPhotosService photosService
@inject NavigationManager navigationManager

<MatH3>Update Photo</MatH3>

@if (photo == null) {
  <p>...Loading...</p>
} else {
  <div class="mat-layout-grid">
    <div class="mat-layout-grid-inner">
      <div class="mat-layout-grid-cell mat-layout-grid-cell-span-12">
        <MatCard>
          <MatCardContent>
            <MatH3>Update Photo</MatH3>
            <EditForm Model="@photo" OnValidSubmit="HandleValidSubmit">
              <p>
                <MatTextField @bind-Value="@photo.Title" Label="Title" FullWidth></MatTextField>
              </p>
              <p>
                <MatTextField @bind-Value="@photo.Description" Label="Description" TextArea FullWidth></MatTextField>
              </p>
              <p>
                <MatFileUpload OnChange="@HandleMatFileSelected"></MatFileUpload>
              </p>
              <p>
                <MatButton Type="submit">Update</MatButton>
              </p>
            </EditForm>
            <MatCardMedia Wide="true" ImageUrl="@(photo.PhotoFile == null ? "" : $"data:{photo.ImageMimeType};base64,{Convert.ToBase64String(photo.PhotoFile)}")"></MatCardMedia>
          </MatCardContent>
        </MatCard>
      </div>
    </div>
  </div>
}
@code {
  [Parameter]
  public int Id { get; set; }

  Photo photo;

  protected override async Task OnInitializedAsync() {
    photo = await photosService.FindAsync(Id);
  }

  private async Task HandleValidSubmit() {
    await photosService.UpdateAsync(photo);
    navigationManager.NavigateTo("/photos/all");
  }

  async Task HandleMatFileSelected(IMatFileUploadEntry[] files) {
    IMatFileUploadEntry file = files.FirstOrDefault();
    
    if (file == null) {
      return;
    }
    
    photo.ImageMimeType = file.Type;

    using (var stream = new System.IO.MemoryStream()) {
      await file.WriteToStreamAsync(stream);
      photo.PhotoFile = stream.ToArray();
    }
  }
}
```

## The Delete Page

The last template we have to change is the one of the `DeletePhoto.razor` Page, which will look similar to the `PhotoDetails` page.

```html
@page "/photos/delete/{id:int}"

@using PhotoSharingApplication.Frontend.Core.Interfaces
@using PhotoSharingApplication.Frontend.Core.Entities
@inject IPhotosService photosService
@inject NavigationManager navigationManager

<MatH3>Delete</MatH3>

@if (photo == null) {
  <p>...Loading...</p>
} else {
  <div class="mat-layout-grid">
    <div class="mat-layout-grid-inner">
      <div class="mat-layout-grid-cell mat-layout-grid-cell-span-12">
        <MatCard>
          <div>
            <MatHeadline6>
              @photo.Id - @photo.Title
            </MatHeadline6>
            <MatSubtitle2>
              @photo.CreatedDate.ToShortDateString()
            </MatSubtitle2>
          </div>
          <MatCardContent>
            <MatCardMedia Wide="true" ImageUrl="@(photo.PhotoFile == null ? "" : $"data:{photo.ImageMimeType};base64,{Convert.ToBase64String(photo.PhotoFile)}")"></MatCardMedia>
            <MatBody2>
              @photo.Description
            </MatBody2>
          </MatCardContent>
          <MatCardActions>
            <MatCardActionButtons>
              <MatButton OnClick="DeleteConfirm">Confirm Deletion</MatButton>
            </MatCardActionButtons>
          </MatCardActions>
        </MatCard>
      </div>
    </div>
  </div>
}

@code {
  [Parameter]
  public int Id { get; set; }

  Photo photo;

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
