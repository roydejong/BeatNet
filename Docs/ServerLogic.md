# WIP / DRAFT / POSSIBLY INCORRECT


# Dedicated Server Logic

This is some analysis from reverse engineering Beat Saber as a reference for implementing custom servers.

While we do not have access to the dedicated server source code, there is some shared code and game and the official
dedicated server. This in part, due to its original P2P nature.

🕐 Last updated / reviewed on 2024-03-20 for Beat Saber 1.35.0.


# General

The dedicated server is internally referred to as the "connection owner". Owing to the original P2P nature, the
dedicated server acts as the host player. It even exists in the lobby as an invisible player at slot index `-1`.

The dedicated server is responsible for:

- Accepting and managing connections
- Spawning players in the lobby and determining their position
- Relaying messages between players
- Game mode logic (voting, countdown, state management)

The client does a lot on its own and makes assumptions about the logical game state. For example, the results screen
starts and ends automatically when all players have submitted level completion results. This is not an authoritative
decision the dedicated server makes.

Note: The game uses (parts of) the Ignorance library in the client code for networking, many things in the source code
are referred to as `Ignorance` (and not so much ENet).


# Official game modes

A brief summary of the logical behavior and flow of the game modes, as it happens on official servers.

## Private / managed lobbies

Private / managed lobbies are created by the player. A server code can be used to join.

One player will be considered the party leader (the lobby owner). That player chooses the level and modifiers, and
chooses when to start the level. If the party leader leaves, a different player will be randomly selected.

Players can make suggestions for levels and modifiers, but the party leader has the final say.

## Quick Play

In official Quick Play lobbies, the game runs continuously:

1. As soon as more than one player is in the lobby, a 60 second countdown begins.
2. During the countdown, players can vote for a beatmap.
3. When all players are ready, or when the countdown reaches 5 seconds, the gong sounds. The level with the most votes
   will be selected. If no level has been voted for, a random level is selected.
4. During the 5 second countdown, voting is locked, players can no longer ready up. After the countdown, the level
   starts.
5. When the level is over, players return to the lobby and the 60 second countdown begins again.

Quick play has a few differences compared to private / managed lobbies:

- There are always 5 player slots in the lobby.
- You cannot turn spectator mode on in the lobby (but you can still spectate by giving up).
- Modifiers cannot be selected or voted on. The "no fail" modifier is always enabled.
- Official servers will sometimes prefer OST levels over DLC levels, regardless of votes, if not all players in the
  lobby own the DLC. Democracy is a lie.


# Connection management

## Player connection flow

After matchmaking (normally via GameLift) completes succesfully, the client will connect to the dedicated server. The
connection flow roughly looks like this:

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

1. A new `PingPacket` will immediately be sent to all connections (possibly so that the client can immediately trigger a
   sync time packet from the server).
2. A `PlayerConnectedPacket` for the connecting player is broadcast to all first-degree connections (but not to the
   connecting player itself).
3. If it is a direct connection, for every *already known* player send the following packets to the connecting player:
    1. `PlayerConnectedPacket` (SendImmediatelyToPlayer)
    2. `PlayerSortOrderPacket`, *if set* (SendImmediatelyToPlayer)
    3. `PlayerIdentityPacket`, *if set* (SendImmediatelyFromPlayerToPlayer)
4. If it is a direct connection, also send the following for the local player to the connecting player:
    1. `PlayerSortOrderPacket`, *if set* (SendImmediatelyToPlayer)
    2. `PlayerIdentityPacket`, *if set* (SendImmediatelyFromPlayerToPlayer)

To summarize these `AddPlayer` behaviors:

- Client: After connecting to the server, they initially only send `PlayerIdentityPacket` directly to the server.
- Server: After a client connects, the server will immediately inform all other players, and the new player of the
  existing players. The server will also inform the new player of the existing players' sort order and identity if
  available.

## Connection requests

