# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY RestODb/*.csproj .
RUN dotnet restore --use-current-runtime  

# copy everything else and build app
COPY RestODb/. .
RUN dotnet publish --use-current-runtime --self-contained false --no-restore -o /app


# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 80 443

WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "RestODb.dll"]