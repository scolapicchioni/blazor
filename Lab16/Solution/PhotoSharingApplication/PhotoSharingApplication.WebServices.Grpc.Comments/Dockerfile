#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PhotoSharingApplication.WebServices.Grpc.Comments/PhotoSharingApplication.WebServices.Grpc.Comments.csproj", "PhotoSharingApplication.WebServices.Grpc.Comments/"]
COPY ["PhotoSharingApplication.Shared.Authorization/PhotoSharingApplication.Shared.Authorization.csproj", "PhotoSharingApplication.Shared.Authorization/"]
COPY ["PhotoSharingApplication.Shared/PhotoSharingApplication.Shared.csproj", "PhotoSharingApplication.Shared/"]
RUN dotnet restore "PhotoSharingApplication.WebServices.Grpc.Comments/PhotoSharingApplication.WebServices.Grpc.Comments.csproj"
COPY . .
WORKDIR "/src/PhotoSharingApplication.WebServices.Grpc.Comments"
RUN dotnet build "PhotoSharingApplication.WebServices.Grpc.Comments.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PhotoSharingApplication.WebServices.Grpc.Comments.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PhotoSharingApplication.WebServices.Grpc.Comments.dll"]