# CongCraft - Medieval Action RPG

[![CI](https://github.com/Koschnag/cong-craft/actions/workflows/ci.yml/badge.svg)](https://github.com/Koschnag/cong-craft/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/8.0)

A Gothic/Risen-inspired action RPG built from scratch in .NET 8 with OpenGL 3.3. No game engine — all systems hand-crafted, all graphics procedurally generated.

## Features

- **ECS Architecture** — Custom Entity Component System with priority-based system execution
- **Procedural World** — Terrain, vegetation, rocks, water, and sky all generated at runtime
- **Combat System** — Melee attacks, blocking, dodging with damage and cooldown mechanics
- **Magic System** — 4 spells (Fireball, Heal, Shield, Ice Nova) with mana and cooldowns
- **Boss Fights** — Multi-phase boss encounters with special attacks and enrage mechanics
- **Crafting** — Gather materials, craft weapons/armor/potions at stations (Anvil, Alchemy, Workbench)
- **Skill/Leveling** — XP from kills, skill points for Strength/Endurance/Agility
- **Quests & Dialogue** — NPC dialogue trees with branching choices and quest objectives
- **Inventory & Equipment** — Collect, equip, and manage items across weapon/armor slots
- **Dungeons** — Procedurally generated dungeon rooms with enemies and loot
- **Weather** — Dynamic weather transitions (Clear, Cloudy, Rain, Storm, Fog) with lighting effects
- **Day/Night Cycle** — Sun movement, ambient lighting changes, torch lighting at night
- **Save/Load** — JSON-based save system (F5 Save, F9 Load)
- **Minimap** — Real-time terrain minimap with NPC, enemy, boss, and station markers
- **Particle VFX** — Combat hits, spell effects, boss attacks, rain, lightning
- **Procedural Audio** — Generated sound effects and ambient music

## Controls

| Key | Action |
|-----|--------|
| WASD | Move |
| Shift | Run |
| Space | Jump |
| Left Click | Attack |
| Right Click | Block |
| Q | Dodge |
| I | Inventory |
| T | Talk to NPC |
| C | Craft (near station) |
| L | Skill menu |
| F1-F4 | Cast spells |
| F5 | Save |
| F9 | Load |
| G | Enter dungeon |

## Download

Grab the latest release from the [Releases page](../../releases). Self-contained Native AOT builds — no .NET runtime required.

| Platform | Download |
|----------|----------|
| macOS ARM64 (M1/M2/M3) | `CongCraft-macOS-arm64.tar.gz` |
| macOS x64 (Intel) | `CongCraft-macOS-x64.tar.gz` |
| Linux x64 | `CongCraft-linux-x64.tar.gz` |
| Windows x64 | `CongCraft-windows-x64.zip` |

### macOS

```bash
tar -xzf CongCraft-macOS-arm64.tar.gz
chmod +x CongCraft.Game
./CongCraft.Game
```

If macOS blocks the app: System Settings > Privacy & Security > Allow.

### Linux

```bash
tar -xzf CongCraft-linux-x64.tar.gz
chmod +x CongCraft.Game
./CongCraft.Game
```

### Windows

Extract the zip and run `CongCraft.Game.exe`.

## Build from Source

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
dotnet build
dotnet test
dotnet run --project src/CongCraft.Game
```

## Project Structure

```
CongCraft.sln
├── src/
│   ├── CongCraft.Engine/     # Core game engine (class library)
│   │   ├── Audio/            # Procedural audio & music
│   │   ├── Boss/             # Boss fight mechanics
│   │   ├── Combat/           # Combat system & enemy AI
│   │   ├── Core/             # ECS, GameTime, ServiceLocator
│   │   ├── Crafting/         # Crafting stations & recipes
│   │   ├── Dialogue/         # NPC dialogue trees
│   │   ├── Dungeon/          # Procedural dungeon generation
│   │   ├── ECS/              # Entity Component System
│   │   ├── Environment/      # Day/night cycle, sky, water
│   │   ├── Input/            # Input handling
│   │   ├── Inventory/        # Items & equipment
│   │   ├── Leveling/         # XP, levels, skill trees
│   │   ├── Magic/            # Spells & mana
│   │   ├── Procedural/       # Mesh & texture generation
│   │   ├── Quest/            # Quest system
│   │   ├── Rendering/        # OpenGL rendering, shaders
│   │   ├── SaveLoad/         # JSON save/load system
│   │   ├── Terrain/          # World terrain generation
│   │   ├── UI/               # HUD & minimap
│   │   ├── VFX/              # Particle effects & lighting
│   │   └── Weather/          # Weather system
│   └── CongCraft.Game/       # Executable entry point (Native AOT)
└── tests/
    └── CongCraft.Engine.Tests/  # xUnit test suite (53 test files)
```

## Tech Stack

- **.NET 8** with Native AOT — compiled to native binaries
- **Silk.NET 2.21** — OpenGL 3.3 Core, GLFW windowing, OpenAL audio
- **xUnit** — comprehensive unit test suite
- **System.Text.Json** — AOT-safe serialization with source generators

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for development setup and guidelines.

## License

MIT
