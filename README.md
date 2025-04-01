# BeatNet

**⚡ Beat Saber multiplayer game server**

[![.NET Build](https://github.com/roydejong/BeatNet/actions/workflows/dotnet.yml/badge.svg?branch=main&event=push)](https://github.com/roydejong/BeatNet/actions/workflows/dotnet.yml)

BeatNet is a server for hosting high performance Beat Saber multiplayer lobbies on LAN or WAN. It supports official,
custom and modded content across multiple game modes. You can host public or private servers for you and your friends,
or the community at large.

**👉 Currently, BeatNet only supports direct connections via
the [Server Browser mod](https://github.com/roydejong/BeatSaberServerBrowser) on PC.**

**✅ This version is compatible/tested with Beat Saber version 1.40.4.**

> [!TIP]
> Lost? Confused? If you don't want to run or host your own server, check out **[BeatTogether](https://discord.com/invite/gezGrFG4tz)**! They provide free public servers for the community. BeatNet is primarily aimed at power users, modders and developers.

## Setup

The easiest and recommended way to deploy BeatNet is via the **[🐳 Docker image](https://hub.docker.com/repository/docker/hippomade/beatnet)**.

Alternatively, you can **✅ [Download the latest stable release](https://github.com/roydejong/BeatNet/releases/latest)** or **💀 [Development build](https://github.com/roydejong/BeatNet/actions/workflows/dotnet.yml?query=event%3Apush+is%3Asuccess+branch%3Amain)**.

### Using Docker

Pull the latest image from Docker Hub, then run it in a container:

```bash
docker pull hippomade/beatnet:latest
docker run -e SERVER_PORT=7777 -e PUBLIC=1 -p 7777:7777/udp -p 47777:47777/udp -v config:/app/config -d hippomade/beatnet:latest
```

This will start a **public Quick Play lobby** server on UDP port 7777, with local network discovery enabled.

> [!NOTE]
> The `latest` version of the image is the latest stable release. You can also use `dev-main` to grab the latest development build.

> [!WARNING]   
> You should not rebind the ports; the server needs to know its own port number to announce itself to the Server Browser and to identify itself via local network discovery.
> If you wish to use a different port, set the `SERVER_PORT` environment variable and expose that port, e.g.:
> ```
> docker run -e SERVER_PORT=12345 -p 12345:12345/udp -d hippomade/beatnet:latest
> ```
> Local network discovery always uses port 47777. As it can only be bound once, you may have to consider [using host networking](https://docs.docker.com/engine/network/drivers/host/) if you want multiple game servers to support local discovery.

## Configuration

You can configure the server by creating or editing `config/server.json`.

> [!TIP]
> When using Docker, it is strongly recommended to mount the config directory as a volume (`-v config:/app/config`).

| Setting                    | Type      | Default           | Description                                                                                                                                                       |
|----------------------------|-----------|-------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **`UdpPort`**              | `ushort`  | 7777              | The UDP port number the lobby should be hosted on. Can be overriden with the `SERVER_PORT` environment variable.                                                  |
| **`EnableLocalDiscovery`** | `bool`    | true              | If true, enable local network discovery responses (LAN discovery) for the [Server Browser](https://github.com/roydejong/BeatSaberServerBrowser).                  |
| **`WanAddress`**           | `string?` | null              | Optional: Manual override for the server's public IPv4 or IPv6 address. Used for public server browser lobbies only, this is the address clients will connect to. |
| **`MaxPlayerCount`**       | `int`     | 5                 | The lobby size; how many players can join. Normally 2-5; modded lobbies support any player count up to 127.                                                       |
| **`GameMode`**             | `string`  | beatnet:quickplay | The name of a supported game mode the lobby will run. See below for a complete list of built-in game modes.                                                       |
| **`Public`**               | `bool`    | false             | If true, the server will be listed in the public server browser. Your IP address will be visible to the public.                                                   |
| **`Name`**                 | `string`  | BeatNet Server    | The name to use for local discovery / server browser listings.                                                                                                    |
| **`WelcomeMessage`**       | `string`  | null              | The "message of the day" (MOTD) to send to players when they join (requires [MultiplayerChat](https://github.com/roydejong/BeatSaberMultiplayerChat/)).           |

🔁 The server must be restarted to apply any changes.

### Environment variables

Some settings can be forced via environment variables. This can be useful for Docker users or situations where you do not want to customize the config file.

| Environment variable | Description                                                                                                                       |
|----------------------|-----------------------------------------------------------------------------------------------------------------------------------|
| `SERVER_PORT`        | The UDP port number the lobby should be hosted on. This overrides the `UdpPort` setting in the config file.                       |
| `PUBLIC`             | If `1` or `true`, the server will be listed in the public server browser. This overrides the `Public` setting in the config file. |
| `NAME`               | The name to use for local discovery / server browser listings. This overrides the `Name` setting in the config file.              |

## Game Modes

### `beatnet:quickplay`

**Quick Play**: This is a continuous mode where players can vote for the next level after completing the last. Most votes win!

- **Countdown:** As soon as any suggestion is made, the lobby will begin counting down. The top voted level will be shown in the center. When the countdown reaches 5 seconds, or if all players are ready, the vote is locked in and the next level is chosen.
- **Modifier voting**: Modifiers can also be voted on. The modifier set with the most votes will win. If no modifiers are selected, the default "No Fail" modifier is used.
