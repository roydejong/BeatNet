########################################################################################################################
# Build

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY *.sln .

COPY BeatNet.Lib/*.csproj BeatNet.Lib/
RUN dotnet restore BeatNet.Lib/BeatNet.Lib.csproj --no-cache --force

COPY BeatNet.GameServer/*.csproj BeatNet.GameServer/
RUN dotnet restore BeatNet.GameServer/BeatNet.GameServer.csproj --no-cache --force

COPY . .
RUN dotnet publish BeatNet.GameServer/BeatNet.GameServer.csproj -c release -o /build

########################################################################################################################
# Runtime

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /build .
EXPOSE 7777/udp
EXPOSE 47777/udp
ENTRYPOINT ["dotnet", "BeatNet.GameServer.dll"]

########################################################################################################################
# Metadata

ARG VERSION="dev"

LABEL \
  maintainer="beatnet@hippomade.com" \
  version=$VERSION \
  description="BeatNet: Beat Saber multiplayer game server" \
  org.opencontainers.image.authors="beatnet@hippomade.com" \
  org.opencontainers.image.version="1.0.0" \
  org.opencontainers.image.source="https://github.com/roydejong/BeatNet" \
  org.opencontainers.image.title="BeatNet Game Server" \
  org.opencontainers.image.description="Containerized Beat Saber multiplayer game server" \
  org.opencontainers.image.url="https://bssb.app"