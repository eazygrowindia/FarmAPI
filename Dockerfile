# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["farmAPI.csproj", "."]
RUN dotnet restore "farmAPI.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "farmAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "farmAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --self-contained false -r linux-x64

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "farmAPI.dll"]
