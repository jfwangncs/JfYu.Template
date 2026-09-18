# syntax=docker/dockerfile:1

# See https://aka.ms/customizecontainer for details on customizing the .NET container.

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Version baked into the assembly; override at build time, e.g. --build-arg VERSION=1.0.42
ARG VERSION=1.0.0

# Copy only the project file first to leverage Docker layer caching for NuGet restore.
COPY ["JfYu.WebApi.Template.csproj", "./"]
RUN dotnet restore "JfYu.WebApi.Template.csproj"

# Copy the remaining source and publish a release build.
COPY . .
RUN dotnet publish "JfYu.WebApi.Template.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    --property:UseAppHost=false \
    --property:IsPackable=false \
    --property:Version=${VERSION}

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# The official ASP.NET Core images listen on port 8080 by default.
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "JfYu.WebApi.Template.dll"]
