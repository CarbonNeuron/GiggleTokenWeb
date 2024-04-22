FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-jammy-chiseled AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
RUN apt-get update -y && apt-get install clang zlib1g-dev -y
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GiggleTokenWeb/GiggleTokenWeb.csproj", "GiggleTokenWeb/"]
RUN dotnet restore "GiggleTokenWeb/GiggleTokenWeb.csproj"
COPY . .
WORKDIR "/src/GiggleTokenWeb"
RUN dotnet build "GiggleTokenWeb.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "GiggleTokenWeb.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["/app/GiggleTokenWeb"]
