# Frontend - Razor Class Libraries and Components

## Goal one: refactor the common UI code into reusable components
Right now we have three pages (All, Details and Delete) that basically use the same layout: a card with the photo information. The only thing that changes are which buttons to show. This means that every time we make a change, we have to update the UI and the logic of three components (AllPhotos, Details and Delete), writing  the same code three times. This situation is less than ideal and it violates the [DRY principle](https://en.wikipedia.org/wiki/Don%27t_repeat_yourself#:~:text=%22Don't%20repeat%20yourself%22,data%20normalization%20to%20avoid%20redundancy.).  
We should refactor the `card` into its own component.  

## Goal two: move the UI code into a reusable library 
Since we're talking about the DRY principle, another goal of this lab is to refactor the UI into a [Razor Class Library](https://docs.microsoft.com/en-us/aspnet/core/razor-pages/ui-class?view=aspnetcore-6.0&tabs=visual-studio).    
We would see the added value whenever we would start building a new project type, for example a [Hosted Blazor WebAssembly App](https://docs.microsoft.com/en-us/aspnet/core/blazor/hosting-models?view=aspnetcore-6.0) or a [.NET MAUI Desktop Application](https://visualstudiomagazine.com/articles/2021/04/12/maui-desktop.aspx), because we could reuse our components without having to rewrite the same code.


We're going to start with creating a Razor Library first, moving averything we have there and checking if it still works, then we'll create separate components that we'll reuse in multiple pages.

## Razor Class Library

Let's add a `new project` to our solution.
- In the `SolutionExplorer`, right click on the solution then select `Add` -> `New Project`
- As `Template`, select `Razor Class Library`. Click `Next`
- In the `Project Name` field, type `PhotoSharingApplication.Frontend.BlazorComponents`
- Make Sure you select the latest version (.NET 6.0)
- Do not check the option to support Pages and Views
- Click Create
- Add the `MatBlazor`NuGet Package (make sure to install the latest prerelease)
- Add a reference to the `PhotoSharingApplication.Shared` project
- Open `_Imports.razor` and add

```cs
@using Microsoft.AspNetCore.Components.Forms
@using MatBlazor
@using PhotoSharingApplication.Shared.Entities
@using PhotoSharingApplication.Shared.Interfaces
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

Let's add an [additional assembly](https://docs.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-6.0#route-to-components-from-multiple-assemblies), so that the router can find our pages.

```html
<Router AppAssembly="@typeof(Program).Assembly" AdditionalAssemblies="new[] { typeof(MainLayout).Assembly }">
```

Run the application and verify that everything still works as before.

Now for the other goal: we're going to move the shared UI code into a reusable component, making sure that we can also configure which buttons to show depending on the scenario.

## The PhotoDetailsComponent

First of all, let's create a `Components` folder under our `PhotoSharingApplication.Frontend.BlazorComponents` project.

In the `Components` folder, create a new `Razor Component` called  `PhotoDetailsComponent.razor`.

Now cut the `MatCard` of the `AllPhotos` component and paste it in the `PhotoDetailsComponent` component.

```html
<MatCard>
  <div>
    <MatHeadline6>
      @Photo.Id - @Photo.Title
    </MatHeadline6>
    <MatSubtitle2>
      @Photo.CreatedDate.ToShortDateString()
    </MatSubtitle2>
  </div>
  <MatCardContent>
    <MatCardMedia Wide="true" ImageUrl="@(Photo.PhotoFile is null ? "" : $"data:{Photo.ImageMimeType};base64,{Convert.ToBase64String(Photo.PhotoFile)}")" ></MatCardMedia>
    <MatBody2>
      @Photo.Description
    </MatBody2>
  </MatCardContent>
  <MatCardActions>
    <MatCardActionButtons>
      <MatButton Link="@($"photos/details/{Photo.Id}")">Details</MatButton>
      <MatButton Link="@($"photos/update/{Photo.Id}")">Update</MatButton>
      <MatButton Link="@($"photos/delete/{Photo.Id}")">Delete</MatButton>
    </MatCardActionButtons>
  </MatCardActions>
</MatCard>
```

This time, instead of loading the product by asking it to the service, we will accept it as a [Parameter](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/?view=aspnetcore-6.0#parameters). So the `code` becomes

```cs
@code {
  [Parameter]
  public Photo Photo { get; set; }
}
```
Now let's [use the component](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/?view=aspnetcore-6.0#use-components)  from within the `AllPhotos` page.

We could already do it like this:

```html
<div class="mat-layout-grid">
  <div class="mat-layout-grid-inner">
    @foreach (var photo in photos) {
      <div class="mat-layout-grid-cell mat-layout-grid-cell-span-4">
        <PhotoSharingApplication.Frontend.BlazorComponents.Components.PhotoDetailsComponent Photo="photo"></PhotoSharingApplication.Frontend.BlazorComponents.Components.PhotoDetailsComponent>
      </div>
    }
  </div>
</div>
```

It's a bit annoying that we have to write the whole [namespace](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/?view=aspnetcore-6.0#namespaces).
To avoid this (here and in the other pages where we will use this components), let's add the `using` on our `_Imports.razor` in the `@using PhotoSharingApplication.Frontend.BlazorComponents` project:

```cs
@using PhotoSharingApplication.Frontend.BlazorComponents.Components
```

This means that we can now change the `AllPhotos.razor` like this:

```html
<div class="mat-layout-grid">
  <div class="mat-layout-grid-inner">
    @foreach (var photo in photos) {
      <div class="mat-layout-grid-cell mat-layout-grid-cell-span-4">
        <PhotoDetailsComponent Photo="photo"></PhotoDetailsComponent>
      </div>
    }
  </div>
</div>
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
<MatCardActionButtons>
  @if (Details) {
    <MatButton Link="@($"photos/details/{Photo.Id}")">Details</MatButton>
  }
  @if (Edit) {
    <MatButton Link="@($"photos/update/{Photo.Id}")">Update</MatButton>
  }
  @if (Delete) {
    <MatButton Link="@($"photos/delete/{Photo.Id}")">Delete</MatButton>
  }
  @if (DeleteConfirm) {
    <MatButton>Confirm Deletion</MatButton>
  }
  </MatCardActionButtons>
```

Now let's have the `AllPhotos` component it needs to show. We don't need to pass the rest, as they are already `false`.

```html
<PhotoDetailsComponent Photo="photo" Details Edit Delete></PhotoDetailsComponent>
```

Let's repeat for the `PhotoDetails.razor` and `DeletePhoto.razor` Pages.

## Details page

```html
<div class="mat-layout-grid">
  <div class="mat-layout-grid-inner">
    <div class="mat-layout-grid-cell mat-layout-grid-cell-span-12">
      <PhotoDetailsComponent Photo="photo" Edit Delete></PhotoDetailsComponent>
    </div>
  </div>
</div>
```

## Delete Page

```html
<div class="mat-layout-grid">
  <div class="mat-layout-grid-inner">
    <div class="mat-layout-grid-cell mat-layout-grid-cell-span-12">
      <PhotoDetailsComponent Photo="photo" DeleteConfirm></PhotoDetailsComponent>
    </div>
  </div>
</div>
```

The UI should work, but what shall we do with the button logic?

From a design perspective, we want our component to be totally ignorant of its surroundings. Not only does it not know where the data comes from, it also knows nothing about what the logic should do. All it does is 
- It renders html data received from a parent 
- It alerts the parent whenever it's time to perform an action

The action can be handled by the parent (the page in our case).

It's time to introduce [Event Handling](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/event-handling?view=aspnetcore-6.0).

We need to do is to define and handle an [EventCallback](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/event-handling?view=aspnetcore-6.0#eventcallback) for each event we want to define.

## DeleteConfirmed

We're going to tackle the deletion of a photo. 

In our `PhotoDetailsComponent`, let's define an `EventCallback` that can provide the `id` of the photo to delete:

```cs
[Parameter]
public EventCallback<int> OnDeleteConfirmed { get; set; }
```

Then, let's invoke the callback at the click of the button:

```html
<MatButton OnClick="@(async()=> await OnDeleteConfirmed.InvokeAsync(Photo.Id))">Confirm Deletion</MatButton>
```
Now let's handle the event in the `Delete` page.

First let's bind the event to a method:

```html
<PhotoDetailsComponent Photo="photo" DeleteConfirm OnDeleteConfirmed="Delete"></PhotoDetailsComponent>
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

Cut the `<MatCard>` from the `Upload` page and paste it into the new component. For consistency, change the `photo` property into a `Photo` property.

```html
<MatCard>
  <MatH3>Upload Photo</MatH3>
  <MatCardContent>
    <EditForm Model="@Photo" OnValidSubmit="HandleValidSubmit">
      <p>
        <MatTextField @bind-Value="@Photo.Title" Label="Title" FullWidth></MatTextField>
      </p>
      <p>
        <MatTextField @bind-Value="@Photo.Description" Label="Description" TextArea FullWidth></MatTextField>
      </p>
      <p>
        <MatFileUpload OnChange="@HandleMatFileSelected"></MatFileUpload>
      </p>
      <p>
        <MatButton Type="submit">Upload</MatButton>
      </p>
    </EditForm>
    <MatCardMedia Wide="true" ImageUrl="@(Photo.PhotoFile is null ? "" : $"data:{Photo.ImageMimeType};base64,{Convert.ToBase64String(Photo.PhotoFile)}")"></MatCardMedia>
  </MatCardContent>
</MatCard>
```

Add a `Parameter` of type `Photo`

```cs
@code {
  [Parameter]
  public Photo Photo { get; set; }
}
```

We can move here the `HandleMatFileSelected` code:

```cs
async Task HandleMatFileSelected(IMatFileUploadEntry[] files) {
  IMatFileUploadEntry file = files.FirstOrDefault();
  Photo.ImageMimeType = file.Type;

  if (file == null) {
    return;
  }

  using (var stream = new System.IO.MemoryStream()) {
    await file.WriteToStreamAsync(stream);
    Photo.PhotoFile = stream.ToArray();
  }
}
```

Once again, we want to delegate some logic to the parent component.

Let's add and EventCallback:

```cs
[Parameter]
public EventCallback<Photo> OnSave { get; set; }
```

Let's invoke the callback whenever the `EditForm` is submitted succesfully:

```html
<EditForm Model="@Photo" OnValidSubmit="@(async ()=> await OnSave.InvokeAsync(Photo))">
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

@inject IPhotosService photosService
@inject NavigationManager navigationManager

<PageTitle>Update Photo @photo?.Title</PageTitle>

@if (photo is null) {
    <p>...Loading...</p>
} else {
    <div class="mat-layout-grid">
        <div class="mat-layout-grid-inner">
            <div class="mat-layout-grid-cell mat-layout-grid-cell-span-12">
                <PhotoEditComponent Photo="photo" OnSave="Update"></PhotoEditComponent>
            </div>
        </div>
    </div>
}

@code {
    [Parameter]
    public int Id { get; set; }

    Photo? photo;

    protected override async Task OnInitializedAsync() {
        photo = await photosService.FindAsync(Id);
    }

    private async Task Update() {
        await photosService.UpdateAsync(photo!);
        navigationManager.NavigateTo("/photos/all");
    }
}
```

Run the application and ensure that everything works just like before refactoring.

## Further refactor

Both the `PhotoEditComponent.razor` and the `PhotoDetailsComponent.razor` share this bit:

```html
<MatCardMedia Wide="true" ImageUrl="@(Photo.PhotoFile is null ? "" : $"data:{Photo.ImageMimeType};base64,{Convert.ToBase64String(Photo.PhotoFile)}")"></MatCardMedia>
```

so we can move that into a `PhotoPictureComponent.razor`, then reference that from both the parents.

In the `Components` folder of the `PhotoSharingApplication.Frontend.BlazorComponents` project, add a new `PhotoPictureComponent.razor` Razor Component.

Cut the `<MatCardMedia>` from the `PhotoEditComponent` and paste it into the new component. 

```html
<MatCardMedia Wide="true" ImageUrl="@(Photo.PhotoFile is null ? "" : $"data:{Photo.ImageMimeType};base64,{Convert.ToBase64String(Photo.PhotoFile)}")"></MatCardMedia>
```


Add a `Photo` property.

```cs
@code {
    [Parameter]
    public Photo Photo { get; set; }
}
```

In both the `PhotoEditComponent` and the `PhotoDetailsComponent`, replace this code

```html
<MatCardMedia Wide="true" ImageUrl="@(Photo.PhotoFile is null ? "" : $"data:{Photo.ImageMimeType};base64,{Convert.ToBase64String(Photo.PhotoFile)}")"></MatCardMedia>
```

with this code

```html
<PhotoPictureComponent Photo="Photo"></PhotoPictureComponent>
```

Ok, we're done for this lab, we can now move on to the next step, where we build an actual Backend.

Time for Lab05!