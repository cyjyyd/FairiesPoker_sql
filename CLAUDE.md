# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

FairiesPoker is a Windows Forms poker game (斗地主/Dou Di Zhu - a Chinese card game) with offline single-player and online multiplayer modes. Disney fairies theme, built with C#/.NET 9.0.

## Build Commands

```bash
# Build the solution (Debug)
dotnet build FairiesPoker.sln

# Build the solution (Release)
dotnet build FairiesPoker.sln -c Release

# Restore NuGet packages
dotnet restore FairiesPoker.sln

# Run the client
dotnet run --project FairiesPoker/FairiesPoker.csproj

# Run the server
dotnet run --project FPServer/FPServer.csproj

# Run server tests
dotnet run --project FPServer/FPServer.csproj -- --test
```

First-time build requires restoring NuGet packages. In Visual Studio: right-click solution → "Restore NuGet Packages".

## Project Structure

- **FairiesPoker.sln** - Solution with 3 projects
- **FairiesPoker/** - Windows Forms client application
- **FairiesPoker/Protocol/** - Shared protocol library (client-server communication)
- **FPServer/** - Cross-platform TCP server (MySQL backend)

## Architecture

### Protocol Layer (`FairiesPoker/Protocol/`)
Shared between client and server:
- `Code/` - Operation codes (OpCode, AccountCode, FightCode, MatchCode, etc.)
- `Constant/` - Game constants (CardColor, CardType, CardWeight, Identity)
- `Dto/` - Data Transfer Objects for protobuf serialization

### Client Network Layer (`FairiesPoker/Net/`)
- `ClientPeer.cs` - Socket client with async receive/send
- `NetManager.cs` - Routes messages to handlers by OpCode
- `HandlerBase.cs` - Abstract handler base class
- `Impl/` - Handler implementations (AccountHandler, FightHandler, etc.)

### Server (`FPServer/`)
- `Network/ServerPeer.cs` - TCP server
- `Network/ClientConnection.cs` - Per-connection handling
- `Game/GameState.cs` - Game state machine (WAITING→DEALING→GRABBING→PLAYING→FINISHED)
- `Game/Room.cs`, `RoomManager.cs` - Match room management
- `Handlers/` - Server-side message handlers
- `Database/DbHelper.cs` - MySQL operations (MySqlConnector)
- `Tests/MultiplayerGameTests.cs` - Game logic unit tests

### Game Logic (Client)
- `Chupai.cs` - Validates played card combinations (牌型判断)
- `Jiepai.cs` - Determines if cards can beat previous play (接牌逻辑)
- `ComputerChuPai.cs` - AI opponent with staged strategy (early/mid/late game)
- `AIPlayer.cs` - AI decision-making with card memory tracking
- `CardMemory.cs` - Tracks played cards for AI inference

### UI Layer (Windows Forms)
- `Form1.cs` - Entry point
- `Main.cs` - Main menu (single/multiplayer selection)
- `Login.cs`, `Register.cs` - Authentication
- `DdzMian.cs` - Main game board (supports both offline and online modes)
- `Settings.cs` - Settings form
- `ImageCropperForm.cs` - Custom avatar upload with crop functionality

### Message Flow
Client sends `SocketMsg` with `OpCode`, `SubCode`, `Value` → `NetManager` routes to handler → Handler processes and updates UI via callbacks (`Models` events).

### OpCode Categories
- `ACCOUNT (0)` - Login, register
- `USER (1)` - User profile operations
- `MATCH (2)` - Matchmaking, room management
- `CHAT (3)` - Chat messages
- `FIGHT (4)` - Game actions (deal, grab landlord, play cards)
- `AVATAR (5)` - Avatar upload/approval

## Server Configuration

Server reads `appsettings.json` (auto-created on first run):
- Server: Host, Port (default 40960), MaxConnections
- Database: MySQL connection (host, port, database, username, password)
- Game: InitialBeans, BeansPerWin, BeansPerLose
- Avatar: AutoApprove toggle

Server console commands: `online`, `test`, `avatar on/off/list/ok/no/all`, `exit`

## Key Dependencies

Client: Newtonsoft.Json, protobuf-net, NAudio, System.Data.SqlClient
Server: MySqlConnector, protobuf-net, BCrypt.Net-Next, Microsoft.Extensions.Configuration/Logging

## UI Notes

- Forms use `ControlStyles.OptimizedDoubleBuffer` to reduce flicker
- Game uses embedded resources for card images and sounds
- Supports sliding card selection in multiplayer mode
- UI primarily in Chinese