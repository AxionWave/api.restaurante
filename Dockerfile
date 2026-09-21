# Orion API (.NET 8)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Orion.sln Directory.Build.props ./
COPY src/Orion.Core/Orion.Core.csproj src/Orion.Core/
COPY src/Orion.Application/Orion.Application.csproj src/Orion.Application/
COPY src/Orion.Infrastructure/Orion.Infrastructure.csproj src/Orion.Infrastructure/
COPY src/Orion.API/Orion.API.csproj src/Orion.API/
RUN dotnet restore src/Orion.API/Orion.API.csproj
COPY src ./src
RUN dotnet publish src/Orion.API/Orion.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "Orion.API.dll"]
