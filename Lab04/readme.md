# Frontend - Razor Class Libraries and Components

## Goal one: refactor the common UI code into reusable components
Right now we have three pages (All, Details and Delete) that basically use the same layout: a card with the photo information. The only thing that changes are which buttons to show. This means that every time we make a change, we have to update the UI and the logic of three components (AllPhotos, Details and Delete), writing  the same code three times. This situation is less than ideal and it violates the [DRY principle](https://en.wikipedia.org/wiki/Don%27t_repeat_yourself#:~:text=%22Don't%20repeat%20yourself%22,data%20normalization%20to%20avoid%20redundancy.).  
We should refactor the `card` into its own component.  

## Goal two: move the UI code into a reusable library 
Since we're talking about the DRY principle, another goal of this lab is to refactor the UI into a [Razor Class Library](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/ui-class?view=aspnetcore-7.0&tabs=visual-studio).    
We would see the added value whenever we would start building a new project type, for example a [Hosted Blazor WebAssembly App](https://learn.microsoft.com/en-us/aspnet/core/blazor/hosting-models?view=aspnetcore-7.0#blazor-server) or a [Blazor Hybrid Application](https://learn.microsoft.com/en-us/aspnet/core/blazor/hosting-models?view=aspnetcore-7.0#blazor-hybrid), because we could reuse our components without having to rewrite the same code.


We're going to start with creating a Razor Library first, moving everything we have there and checking if it still works, then we'll create separate components that we'll reuse in multiple pages.

## Razor Class Library

Let's add a `new project` to our solution.
- In the `SolutionExplorer`, right click on the solution then select `Add` -> `New Project`
- As `Template`, select `Razor Class Library`. Click `Next`
- In the `Project Name` field, type `PhotoSharingApplication.Frontend.BlazorComponents`
- Make Sure you select the latest version (.NET 7.0)
- Do not check the option to `Support pages and views`
- Click Create
- Add the `MudBlazor`NuGet Package
- Add a reference to the `PhotoSharingApplication.Shared` project
- Open `_Imports.razor` and add

```cs
@using Microsoft.AspNetCore.Components.Forms
@using MudBlazor
@using PhotoSharingApplication.Shared.Interfaces
@using PhotoSharingApplication.Shared.Entities
```

- Create a new `Pages` Folder 
- Move the `AllPhotos`, `PhotoDetails`, `UploadPhoto`, `UpdatePhoto`, `DeletePhoto` page from the `PhotoSharingApplication.Frontend.Client` project to the `Pages` folder of the `PhotoSharingApplication.Frontend.BlazorComponents` project.
- Create a new `Shared` Folder 
- Move the `MainLayout` and `NavMenu` from the `PhotoSharingApplication.Frontend.Client` project to the `Shared` folder of the `PhotoSharingApplication.Frontend.BlazorComponents` project.
- Add a project reference to the `PhotoSharingApplication.Frontend.BlazorComponents` project in the `PhotoSharingApplication.Frontend.Client` project.
- Open the `_Imports.razor` file of the `PhotoSharingApplication.Frontend.Client` project
- Add the following code:

```
@using PhotoSharingApplication.Frontend.BlazorComponents.Pages
@using PhotoSharingApplication.Frontend.BlazorComponents.Shared
```

Running the application now should not work, since the router cannot find the pages. That is because the only assembly where it looks is the assembly where de Layout is defined. You can see that if you open the `App.razor` file of the `PhotoSharingApplication.Frontend.Client` project:

```html
<Router AppAssembly="@typeof(Program).Assembly">
```

Let's add an [additional assembly]https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-7.0#route-to-components-from-multiple-assemblies), so that the router can find our pages.

```html
<Router AppAssembly="@typeof(Program).Assembly" AdditionalAssemblies="new[] { typeof(MainLayout).Assembly }">
```

Run the application and verify that everything still works as before.

Now for the other goal: we're going to move the shared UI code into a reusable component, making sure that we can also configure which buttons to show depending on the scenario.

## The PhotoDetailsComponent

First of all, let's create a `Components` folder under our `PhotoSharingApplication.Frontend.BlazorComponents` project.

In the `Components` folder, create a new `Razor Component` called  `PhotoDetailsComponent.razor`.

Now cut the `MudCard` of the `AllPhotos` component, paste it in the `PhotoDetailsComponent` component and write each `photo` with a capital P.

```html
<MudCard>
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.body1">@Photo.Id</MudText>
            <MudText Typo="Typo.body2">@Photo.Title</MudText>
        </CardHeaderContent>
    </MudCardHeader>
    <MudCardMedia Image="@(Photo.PhotoFile == null ? "" : $"data:{Photo.ImageMimeType};base64,{Convert.ToBase64String(Photo.PhotoFile)}")" Height="250" />
    <MudCardContent>
        <MudText Typo="Typo.body2">@Photo.Description</MudText>
        <MudText Typo="Typo.subtitle1">@Photo.CreatedDate.ToShortDateString()</MudText>
    </MudCardContent>
    <MudCardActions>
        <MudIconButton Icon="@Icons.Material.Filled.Photo" Color="Color.Default" Href="@($"photos/details/{Photo.Id}")" />
        <MudIconButton Icon="@Icons.Material.Filled.PhotoSizeSelectLarge" Color="Color.Default" Href="@($"photos/update/{Photo.Id}")" />
        <MudIconButton Icon="@Icons.Material.Filled.Delete" Color="Color.Warning" Href="@($"photos/delete/{Photo.Id}")" />
    </MudCardActions>
</MudCard>
```

This time, instead of loading the product by asking it to the service, we will accept it as a [Parameter](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/?view=aspnetcore-7.0#parameters). So the `code` becomes

```cs
@code {
  [Parameter]
  public Photo Photo { get; set; }
}
```
Now let's use the component from within the `AllPhotos` page.

We could already do it like this:

```html
@foreach (var photo in photos) {
  <MudItem xs="12" sm="4">
      <PhotoSharingApplication.Frontend.BlazorComponents.Components.PhotoDetailsComponent Photo="photo"/>
  </MudItem>
}
```

It's a bit annoying that we have to write the whole [namespace](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/?view=aspnetcore-7.0#component-name-class-name-and-namespace).
To avoid this (here and in the other pages where we will use this components), let's add the `using` on our `_Imports.razor` in the `@using PhotoSharingApplication.Frontend.BlazorComponents` project:

```cs
@using PhotoSharingApplication.Frontend.BlazorComponents.Components
```

This means that we can now change the `AllPhotos.razor` like this:

```html
@page "/photos/all"

@using PhotoSharingApplication.Shared.Entities
@using PhotoSharingApplication.Shared.Interfaces
@inject IPhotosService photosService

<PageTitle>All Photos</PageTitle>

<MudText Typo="Typo.h1">All Photos</MudText>

<MudIconButton Icon="@Icons.Material.Filled.AddAPhoto" Color="Color.Default" Href="photos/upload" />

<MudGrid Spacing="2" Justify="Justify.FlexStart">
@if (photos is null) {
    <MudText Typo="Typo.caption">...Loading...</MudText>
} else {
    @foreach (var photo in photos) {
    <MudItem xs="12" sm="4">
        <PhotoDetailsComponent Photo="photo"/>
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

Much better.

Save and verify that the AllPhotos page still works as before.

## The Buttons Bar

Because we want to use the card from within the `Details` and `Delete` pages as well, we need to be able to configure which buttons to show.
- The `AllPhotos` page will configure the `PhotoDetailsComponent` to show: 
  - A button to navigate to the `Details` Page
  - A button to navigate to the `Update` Page
  - A button to navigate to the `Delete` Page
- The `Details` Page  will configure the `PhotoDetailsComponent` to show:
  - A button to navigate to the `Update` Page
  - A button to navigate to the `Delete` Page
- The `Delete` Page  will configure the `PhotoDetailsComponent` to show:
  - A `Confirm` button to actually delete the photo

Let's create some `Boolean` `Parameter` into the `PhotoDetailsComponent` component:

```cs
[Parameter]
public bool Details { get; set; }

[Parameter]
public bool Edit { get; set; }

[Parameter]
public bool Delete { get; set; }

[Parameter]
public bool DeleteConfirm { get; set; }
```

Let's use the parameters to conditionally show the corresponding buttons.

```html
<MudCardActions>
    @if (Details)
    {
        <MudIconButton Icon="@Icons.Material.Filled.Photo" Color="Color.Default" Href="@($"photos/details/{Photo.Id}")" />
    }
    @if (Edit) {
        <MudIconButton Icon="@Icons.Material.Filled.PhotoSizeSelectLarge" Color="Color.Default" Href="@($"photos/update/{Photo.Id}")" />
    }
    @if (Delete) {
        <MudIconButton Icon="@Icons.Material.Filled.Delete" Color="Color.Warning" Href="@($"photos/delete/{Photo.Id}")" />
    }
    @if (DeleteConfirm) {
        <MudIconButton Icon="@Icons.Material.Filled.ArrowBack" Color="Color.Default" Href="photos/all" />
        <MudIconButton Icon="@Icons.Material.Filled.DeleteForever" Color="Color.Error" />
    }
</MudCardActions>
```

Now let's have the `AllPhotos` component it needs to show. We don't need to pass the rest, as they are already `false`.

```html
<PhotoDetailsComponent Photo="photo" Details Edit Delete />
```

Let's repeat for the `PhotoDetails.razor` and `DeletePhoto.razor` Pages.

## Details page

```html
<PhotoDetailsComponent Photo="photo" Edit Delete />
```

## Delete Page

```html
<PhotoDetailsComponent Photo="photo" DeleteConfirm />
```

The UI should work, but what shall we do with the button logic?

From a design perspective, we want our component to be totally ignorant of its surroundings. Not only does it not know where the data comes from, it also knows nothing about what the logic should do. All it does is 
- It renders html data received from a parent 
- It alerts the parent whenever it's time to perform an action

The action can be handled by the parent (the page in our case).

It's time to introduce [Event Handling](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/event-handling?view=aspnetcore-7.0).

We need to do is to define and handle an [EventCallback](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/event-handling?view=aspnetcore-7.0#eventcallback) for each event we want to define.

## DeleteConfirmed

We're going to tackle the deletion of a photo. 

In our `PhotoDetailsComponent`, let's define an `EventCallback` that can provide the `id` of the photo to delete:

```cs
[Parameter]
public EventCallback<int> OnDeleteConfirmed { get; set; }
```

Then, let's invoke the callback at the click of the button:

```html
<MudIconButton Icon="@Icons.Material.Filled.DeleteForever" Color="Color.Error" OnClick="@(async ()=> await OnDeleteConfirmed.InvokeAsync(Photo.Id))" />
```
Now let's handle the event in the `Delete` page.

First let's bind the event to a method:

```html
<PhotoDetailsComponent Photo="photo" DeleteConfirm OnDeleteConfirmed="Delete" />
```

Then, let's change the old Delete method to match the signature of EventCallback:

```cs
private async Task Delete(int id) {
    await photosService.RemoveAsync(id);
    navigationManager.NavigateTo("/photos/all");
}
```

We're done refactoring the Details. Let's repeat the same process for the Create / Update.

## The PhotoEditComponent

Both the `Upload` and the `Update` page have more or less the same UI, so let's refactor it into a new `PhotoEditComponent` component.

In the `Components` folder of the `PhotoSharingApplication.Frontend.BlazorComponents` project, add a new `PhotoEditComponent.razor` Razor Component.

Cut the `<MudCard>` from the `UploadPhoto.razor` page and paste it into the new component. For consistency, change the `photo` property into a `Photo` property.

```html
<MudCard>
    <MudForm Model="Photo">
        <MudCardContent>
            <MudTextField @bind-Value="Photo.Title"
                          For="@(() => Photo.Title)"
                          Label="Title" />
            <MudTextField @bind-Value="Photo.Description"
                          Lines="3"
                          For="@(() => Photo.Description)"
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
            <MudImage Fluid Src="@(Photo.PhotoFile == null ? "" : $"data:{Photo.ImageMimeType};base64,{Convert.ToBase64String(Photo.PhotoFile)}")" Elevation="25" Class="rounded-lg" />
        </MudCardContent>
    </MudForm>
    <MudCardActions>
        <MudIconButton Color="Color.Primary" Icon="@Icons.Material.Filled.FileUpload" OnClick="HandleValidSubmit">Upload</MudIconButton>
    </MudCardActions>
</MudCard>
```

Add a `Parameter` of type `Photo`

```cs
@code {
  [Parameter, EditorRequired]
  public Photo Photo { get; set; } = default!;
}
```

We can move here the `HandleFileSelected` code:

```cs
private async Task HandleFileSelected(IBrowserFile args) {
    Photo.ImageMimeType = args.ContentType;

    using (var streamReader = new System.IO.MemoryStream()) {
        await args.OpenReadStream().CopyToAsync(streamReader);
        Photo.PhotoFile = streamReader.ToArray();
    }
}
```

Once again, we want to delegate some logic to the parent component.

Let's add and EventCallback:

```cs
[Parameter]
public EventCallback<Photo> OnSave { get; set; }
```

Let's invoke the callback whenever the `Upload` button is clicked:

```html
<MudIconButton Color="Color.Primary" Icon="@Icons.Material.Filled.FileUpload" OnClick="@(async ()=> await OnSave.InvokeAsync(Photo))">Upload</MudIconButton>
```

Let's bind the callback with a method of our `Upload` page:

```html
<PhotoEditComponent Photo="photo" OnSave="Upload"></PhotoEditComponent>
```

Then handle the event in a method of the `Upload` page (instead of the ValidSubmit of the EditForm):

```cs
  private async Task Upload() {
      await photosService.UploadAsync(photo);
      navigationManager.NavigateTo("/photos/all");
  }
```

The `Update` page also looks more or less the same:

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
    <PhotoEditComponent Photo="photo" OnSave="Upload"></PhotoEditComponent>
}

@code {
    [Parameter]
    public int Id { get; set; }

    Photo? photo;

    protected override async Task OnInitializedAsync() {
        photo = await photosService.FindAsync(Id);
    }

    private async Task Upload() {
        await photosService.UpdateAsync(photo);
        navigationManager.NavigateTo("/photos/all");
    }
}
```

Run the application and ensure that everything works just like before refactoring.

Ok, we're done for this lab, we can now move on to the next step, where we build an actual Backend.

Time for Lab05!