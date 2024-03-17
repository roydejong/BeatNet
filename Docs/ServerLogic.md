# WIP / DRAFT / POSSIBLY INCORRECT

# Dedicated Server Logic
This is some analysis from reverse engineering Beat Saber as a reference for implementing custom servers.

While we do not have access to the dedicated server source code, there is some shared code and game and the official dedicated server. This in part, due to its original P2P nature.

🕐 Last updated / reviewed on 2024-02-25 for Beat Saber 1.34.6.

# General
The dedicated server is internally referred to as the "connection owner". Owing to the original P2P nature, the dedicated server acts as the host player. It even exists in the lobby as an invisible player at slot index `-1`.

The dedicated server is responsible for:

 - Accepting and managing connections
 - Spawning players in the lobby and determining their position
 - Relaying messages between players
 - Game mode logic (voting, countdown, state management)

The client does a lot on its own and makes assumptions about the logical game state. For example, the results screen starts and ends automatically when all players have submitted level completion results. This is not an authoritative decision the dedicated server makes.

Note: The game uses (parts of) the Ignorance library in the client code for networking, many things in the source code are referred to as `Ignorance` (and not so much ENet).

# Official game modes
A brief summary of the logical behavior and flow of the game modes, as it happens on official servers.

## Private / managed lobbies
Private / managed lobbies are created by the player. A server code can be used to join.

One player will be considered the party leader (the lobby owner). That player chooses the level and modifiers, and chooses when to start the level. If the party leader leaves, a different player will be randomly selected.

Players can make suggestions for levels and modifiers, but the party leader has the final say.

## Quick Play
In official Quick Play lobbies, the game runs continuously:

1. As soon as more than one player is in the lobby, a 60 second countdown begins.
2. During the countdown, players can vote for a beatmap.
3. When all players are ready, or when the countdown reaches 5 seconds, the gong sounds. The level with the most votes will be selected. If no level has been voted for, a random level is selected.
4. During the 5 second countdown, voting is locked, players can no longer ready up. After the countdown, the level starts.
5. When the level is over, players return to the lobby and the 60 second countdown begins again.

Quick play has a few differences compared to private / managed lobbies:

- There are always 5 player slots in the lobby.
- You cannot turn spectator mode on in the lobby (but you can still spectate by giving up).
- Modifiers cannot be selected or voted on. The "no fail" modifier is always enabled.
- Official servers will sometimes prefer OST levels over DLC levels, regardless of votes, if not all players in the lobby own the DLC. Democracy is a lie.

# Connection management

## Player connection flow
After matchmaking (normally via GameLift) completes succesfully, the client will connect to the dedicated server. The connection flow roughly looks like this: 

From the client's perspective:
1. Target server info is provided from matchmaking
2. Client opens ENet connection to the server
3. On connect, client sends an `IgnCon` connection request to the server
4. Client runs `AddPlayer` for the server connection

From the server's perspective:
1. Some backend service (GameLift) will notify the server of an upcoming connection
2. Server will accept a new ENet connection
3. Server should immediately receive a connection request from the client
4. If accepted, server runs `AddPlayer` for the client connection

When a client or server runs `AddPlayer`, a bunch of stuff happens - some of it not quite logical:
1. A new `PingPacket` will immediately be sent to all connections (possibly so that the client can immediately trigger a sync time packet from the server).
2. A `PlayerConnectedPacket` for the connecting player is broadcast to all first-degree connections (but not to the connecting player itself).
3. If it is a direct connection, for every known player send the following packets to the connecting player:
   1. `PlayerConnectedPacket` (SendImmediatelyToPlayer)
   2. `PlayerSortOrderPacket`, if set (SendImmediatelyToPlayer)
   3. `PlayerIdentityPacket`, if set (SendImmediatelyFromPlayerToPlayer)
4. If it is a direct connection, also send the following for the local player to the connecting player:
   1. `PlayerSortOrderPacket`, if set (SendImmediatelyToPlayer)
   2. `PlayerIdentityPacket`, if set (SendImmediatelyFromPlayerToPlayer)
 
## On lobby enter
Based on packet capture.

A client entering or transitioning to the lobby will immediately send a number of RPCs (broadcast):

- GetIsReadyRpc
- GetIsInLobbyRpc
- GetRecommendedBeatmapRpc
- GetRecommendedGameplayModifiersRpc
- GetPlayersPermissionConfigurationRpc
- SetOwnedSongPacksRpc
- SetIsReadyRpc
- SetIsInLobbyRpc
- GetSelectedBeatmapRpc
- GetSelectedGameplayModifiersRpc
- GetMultiplayerGameStateRpc
- GetPlayersPermissionConfigurationRpc

