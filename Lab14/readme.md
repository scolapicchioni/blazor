Lab 14 -Docker
[Creating a simple data-driven CRUD microservice](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/data-driven-crud-microservice)  
[Tutorial: Create a multi-container app with Docker Compose](https://docs.microsoft.com/en-us/visualstudio/containers/tutorial-multicontainer?view=vs-2019)  
[Hosting ASP.NET Core images with Docker Compose over HTTPS](https://docs.microsoft.com/en-us/aspnet/core/security/docker-compose-https?view=aspnetcore-5.0)
[ASP.NET Core Blazor hosting model configuration](https://docs.microsoft.com/en-us/aspnet/core/blazor/hosting-model-configuration?view=aspnetcore-5.0#configuration)  
[Development workflow for Docker apps](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/docker-application-development-process/)  
[Quickstart: Run SQL Server container images with Docker](https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-linux-ver15&pivots=cs1-powershell)     
[Containerising a Blazor WebAssembly App](https://chrissainty.com/containerising-blazor-applications-with-docker-containerising-a-blazor-webassembly-app/)  

```
dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\PhotoSharingApplication.WebServices.REST.Photos.pfx -p Pass@word  
dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\PhotoSharingApplication.WebServices.Grpc.Comments.pfx -p Pass@word  
dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\PhotoSharingApplication.IdentityServer.pfx -p Pass@word  
dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\PhotoSharingApplication.Frontend.BlazorWebAssembly.pfx -p Pass@word  
dotnet dev-certs https --trust  
```

- ${APPDATA}/ASP.NET/Https:/root/.aspnet/https:ro
      - ASPNETCORE_Kestrel__Certificates__Default__Password=Pass@word

C:\Users\Administrator\AppData\Roaming\nginx_certs

"C:\Program Files\Git\usr\bin\openssl.exe" req -new -x509 -newkey rsa:2048 -keyout dev-certificate.key -out dev-certificate.cer -days 365 -nodes -subj /CN=localhost