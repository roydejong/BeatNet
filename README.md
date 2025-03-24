# BeatNet
**⚡ Beat Saber multiplayer game server**

[![.NET Build](https://github.com/roydejong/BeatNet/actions/workflows/dotnet.yml/badge.svg?branch=main&event=push)](https://github.com/roydejong/BeatNet/actions/workflows/dotnet.yml)

BeatNet is a server for hosting high performance Beat Saber multiplayer lobbies on LAN or WAN. It supports official, custom and modded content across multiple game modes. You can host public or private servers for you and your friends, or the community at large.

> [!TIP]
> Lost? Confused? If you don't want to run or host your own server, check out **[BeatTogether](https://discord.com/invite/gezGrFG4tz)**! They provide free public servers for the community. BeatNet is primarily aimed at modders, developers and power users.

**✅ This version is compatible/tested with Beat Saber version 1.40.2.**

## Setup
The easiest and recommended way to deploy BeatNet is via the **[🐳 Docker image](https://hub.docker.com/repository/docker/hippomade/beatnet)**.

Alternatively, you can **✅ [Download the latest stable release](https://github.com/roydejong/BeatNet/releases/latest)** or **💀 [Development build](https://github.com/roydejong/BeatNet/actions/workflows/dotnet.yml?query=event%3Apush+is%3Asuccess+branch%3Amain)**.

### Using Docker
Pull the latest image from Docker Hub, then run it in a container:

```bash
docker pull hippomade/beatnet:latest
docker run -e SERVER_PORT=7777 -p 7777:7777/udp -p 47777:47777/udp -d hippomade/beatnet:latest
```

This will start a default Quick Play lobby server on UDP port 7777.

> [!NOTE] 
> The `latest` version of the image is the latest stable release. You can also use `dev-main` to grab the latest development build.
 
> [!WARNING]   
> Never rebind the ports; the server needs to know its own port number to announce itself to the Server Browser and to identify itself via Local Discovery.
> If you wish to use a different port, change the configuration and set the `SERVER_PORT` environment variable, e.g.:
> ```
> docker run -e SERVER_PORT=12345 -p 12345:12345/udp -d hippomade/beatnet:latest
> ```
