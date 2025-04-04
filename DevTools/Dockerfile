# Use .NET SDK 9.0 for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy everything inside src/DevTools/
COPY . ./
COPY src/DevTools.API/*.csproj ./src/DevTools.API/
COPY src/DevTools.Application/*.csproj ./src/DevTools.Application/
COPY src/DevTools.Domain/*.csproj ./src/DevTools.Domain/
COPY src/DevTools.Infrastructure/*.csproj ./src/DevTools.Infrastructure/

# Restore dependencies
WORKDIR /app/src/DevTools.API
RUN dotnet restore

WORKDIR /app
COPY . .

# Build & publish the app
WORKDIR /app/src/DevTools.API
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Use ASP.NET runtime to run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy built application
COPY --from=publish /app/publish .

# Copy dynamic tool plugins
COPY src/DevTools.Application/Tools /app/Tools

# Expose both HTTP and HTTPS ports
EXPOSE ${APP_HTTPS_PORT}

RUN mkdir -p /app/certificates
COPY ./certificates/aspnetcore.pfx /app/certificates

# Set environment variables
ENV ASPNETCORE_URLS="https://+:${APP_HTTPS_PORT}"
ENV ASPNETCORE_ENVIRONMENT=Development

# Mount HTTPS development certificates
RUN mkdir /https
VOLUME ["/https"]

# Run the application
ENTRYPOINT ["dotnet", "DevTools.API.dll"]
