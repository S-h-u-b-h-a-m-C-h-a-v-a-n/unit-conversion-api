# syntax=docker/dockerfile:1

# ---- Build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so the dependency layer is cached between source changes.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/UnitConversion.Core/UnitConversion.Core.csproj src/UnitConversion.Core/
COPY src/UnitConversion.Infrastructure/UnitConversion.Infrastructure.csproj src/UnitConversion.Infrastructure/
COPY src/UnitConversion.Api/UnitConversion.Api.csproj src/UnitConversion.Api/
RUN dotnet restore src/UnitConversion.Api/UnitConversion.Api.csproj

COPY .editorconfig ./
COPY src/ src/
RUN dotnet publish src/UnitConversion.Api/UnitConversion.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# The base image ships a non-root "app" user and listens on 8080.
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "UnitConversion.Api.dll"]
