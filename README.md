# Building a PhotoSharing Application with Blazor Web Assembly, Web API, gRPC and Identity Server

We're going to build a simple web site where people can post pictures and comments.
- Everyone can browse existing pictures and comments.
- Only authenticated users can upload new pictures and comments.
- Only a picture owner can edit or delete a picture.
- Only a comment owner can delete or update a comment.

We are going to build 3 parts. 
- The *FrontEnd*, a **Blazor Client** Web Application
- The *Backend*, built with .NET 5, will consist of 
    - A **REST** service for the managing of the pictures
    - A **gRPC** service for the comments
- The *Identity Provider* will be our own **Identity Server 4**.

1 - FrontEnd
   - Blazor Client
   - HTML 5
   - CSS 3
   - Open Id Connect Client

This project will interact with the user through a browser by dinamically constructing an HTML user interface and will talk to the server by using **gRPC Web** and **HttpClient**.

2.1  - Photos REST Service 
   - .NET 5 Web API Controller
   - Entity Framework 5
   - Sql Server Database
   - Identity Server Client Authentication

2.2  - Comments gRPC Service 
   - .NET 5 gRPC Service
   - Entity Framework 5
   - Sql Server Database
   - Identity Server Client Authentication

These projects will be responsible to store the data on the server and respond to the client requests through http, json and protobuf.

3. Authentication Server
   - Identity Server 4
   - Entity Framework Core

This project will take care of the authentication part. It will issue JWT tokens that will be used by the client application to gain access to the services.

## What you already need to know:
- C#
- HTML 5
- CSS 3

## What you're going to learn:
- REST
- gRPC
- Blazor
- ASP.NET 5 Web API Controller
- ASP.NET 5 gRPC Service
- Entity Framework
- PostMan
- CORS
- Authentication and Authorization
- OAuth 2 and Open Id Connect
- Identity Server 4
- Simple Authorization
- Resource Owner Authorization
- CLEAN Architecture

## Before you begin

At the time of the writing of this tutorial, .NET 5 is in preview and  Blazor Web Assembly is in Release Candidate. It is recommended that you install the latest preview version of Visual Studio 2019 in order to work with them.

If you're following this tutorial in the future (after November 2020), you may just need a normal version of Visual Studio which may come with .NET 5 on board, so your installations may vary. 
 
- Install the latest preview of [Visual Studio 2019](https://visualstudio.microsoft.com/vs/preview/) with the **ASP.NET and web development** workload  
- Install .NET 5 by downloading the [.NET 5 SDK](https://dotnet.microsoft.com/download/dotnet/5.0)
- Add the Blazor WebAssembly templates, which you can at the moment install with the following command 
```
dotnet new -i Microsoft.AspNetCore.Components.WebAssembly.Templates::3.2.0-rc1.20223.4
```

You may want to go to the [getting started](https://docs.microsoft.com/en-gb/aspnet/core/blazor/get-started?view=aspnetcore-5.0&tabs=visual-studio) documentation for an updated set of instructions.

---

# Our workflow

We are going follow simple steps. Each step will focus on one task and will build on top of the previous step. We will start with simple projects that will become more and more complex along the way. For example, we will not focus on authentication and authorization at first. We will add it at a later step.

# How to follow this tutorial

This folder contains different subfolders. Each subfolder represents a phase in our project. `Start` folders are the starting points of each step. `Solution` folders are the final versions of each step, given to you just in case you want to check what your project is supposed to become at the end of each lab.
What you have to do is to open a `Start` folder corresponding to the lab you want to try (for example `Lab01/Start` in order to begin) and follow the instructions you find on the `readme.md` file. When you are done, feel free to compare your work with the solution provided in the `Solution` folder.     

# To START

1. Open the `Labs` folder
2. Navigate to the `Lab01` subfolder
3. Navigate to the `Start` subfolder
4. Follow the instructions contained in the `readme.md` file to continue


# If you want to see the final application

## Configure and start the Identity Server Application

- Open `Lab08\Solution\MarketPlace\IdentityServer\IdentityServer.sln` in Visual Studio
- Build the project but do not start it from Visual Studio
- Open a command prompt under the `Lab08\Solution\MarketPlace\IdentityServer` folder
- Type `dotnet run /seed`
- Navigate to `http://localhost:5002` and ensure that the project is up and running

## Configure and start the REST Service

- Open `Lab08\Solution\MarketPlace\Marketplace\MarketPlace.sln` in Visual Studio
- Build the project and start it from Visual Studio

## Configure and start the Javascript client 

- Open `Lab08\Solution\MarketPlace\spaclient` in Visual Studio Code
- Open a terminal window
- Type `npm install`
- Type `npm run serve`

## To Logon

- Username: alice
- Password: Pass123$


## A last note before we begin

I assume you're a C# programmer interested in building a web application. Depending on how old you are, you may have already used asp, aspx, mvc and / or razor pages and now you want to try blazor. You may be already familiar with HTML and CSS and maybe you even played with some javascript framwork such as Angular, React or Vue.

I am going to link a ton of documentation about web concepts and technologies, so don't worry if you're not a web developer expert, you can learn everything along the way. You *should* at least be fluent in C#, though, or this tutorial will be hard to follow. Most of the code that we're going to write will be, in fact, C#. We are also going to write some HTML, but that's easy to learn so that is not going to be a problem. 