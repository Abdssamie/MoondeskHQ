FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AquaPP.Control.Relay/AquaPP.Control.Relay.csproj", "AquaPP.Control.Relay/"]
RUN dotnet restore "AquaPP.Control.Relay/AquaPP.Control.Relay.csproj"
COPY . .
WORKDIR "/src/AquaPP.Control.Relay"
RUN dotnet build "./AquaPP.Control.Relay.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AquaPP.Control.Relay.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AquaPP.Control.Relay.dll"]
