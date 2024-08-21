# Stage 1: Base runtime image
# This is the base image used to run your application. It is a lightweight image containing only the runtime environment.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: Build stage with the SDK
# This stage uses the full SDK image to build the application.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Step 1: Copy only the project files (.csproj)
# By copying only the .csproj files first, Docker can cache the restore step. 
# The restore step will only re-run if the project files change, not the entire source code.
COPY ["CommandService.csproj", "./"]

# Step 2: Run dotnet restore
# This restores dependencies based on the project files. Because we only copied the .csproj files,
# Docker can cache this layer, and it will only re-run if the .csproj files have changed.
RUN dotnet restore "CommandService.csproj"

# Step 3: Copy the remaining source code
# Now we copy the rest of the source code. Since this step comes after the restore,
# changes to the source code won't invalidate the cache of the restore step.
COPY . .

# Step 4: Build the application
# The build step compiles the application. This step runs only when source code changes.
RUN dotnet build "CommandService.csproj" -c Release -o /app/build

# Stage 3: Publish stage
# This stage prepares the application for production, creating a self-contained package.
FROM build AS publish
RUN dotnet publish "CommandService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final Stage: Runtime image
# This is the final image that will be used to run the application. It includes only the necessary files.
FROM base AS final
WORKDIR /app
# Copy the published output from the publish stage
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CommandService.dll"]
