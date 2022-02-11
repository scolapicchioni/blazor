#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PhotoSharingApplication.Frontend/Server/PhotoSharingApplication.Frontend.Server.csproj", "PhotoSharingApplication.Frontend/Server/"]
COPY ["PhotoSharingApplication.Frontend/Client/PhotoSharingApplication.Frontend.Client.csproj", "PhotoSharingApplication.Frontend/Client/"]
COPY ["PhotoSharingApplication.Frontend.BlazorComponents/PhotoSharingApplication.Frontend.BlazorComponents.csproj", "PhotoSharingApplication.Frontend.BlazorComponents/"]
COPY ["PhotoSharingApplication.Shared/PhotoSharingApplication.Shared.csproj", "PhotoSharingApplication.Shared/"]
COPY ["PhotoSharingApplication.Shared.Authorization/PhotoSharingApplication.Shared.Authorization.csproj", "PhotoSharingApplication.Shared.Authorization/"]
RUN dotnet restore "PhotoSharingApplication.Frontend/Server/PhotoSharingApplication.Frontend.Server.csproj"
COPY . .
WORKDIR "/src/PhotoSharingApplication.Frontend/Server"
RUN dotnet build "PhotoSharingApplication.Frontend.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PhotoSharingApplication.Frontend.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PhotoSharingApplication.Frontend.Server.dll"]