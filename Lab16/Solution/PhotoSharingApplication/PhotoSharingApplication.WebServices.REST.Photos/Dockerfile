#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PhotoSharingApplication.WebServices.Rest.Photos/PhotoSharingApplication.WebServices.Rest.Photos.csproj", "PhotoSharingApplication.WebServices.Rest.Photos/"]
COPY ["PhotoSharingApplication.Shared.Authorization/PhotoSharingApplication.Shared.Authorization.csproj", "PhotoSharingApplication.Shared.Authorization/"]
COPY ["PhotoSharingApplication.Shared/PhotoSharingApplication.Shared.csproj", "PhotoSharingApplication.Shared/"]
RUN dotnet restore "PhotoSharingApplication.WebServices.Rest.Photos/PhotoSharingApplication.WebServices.Rest.Photos.csproj"
COPY . .
WORKDIR "/src/PhotoSharingApplication.WebServices.Rest.Photos"
RUN dotnet build "PhotoSharingApplication.WebServices.Rest.Photos.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PhotoSharingApplication.WebServices.Rest.Photos.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PhotoSharingApplication.WebServices.Rest.Photos.dll"]