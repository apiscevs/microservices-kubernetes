# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 6666

# Stage 2: Build stage with the SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Step 1: Copy the project files (.csproj)
# Since the Dockerfile is in PlatformService, we need to go up one level to access ServiceDefaults
COPY ["PlatformService/PlatformService.csproj", "PlatformService/"]
COPY ["ServiceDefaults/ServiceDefaults.csproj", "ServiceDefaults/"]

# Step 2: Run dotnet restore
RUN dotnet restore "PlatformService/PlatformService.csproj"

# Step 3: Copy the remaining source code
# Ensure all necessary source files from both projects are copied
COPY . .

# Step 4: Build the application
RUN dotnet build "PlatformService/PlatformService.csproj" -c Release -o /app/build

# Stage 3: Publish stage
FROM build AS publish
RUN dotnet publish "PlatformService/PlatformService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final Stage: Runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PlatformService.dll"]

#docker build -t platformservice:dev -f PlatformService/Dockerfile .