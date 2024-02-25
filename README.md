# BeatNet
**⚡ Beat Saber modded multiplayer game server**

BeatNet is a server that can host one or more Beat Saber multiplayer lobbies. It supports official, custom and modded content across multiple game modes.

You can host public or private servers for you and your friends, or the community at large.

ℹ️ **Compatible / tested with Beat Saber 1.34.6+ only**
 
## Download
You will need the [.NET 7 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) installed to run the server.

- ✅ **[Download latest stable release](https://github.com/roydejong/BeatNet/releases/latest)** (recommended)
- 💀 **[Download latest development build](https://github.com/roydejong/BeatNet/actions?query=is%3Asuccess+workflow%3Abuild+)**

*Note: MacOS is not supported at this time.*

## Setup
Extract the server files to the desired location. The server configuration will be stored in `config/server.json`.

### Local / LAN server
💡 *Do you want to run a private lobby that is not internet accessible?*

In this setup:
 - You will use BeatNet to run a single lobby server on your local network
 - Your server will not be publicly accessible nor listed in the server browser

Example configuration:
```json
{
  "PortRangeMin": 7777,
  "PortRangeMax": 7777,
  "SpareLobbyCount": 1
}
```