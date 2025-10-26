# Use official .NET 8 SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything and restore dependencies
COPY . ./
RUN dotnet restore

# Build and publish the project
RUN dotnet publish -c Release -o /app/publish

# Use a smaller runtime image for final container
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Expose port 5191 (the default ASP.NET port)
EXPOSE 5191

# Set environment variable for Render (and ASP.NET Core)
ENV ASPNETCORE_URLS=http://+:5191

# Run the application
ENTRYPOINT ["dotnet", "mini-project-manager.dll"]
