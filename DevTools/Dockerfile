# Use .NET SDK 9.0 for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy everything inside DevTools/ (this ensures DevTools.csproj is found)
COPY . ./

# Restore dependencies
WORKDIR /app/DevTools
RUN dotnet restore

# Build & publish the app
RUN dotnet publish -c Release -o out

# Use ASP.NET runtime to run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/DevTools/out .

COPY DevTools/Tools /app/Tools

ARG APP_PORT=5000
EXPOSE ${APP_PORT}

# Set environment variables
ENV ASPNETCORE_URLS=http://0.0.0.0:${APP_PORT:-5000}
ENV ASPNETCORE_ENVIRONMENT=Development

# Run the application
ENTRYPOINT ["dotnet", "DevTools.dll"]