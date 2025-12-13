# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SMARTCAMPUS.API/SMARTCAMPUS.API.csproj", "SMARTCAMPUS.API/"]
COPY ["SMARTCAMPUS.BusinessLayer/SMARTCAMPUS.BusinessLayer.csproj", "SMARTCAMPUS.BusinessLayer/"]
COPY ["SMARTCAMPUS.DataAccessLayer/SMARTCAMPUS.DataAccessLayer.csproj", "SMARTCAMPUS.DataAccessLayer/"]
COPY ["SMARTCAMPUS.EntityLayer/SMARTCAMPUS.EntityLayer.csproj", "SMARTCAMPUS.EntityLayer/"]
RUN dotnet restore "./SMARTCAMPUS.API/SMARTCAMPUS.API.csproj"
COPY . .
WORKDIR "/src/SMARTCAMPUS.API"
RUN dotnet build "./SMARTCAMPUS.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SMARTCAMPUS.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SMARTCAMPUS.API.dll"]
