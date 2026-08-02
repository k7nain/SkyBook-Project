# Build context is the FlyzenApi solution root (SkyBook-Project/FlyzenApi),
# not this folder — the API project references three sibling projects
# (Application/Domain/Infrastructure/Persistence) that live outside
# Presentation/, so the whole solution tree has to be in the build context.
# See docker-compose.yml: `build: { context: ./SkyBook-Project/FlyzenApi,
# dockerfile: Presentation/FlyzenApi.API/Dockerfile }`.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first (not the whole tree) so `dotnet restore`'s layer
# is cached across builds unless a .csproj actually changed.
COPY Presentation/FlyzenApi.API/FlyzenApi.API.csproj Presentation/FlyzenApi.API/
COPY src/Core/FlyzenApi.Application/FlyzenApi.Application.csproj src/Core/FlyzenApi.Application/
COPY src/Core/FlyzenApi.Domain/FlyzenApi.Domain.csproj src/Core/FlyzenApi.Domain/
COPY src/Infrastructure/FlyzenApi.Infrastructure/FlyzenApi.Infrastructure.csproj src/Infrastructure/FlyzenApi.Infrastructure/
COPY src/Infrastructure/FlyzenApi.Persistence/FlyzenApi.Persistence.csproj src/Infrastructure/FlyzenApi.Persistence/
RUN dotnet restore Presentation/FlyzenApi.API/FlyzenApi.API.csproj

COPY . .
RUN dotnet publish Presentation/FlyzenApi.API/FlyzenApi.API.csproj \
    -c Release -o /app/publish --no-restore

# Runtime stage: ASP.NET-only image, no SDK — publish output only.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# .NET 8's aspnet image already runs as the non-root `app` user by default.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FlyzenApi.API.dll"]
