# Interoperability between Blazor and Javascript

- https://github.com/dotnet-presentations/blazor-workshop/blob/master/docs/07-javascript-interop.md
- https://leafletjs.com/

Our goal for this lab is to add a map on the `PhotoDetailsPage`, showing a marker on the coordinates where the picture was taken.  
We're going to use [Leaflet](https://leafletjs.com/), an open-source JavaScript library for mobile-friendly interactive maps.  
Since Leaflet is a javascript library, we need to learn not only how to use Leaflet itself, but also how to invoke javascript from Blazor.

Let's start with a simple example, just to understand the steps to interoperate with javascript.  
After we are sure that we have everything setup, we'll include Leaflet.
For now, we'll start by adding a new `Map.razor` component, where we will invoke a simple `showMap` javascript function at the click of a button, just to see if we can do it.  
We will write the `showMap` javascript function in a separate `map.js` file. This function for now just uses the `alert` function, so that we can easily see if it works. We'll change it later to actually show a map.  
So, as a recap: the click handler of the button invokes a `ShowMap` C# function of ours, the `ShowMap` invokes `showMap` javascript function of ours, the `showMap` javascript function invokes the `alert` native javascript function.  
I promise, it sounds more complicated than it actually is.   

## Creating a Map.razor component

- In your `PhotoSharingApplication.Frontend.BlazorComponents` project, add a `Map.razor` razor component
- Add a `button` and handle the `click` event by invoking an `async` `ShowMap` method

```html
<button @onclick="@(async ()=>await ShowMap())">Click here to show the map</button>
```

