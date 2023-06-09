# The Blazor FrontEnd

The goal of our first lab is to start building our website. For now, we will focus on displaying the existing pictures.

Since we don't have a working backend, our data won't come from a real database yet. We'll fix that in a later lab.

Let's start by building the client side application using **Blazor**.

From the [official Blazor documentation:](https://learn.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-7.0)

## What is Blazor?

> Blazor is a framework for building interactive client-side web UI with .NET:
> - Create rich interactive UIs using C# instead of JavaScript.
> - Share server-side and client-side app logic written in .NET.
> - Render the UI as HTML and CSS for wide browser support, including mobile browsers.
> - Integrate with modern hosting platforms, such as Docker.
> - Build hybrid desktop and mobile apps with .NET and Blazor.

Take some time to read [the whole document](https://learn.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-7.0) to get a hang of what it is and how it works. Especially because, in order to begin, we need to make a choice between [Blazor Server](https://learn.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-7.0#blazor-server) and [Blazor Web Assembly](https://learn.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-7.0#blazor-webassembly).

Personally, I'm not a big fan of Blazor Server. Sure,

- The user doesn't need a modern fancy browser
- The loading time is not that long
- The code runs server side, so you don't even need REST services to access your data on the server
- Debugging works

But

-  *Every* client needs an *active open connection* with the server
- The server keeps the state of  the page in memory for *every* client
- If the connection is lost for more than a couple of seconds the state of the page is lost, the server doesn't know anything about the client anymore, everything explodes in a million pieces, AAAARGH
- If you have more than 5000 active users you need Azure to scale your site

I don't know, it seems like a bad idea to me, but maybe there's something I'm missing. I may probably want to consider such a technology for an intranet application, but for an Internet Web Site? Thanks, I'll pass.

So I guess we're left with Blazor Web Assembly (AKA Blazor Client), which means:
- The client downloads a ton of code ([but they are working on it](https://www.youtube.com/watch?v=iDLWv3xM1s0))
- The browser runs our C# code (kind of, it's actually [WASM](https://webassembly.org/), but as far as we're concerned we treat it as if it were C#, ok?)
- The code on the browser takes care of 
  - Dynamically building / updating the HTML of the page
  - Handling events such as the click of a button or on a link
  - Fetching data from the server
  - Updating the URL on the browser

Since we're going to have multiple hosted applications anyway (one for the Photos Rest Service, one for the Comments gRpc service, one for our Identity Provider), we're going to host our Blazor Wasm server side as well, meaning that the client will get the files from an ASP.NET core application (which, in technical terms, is usually referred to as BFF, Backend For Frontend).

So, now that we know what we're going to use, let's see how.

In short, you need:
- The latest Visual Studio 2022
- .NET 7  

- [Install the latest Visual Studio 2022](https://visualstudio.microsoft.com/vs/) *with the ASP.NET and web development workload*.
- [Install .NET 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

If my instructions don't work it's probably because the version changed, so take a look at the [Get Started](https://dotnet.microsoft.com/learn/aspnet/blazor-tutorial/install) page and follow those instructions on what to install (the versions change *pretty* often).

Anyway, when you're done installing, we can start creating our first project.

- Open Visual Studio.
- Create a new project.
- Select `Blazor WebAssembly App`. Select `Next`.
- In the `Solution Name` field, type `PhotoSharingApplication`
- In the `Project name` field, type `PhotoSharingApplication.Frontend` 
- Provide a `Location` for the project, such as `Lab01\Start`. 
- Be sure to select the latest .net version (.Net 7.0)
- Ensure that the `ASP.NET Core hosted` Checkbox is checked.
- None for the `Authentication Type`
- Check `Enable Https`
- Uncheck `Progressive Web Application`
- Select `Create`.

Three projects are created for us:
- PhotoSharingApplication.Frontend.Client
- PhotoSharingApplication.Frontend.Server
- PhotoSharingApplication.Frontend.Shared
with the `Server` as startup project.  

Ok, so now we have a working application (you can run it with `F5` if you want to see how it looks).

## What's going on?

It is going to be a very long journey to understand what's going on, so bear with me and don't get discouraged if you have the impression that it's overwhelming. It will all make sense in the end (hopefully).

We got a lot of folders and files, so let's start from the start.

What is important to understand is that no matter what address you type, if your server doesn't find it it will reply with the content of `index.html`. That's because in the `Program.cs` of the `Server` project, we find the line ```app.MapFallbackToFile("index.html");``` and that's where we tell the server to serve the `index.html` file if it doesn't find the requested resource.

So wether you navigate to `http://localhost:{yourport}`, `http://localhost:{yourport}/counter`, `http://localhost:{yourport}/some/other/address` or anywhere else, you are going to get `index.html`. 

You can find `index.html` in your `Client` project, under the `wwwroot` folder. This folder contains all static files (such as html, css, js, images etc.) and is served as the root folder of the web site. 

If you open `index.html`, you will find in the `<body>` section two interesting tags. 

The first one is 
```html
<div id="app">
    <svg class="loading-progress">
        <circle r="40%" cx="50%" cy="50%" />
        <circle r="40%" cx="50%" cy="50%" />
    </svg>
    <div class="loading-progress-text"></div>
</div>
```

The second one is

```html
<script src="_framework/blazor.webassembly.js"></script>
```

The `<script>` tag is where the magic happens: that is the file that starts downloading `dotnet.wasm` and lots and lots and *lots* of dlls. If you run the application and type `F12` in your browser (opening the developer tools), you can see on the *Network* tab all the files that the client gets (I told you they were a lot).
Between those dll you should see `PhotoSharingApplication.Frontend.Client.dll`, which is where our code resides.

So whenever you navigate to your server (no matter the address):

- `index.html` gets served
- `index.html` downloads `blazor.webassembly.js`
- `blazor.webassembly.js` downloads `dotnet.wasm` and all the necessary libraries, including our Blazor code

From now on, your server is free to shut itself down if it pleases, because your client is not going to need it anymore. All what follows happens on the browser.

- `dotnet.wasm` starts (it's the mono runtime) and runs `PhotoSharingApplication.Frontend.Client.dll`
- Our dll contains the `Program` class with the `Main` method
- The `Main` method 
  - creates a `WebAssemblyHostBuilder` (which acts as a sort of hosting environment for our application) 
  - adds `App` as a `RootComponent`, mounting it on the `app` id
  - builds and runs the `WebAssemblyHost`
- The `WebAssemblyHost` now looks for the `app` id (which we saw on the `index.html`) and replaces it with the `App` component
- The `App` component (which you can find in the root of your project, it's called `App.razor`) contains the `Router`  component (that `<Router>` tag that you see in `App.razor`)
- The [`Router` component](https://learn.microsoft.com/en-gb/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-7.0) (which is a Microsoft component,  not part of our project source code)
  - Checks the address in the browser
  - Checks if there's any component that registered itself on that address
  - If it finds it, it renders its content
  - If it doesn't, it renders a `<p>Sorry, there's nothing at this address.</p>`

How does a component register itself on a specific address? By using an `@page` directive.

Open the `Pages` folder in your project. You will find three *.razor* components:
- Counter.razor
- FetchData.razor
- Index.razor

If you open them you will see that they all start with an `@page` followed by a string. That string is the address (*route*) where they will be rendered. That's why if you navigate to the root of your site, you see the content of `index.razor`.

Whoa, that was a lot of talking just to understand how our page gets rendered....

But now, hopefully, you understand how to create a new page (which you now know is not an *actual* page, it's just a component that corresponds to a route).

## So let's create a new page.

- In the `Solution Explorer`, right click the `Pages` folder of the `Client` project and select `Add` -> `Razor Component`
- Name the file `AllPhotos.razor`

If you start the application now and navigate to `/photos/all` you'll see a `sorry, there's nothing at this adddress` message.
This is because we did not register the component with the route, so the `<Router>` component renders the `<NotFound>` part.

Open the `Pages/AllPhotos.razor` file in Visual Studio and insert this code on the first line:

```c#
@page "/photos/all"
```

Start your application and navigate to `http://localhost:{your port}/photos/all`. You should now see the content of your page (which is not much, but we'll fix this soon).

We did it! We created our first *Razor component*!

## Ok, but what is, actually, a *Razor component*?

Let's read some more [documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/?view=aspnetcore-7.0):

> A component is a self-contained chunk of user interface (UI), such as a page, dialog, or form. Components can be nested, reused, and shared among projects.



Read the [Component Classes](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/?view=aspnetcore-7.0#component-classes) chapter on the docs to understand how a component works.

As a recap: each component contains
- UI
- Logic for the UI

The UI is mostly HTML, but there can also be tags that *look like* HTML but they're actually nested components.
What you can also find are bits and pieces of C#, maybe because you want to loop through your data in order to build some table rows, or you want to conditionally render a button only under a certain condition and so on.

The logic can be contained in a `code` section or in a separate .cs file (more on this later). In the logic you can have data and behavior, in the form of properties and methods.

## The AllPhotos Component

Our goal is to display a list of photos.  
In future labs we will take care of 
- the UI by using Bootstrap
- the data by creating and using a REST service

For the time being we will display a simple list retrieved from memory.

Let's add some data and use it to dynamically render the UI.

According to the [docs](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/?view=aspnetcore-7.0#markup): 

> A component's UI is defined using Razor syntax, which consists of Razor markup, C#, and HTML. When an app is compiled, the HTML markup and C# rendering logic are converted into a component class. The name of the generated class matches the name of the file.
> 
> Members of the component class are defined in one or more @code blocks. In @code blocks, component state is specified and processed with C#:
> 
> - Property and field initializers.
> - Parameter values from arguments passed by parent components and route parameters.
> - Methods for user event handling, lifecycle events, and custom component logic.
> 
> Component members are used in rendering logic using C# expressions that start with the @ symbol. For example, a C# field is rendered by prefixing @ to the field name. 

So we need:
- a component member
- a C# expression that start with @

We write a component member as a variable or property in the `code` section. We write the C# expression starting with the `@` in the HTML.

Let's change the `AllPhotos.razor` component like this:

```cs
@page "/photos/all"

<h3>All Photos</h3>

<p>@photoTitle</p>

@code {
    string photoTitle = "My title";
}
```

If you run the application and navigate to `photos/all` you should see the paragraph with *My Title* in it.

Now of course we want to show something more than just a  title, so let's create a new data type for a Photo and use that instead of a simple string:

```cs
@page "/photos/all"

<h3>All Photos</h3>

<p>@photo.Id</p>
<p>@photo.Title</p>
<p>@photo.Description</p>

@code {
    Photo photo;

    class Photo {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}
```

Right now, running the application would result in an exception because our photo is null. 
Let's correct the UI to handle this problem, by testing if the photo is null and conditionally render a loading message if it is. For this we will use an [if Razor control structure](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-7.0#control-structures):

```cs
@page "/photos/all"

<h3>All Photos</h3>

@if (photo is null)
{
    <p>...Loading...</p>
}
else
{
    <p>@photo.Id</p>
    <p>@photo.Title</p>
    <p>@photo.Description</p>
}
```

We need to create a new Photo instance and we can do it in one of the [lifecycle](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/lifecycle?view=aspnetcore-7.0) methods of our component:  

```cs
protected override void OnInitialized()
{
    photo = new Photo { Id = 1, Title = "My  Title", Description = "Lorem ipsum dolor sit amen" };
}
```

Running the application now should show the data we have.

We are expecting more than one picture, so let's change the code to have a List and let's loop through the list to build multiple UI elements by using a [foreach Razor control structure](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-7.0#looping-for-foreach-while-and-do-while).

The final code will look like this:

```cs
@page "/photos/all"

<h3>All Photos</h3>

@if (photos is null)
{
    <p>...Loading...</p>
}
else
{
    @foreach (var photo in photos)
    {
<article>
    <p>@photo.Id</p>
    <p>@photo.Title</p>
    <p>@photo.Description</p>
</article>
    }
}

@code {
    List<Photo>? photos;

    protected override void OnInitialized()
    {
        photos = new List<Photo>{
            new Photo { Id = 1, Title = "My Title", Description = "Lorem ipsum dolor sit amen" },
            new Photo { Id = 2, Title = "Another Title", Description = "All work and no play makes Jack a dull boy" },
            new Photo { Id = 3, Title = "Yet another Title", Description = "Some description" }
        };
    }

    class Photo
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}
```
Save your file. 
Go to the browser and verify that the page now contains three articles with the details of our photos.
Do not worry about the style. We will fix it on a later lab.

In the next lab we will build four additional pages:

- Details
- Insert
- Update
- Delete 

To continue, open ```Labs/Lab02/readme.md``` and follow the provided instructions.