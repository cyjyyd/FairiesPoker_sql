# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

FairiesPoker is a Windows Forms poker game (斗地主/Dou Di Zhu - a Chinese card game) with both offline single-player and online multiplayer modes. The project has a Disney fairies theme and is built with C#/.NET Framework 4.8.

## Build Commands

Build the solution using Visual Studio or MSBuild:

```bash
# Build with MSBuild (Debug)
msbuild FairiesPoker.sln /p:Configuration=Debug

# Build with MSBuild (Release)
msbuild FairiesPoker.sln /p:Configuration=Release

# Restore NuGet packages first (required for first build)
nuget restore FairiesPoker.sln
```

First-time build requires restoring NuGet packages: In Visual Studio, right-click the solution in Solution Explorer and select "Restore NuGet Packages".

## Project Structure

- **FairiesPoker.sln** - Main solution file
- **FairiesPoker/** - Main Windows Forms application project
- **FairiesPoker/Protocol/** - Shared protocol library (class library for client-server communication)

## Architecture

### Layered Architecture

The application follows a layered architecture:

1. **Protocol Layer** (`FairiesPoker/Protocol/`):
   - `Code/` - Operation codes (OpCode, AccountCode, FightCode, etc.)
   - `Constant/` - Game constants (CardColor, CardType, CardWeight, Identity)
   - `Dto/` - Data Transfer Objects for serialization

2. **Network Layer** (`FairiesPoker/Net/`):
   - `ClientPeer.cs` - Socket client wrapper with async receive/send
   - `NetManager.cs` - Network manager, routes messages to handlers
   - `HandlerBase.cs` - Abstract base class for message handlers
   - `Impl/` - Handler implementations (AccountHandler, FightHandler, etc.)

3. **Game Logic**:
   - `Chupai.cs` - Card playing rules validation (determines if played cards are valid)
   - `Jiepai.cs` - Card following logic (determines if cards can beat previous play)
   - `ComputerChuPai.cs` - AI for computer opponent
   - `Pai.cs`, `Puke.cs`, `Juese.cs` - Card and character models

4. **UI Layer** (Windows Forms):
   - `Form1.cs` - Entry point form
   - `Main.cs` - Main menu (single player / multiplayer selection)
   - `Login.cs`, `Register.cs` - Authentication forms
   - `DdzMian.cs` - Main game board (斗地主 gameplay)
   - `Settings.cs` - Settings form

5. **Models** (`FairiesPoker/Model/`):
   - `GameModel.cs` - Game state storage (user data, match room data)
   - `Models.cs` - Model container

### Message Flow

- Client sends `SocketMsg` objects containing `OpCode`, `SubCode`, and `Value`
- `NetManager` routes incoming messages to appropriate handlers based on `OpCode`
- Handlers process messages and update UI through callbacks

### OpCode Categories

- `ACCOUNT (0)` - Account operations (login, register)
- `USER (1)` - User operations
- `MATCH (2)` - Matchmaking
- `CHAT (3)` - Chat functionality
- `FIGHT (4)` - Game/battle operations

### Server Connection

The client connects to a server via TCP sockets. Default configuration in `NetManager.cs`:
- Debug mode: `127.0.0.1:40960`
- Production: `www.fairybcd.top:40960`

A separate server project (FPServer) is required for online functionality.

## Key Dependencies

- Newtonsoft.Json (13.0.2) - JSON serialization
- Nancy (2.0.0) - HTTP framework
- SanNiuSignal - Signal/library for socket communication
- Windows Media Player COM references - For audio playback

## UI Notes

- Forms use double buffering (`ControlStyles.OptimizedDoubleBuffer`) to reduce flicker
- Game uses embedded resources for card images and sounds
- UI is primarily in Chinese