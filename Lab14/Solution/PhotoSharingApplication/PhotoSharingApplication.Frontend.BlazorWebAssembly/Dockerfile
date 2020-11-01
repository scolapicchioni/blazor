FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["PhotoSharingApplication.Frontend.BlazorWebAssembly/PhotoSharingApplication.Frontend.BlazorWebAssembly.csproj", "PhotoSharingApplication.Frontend.BlazorWebAssembly/"]
COPY ["PhotoSharingApplication.Frontend.BlazorComponents/PhotoSharingApplication.Frontend.BlazorComponents.csproj", "PhotoSharingApplication.Frontend.BlazorComponents/"]
COPY ["PhotoSharingApplication.Shared.Authorization/PhotoSharingApplication.Shared.Authorization.csproj", "PhotoSharingApplication.Shared.Authorization/"]
COPY ["PhotoSharingApplication.Shared.Core/PhotoSharingApplication.Shared.Core.csproj", "PhotoSharingApplication.Shared.Core/"]
RUN dotnet restore "PhotoSharingApplication.Frontend.BlazorWebAssembly/PhotoSharingApplication.Frontend.BlazorWebAssembly.csproj"
COPY . .
WORKDIR "/src/PhotoSharingApplication.Frontend.BlazorWebAssembly"
RUN dotnet build "PhotoSharingApplication.Frontend.BlazorWebAssembly.csproj" -c Release -o /app/build
EXPOSE 443

FROM build AS publish
RUN dotnet publish "PhotoSharingApplication.Frontend.BlazorWebAssembly.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY ["PhotoSharingApplication.Frontend.BlazorWebAssembly/nginx.conf", "/etc/nginx/nginx.conf"]