If the lobby is a brand new managed lobby, the server would reply as follows:

- ClearSelectedBeatmapRpc
- ClearSelectedGameplayModifiersRpc
- SetIsStartButtonEnabledRpc
- GetRecommendedBeatmapRpc
  - Client replies: ClearRecommendedBeatmapRpc 
- GetRecommendedGameplayModifiersRpc
  - Client replies: RecommendGameplayModifiersRpc 
- GetOwnedSongPacksRpc
  - Client replies: SetOwnedSongPacksRpc 
- GetIsReadyRpc
  - Client replies: SetIsReadyRpc
- GetIsInLobbyRpc
  - Client replies: SetIsInLobbyRpc
- SetPlayersPermissionConfigurationRpc
- SetIsStartButtonEnabledRpc
- SetIsStartButtonEnabledRpc
- SetIsStartButtonEnabledRpc
- ClearSelectedBeatmapRpc
- ClearSelectedGameplayModifiersRpc
- SetMultiplayerGameStateRpc
- SetPlayersPermissionConfigurationRpc
- SetIsStartButtonEnabledRpc
- SetSelectedGameplayModifiersRpc
- SetIsStartButtonEnabledRpc

See `LobbyDataModel.Activate()`.

Once the lobby fades in and the UI is presented, the game will begin listening for game start / countdown events and send:

- GetCountdownEndTimeRpc
- GetStartedLevelRpc
 
## Connection requests
When a user connects to the dedicated server via ENet, the first message they send is a specially formatted connection request.

The current connection request starts with an `IgnCon` (meaning "IgnoranceConnection") string prefix and then the following fields:

```csharp
// TODO Figure out which IConnectionRequestHandler is used by IgnoranceConnectionManager
// I think GameLiftClientConnectionRequestHandler(?):
string userId
string userName
bool isConnectionOwner
string playerSessionId
```

### Notes
- The client is only given one chance to send a valid connection request. If the server does not receive a valid connection request, the client is disconnected immediately.

## Packet relaying
Packets have a source (byte), target (byte) and options (byte - flags). The source and target refer to the connection slot of the player in the lobby, and also support some special values.

The target value should be interpreted as follows:
- If **0**, this is considered a direct send. The message should not be relayed, the receiver should process it.
- If **127**, this is considered a broadcast. The receiver should relay it to all its connections, and also process it.  
- If a **1 - 126** value, this is a relay request. The receiver should try to relay it to the target, but not process it.

Additional notes:

- Broadcasts and relays are only sent through if multiple connections are established to the receiver. In practice, this means **only dedicated servers will actually relay any messages**.
- Target values **above 127 cannot be used** (will loop back around due to a bitwise AND in the game's code). That means the absolute maximum amount of players in a lobby is 126 (host occupies slot 0).

Additional notes - currently unused by game:

- Packets with the `Encrypted` option are dropped by the server if they are sent to target 127 (broadcast) (guess: future use for end-to-end encrypted voice chat packets).
- Packets with the `FirstDegreeOnly` option are never relayed, regardless of their target value. Not sure what this is for, because a direct send has the same effect (maybe voice chat will be P2P?).

## SyncTime, Pings & Pongs

### Sync time logic
Every network synchronized action is timestamped with a sync time. 

Game time is based on the amount of ticks in UTC since the Unix epoch (00:00:00 on 1 January 1970).

When a server starts, or when a client initializes their connection, they will mark the start time in ticks and track the total runtime in milliseconds:
```csharp
RunTime = (CurrentTicksSinceUtcEpoch - StartTimeTicksSinceUtcEpoch) / 10000
// Note: dividing by 10,000 to convert from ticks to milliseconds
```

Sync time is based on the run time, offset by the measured time difference with the server:
```csharp
SyncTime = RunTime + AverageSyncTimeOffsetMs
// Note: offset will always be 0 on the server as it does not receive sync time packets
```

A `SyncTimePacket` is periodically sent by the server to the client (on receipt of a ping), and the client uses it to calculate an average offset.  At least one sync time packet must be received before a client can fully connect to the server.

ℹ️ A client will reject sync time packets if the time between update ticks is too large (>30ms). Therefore, clients running very slowly (~33fps or lower) will actually never connect successfully. 

### Ping / pong logic

- Every peer sends a `PingPacket` broadcast every 2000ms.
- Every peer that receives a Ping, responds with a `PongPacket`.
- Both ping and pong messages contain the sender's sync time.
- Servers, when receiving a ping, will send a `SyncTimePacket` in addition to a `PongPacket`.