When a user connects to the dedicated server via ENet, the first message they send is a specially formatted connection
request.

The current connection request starts with an `IgnCon` (meaning "IgnoranceConnection") string prefix and then the
following fields:

```csharp
// TODO Figure out which IConnectionRequestHandler is used by IgnoranceConnectionManager
// I think GameLiftClientConnectionRequestHandler(?):
string userId
string userName
bool isConnectionOwner
string playerSessionId
```

### Notes

- The client is only given one chance to send a valid connection request. If the server does not receive a valid
  connection request, the client is disconnected immediately.

## Packet relaying

Packets have a source (byte), target (byte) and options (byte - flags). The source and target refer to the connection
slot of the player in the lobby, and also support some special values.

The target value should be interpreted as follows:

- If **0**, this is considered a direct send. The message should not be relayed, the receiver should process it.
- If **127**, this is considered a broadcast. The receiver should relay it to all its connections, and also process it.
- If a **1 - 126** value, this is a relay request. The receiver should try to relay it to the target, but not process
  it.

Additional notes:

- Broadcasts and relays are only sent through if multiple connections are established to the receiver. In practice, this
  means **only dedicated servers will actually relay any messages**.
- Target values **above 127 cannot be used** (will loop back around due to a bitwise AND in the game's code). That means
  the absolute maximum amount of players in a lobby is 126 (host occupies slot 0).

Additional notes - currently unused by game:

- Packets with the `Encrypted` option are dropped by the server if they are sent to target 127 (broadcast) (guess:
  future use for end-to-end encrypted voice chat packets).
- Packets with the `FirstDegreeOnly` option are never relayed, regardless of their target value. Not sure what this is
  for, because a direct send has the same effect (maybe voice chat will be P2P?).

## SyncTime, Pings & Pongs

### Sync time logic

Every network synchronized action is timestamped with a sync time.

Game time is based on the amount of ticks in UTC since the Unix epoch (00:00:00 on 1 January 1970).

When a server starts, or when a client initializes their connection, they will mark the start time in ticks and track
the **total runtime in milliseconds**:

```csharp
RunTime = (CurrentTicksSinceUtcEpoch - StartTimeTicksSinceUtcEpoch) / 10000
// Note: dividing by 10,000 to convert from ticks to milliseconds
```

Sync time is based on the run time, offset by the measured time difference with the server:

```csharp
SyncTime = RunTime + AverageSyncTimeOffsetMs
// Note: offset will always be 0 on the server as it does not receive sync time packets
```

A `SyncTimePacket` is periodically sent by the server to the client (on receipt of a ping), and the client uses it to
calculate an average offset. At least one sync time packet must be received before a client can fully connect to the
server.

ℹ️ A client will reject sync time packets if the time between update ticks is too large (>30ms). Therefore, clients
running very slowly (~33fps or lower) will actually never connect successfully.

### Ping / pong logic

- Every peer sends a `PingPacket` broadcast every 2000ms.
- Every peer that receives a Ping, responds with a `PongPacket`.
- Both ping and pong messages contain the sender's sync time.
- Servers, when receiving a ping, will send a `SyncTimePacket` in addition to a `PongPacket`.

## SortOrder
Each player is assigned a sort index. This affects their position in the lobby and in gameplay.

The special sort index `-1` is used to indicate that a player is not (visibly) in the lobby. This applies to the hidden server player.

The player's sort order may be changed during a session.

### Assigning initial sort index
Sort indices are assigned by the server, but only when the following conditions are met:
 - The player must be connected / not disconnecting / not kicked
 - The player must have sent their identity / state, and the state must contain the `player` state
 - The player must have a valid latency (at least 1 sample, so must have completed ping/pong sequence)

If these conditions are not yet met, the player is not considered "fully connected" by the server and no sort index will be assigned. (Such players are also not in the "connected players" list in the session manager.)

### Updating sort indexes
Official servers are known to move players around when transitioning between lobby <-> gameplay. How this works exactly is unknown.


# Countdown & Gameplay RPCs

## Countdown
The server controls when the countdown begins. It does this by sending a **`SetCountdownEndTimeRpc`** to all clients, which contains the sync time at which the countdown should end. 

The countdown can be cancelled with a **`CancelCountdownRpc`**.

When there are 5 seconds or less remaining, the gong sounds and the lobby background color changes. Some UI elements will be locked. This is a client-side effect that the server cannot control.

### Notes
- `SetCountdownEndTimeRpc` only works once; if the countdown is already running, this RPC is ignored.
- `CancelCountdownRpc` can only be sent if the countdown is running, which means it does not work if the level is already starting.
- The gong can only be "unsounded" by letting the level start or cancelling the countdown.

## Level loading
At the 5 second mark, when the client sounds the gong, voting should end and level / modifier selection should be locked in.

At this time, the server should send a **`StartLevelRpc`**, which contains the level, gameplay modifiers and start time. This will begin loading the level and transition to the `GameStarting` lobby state.

The level start can be cancelled with a **`CancelLevelStartRpc`**, which will abort the level loading and transition back to the `LobbySetup` state.

### Notes
- Level start / cancel RPCs do not affect the countdown in any way.

## Level start
When the client has loaded the level, and the level start time (from `StartLevelRpc`) is reached, the level starts automatically.

(If loading is not complete when the level start time is reached, it will continue loading and perform the transition as soon as it's ready. If loading gets stuck or fails or any reason, the player would simply remain in the lobby while others play the level.)

On level load completion:

- The lobby will transition to the `GameRunning` state, and the multiplayer game state will also *automatically* transition to the `Game` state.
- The player's state will be updated at this time (`was_active_at_level_start`, `is_active`, `finished_level`). 
- The game will start the level and perform a scene transition.

## Gameplay start

Scene transition will occur and the `MultiplayerController` will start:

### 1. Game state transition
The server sends a `SetMultiplayerGameStateRpc` (with `MultiplayerGameState.Game`) to all clients. The clients will enter the `WaitingForPlayers` state and start the "sync scene load" process.

### 2. Sync scene load
The server will generate a Session Game ID (`Guid.NewGuid()`) and send `GetGameplaySceneReadyRpc` to all players.

Clients will immediately send a `SetGameplaySceneReadyRpc` to the server with their player-specific settings (both on their own initiative, and as a reply to the server when it asks).

When all players have sent `SetGameplaySceneReadyRpc`, the server will send `SetGameplaySceneSyncFinished` to all players (containing the session ID and a list of all collected player settings).

Time-outs:
- The server will time out after **15 seconds**, and will **force a level** to start with the players that are ready.
- The clients will time out after **20 seconds**, and will **return to the lobby**.

The server can send `SetPlayerDidConnectLateRpc` in this stage to indicate that a player connected late / will spectate.

### 3. Sync song load
Clients will enter the `SongStartSync` state, and fade in the multiplayer environment. Players will spawn in.

The server will send a `GetGameplaySongReadyRpc` to all players.

Clients will load the song audio, and then send `SetGameplaySongReadyRpc` to the server (both on their own initiative, and as a reply to the server when it asks).

When all players have sent `SetGameplaySongReadyRpc`, the server will determine the song start time and send a `SetSongStartTimeRpc` to all players.

The song start time is determined as follows: `Current syc time + 250ms + (Highest player latency ms * 2)`.

Time-outs: TBD.

### 4. Gameplay
During gameplay, the server mostly only acts as a relay for the gameplay RPCs (block spawns, hits, misses, etc.).

Clients will play until they all finish, fail or give up on the level and send their results on completion (`LevelFinishedRpc`).

### 5. Level end

#### Level finish
When all players have sent their level completion results, the level will end automatically and show the results screen.

The server has no authoritative control over the results screen. The clients do all of this on their own initiative. When the results screen is done, the clients will return to the lobby automatically.

The results screen will only be shown if:
- *Any* player has cleared the level (`MultiplayerPlayerLevelEndReason.Cleared`)
- The local player must have played the level without quitting (or was spectating).

Timings:
- Once a level is finished, clients will only wait up to **10 seconds** for all remaining players to send their results. 
- The duration of the level results screen seems to be around **20 seconds** (may be variable?).

#### Level cancel
When no active players remain, the server will abort the level and send players back to the menu with a `ReturnToMenuRpc`. No results screen will be shown/

# Player states
Players can have a number of states in their player hash set. The client sends their initial state via the `PlayerIdentityPacket`, and a `PlayerStatePacket` whenever its state changes. 

## multiplayer_session
Set on multiplayer session start.

## dedicated_server
Indicates session type: Dedicated server.

Set on multiplayer session start (if `SessionType.DedicatedServer`).

This should never happen in a client connection.

## player
Indicates session type: Player.

Set on multiplayer session start (if `SessionType.Player`).

This should **always** be set in a client connection; through a vanilla game client it is not possible to start any other session type.

## spectating
Indicates session type: Spectating.

Set on multiplayer session start (if `SessionType.Spectator`).

This should never happen in a client connection.

This is actually NOT related to "normal" spectating in a multiplayer lobby as a regular player. This is something only used by the game devs to look inside lobbies I think. :)

## backgrounded
Indicates that the game is paused / backgrounded for this player.

Set/unset on `OnApplicationPause` (this is a [Unity event](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnApplicationPause.html)).

Unsure when this triggers in the case of Beat Saber on PC; most likely applies primarily to Quest players?

## terminating
Indicates that this server is shutting down (lobby host only).

May be set by the dedicated server (connection owner player) if it is shutting down.

If set, the client will disconnect and show a DCR-10 (ServerTerminated) error or fail the connection with a CFR-13 (ServerIsTerminating) error.

## wants_to_play_next_level
Indicates that the player wants to play the next level.

Set by default on multiplayer session start.

Unset when the "spectator mode" toggle is turned on.

## is_active
Indicates that the player is currently participating in gameplay.

- Set/unset by the client when level loading is complete.
    - Set if: `wants_to_play_next_level` was set, and `StartLevelRpc` was received while still in the lobby.
- Unset by the client if they connected late (spectator join).
- Unset by the client when the gameplay song finishes.
- Unset by the client if the player fails out of a level.
- Unset by the client if the player gives up on a level (menu quit).
- (Unset if level data is not initialized when transitioning to the gameplay scene.)

## was_active_at_level_start
Indicates that player had the `is_active` state when the gameplay level begun.

- Set/unset by the client when level loading is complete.
  - Set if: `wants_to_play_next_level` was set, and `StartLevelRpc` was received while still in the lobby.
- Unset by the client if they connected late (spectator join).
- (Unset if level data is not initialized when transitioning to the gameplay scene.)

## finished_level
Indicates that the player has fully finished a level.

- Unset by the client when the countdown finishes (level loading complete).
- Set by the client when the gameplay song finishes, right before sending completion results.
- Unset by the client if the player fails out of a level.
- Unset by the client if the player gives up on a level (menu quit).

## in_menu
Indicates that the client's `MenuRpcManager` is active and Menu RPCs can be handled.

## in_gameplay
Indicates that the client's `GameplayRpcManager` is active and Gameplay RPCs can be handled.

## modded
Special state added by `MultiplayerCore`. Indicates a modded client.


# Packet logs

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

Once the lobby fades in and the UI is presented, the game will begin listening for game start / countdown events and
send:

- GetCountdownEndTimeRpc
- GetStartedLevelRpc


# Unused RPCs
Some RPCs in the game appear to be unused:

- `CancelStartGameTime` - This RPC is defined but the event is not subscribed to. It seems like it would be a duplicate of `CancelCountdownRpc` and/or .
