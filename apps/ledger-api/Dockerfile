# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Sentinel.Api/Sentinel.Api.csproj", "Sentinel.Api/"]
RUN dotnet restore "Sentinel.Api/Sentinel.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Sentinel.Api"
RUN dotnet build "Sentinel.Api.csproj" -c Release -o /app/build

# 2. Publish Stage
FROM build AS publish
RUN dotnet publish "Sentinel.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 3. Final Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Environment variables for SQLite and Port
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "Sentinel.Api.dll"]