We want to [call a JavaScript function from our .NET `ShowMap` method](https://docs.microsoft.com/en-us/aspnet/core/blazor/call-javascript-from-dotnet?view=aspnetcore-6.0) so we need to inject the [IJSRuntime](https://docs.microsoft.com/en-us/aspnet/core/blazor/call-javascript-from-dotnet?view=aspnetcore-6.0#ijsruntime).

```cs
@using Microsoft.JSInterop
@inject IJSRuntime js
```

Now we can [call a void function](https://docs.microsoft.com/en-us/aspnet/core/blazor/call-javascript-from-dotnet?view=aspnetcore-6.0#call-a-void-javascript-function) with the following code:

```cs
public async ValueTask ShowMap() {
    await module.InvokeVoidAsync("showMap");
}
```

Who's `module`? That is a [reference](https://docs.microsoft.com/en-us/aspnet/core/blazor/call-javascript-from-dotnet?view=aspnetcore-6.0#blazor-javascript-isolation-and-object-references) to a [javascript module](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Modules) that will contain our function. Let's declare it and initialize it:

```cs
IJSObjectReference module;
protected override async Task OnInitializedAsync() {
    module = await js.InvokeAsync<IJSObjectReference>("import", "./_content/PhotoSharingApplication.Frontend.BlazorComponents/map.js");
}
```

Now it's time to write our javascript function.

## Map.js

- In the `wwwroot` folder of the `PhotoSharingApplication.Frontend.BlazorComponents` project, add a `map.js` file.
- In the `map.js` file, add a `showMap` function that invokes the `alert` function

```js
export function showMap() {
    alert('here is your map! (well, sort of....)');
}
```

## Wiring everything up

- In the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project, open the `PhotoDetails.razor` component located under the `Pages` folder
- Add the `<Map>` component to the page

```html
<Map></Map>
```

One last step, described in the [Create an RCL with static assets](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/class-libraries?view=aspnetcore-6.0&tabs=visual-studio#create-an-rcl-with-static-assets) document, is:

- In the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project, open the `index.html` page located under the `wwwroot` folder
- Add the reference to the static assets with the following path in the app: `_content/{LIBRARY NAME}/{ASSET FILE NAME}`

```html
<script src="_content/PhotoSharingApplication.Frontend.BlazorComponents/map.js"></script>
```

## Try it out

Run the application, go to the `Details` of a photo and click on the button. You should see the message popping up.

We just learned how to invoke a javascript function form Blazor. Now it's time to talk to `Leaflet` to actually show a map.

## Preparing your page

Before writing any code for the map, you need to do the following preparation steps on your `index.html` page:

Include Leaflet CSS file in the head section of your document:

```html
 <link rel="stylesheet" href="https://unpkg.com/leaflet@1.7.1/dist/leaflet.css"
   integrity="sha512-xodZBNTC5n17Xt2atTPuE1HxjVMSvLVW9ocqUKLsCC5CXdbqCmblAshOMAS6/keqq/sMZMZ19scR4PsZChSR7A=="
   crossorigin=""/>
```
Include Leaflet JavaScript file after Leaflet’s CSS:

```html
 <script src="https://unpkg.com/leaflet@1.7.1/dist/leaflet.js"
   integrity="sha512-XQoYMqMTK8LvdxXYG3nZ448hOEQiglfqkJs1NOQV44cWnUrBc8PkAOcXy20w0vlaXaVUearIOBhiXZ5V3ynxwA=="
   crossorigin=""></script>
```

- In the `` component, add a div element with a `map` id

```html
 <div id="map"></div>
```

Make sure the map container has a defined height, setting it in an new [isolated CSS](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/css-isolation?view=aspnetcore-6.0) named `Map.razor.css`, located in the `PhotoSharingApplication.Frontend.BlazorComponents` project:

```css
#map { 
    height: 180px; 
    width: 100%;
}
```

Now you’re ready to initialize the map and do some stuff with it.

- Open the `` file, located in the `` project, under the `` folder
- Locate the `showMap` function and replace its code
- Here we create a map in the 'map' div, add tiles of our choice, and then add a marker with some text in a popup:

```js
export function showMap() {
    const map = L.map('map').setView([51.505, -0.09], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    L.marker([51.5, -0.09]).addTo(map)
        .bindPopup('A pretty CSS3 popup.<br> Easily customizable.')
        .openPopup();
}
```

## Try it out

Run the application, go to the `Details` of a photo and click on the button. You should see the map showing up, with a marker and a popup.

We just learned how to talk to `Leaflet` to show a map.  

## Pass parameters to the function

As you can see, the map shows a marker in London. This is because we are using `51.505, -0.09` as Latitude and Longitude.  
Also, the popup says `A pretty CSS3 popup.<br> Easily customizable.`, since we hardcoded that message in the javascript function.  
These are parameters that we will have to pass to our `showMap` function.  
Let's  modify the javascript function to accept parameter, then let's pass the parameters from within out Blazor component.

- In the `wwwroot` folder of the `PhotoSharingApplication.Frontend.BlazorComponents` project, open `map.js` file.
- Locate the `showMap` function and modify the signature to accept three parameters `lat`, `lon` and `msg`
- Use the parameters in the function instead of the hardcoded values

```js
export function showMap(lat, lon, msg) {
    const map = L.map('map').setView([lat, lon], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    L.marker([lat, lon]).addTo(map)
        .bindPopup(msg)
        .openPopup();
}
```

- In the `PhotoSharingApplication.Frontend.BlazorComponents` project, open the `Map.razor` component
- Use the [overload](https://docs.microsoft.com/en-us/dotnet/api/microsoft.jsinterop.jsruntimeextensions.invokevoidasync?view=dotnet-plat-ext-6.0#Microsoft_JSInterop_JSRuntimeExtensions_InvokeVoidAsync_Microsoft_JSInterop_IJSRuntime_System_String_System_Object___) to pass the arguments

```cs
public async ValueTask ShowMap() {
    await module.InvokeVoidAsync("showMap",51.505, -0.09, "A pretty CSS3 popup.<br> Easily customizable.");
}
```

## Try it out

Run the application, go to the `Details` of a photo and click on the button. You should see the map showing up, with the same marker and popup.

## Reading the data from the Photo

Obviously, what we're passing now is not what we want. We don't have the coordinates yet, but we can start by passing the Title of the photo as a message for the popup.  
We need to know which photo we're showing on the map, so we'll accept a `Photo` Parameter and use it to read the rest.

- In the `PhotoSharingApplication.Frontend.BlazorComponents` project, open the `Map.razor` component
- Add a `Photo` parameter
- Pass the `Photo.Title` to the `showMap` function

```cs
[Parameter]
public Photo Photo { get; set; }

public async ValueTask ShowMap() {
    await module.InvokeVoidAsync("showMap",51.505, -0.09, Photo.Title);
}
```

### Modify the Photo Entity

Let's add the `Latitude` and `Longitude` properties to the `Photo` table in our database

- In the `PhotoSharingApplication.Shared.Core` project, under the `Entities` folder, open the `Photo` class
- Add a `Latitude` and a `Longitude` property of type `double`

```cs
public double Latitude { get; set; }
public double Longitude { get; set; }
```

- Add a `LatLon` migration, like we did in [Lab 5](https://github.com/scolapicchioni/blazor/tree/master/Lab05#generate-migrations-and-database)

```
Add-Migration LatLon -Project PhotoSharingApplication.Backend.Infrastructure -StartupProject PhotoSharingApplication.WebServices.REST.Photos
```
There should be a new file in the `Migrations` folder of the `PhotoSharingApplication.Backend.Infrastructure` project.

- Update the Database

```
Update-Database -Project PhotoSharingApplication.Backend.Infrastructure -StartupProject PhotoSharingApplication.WebServices.REST.Photos
```

Your db should now have two new columns. If you have photos in your table, they should all have `0`,`0` as values.  
Optionally, you may change the values in the db, so that you can then try the application and see the marker in different positions.  
If you write random values, remember that `Latitude` should be between -90 and 90, while `Longitude` should be between -180 and 180.

### Use Latitude and Longitude for the Map

- In the `PhotoSharingApplication.Frontend.BlazorComponents` project, open the `Map.razor` component
- Pass the `Photo.Latitude` and the `Photo.Longitude` to the `showMap` function

```cs
[Parameter]
public Photo Photo { get; set; }

public async ValueTask ShowMap() {
    await module.InvokeVoidAsync("showMap",Photo.Latitude, Photo.Longitude, Photo.Title);
}
```

### Try it out
If you run the application now and go to the details of a photo with `0`,`0`, you should see the marker on the Equator, at the Greenwitch parallel.

## Extract GPS location from the picture metadata

Our next task is to write the GPS data of the picture into our db. We could, of course, just add a couple of textboxes and let the user write down the coordinates, but where's the fun in that? It would be much better for the user if we could read the coordinates from the file she selects from her device. So this is what we're trying to do in our next step: read the [EXIF metadata](https://en.wikipedia.org/wiki/Exchangeable_image_file_format) to retrieve the GPS location, then write them our fields.
We're going to try to reproduce this [example](https://awik.io/extract-gps-location-exif-data-photos-using-javascript/), which uses [exif.js](https://github.com/exif-js/exif-js).

## Add exif.js

Our first step is to add exif.js to our `index.html` page.

- In the `PhotoSharingApplication.Frontend.BlazorWebAssembly` project, open the `index.html` page located under the `wwwroot` folder
- Add the reference to the minified version of exif.js hosted on jsDelivr

```html
<script src="https://cdn.jsdelivr.net/npm/exif-js"></script>
```

> Start with calling the EXIF.getData function. You pass it an image as a parameter:
> 
> either an image from a `<img src="image.jpg">`
> OR a user selected image in a `<input type="file">` element on your page.
> As a second parameter you specify a callback function. In the callback function you should use this to access the image with the aforementioned metadata you can then use as you want. That image now has an extra exifdata property which is a Javascript object with the EXIF metadata. You can access its properties to get data like the image caption, the date a photo was taken or its orientation.

- In the `PhotoSharingApplication.Frontend.BlazorComponents` project, open the `map.js` javascript file located under the `wwwroot` folder
- Add an `extractCoords` function with one `img` parameter
- Invoke `EXIF.getData` passing `img` as first parameter and a function as second parameter
- In the function, print to the console `this.exifdata` and `EXIF.getAllTags(this)` to check what data we get


```js
export function extractCoords(img) {
    EXIF.getData(img, function () {

        const myData = this;

        console.log(myData.exifdata);

        var allMetaData = EXIF.getAllTags(this);
        console.log(JSON.stringify(allMetaData, null, "\t"));
    });
}
```

### Try it out

If we check the [source code of exif](https://github.com/exif-js/exif-js/blob/53b0c7c1951a23d255e37ed0a883462218a71b6f/exif.js#L368), we can see that one of the parameters it can expect is an object with a `src` property in a [Data URI](https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/Data_URIs) format. So when a user selects an image, we're going to construct such an object and pass it to the javascript  function we wrote.  

For now, just to see if it works, let's write this code on the `PhotoEditComponent`, where the user selects a file from her device.
- In the `PhotoSharingApplication.Frontend.BlazorComponents` project, open the `PhotoEditComponent.razor` component
- Inject the `IJSRuntime`

```cs
@using Microsoft.JSInterop
@inject IJSRuntime js
```

- Initialize a `module`, like we did previously (you may have to change the `OnInitialized` to `OnInitializedAsync`)

```cs
IJSObjectReference module;
protected override async Task OnInitializedAsync() {
    if (Photo.PhotoImage is null) Photo.PhotoImage = new PhotoImage();
    module = await js.InvokeAsync<IJSObjectReference>("import", "./_content/PhotoSharingApplication.Frontend.BlazorComponents/map.js");
}
```

- Write a `ExtractCoords` function, passing a new object with a `src` property constructed as a `Data URI`

```cs
public async ValueTask ExtractCoords() {
    await module.InvokeVoidAsync("extractCoords",
        new {
            src = Photo.PhotoImage?.PhotoFile is null ? "" :
            $"data:{Photo.PhotoImage.ImageMimeType};base64,{Convert.ToBase64String(Photo.PhotoImage.PhotoFile)}"
        });
}
```

At the end of `HandleMatFileSelected`, invoke `ExtractCoords`

```cs
async Task HandleMatFileSelected(IMatFileUploadEntry[] files) {
    IMatFileUploadEntry file = files.FirstOrDefault();
    if (file == null) {
        return;
    }
    if (Photo.PhotoImage is null) Photo.PhotoImage = new PhotoImage();
    Photo.PhotoImage.ImageMimeType = file.Type;
    using (var stream = new System.IO.MemoryStream()) {
        await file.WriteToStreamAsync(stream);
        Photo.PhotoImage.PhotoFile = stream.ToArray();
    }
    await ExtractCoords();
}
```

### Try it out

Run the application, go to the page to upload a photo, select a file, open the console and check the metadata.

## Latitude and Longitude

> We can use `myData.exifdata.GPSLatitude` and `myData.exifdata.GPSLongitude` arrays (3 values in each: `degree`, `minute`, `second`), and also `GPSLatitudeRef` and `GPSLongitudeRef` to get `latitude` and `longitude` decimal values, which is what we need.

Let’s write a function which will take the degrees, minutes, seconds and direction (E / W  N / S) and turn it into a decimal number:

```js
function ConvertDMSToDD(degrees, minutes, seconds, direction) {
    //DD = d + (min / 60) + (sec / 3600)
    let dd = degrees + (minutes / 60) + (seconds / 3600);

    if (direction == "S" || direction == "W") {
        dd = dd * -1;
    }

    return dd;
}
```

> We'll call the function twice – once to get the `Latitude` and once to get the `Longitude`. 

```js
function latlon(myData) {
    // Calculate latitude decimal
    const latDegree = myData.exifdata.GPSLatitude?.[0] ?? 0;
    const latMinute = myData.exifdata.GPSLatitude?.[1] ?? 0;
    const latSecond = myData.exifdata.GPSLatitude?.[2] ?? 0;
    const latDirection = myData.exifdata.GPSLatitudeRef ?? "N";
    
    const latFinal = ConvertDMSToDD(latDegree, latMinute, latSecond, latDirection);
    console.log("Latitude", latFinal);

    // Calculate longitude decimal
    const lonDegree = myData.exifdata.GPSLongitude?.[0] ?? 0;
    const lonMinute = myData.exifdata.GPSLongitude?.[1] ?? 0;
    const lonSecond = myData.exifdata.GPSLongitude?.[2] ?? 0;
    const lonDirection = myData.exifdata.GPSLongitudeRef ?? "E";
    
    const lonFinal = ConvertDMSToDD(lonDegree, lonMinute, lonSecond, lonDirection);
    console.log("Longitude", lonFinal);
}
```

Let's call this function from our previous `extractCoords`

```js
export function extractCoords(img) {
    EXIF.getData(img, function () {

        const myData = this;

        console.log(myData.exifdata);

        var allMetaData = EXIF.getAllTags(this);
        console.log(JSON.stringify(allMetaData, null, "\t"));

        latlon(myData);
    });
}
```

### Try it out

Run the application, go to the page to upload a photo, select a file with gps data, open the console and check the metadata.

## Pass the results back to Blazor

Now we  need to take our two results and pass them back to Blazor.  
We need to learn how to [call .NET methods from JavaScript functions in ASP.NET Core Blazor](https://docs.microsoft.com/en-us/aspnet/core/blazor/call-dotnet-from-javascript?view=aspnetcore-6.0).

As described in the [docs](https://docs.microsoft.com/en-us/aspnet/core/blazor/call-dotnet-from-javascript?view=aspnetcore-6.0#component-instance-method-call)

> To invoke a static .NET method from JavaScript, use the `DotNet.invokeMethod` or `DotNet.invokeMethodAsync` functions. Pass in the identifier of the static method you wish to call, the name of the assembly containing the function, and any arguments. The asynchronous version is preferred to support Blazor Server scenarios. The .NET method must be public, static, and have the `[JSInvokable]` attribute. 
>
> To invoke a component's .NET methods:
>
> Use the `invokeMethod` or `invokeMethodAsync` function to make a static method call to the component.
> The component's static method wraps the call to its instance method as an invoked `Action`.

Our ultimate goal is to invoke something like this:

```cs
public void UpdatePhotoCoords(double latitude, double longitude) {
    Photo.Latitude = latitude;
    Photo.Longitude = longitude;
}
```

But it would be too easy if we could do this directly. Instead, from javascript we can only invoke a static function, like this one:

```cs
[JSInvokable]
public static Task UpdatePhotoCoordinates(double latitude, double longitude) {

}
```

In javascript we would invoke it like this:

```js
DotNet.invokeMethodAsync('PhotoSharingApplication.Frontend.BlazorComponents', 'UpdatePhotoCoordinates', latitude, longitude);
```

So, what should our static `UpdatePhotoCoordinates` do?  
It should use an `Action` to invoke our `UpdatePhotoCoords`.  
We need to 
- declare a `static Action<double, double>`, 
- connect the `Action` with our `UpdatePhotoCoords`
- Invoke the Action from the static `UpdatePhotoCoordinates`

- In the `PhotoSharingApplication.Frontend.BlazorComponents` project, the `PhotoEditComponent.razor` component now has


```cs
@code {
    [Parameter]
    public Photo Photo { get; set; }

    [Parameter]
    public EventCallback<Photo> OnSave { get; set; }

    async Task HandleMatFileSelected(IMatFileUploadEntry[] files) {
        IMatFileUploadEntry file = files.FirstOrDefault();
        if (file == null) {
            return;
        }
        if (Photo.PhotoImage is null) Photo.PhotoImage = new PhotoImage();
        Photo.PhotoImage.ImageMimeType = file.Type;
        using (var stream = new System.IO.MemoryStream()) {
            await file.WriteToStreamAsync(stream);
            Photo.PhotoImage.PhotoFile = stream.ToArray();
        }

        await ExtractCoords();
    }

    IJSObjectReference module;
    protected override async Task OnInitializedAsync() {
        if (Photo.PhotoImage is null) Photo.PhotoImage = new PhotoImage();

        action = UpdatePhotoCoords;

        module = await js.InvokeAsync<IJSObjectReference>("import", "./_content/PhotoSharingApplication.Frontend.BlazorComponents/map.js");
    }

    public async ValueTask ExtractCoords() {
        await module.InvokeVoidAsync("extractCoords",
            new {
                src = Photo.PhotoImage?.PhotoFile is null ? "" :
                $"data:{Photo.PhotoImage.ImageMimeType};base64,{Convert.ToBase64String(Photo.PhotoImage.PhotoFile)}"
            });
    }

    private static Action<double, double> action;

    [JSInvokable]
    public static Task UpdatePhotoCoordinates(double latitude, double longitude) {
        action.Invoke(latitude,longitude);
        return Task.CompletedTask;
    }

    public void UpdatePhotoCoords(double latitude, double longitude) {
        Photo.Latitude = latitude;
        Photo.Longitude = longitude;
    }
}
```

while the `map.js` can become

```js
// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

export function showMap(lat, lon, msg) {
    const map = L.map('map').setView([lat, lon], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    L.marker([lat, lon]).addTo(map)
        .bindPopup(msg)
        .openPopup();
}

export function extractCoords(img) {
    EXIF.getData(img, function () {
        latlon(this);
    });
}

function ConvertDMSToDD(degrees, minutes, seconds, direction) {
    let dd = degrees + (minutes / 60) + (seconds / 3600);

    if (direction == "S" || direction == "W") {
        dd = dd * -1;
    }

    return dd;
}

function latlon(myData) {
    // Calculate latitude decimal
    const latDegree = myData.exifdata.GPSLatitude?.[0] ?? 0;
    const latMinute = myData.exifdata.GPSLatitude?.[1] ?? 0;
    const latSecond = myData.exifdata.GPSLatitude?.[2] ?? 0;
    const latDirection = myData.exifdata.GPSLatitudeRef ?? 'N';
    
    const latFinal = ConvertDMSToDD(latDegree, latMinute, latSecond, latDirection);

    // Calculate longitude decimal
    const lonDegree = myData.exifdata.GPSLongitude?.[0] ?? 0;
    const lonMinute = myData.exifdata.GPSLongitude?.[1] ?? 0;
    const lonSecond = myData.exifdata.GPSLongitude?.[2] ?? 0;
    const lonDirection = myData.exifdata.GPSLongitudeRef ?? 'E';
    
    const lonFinal = ConvertDMSToDD(lonDegree, lonMinute, lonSecond, lonDirection);

    DotNet.invokeMethodAsync('PhotoSharingApplication.Frontend.BlazorComponents', 'UpdatePhotoCoordinates', latFinal, lonFinal);
}
```

### Try it out

Upload a photo with geo tags metadata. Then go to the details of the photo. You should see the location on the map.

We're done!