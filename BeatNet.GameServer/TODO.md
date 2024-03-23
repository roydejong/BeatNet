# TODO
Everything is mostly complete, in terms of running a vanilla-style Quick Play lobby.

## Scenarios
Mostly just need to test, and probably need some additional implementation for late joiners.

- Test / verify with multiple players
- Test / verify with high ping
- Test spectator (via toggle)
- Test "didn't load in time" scenario
- Test / implement "joined late" scenario
- Test "join during final countdown" scenario
- Maybe update test bot project for game 1.35

## Modded content
- MultiplayerCore generic packet support (test/verify) 👈 **Important**
- Custom song selection via MultiplayerCore packets  👈 **Important**
- BSSB announces 

## Game modes
- Managed game mode
  - Lobbies with (auto-rotating) party leader
  - Game start button / entitlement checks
  - RequestReturnToMenu RPC (force end level by leader) 

## Maybe
- Permissions / auth (but can we really auth reliably? is it even worth it?)
  - Vote kick permission?
- Radio game mode?
  - Predefined or AI-algo playlist QuickPlay without voting

## Optimizations
- Merge multiple messages per player-send (would that actually be more efficient? maybe some sort of send-batch API per update tick?)
- Object / packet pools
- Byte array pools

## Release
- README
- CI Auto builds
- Docker
- NuGet lib package? May want to use generated types on BeatTogether