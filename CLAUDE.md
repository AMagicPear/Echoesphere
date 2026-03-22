# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Echoesphere (回声之境) is a Unity 6000.3.9f1 project - a multimodal immersive interactive system for physical exhibitions. It communicates with a Raspberry Pi via TCP Socket for hardware interaction and uses gesture data to drive visual effects.

## Unity Development

- **Unity Version**: 6000.3.9f1
- **Render Pipeline**: URP (Universal Render Pipeline)
- **Input System**: Unity Input System (PlayerInput component)

## Build & Run

Open the project in Unity Hub or via command line:
```bash
open -a Unity
```

## Architecture

### Core Components

- **GameRoot** (`Assets/Scripts/Configuration/GameRoot.cs`): Singleton that holds references to `SaveManager` and `RaspberryPiCommunicator`. Persists across scenes via `DontDestroyOnLoad`.

- **SaveManager** (`Assets/Scripts/Configuration/SaveManager.cs`): Implements a provider pattern for game state persistence. Components implementing `ISaveProvider` can capture/restore state. Data is serialized as JSON and compressed with GZip.

- **RaspberryPiCommunicator** (`Assets/Scripts/RaspberryPi/RaspberryPiCommunicator.cs`): TCP server (default port 65432) that accepts multiple clients. Supports text and image messages. Used to communicate with external hardware (see [echoesphere-communication](https://github.com/AMagicPear/echesphere-mediapipe)).

### Key Scripts

| Script | Namespace | Purpose |
|--------|-----------|---------|
| `TravelerController` | `Echoesphere.Runtime.Traveler` | FPS character controller with gravity, rotation smoothing, and animation |
| `GhostMountain` | `Echoesphere.Runtime.Stuff` | Mountain wobble effect driven by gesture data (received via TCP) |
| `HitBlock` | `Echoesphere.Runtime.Stuff` | Trigger-based block that sends collision events to Raspberry Pi |
| `AudioRecorder` | `Echoesphere.Runtime.RaspberryPi` | Records game audio via `OnAudioFilterRead`, outputs WAV files |
| `SceneLoader` | `Echoesphere.Runtime.UI` | Async scene loading with fade animations |
| `ButtonScale` | `Echoesphere.Runtime.UI` | UI button hover/select animations using DOTween |

### Assembly Definitions

- `Assets/Scripts/Echoesphere.Runtime.asmdef` - Runtime scripts
- `Assets/Tests/Echoesphere.Tests.asmdef` - Test scripts

### Script Organization

```
Assets/Scripts/
├── Configuration/     # GameRoot, SaveManager, ISaveProvider, EchoesphereSaveData
├── RaspberryPi/        # TCP communication and audio recording
├── Traveler/           # Player character controller
├── Stuff/              # Interactive objects (GhostMountain, HitBlock, RotatableAvatar)
├── Puzzle/             # Avatar rotation puzzle logic
├── UI/                 # Scene loading and button animations
└── Helpers/            # CompressionHelper for GZip
```

## Communication Protocol

The Raspberry Pi communicates via TCP with a custom binary protocol:
- 4-byte length prefix (network byte order)
- 1-byte message type (`0x00` = Text, `0x01` = Image)
- Payload data

Text messages are JSON-encoded. The `GhostMountain.OnReceiveTcpMessage` expects `HandData` JSON:
```json
{"h": 1, "x": 0.5, "y": 0.5, "v": 0.1}
```

## Testing

Unit tests use NUnit. Run tests via Unity's Test Runner or via command line using the test assemblies:
- `Tests.csproj` - Editor tests
- `TestsPlayMode.csproj` - Play mode tests

## Shaders

Custom shader graphs in `Assets/Shader/`:
- `GhostMountain.shadergraph` - Mountain material with wobble effect
- `StylizedGrassShader.shadergraph` - Grass rendering
- `Water/Water.shadergraph` - Water rendering
