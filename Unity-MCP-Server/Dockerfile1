# See https://docs.microsoft.com/aspnet/core/host-and-deploy/docker/building-net-docker-images
# for more information about using .NET with Docker.

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy main project file
COPY ["com.IvanMurzak.Unity.MCP.Server.csproj", "."]

# Restore dependencies
RUN dotnet restore "com.IvanMurzak.Unity.MCP.Server.csproj"

# Copy the rest of the source code
COPY . .

# Publish
RUN dotnet publish "com.IvanMurzak.Unity.MCP.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "unity-mcp-server.dll"]
