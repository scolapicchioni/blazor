Lab 14 -Docker
[Creating a simple data-driven CRUD microservice](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/data-driven-crud-microservice)  
[Tutorial: Create a multi-container app with Docker Compose](https://docs.microsoft.com/en-us/visualstudio/containers/tutorial-multicontainer?view=vs-2019)  
[Hosting ASP.NET Core images with Docker Compose over HTTPS](https://docs.microsoft.com/en-us/aspnet/core/security/docker-compose-https?view=aspnetcore-5.0)
[ASP.NET Core Blazor hosting model configuration](https://docs.microsoft.com/en-us/aspnet/core/blazor/hosting-model-configuration?view=aspnetcore-5.0#configuration)  
[Development workflow for Docker apps](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/docker-application-development-process/)  
[Quickstart: Run SQL Server container images with Docker](https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-linux-ver15&pivots=cs1-powershell)     
[Containerising a Blazor WebAssembly App](https://chrissainty.com/containerising-blazor-applications-with-docker-containerising-a-blazor-webassembly-app/)  

```
dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\PhotoSharingApplication.WebServices.Rest.Photos.pfx -p password   
dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\PhotoSharingApplication.WebServices.Grpc.Comments.pfx -p password    
dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\PhotoSharingApplication.IdentityProvider.pfx -p password    
dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\PhotoSharingApplication.Frontend.Server.pfx -p password  
dotnet dev-certs https --trust  
```

```powershell
$cert = New-SelfSignedCertificate -CertStoreLocation Cert:\CurrentUser\My -DnsName photosharingapplication.webservices.rest.photos -Subject photosharingapplication.webservices.rest.photos
$pwd = ConvertTo-SecureString -String 'password' -Force -AsPlainText
$path = 'Cert:\CurrentUser\my\' + $cert.thumbprint
Export-PfxCertificate -cert $path -FilePath $env:USERPROFILE\.aspnet\https\photosharingapplication.webservices.rest.photos.pfx -Password $pwd

$cert = New-SelfSignedCertificate -CertStoreLocation Cert:\CurrentUser\My -DnsName photosharingapplication.webservices.grpc.comments -Subject photosharingapplication.webservices.grpc.comments
$pwd = ConvertTo-SecureString -String 'password' -Force -AsPlainText
$path = 'Cert:\CurrentUser\my\' + $cert.thumbprint
Export-PfxCertificate -cert $path -FilePath $env:USERPROFILE\.aspnet\https\photosharingapplication.webservices.grpc.comments.pfx -Password $pwd

$cert = New-SelfSignedCertificate -CertStoreLocation Cert:\CurrentUser\My -DnsName photosharingapplication.identityprovider -Subject photosharingapplication.identityprovider
$pwd = ConvertTo-SecureString -String 'password' -Force -AsPlainText
$path = 'Cert:\CurrentUser\my\' + $cert.thumbprint
Export-PfxCertificate -cert $path -FilePath $env:USERPROFILE\.aspnet\https\photosharingapplication.identityprovider.pfx -Password $pwd

$cert = New-SelfSignedCertificate -CertStoreLocation Cert:\CurrentUser\My -DnsName PhotoSharingApplication.Frontend.Server -Subject PhotoSharingApplication.Frontend.Server
$pwd = ConvertTo-SecureString -String 'password' -Force -AsPlainText
$path = 'Cert:\CurrentUser\my\' + $cert.thumbprint
Export-PfxCertificate -cert $path -FilePath $env:USERPROFILE\.aspnet\https\PhotoSharingApplication.Frontend.Server.pfx -Password $pwd

```




```powershell
$cert = New-SelfSignedCertificate -CertStoreLocation Cert:\LocalMachine\My -Subject photosharingapplication.webservices.rest.photos
$pwd = ConvertTo-SecureString -String 'password' -Force -AsPlainText
$path = 'Cert:\LocalMachine\My\' + $cert.thumbprint
Export-PfxCertificate -cert $path -FilePath $env:USERPROFILE\.aspnet\https\photosharingapplication.webservices.rest.photos.pfx -Password $pwd

$rootStore = New-Object System.Security.Cryptography.X509Certificates.X509Store -ArgumentList Root, LocalMachine
$rootStore.Open("MaxAllowed")
$rootStore.Add($cert)
$rootStore.Close()

$cert = New-SelfSignedCertificate -CertStoreLocation Cert:\LocalMachine\My -Subject photosharingapplication.webservices.grpc.comments
$path = 'Cert:\LocalMachine\My\' + $cert.thumbprint
Export-PfxCertificate -cert $path -FilePath $env:USERPROFILE\.aspnet\https\photosharingapplication.webservices.grpc.comments.pfx -Password $pwd

$rootStore.Open("MaxAllowed")
$rootStore.Add($cert)
$rootStore.Close()

$cert = New-SelfSignedCertificate -CertStoreLocation Cert:\LocalMachine\My -Subject photosharingapplication.identityprovider
$path = 'Cert:\LocalMachine\My\' + $cert.thumbprint
Export-PfxCertificate -cert $path -FilePath $env:USERPROFILE\.aspnet\https\photosharingapplication.identityprovider.pfx -Password $pwd

$rootStore.Open("MaxAllowed")
$rootStore.Add($cert)
$rootStore.Close()

$cert = New-SelfSignedCertificate -CertStoreLocation Cert:\LocalMachine\My -Subject PhotoSharingApplication.Frontend.Server
$path = 'Cert:\LocalMachine\My\' + $cert.thumbprint
Export-PfxCertificate -cert $path -FilePath $env:USERPROFILE\.aspnet\https\PhotoSharingApplication.Frontend.Server.pfx -Password $pwd

```

environment:
      - ASPNETCORE_Kestrel__Certificates__Default__Password=password
      - ASPNETCORE_Kestrel__Certificates__Default__Path=/certs/photosharingapplication.frontend.server.pfx
volumes:
      - ~/.aspnet/https:/certs:ro


C:\Users\Administrator\AppData\Roaming\nginx_certs

"C:\Program Files\Git\usr\bin\openssl.exe" req -new -x509 -newkey rsa:2048 -keyout dev-certificate.key -out dev-certificate.cer -days 365 -nodes -subj /CN=localhost

Step 1: create certs with same name of projects that need ssl
Step 2: create docker-compose.yml
Step 3: create docker-compose.override.yml
