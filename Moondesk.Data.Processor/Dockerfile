FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AquaPP.Data.Processor/AquaPP.Data.Processor.csproj", "AquaPP.Data.Processor/"]
RUN dotnet restore "AquaPP.Data.Processor/AquaPP.Data.Processor.csproj"
COPY . .
WORKDIR "/src/AquaPP.Data.Processor"
RUN dotnet build "./AquaPP.Data.Processor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AquaPP.Data.Processor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AquaPP.Data.Processor.dll"]
