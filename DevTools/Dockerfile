# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy project file and restore dependencies first

# Copy the rest of the application source code
COPY . .

# Build the application
RUN dotnet publish -c Release -o out

# Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/out .

# Expose the port the app runs on
EXPOSE 5000

# Set environment variables
ENV ASPNETCORE_URLS=http://0.0.0.0:5000
ENV ASPNETCORE_ENVIRONMENT=Development

# Set the entry point to run the app
ENTRYPOINT ["dotnet", "DevTools.dll"]