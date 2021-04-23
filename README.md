# Building a PhotoSharing Application with Blazor Web Assembly, Web API, gRPC and Identity Server

We're going to build a simple web site where people can post pictures and comments.
- Everyone can browse existing pictures and comments.
- Only authenticated users can upload new pictures and comments.
- Only a picture owner can edit or delete a picture.
- Only a comment owner can delete or update a comment.

We are going to build 3 parts. 
- The *FrontEnd*, a **Blazor Client** Web Application
- The *Backend*, built with .NET 6.0, will consist of 
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
   - Entity Framework Core 6.0
   - Sql Server Database
   - Identity Server Client Authentication

2.2  - Comments gRPC Service 
   - .NET 5 gRPC Service
   - Entity Framework Core 6.0
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
- ASP.NET Core 6.0 Web API Controller
- ASP.NET Core 6.0 gRPC Service
- Entity Framework Core 6.0
- Swagger / OpenAPI
- CORS
- Authentication and Authorization
- OAuth 2 and Open Id Connect
- Identity Server 4
- Simple Authorization
- Resource Owner Authorization
- CLEAN Architecture
- Unit Testing with bUnit

## Before you begin

At the time of the writing of this tutorial, .NET Core 6.0 is in preview. It is recommended that you install the latest **preview** version of Visual Studio 2019 in order to work with .NET Core 6.0.

If you're following this tutorial in the future (after November 2021), you may just need a normal version of Visual Studio which may come with .NET Core 6.0 on board, so your installations may vary. 
 
- Install the latest **preview** of [Visual Studio 2019](https://visualstudio.microsoft.com/vs/preview/) with the **ASP.NET and web development** workload  
- Install .NET Core 6.0 by downloading the [.NET Core 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)

You may want to go to the [getting started](https://dotnet.microsoft.com/learn/aspnet/blazor-tutorial/install) documentation for an updated set of instructions.

---

# Our workflow

We are going to follow some simple steps. Each step will focus on one task and will build on top of the previous step. We will start with simple projects that will become more and more complex along the way. For example, we will not focus on authentication and authorization at first. We will add it at a later step.

# How to follow this tutorial

If you start from Lab01 and follow each readme.md, you can complete each lab and continue to the following one using your own code. No need to open neither the `Start` nor the `Solution` folders provided in this repo.

- `Start` folders are the starting points of each step.  
- `Solution` folders are the final versions of each step, given to you just in case you want to check what your project is supposed to become at the end of each lab.  

What you have to do is to open a `Start` folder corresponding to the lab you want to try (`Lab01/Start` in order to begin from scratch) and follow the instructions you find on the `readme.md` file. When you are done, feel free to compare your work with the solution provided in the `Solution` folder.     

# To START

1. Open the `Labs` folder
2. Navigate to the `Lab01` subfolder
3. Navigate to the `Start` subfolder
4. Follow the instructions contained in the `readme.md` file to continue


# If you want to see the final application

## Build the Identity Server DataBase

- Open a command prompt under the `Lab12\Solution\blazor\PhotoSharingApplication\PhotoSharingApplication.IdentityServer` folder
- Type `dotnet run /seed`


## Create the Photos REST Service DataBase

- Open `Lab12\Solution\blazor\PhotoSharingApplication\PhotoSharingApplication.sln` in Visual Studio
- Build the Solution
- Open the `Package Manager Console` and type the following command:

```
Update-Database -Project PhotoSharingApplication.Backend.Infrastructure -StartupProject PhotoSharingApplication.WebServices.REST.Photos
```

## Create Comments gRPC Service DataBase

- Open the `Package Manager Console` and type the following command

```
Update-Database -Project PhotoSharingApplication.Backend.Infrastructure -StartupProject PhotoSharingApplication.WebServices.Grpc.Comments
```

- Ensure that you have multiple startup projects:
   - IdentityServer
   - Rest
   - gRPC
   - Blazor WebAssembly

Start the application

## To Logon

- Username: alice
- Password: alice

Or

- Username: bob
- Password: bob

## A last note before we begin

I assume you're a C# programmer interested in building a web application. Depending on how old you are, you may have already used asp, aspx, mvc and / or razor pages and now you want to try blazor. You may be already familiar with HTML and CSS and maybe you even played with some javascript framwork such as Angular, React or Vue.

I am going to link a ton of documentation about web concepts and technologies, so don't worry if you're not a web developer expert, you can learn everything along the way. You *should* at least be fluent in C#, though, or this tutorial will be hard to follow. Most of the code that we're going to write will be, in fact, C#. We are also going to write some HTML, but that's easy to learn so that is not going to be a problem. 

# The Labs

- Lab 01 - The Blazor Frontend  
   - Exploring the structure of a Blazor Web Assembly project and creating our first page
- Lab 02 - Frontend: Additional Views  
   - CLEAN Architecture
   - Dependency Injection
   - Using additional Blazor Libraries through NuGet Packages
   - Routes
   - Data Binding
   - Event Handling
- Lab 03 - Frontend: Styling the UI with MatBlazor
   - Material Design
   - MatBlazor
   - Layout Pages
- Lab 04 - Frontend - Razor Class Libraries and Components
   - Creating a Razor Class Library
   - Using a Razor Class Library from within a project
   - Razor Components
   - Parent and Child Components
   - Properties
   - EventCallbacks
- Lab 05 - Backend: Web API with ASP.NET Core 6.0 and Visual Studio 2019 Preview
   - REST Protocol
   - Asp.NET Core Web Api
   - Controllers
   - Actions
   - Routes
   - Binding
   - Entity Framework Core
   - JSON
   - Swagger / OpenAPI
- Lab 06 - Frontend: Connecting with the Backend
   - HttpClient
   - HttpClient Configuration
   - GetFromJsonAsync
   - PostAsJsonAsync
   - PutAsJsonAsync
   - DeleteAsync
   - ReadFromJsonAsync
   - CORS
- Lab 07 - Frontend: Comments
   - More CLEAN architecture
   - More Components
- Lab 08 - Backend: gRPC with ASP.NET Core 6.0 and Visual Studio 2019 Preview
   - More CLEAN architecture
   - gRPC
   - protobuf
   - gRPC in Asp.Net Core
- Lab 09 - Frontend: Connecting with the Backend
   - gRPC Web
   - gRPC Client Web in .NET Core
   - Configuration
   - CORS
- Lab 10 - Security: Authentication and Authorization
   - Identity Server 4
   - Configuring the REST Service for JWT Bearer Authentication
   - Configuring the gRPC Service for JWT Bearer Authentication
   - Configuring the Blazor Client for JWT Bearer Authentication
   - Simple Authorization with the Authorize Attribute
   - Retrieving and passing JWT Bearer Tokens
- Lab 11 - Security: Resource Based Authorization
   - AuthorizationService
   - Policies
   - Requirements
   - Handlers
- Lab 12 - Performance Optimization
   - Entity Framework Table Splitting
   - Download a File from a REST Web Api Service
   - Browser Caching
- Lab 13 - Validation
   - Data Annotations
   - Fluent Validation in the Core Service 
   - Fluent Validation in ASP.NET Core REST Service
   - Fluent Validation in Blazor with Blazored.FluentValidation
- Lab 14 - Unit Testing Blazor Components with bUnit
   - Unit Testing
   - Mocking
   - xUnit
   - Moq
   - bUnit



