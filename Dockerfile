# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FarmAPI.csproj", "."]
RUN dotnet restore "FarmAPI.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "FarmAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FarmAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --self-contained false -r linux-x64

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FarmAPI.dll"]
