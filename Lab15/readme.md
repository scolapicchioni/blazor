# Interoperability between Blazor and Javascript

- https://github.com/dotnet-presentations/blazor-workshop/blob/master/docs/07-javascript-interop.md
- https://leafletjs.com/

Our goal for this lab is to add a map on the `PhotoDetailsPage`, showing a marker on the coordinates where the picture was taken.  
We're going to use [Leaflet](https://leafletjs.com/), an open-source JavaScript library for mobile-friendly interactive maps.  
Since Leaflet is a javascript library, we need to learn not only how to use Leaflet itself, but also how to invoke javascript from Blazor.

Let's start with a simple example, just to understand the steps to interoperate with javascript.  
After we are sure that we have everything setup, we'll include Leaflet.
For now, we'll start by adding a new `Map.razor` component.  
Our component will make use of a [separate class](https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/call-javascript-from-dotnet?view=aspnetcore-7.0#class-cs-example-invokeasync) of ours named `MapJsInterop.cs` where we will concentrate all the interactions between C# and javascript. This way, the razor component won't even know that it's interacting with javascript. We will receive the class as a dependency, that's why we will have to register our own class in the DI container.     
In our separate class, we'll load a separate javascript `map.js` file as a [module](https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/call-javascript-from-dotnet?view=aspnetcore-7.0#javascript-isolation-in-javascript-modules).  
Our cs class will also contain methods to [invoke javascript functions](https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/call-javascript-from-dotnet?view=aspnetcore-7.0#invoke-javascript-functions-without-reading-a-returned-value-invokevoidasync).  
We will write the `showMap` javascript function in a separate `map.js` file. This function for now just uses the `alert` function, so that we can easily see if it works. We'll change it later to actually show a map.  
So, as a recap: the click handler of the button invokes a `ShowMap` C# function of a separate class of ours, the `ShowMap` invokes `showMap` javascript function of ours, the `showMap` javascript function invokes the `alert` native javascript function.  
I promise, it sounds more complicated than it actually is.   

## Creating a MapComponent.razor component

- In your `PhotoSharingApplication.Frontend.BlazorComponents` project, under the `Components` folder, add a `MapComponent.razor` razor component
- Add a dependency to MapJsInterop (the class that we will write later)
- Add a `button` and handle the `click` event by invoking a `ShowMap` method

```html
<MudIconButton Icon="@Icons.Material.Filled.Map" OnClick="ShowMap" aria-label="map"></MudIconButton>
```
Finally, we can invoke the method of our class:

```cs
public async Task ShowMap() {
    await mapInterop.ShowMap();
}
```

## Creating MapJsInterop.cs

- Add a new class and name it `MapJsInterop.cs`. Let the class implement the `IAsyncDisposable` interface
- Add a private readonly field of type `Lazy<Task<IJSObjectReference>>` named `moduleTask`
- Add a constructor that takes a `IJSRuntime` as a parameter
- In the constructor, initialize the `moduleTask` field with a `Lazy<Task<IJSObjectReference>>` that creates a `Task<IJSObjectReference>` that loads the `map.js` module.  How to understand the path of the import is explained in the [Create an RCL with JavaScript files collocated with components](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/class-libraries?view=aspnetcore-7.0&tabs=visual-studio#create-an-rcl-with-javascript-files-collocated-with-components).

```cs
public class MapJsInterop : IAsyncDisposable {
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    public MapJsInterop(IJSRuntime jsRuntime) {
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/PhotoSharingApplication.Frontend.BlazorComponents/Components/MapComponent.razor.js").AsTask());
    }
}
```

- Add a method named `ShowMap` that takes no parameters and returns a `ValueTask`
- In the `ShowMap` method, call the `InvokeVoidAsync` method of the `moduleTask` field to [invoke](https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/call-javascript-from-dotnet?view=aspnetcore-7.0#invoke-javascript-functions-without-reading-a-returned-value-invokevoidasync) the `showMap` function.

The code should look like this:
```cs
public async ValueTask ShowMap() {
    var module = await moduleTask.Value;
    await module.InvokeVoidAsync("showMap");
}
```

- Dispose of the module when the class is disposed

```cs
public async ValueTask DisposeAsync() {
    if (moduleTask.IsValueCreated) {
        var module = await moduleTask.Value;
        await module.DisposeAsync();
    }
}
```

What's `module`? That is a [reference](https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/call-javascript-from-dotnet?view=aspnetcore-7.0#javascript-isolation-in-javascript-modules) to a [javascript module](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Modules) that will contain our function.  
Now it's time to write our javascript function.

## MapComponent.razor.js

- In the `Components` folder of the `PhotoSharingApplication.Frontend.BlazorComponents` project, add a `MapComponent.razor.js.js` file.
- In the `mapJsInterop.js` file, add a `showMap` function that invokes the `alert` function and export it

```js
export function showMap() {
   alert(`here's your map! (well, not yet but we're working on it)`);
}
```

## Wiring everything up

- In the `PhotoSharingApplication.Frontend.BlazorComponents` project, open the `PhotoDetails.razor` component located under the `Pages` folder
- Add the `<MapComponent>` component to the page

```html
<MapComponent/>
```
Last but not least, let's register our `MapJsInterop` class as a service in the DI container.  
- In the `PhotoSharingApplication.Frontend.Client` project, open the `Program.cs` file and add the following code:

```cs
builder.Services.AddScoped<MapJsInterop>();
```

which requires

```cs
using PhotoSharingApplication.Frontend.BlazorComponents;
```

## Try it out

Run the application, go to the `Details` of a photo and click on the button. You should see the message popping up.

We just learned how to invoke a javascript function form Blazor. Now it's time to talk to `Leaflet` to actually show a map.

## Preparing your page

Before writing any code for the map, you need to do the following preparation steps on your `index.html` page:

Include Leaflet CSS file in the head section of your document, as described in the [Leaflet Quickstart](https://leafletjs.com/examples/quick-start/):

```html
<link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"
     integrity="sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY="
     crossorigin=""/>
```
Include Leaflet JavaScript file after Leaflet's CSS:

```html
<script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"
     integrity="sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo="
     crossorigin=""></script>
```

- In the `MapComponent.razor` component, add a div element with a `map` id

```html
 <div id="map"></div>
```

Make sure the map container has a defined height, setting it in an new [isolated CSS](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/css-isolation?view=aspnetcore-7.0) named `MapComponent.razor.css`, to be added in the `Components` folder of the `PhotoSharingApplication.Frontend.BlazorComponents` project:

```css
#map { 
    height: 180px; 
    width: 100%;
}
```

Now you’re ready to initialize the map and do some stuff with it.

- Open the `MapComponent.razor.js` file, located in the `PhotoSharingApplication.Frontend.BlazorComponents` project, under the `Components` folder
- Locate the `showMap` function and replace its code
- Here we create a map in the 'map' div, add tiles of our choice, and then add a marker with some text in a popup:

```js
export function showMap() {
    const map = L.map('map').setView([51.505, -0.09], 13);

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    L.marker([51.5, -0.09]).addTo(map)
        .bindPopup('A pretty CSS popup.<br> Easily customizable.')
        .openPopup();
}
```

## Try it out

Run the application, go to the `Details` of a photo and click on the button. You should see the map showing up, with a marker and a popup. If it doesn't, you may need to empty the cache of your browser and reload the page.

We just learned how to talk to `Leaflet` to show a map.  

## Pass parameters to the function

As you can see, the map shows a marker in London. This is because we are using `51.505, -0.09` as Latitude and Longitude.  
Also, the popup says `A pretty CSS popup.<br> Easily customizable.`, since we hardcoded that message in the javascript function.  
These are parameters that we will have to pass to our `showMap` function.  
Let's  modify the javascript function to accept parameter, then let's pass the parameters from within out Blazor component.

- In the `wwwroot` folder of the `PhotoSharingApplication.Frontend.BlazorComponents` project, open `MapComponent.razor.js` file.
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

- In the `PhotoSharingApplication.Frontend.BlazorComponents` project, open the `MapJsInterop.cs`
- Modify the `ShowMap` method to accept a `double latitude, double longitude, string description` parameters
- Use the [overload](https://docs.microsoft.com/en-us/dotnet/api/microsoft.jsinterop.jsruntimeextensions.invokevoidasync?view=aspnetcore-7.0#microsoft-jsinterop-jsruntimeextensions-invokevoidasync(microsoft-jsinterop-ijsruntime-system-string-system-object())) to pass the arguments:

```cs
public async ValueTask ShowMap(double latitude, double longitude, string description) {
    await module.InvokeVoidAsync("showMap", latitude, longitude, description);
}
```

- Open the `MapComponent.razor` component
- Invoke the `HowMap` function passing the coordinates of Amsterdam

```cs
public async Task ShowMap() {
    await mapInterop.ShowMap(52.367, 4.904, "Hi from Amsterdam");
}
```
## Try it out

Run the application, go to the `Details` of a photo and click on the button. You should see the map showing up, with the marker and popup on Amsterdam (again, make sure to empty the browser cache and reload all the scripts).

## Reading the data from the Photo

Obviously, what we're passing now is not what we want. We don't have the coordinates yet, but we can start by passing the Title of the photo as a message for the popup.  
We need to know which photo we're showing on the map, so we'll accept a `Photo` Parameter and use it to read the rest.

- In the `PhotoSharingApplication.Frontend.BlazorComponents` project, open the `MapComponent.razor` component
- Add a `Photo` parameter
- Pass the `Photo.Title` to the `showMap` function

```cs
[Parameter, EditorRequired]
public Photo Photo { get; set; } = default!;

public async ValueTask ShowMap() {
    await mapInterop.ShowMap(52.367, 4.904, Photo.Title);
}
```

### Modify the Photo Entity

Let's add the `Latitude` and `Longitude` properties to the `Photo` table in our database

- In the `PhotoSharingApplication.Shared` project, under the `Entities` folder, open the `Photo` class
- Add a `Latitude` and a `Longitude` property of type `double`

```cs
public double Latitude { get; set; }
public double Longitude { get; set; }
```

- Add a `LatLon` migration, like we did in [Lab 5](https://github.com/scolapicchioni/blazor/tree/master/Lab05#generate-migrations-and-database)

```
Add-Migration LatLon -Project PhotoSharingApplication.WebServices.REST.Photos -StartupProject PhotoSharingApplication.WebServices.REST.Photos
```
There should be a new file in the `Migrations` folder of the `PhotoSharingApplication.Backend.Infrastructure` project.

- Update the Database

```
Update-Database -Project PhotoSharingApplication.WebServices.REST.Photos -StartupProject PhotoSharingApplication.WebServices.REST.Photos
```

Your db should now have two new columns. If you have photos in your table, they should all have `0`,`0` as values.  
Optionally, you may change the values in the db, so that you can then try the application and see the marker in different positions.  
If you write random values, remember that `Latitude` should be between -90 and 90, while `Longitude` should be between -180 and 180.

### Use Latitude and Longitude for the Map

- In the `PhotoSharingApplication.Frontend.BlazorComponents` project, open the `MapComponent.razor` component
- Pass the `Photo.Latitude`, `Photo.Longitude` and `Photo.Title` to the `ShowMap` function

```cs
[Parameter]
public Photo Photo { get; set; }

public async ValueTask ShowMap() {
    await mapInterop.ShowMap(Photo.Latitude, Photo.Longitude, Photo.Title);
}
```

- Open the `PhotoDetails.razor` file located under the `Pages` folder of the `PhotoSharingApplication.Frontend.BlazorComponents` project and pass the photo to the `MapComponent` component:

```html
<MapComponent Photo="photo" />
```

### Try it out
If you run the application now and go to the details of a photo with `0`,`0`, you should see the marker on the Equator, at the Greenwitch parallel.

## Extract GPS location from the picture metadata

Our next task is to write the GPS data of the picture into our db. We could, of course, just add a couple of textboxes and let the user write down the coordinates, but where's the fun in that? It would be much better for the user if we could read the coordinates from the file she selects from her device. So this is what we're trying to do in our next step: read the [EXIF metadata](https://en.wikipedia.org/wiki/Exchangeable_image_file_format) to retrieve the GPS location, then write them our fields.
We're going to try to reproduce this [example](https://awik.io/extract-gps-location-exif-data-photos-using-javascript/), which uses [exif.js](https://github.com/exif-js/exif-js).

## Add exif.js

Our first step is to add exif.js to our `index.html` page.

- In the `PhotoSharingApplication.Frontend.Client` project, open the `index.html` page located under the `wwwroot` folder
- Add the reference to the minified version of exif.js hosted on jsDelivr

```html
<script src="https://cdn.jsdelivr.net/npm/exif-js"></script>
```

We want to extract the coordinate while creating or updating a photo. So we will interact with javasctip in the `PhotoEditComponent.razor`. Let's write our javascript function in a new `PhotoEditComponent.razor.js`, under the `Components` folder.

> Start with calling the EXIF.getData function. You pass it an image as a parameter:
> 
> either an image from a `<img src="image.jpg">`
> OR a user selected image in a `<input type="file">` element on your page.
> As a second parameter you specify a callback function. In the callback function you should use this to access the image with the aforementioned metadata you can then use as you want. That image now has an extra exifdata property which is a Javascript object with the EXIF metadata. You can access its properties to get data like the image caption, the date a photo was taken or its orientation.

- In the `PhotoSharingApplication.Frontend.BlazorComponents` project, create a `PhotoEditComponent.razor.js` javascript file located under the `Components` folder
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

Now create a `CoordinatesJsInterop.cs` class in the root of the project and repeat the process to save the IJSRuntime and dispose it when necessary.

```cs
using Microsoft.JSInterop;

namespace PhotoSharingApplication.Frontend.BlazorComponents; 
public class CoordinatesJsInterop : IAsyncDisposable {
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    public CoordinatesJsInterop(IJSRuntime jsRuntime) {
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                      "import", "./_content/PhotoSharingApplication.Frontend.BlazorComponents/Components/PhotoEditComponent.razor.js").AsTask());
    }

    public async ValueTask DisposeAsync() {
        if (moduleTask.IsValueCreated) {
            IJSObjectReference module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
```
### Try it out

If we check the [source code of exif](https://github.com/exif-js/exif-js/blob/53b0c7c1951a23d255e37ed0a883462218a71b6f/exif.js#L368), we can see that one of the parameters it can expect is an object with a `src` property in a [Data URI](https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/Data_URIs) format. So when a user selects an image, we're going to construct such an object and pass it to the javascript function we wrote.  

- Add a new `ExtractCoords` method accepting an array of byte and a string as parameters
- Construct a string in the DataUri format
- Invoke `extractCoords` javascript function passing an object with a `src` property containing the string as parameter

```cs
public async ValueTask ExtractCoords(byte[]? photoFile, string mimeType) {
    string dataUri = photoFile is null ? "" : $"data:{mimeType};base64,{Convert.ToBase64String(photoFile)}";
    var module = await moduleTask.Value;
    await module.InvokeVoidAsync("extractCoords", new {src = dataUri});
}
```

Now let's use this method from within the `PhotoEditComponent`, where the user selects a file from her device.
- In the `PhotoSharingApplication.Frontend.BlazorComponents` project, open the `PhotoEditComponent.razor` component
- Inject the `CoordinatesJsInterop` class 

```cs
@inject CoordinatesJsInterop coordsInterop
```
At the end of `HandleFileSelected`, invoke `ExtractCoords`

```cs
private async Task HandleFileSelected(IBrowserFile args) {
    if (Photo.PhotoImage is null) Photo.PhotoImage = new PhotoImage();
    Photo.PhotoImage.ImageMimeType = args.ContentType;

    using (var streamReader = new System.IO.MemoryStream()) {
        await args.OpenReadStream().CopyToAsync(streamReader);
        Photo.PhotoImage.PhotoFile = streamReader.ToArray();
    }
    await coordsInterop.ExtractCoords(Photo.PhotoImage.PhotoFile, Photo.PhotoImage.ImageMimeType);
}
```

Run the application, go to the page to upload a photo, select a geotagged file, open the console and check the metadata.

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

    const latitude = ConvertDMSToDD(latDegree, latMinute, latSecond, latDirection);
    console.log("Latitude", latitude);

    // Calculate longitude decimal
    const lonDegree = myData.exifdata.GPSLongitude?.[0] ?? 0;
    const lonMinute = myData.exifdata.GPSLongitude?.[1] ?? 0;
    const lonSecond = myData.exifdata.GPSLongitude?.[2] ?? 0;
    const lonDirection = myData.exifdata.GPSLongitudeRef ?? "E";

    const longitude = ConvertDMSToDD(lonDegree, lonMinute, lonSecond, lonDirection);
    console.log("Longitude", longitude);
    return { latitude, longitude };
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

        let { latitude, longitude } = latlon(myData);
        console.log(`extractCoords has gotten the values lat: ${latitude} and lon: ${longitude}`);
    });
}
```

### Try it out

Run the application, go to the page to upload a photo, select a file with gps data, open the console and check the metadata.

## Pass the results back to Blazor

Now we  need to take our two results and pass them back to Blazor.  
Sadly, we can't just return the values from our `ExtractCoords` function, because the `EXIF.getData` that we're calling is extracting the coordinates in a callback. So in order to send the result back to Blazor, we need a C# callback to invoke when we're done in javascript.  
We need to learn how to [call .NET methods from JavaScript functions in ASP.NET Core Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/call-dotnet-from-javascript?view=aspnetcore-7.0).

As described in the [docs](https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/call-dotnet-from-javascript?view=aspnetcore-7.0#invoke-an-instance-net-method)

> To invoke an instance .NET method from JavaScript (JS):
>  
> Pass the .NET instance by reference to JS by wrapping the instance in a `DotNetObjectReference` and calling `Create` on it.  
> Invoke a .NET instance method from JS using `invokeMethod` or `invokeMethodAsync` from the passed `DotNetObjectReference`. The .NET instance can also be passed as an argument when invoking other .NET methods from JS.  
> Dispose of the `DotNetObjectReference`.

Our `ExtractCoords` c# function needs to be changed like this:

```cs
public async ValueTask ExtractCoords(byte[]? photoFile, string mimeType) {
    string dataUri = photoFile is null ? "" : $"data:{mimeType};base64,{Convert.ToBase64String(photoFile)}";
    var module = await moduleTask.Value;

    DotNetObjectReference<MapJsInterop>? objRef = DotNetObjectReference.Create(this);
    await module.InvokeVoidAsync("extractCoords", objRef, new { src = dataUri });
}
```

Which means that the `exctractCoords` javascript function has to accept a new parameter and use it to invoke a callback:

```js
export function extractCoords(dotNetHelper, img) {
    console.log(`extractCoords invoked with: ${img}`);
    EXIF.getData(img, function () {
        console.log(`innerfunction invoked with: ${this}`);
        const myData = this;

        console.log(myData.exifdata);

        let allMetaData = EXIF.getAllTags(this);
        console.log(JSON.stringify(allMetaData, null, "\t"));

        let { latitude, longitude } = latlon(myData);
        console.log(`extractCoords has gotten the values lat: ${latitude} and lon: ${longitude}`);
        dotNetHelper.invokeMethodAsync('GetLatitudeLongitude', latitude, longitude);
    });
}
```

The `GetLatitudeLongitude` is a c# method that we can write in the `CoordinatesJsInterop` class. It has to be marked with a `[JSInvokable]` attribute in order to be called by javascript.

```cs
[JSInvokable]
public void GetLatitudeLongitude(double Latitude, double Longitude) {

}
```

What should we do in this method? We need to pass the values back to our Blazor component.  
One technique that we can use is to get an `Action` from the Blazor component as a parameter and call it with the values.

Our `PhotoEditComponent` gets a new method:

```cs
public void UpdatePhotoCoords(double latitude, double longitude) {
    Photo.Latitude = latitude;
    Photo.Longitude = longitude;
}
```

In the `HandleFileSelected`, when we invoke the `coordsInterop.ExtractCoords`, let's pass this method as a parameter:

```cs
await coordsInterop.ExtractCoords(Photo.PhotoImage.PhotoFile, Photo.PhotoImage.ImageMimeType, UpdatePhotoCoords);
```

In the `CoordinatesJsInterop.cs`, let's save the Action in a private field, for future use:

```cs
private Action<double, double> callback;
public async ValueTask ExtractCoords(byte[]? photoFile, string mimeType, Action<double, double> callback) {
    this.callback = callback;
    // ...
}
```

Now we can use the callback in the `GetLatitudeLongitude` method:

```cs
[JSInvokable]
public void GetLatitudeLongitude(double Latitude, double Longitude) => callback(Latitude, Longitude);
```

### Try it out

Upload a photo with geo tags metadata. Then go to the details of the photo. You should see the location on the map.

We're done!