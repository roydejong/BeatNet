FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BeatNet.GameServer/BeatNet.GameServer.csproj", "BeatNet.GameServer/"]
RUN dotnet restore "BeatNet.GameServer/BeatNet.GameServer.csproj"
COPY . .
WORKDIR "/src/BeatNet.GameServer"
RUN dotnet build "BeatNet.GameServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BeatNet.GameServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BeatNet.GameServer.dll"]
