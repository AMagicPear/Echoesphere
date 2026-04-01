# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Echoesphere (回声之境) is a Unity 6000.3.9f1 project - a multimodal immersive interactive system for physical exhibitions. It communicates with external hardware (Raspberry Pi) via TCP Socket for hardware interaction and uses gesture data to drive visual effects.

## Unity Development

- **Unity Version**: 6000.3.9f1
- **Render Pipeline**: URP (Universal Render Pipeline)
- **Input System**: Unity Input System (PlayerInput component)
  - Action definitions in `Assets/InputSystem/InputSystem_Actions.inputactions` and `Player_Actions.inputactions`

## Build & Run

Open the project in Unity Hub or via command line:
```bash
open -a Unity
```

Tests run via Unity's Test Runner window (Window > General > Test Runner).

## Key Scenes

- `Assets/Scenes/TheGrass.unity` - Main grass field scene
- `Assets/Scenes/PuzzleAvatar.unity` - Avatar rotation puzzle
- `Assets/Scenes/PuzzleRolling.unity` - Rolling puzzle
- `Assets/Scenes/SaveTest.unity` - Save system test scene

## Architecture

### Core Components

- **GameRoot** (`Assets/Scripts/Configuration/GameRoot.cs`): Singleton that holds references to `SaveManager` and `AgentCommunicator`. Persists across scenes via `DontDestroyOnLoad`.

- **SaveManager** (`Assets/Scripts/Configuration/SaveManager.cs`): Implements a provider pattern for game state persistence. Components implementing `ISaveProvider` can capture/restore state. Data is serialized as JSON and compressed with GZip.

- **AgentCommunicator** (`Assets/Scripts/Agent/AgentCommunicator.cs`): TCP client (default: 127.0.0.1:65432) that connects to external hardware. Supports text, image, command, request/response messages. Sends screenshot on `request_screenshot` command (see [echoesphere-communication](https://github.com/AMagicPear/echesphere-mediapipe)).

### Key Scripts

| Script | Namespace | Purpose |
|--------|-----------|---------|
| `AgentCommunicator` | `Echoesphere.Runtime.Agent` | TCP client for hardware communication, screenshot capture |
| `TravelerController` | `Echoesphere.Runtime.Traveler` | FPS character controller with gravity, rotation smoothing, and animation |
| `GhostMountain` | `Echoesphere.Runtime.Stuff` | Mountain wobble effect driven by gesture data (received via TCP) |
| `HitBlock` | `Echoesphere.Runtime.Stuff` | Trigger-based block that sends collision events to Raspberry Pi |
| `AudioRecorder` | `Echoesphere.Runtime.Agent` | Records game audio via `OnAudioFilterRead`, outputs WAV files |
| `SceneLoader` | `Echoesphere.Runtime.UI` | Async scene loading with fade animations |
| `ButtonScale` | `Echoesphere.Runtime.UI` | UI button hover/select animations using DOTween |

### Assembly Definitions

- `Assets/Scripts/Echoesphere.Runtime.asmdef` - Runtime scripts
- `Assets/Tests/Echoesphere.Tests.asmdef` - Editor tests (references Echoesphere.Runtime)

### Vendor Assets

Third-party assets in `Assets/Vendor/`:
- `DavidJalbert/LowPolyPeople/` - Low-poly character models
- `IL3DN/` - FPS controller and wind scripts
- `Platformer_8_Underworld/` - Animation utilities (BlendShapeAnimator, OscillatePosition/Rotation/Scale)

### Settings

Render pipeline assets in `Assets/Settings/`:
- `PC_RPAsset.asset` / `PC_Renderer.asset` - Desktop URP config
- `Mobile_RPAsset.asset` / `Mobile_Renderer.asset` - Mobile URP config

### Script Organization

```
Assets/Scripts/
├── Agent/             # AgentCommunicator (TCP client), AudioRecorder
├── Configuration/     # GameRoot, SaveManager, ISaveProvider, EchoesphereSaveData
├── Helpers/           # CompressionHelper for GZip
├── Puzzle/            # Avatar rotation puzzle logic
├── Stuff/             # Interactive objects (GhostMountain, HitBlock, RotatableAvatar)
├── Traveler/          # Player character controller
└── UI/                # Scene loading and button animations
```

## Communication Protocol

The external hardware communicates via TCP with a custom binary protocol:
- 4-byte length prefix (network byte order)
- 1-byte message type (`0x00` = Text, `0x01` = Image, `0x02` = Command, `0x03` = Register, `0x04` = Request, `0x05` = Response)
- Payload data

Text/Command messages are JSON-encoded. The `GhostMountain.OnReceiveTcpMessage` expects `HandData` JSON:
```json
{"h": 1, "x": 0.5, "y": 0.5, "v": 0.1}
```

When receiving `request_screenshot` command, `AgentCommunicator` captures the screen and sends it back as a Response message.

## Testing

Unit tests use NUnit. Run via Window > General > Test Runner in Unity. Single tests can be run from the Test Runner UI by right-clicking a test method.

## Shaders

Custom shader graphs in `Assets/Shader/`:
- `GhostMountain.shadergraph` - Mountain material with wobble effect
- `StylizedGrassShader.shadergraph` - Grass rendering
- `Water/Water.shadergraph` - Water rendering
