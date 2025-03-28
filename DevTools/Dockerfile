# Use .NET SDK 9.0 for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy everything inside DevTools/ (ensures DevTools.csproj is found)
COPY . ./

# Restore dependencies
WORKDIR /app/DevTools
RUN dotnet restore

# Build & publish the app
RUN dotnet publish -c Release -o out

# Use ASP.NET runtime to run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy built application
COPY --from=build /app/DevTools/out .

# Copy dynamic tool plugins
COPY DevTools/Tools /app/Tools

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
ENTRYPOINT ["dotnet", "DevTools.dll"]
