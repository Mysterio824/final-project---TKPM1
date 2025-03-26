# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy project file and restore dependencies first
COPY DevTools/*.csproj ./DevTools/
RUN dotnet restore DevTools/DevTools.csproj

# Copy the entire project
COPY DevTools/ ./DevTools/

# Copy the Tools folder (DLLs) into the runtime container
COPY DevTools/Tools /app/Tools

# Build the application
WORKDIR /app/DevTools
RUN dotnet publish -c Release -o out

# Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/DevTools/out .

# Expose the port the app runs on
ARG APP_PORT=5000
EXPOSE ${APP_PORT}

# Set environment variables
ENV ASPNETCORE_URLS=http://0.0.0.0:${APP_PORT:-5000}
ENV ASPNETCORE_ENVIRONMENT=Development

# Set the entry point to run the app
ENTRYPOINT ["dotnet", "DevTools.dll"]