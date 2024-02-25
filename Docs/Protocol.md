# Beat Saber Network Protocol
This document describes the network protocol used by Beat Saber multiplayer.

To understand the terms and types in this document, you should refer to the game's decompiled source code.

🕐 Last updated / reviewed on 2024-02-24 for Beat Saber 1.34.6.

## Transport 
The transport layer is UDP via [ENet](http://enet.bespin.org/).

### Encryption
Beat Games has created their own modified version of ENet which adds DTLS for encryption.

Encryption can be bypassed with mods, and their implementation remains fully compatible with the original ENet protocol.

### Channels
Only two ENet channels are used:

| #    | Name                  | Purpose                                  |
|------|-----------------------|------------------------------------------|
| `0`  | **`ReliableOrdered`** | Default, used for (almost) every packet. |
| `1`  | **`Unreliable`**      | Sync state deltas (movement, score).     |

## Structure

### Routing header
Each packet starts with a routing header, which is 3 bytes in length:

1. `senderId`
2. `receiverId`
3. `packetOptions` (flags)

For sender and receiver, the values work as follows:

- `0` = Direct send
- `1` - `126` = Message to/from another player 
- `127` = Broadcast message to all players

ℹ️ A client may ask the server to relay certain messages to another player (or all players). Players cannot communicate directly, so the server can act as a relay. Note multiplayer was originally designed for P2P.

For packet options, the values work as follows (`PacketOption` flags enum):

- `0` = None
- `1` = Encrypted
- `2` = OnlyFirstDegreeConnections

ℹ️ `OnlyFirstDegreeConnections` is not currently used in the game (but likely will be in the future).

### Sub packets
Each network packet can contain multiple sub packets. They are written continuously into the packet buffer with a length prefix before each sub packet.

The structure of a sub packet at a high level then is:

1. Sub packet length (`VarUInt`)
2. Sub packet data (`byte[]`)

### Sub serializers
Within the each sub packet data, the first `byte` represents the type of the sub packet. This is used to determine which serializer to use to read the rest of the sub packet.

Note: MultiplayerCore is the exception to this rule. Sub packets types are denoted with a `string`, not a `byte`.

The sub serializer hierarchy current look like follows:

```
┏━━ ConnectedPlayerManager
┃   ┣━━ MultiplayerSession
┃   ┃    ┣━━ MenuRpc
┃   ┃    ┣━━ GameplayRpc
┃   ┃    ┗━━ MultiplayerCore (modded)
```

`ConnectedPlayerManager` is the root level and where packet headers are read / written.

### Sub serializer logic
From the perspective of reading a packet, the recursive logic looks like this:

```
While there are bytes left in the buffer:
 1. Read the length prefix (VarUInt)
 2. Slice out the next sub packet given the length
 
Then, within each sub packet slice:
 1. Read the first byte to determine the sub packet type
 2. Use the appropriate (sub)serializer to read the rest of the sub packet
```

⚠️ Consideration for implementation: When writing packets, you need to know the length of the sub packets ahead of time. The length identifier itself is a variable length (VarUInt) too. This means you'll likely need to use a (temporary) sub buffer.


## Message types
Sub packets themselves can be divided into more sub packets. Each message type allows for another serialization layer.

Each sub-serialization layer will usually denote its sub message type with a `byte` prefix.

### Level 1: `ConnectedPlayerManager` 
The `byte` prefix for this serializer is an `InternalMessageType`:

```csharp
private enum InternalMessageType : byte
{
    SyncTime,
    PlayerConnected,
    PlayerIdentity,
    PlayerLatencyUpdate,
    PlayerDisconnected,
    PlayerSortOrderUpdate,
    Party,
    MultiplayerSession,
    KickPlayer,
    PlayerStateUpdate,
    PlayerAvatarUpdate,
    Ping,
    Pong
}
```

The following packets exist directly on this level:
- `PlayerConnected` → `PlayerConnectedPacket`
- `PlayerIdentity` → `PlayerIdentityPacket`
- `PlayerAvatarUpdate` → `PlayerAvatarPacket`
- `PlayerStateUpdate` → `PlayerStatePacket`
- `PlayerSortOrderUpdate` → `PlayerSortOrderPacket`
- `PlayerDisconnected` → `PlayerDisconnectedPacket`
- `KickPlayer` → `KickPlayerPacket`
- `Ping` → `PingPacket`
- `Pong` → `PongPacket`

Most network traffic flows down through the `MultiplayerSession` sub-serializer, as described below.

### Level 2: `ConnectedPlayerManager` → `MultiplayerSession` 
The `byte` prefix for this serializer is a `MultiplayerSessionManager.MessageType`:

```csharp
public enum MessageType : byte
{
    MenuRpc,
    GameplayRpc,
    NodePoseSyncState,
    ScoreSyncState,
    NodePoseSyncStateDelta,
    ScoreSyncStateDelta,
    OptionalAvatarData
    
    // Modded (not part of base game):
    MultiplayerCore = 100
}
```

The following packets exist directly on this level:
- `NodePoseSyncState` → `NodePoseSyncStateNetSerializable`
- `ScoreSyncState` → `StandardScoreSyncStateNetSerializable`
- `NodePoseSyncStateDelta` → `NodePoseSyncStateDeltaNetSerializable`
- `ScoreSyncStateDelta` → `StandardScoreSyncStateDeltaNetSerializable`
- `OptionalAvatarData` → `OptionalAvatarDataPacket`

### Level 3: `ConnectedPlayerManager` → `MultiplayerSession` → `MenuRpc`
This is an RPC serializer that applies to the multiplayer lobby / menu environment.

The `byte` prefix for this serializer is a `MenuRpcManager.RpcType`:
```
public enum MenuRpcType : byte
{
	SetPlayersMissingEntitlementsToLevel = 0,
	GetIsEntitledToLevel = 1,
	SetIsEntitledToLevel = 2,
	InvalidateLevelEntitlementStatuses = 3,
	SelectLevelPack = 4,
	SetSelectedBeatmap = 5,
	GetSelectedBeatmap = 6,
	RecommendBeatmap = 7,
	ClearRecommendedBeatmap = 8,
	GetRecommendedBeatmap = 9,
	SetSelectedGameplayModifiers = 10,
	GetSelectedGameplayModifiers = 11,
	RecommendGameplayModifiers = 12,
	ClearRecommendedGameplayModifiers = 13,
	GetRecommendedGameplayModifiers = 14,
	LevelLoadError = 15,
	LevelLoadSuccess = 16,
	StartLevel = 17,
	GetStartedLevel = 18,
	CancelLevelStart = 19,
	GetMultiplayerGameState = 20,
	SetMultiplayerGameState = 21,
	GetIsReady = 22,
	SetIsReady = 23,
	SetStartGameTime = 24,
	CancelStartGameTime = 25,
	GetIsInLobby = 26,
	SetIsInLobby = 27,
	GetCountdownEndTime = 28,
	SetCountdownEndTime = 29,
	CancelCountdown = 30,
	GetOwnedSongPacks = 31,
	SetOwnedSongPacks = 32,
	RequestKickPlayer = 33,
	GetPermissionConfiguration = 34,
	SetPermissionConfiguration = 35,
	GetIsStartButtonEnabled = 36,
	SetIsStartButtonEnabled = 37,
	ClearSelectedBeatmap = 38,
	ClearSelectedGameplayModifiers = 39
}
```

### Level 3: `ConnectedPlayerManager` → `MultiplayerSession` → `GameplayRpc`
This is an RPC serializer that applies to the multiplayer gameplay environment.

The `byte` prefix for this serializer is a `GameplayRpcManager.RpcType`:

```csharp
public enum GameplayRpcType : byte
{
	SetGameplaySceneSyncFinish = 0,
	SetGameplaySceneReady = 1,
	GetGameplaySceneReady = 2,
	SetActivePlayerFailedToConnect = 3,
	SetGameplaySongReady = 4,
	GetGameplaySongReady = 5,
	SetSongStartTime = 6,
	NoteCut = 7,
	NoteMissed = 8,
	LevelFinished = 9,
	ReturnToMenu = 10,
	RequestReturnToMenu = 11,
	NoteSpawned = 12,
	ObstacleSpawned = 13,
	SliderSpawned = 14
}
```

### Level 3: `ConnectedPlayerManager` → `MultiplayerSession` → `MultiplayerCore`
This layer only applies if `MultiplayerCore` is installed, and custom packet types are provided by dependant mods.

Unlike other sub-serializers, `MultiplayerCore` uses a `string` prefix to denote its sub message type. Specifically, the fully qualified type name of the packet type.

Known mods that provide custom packets:

- **MultiplayerCore** itself (`MpBeatmapPacket`, `MpPlayerData`)
- [MultiplayerChat](https://github.com/roydejong/BeatSaberMultiplayerChat) (in development)
- [MultiplayerAvatars](https://github.com/Goobwabber/MultiplayerAvatars) (outdated, unmaintained)
- [Emoter](https://github.com/Auros/Emoter) (outdated, unmaintained)
- [Snowball](https://github.com/Goobwabber/Snowball) (outdated, unmaintained)