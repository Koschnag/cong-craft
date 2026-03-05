# CLAUDE.md - CongCraft Development Guide

## Project Overview
CongCraft is a medieval action RPG built from scratch in .NET 8 with OpenGL 3.3 via Silk.NET. No game engine - all systems hand-crafted.

## Build & Test Commands
```bash
dotnet restore          # Restore NuGet packages
dotnet build            # Build all projects
dotnet test             # Run all unit tests
dotnet run --project src/CongCraft.Game  # Run the game
```

## Project Structure
```
CongCraft.sln
├── src/
│   ├── CongCraft.Engine/     # Core game engine (class library, .NET 8)
│   │   ├── Audio/            # Procedural audio & music
│   │   ├── Boss/             # Boss fight mechanics
│   │   ├── Combat/           # Combat system & enemy AI
│   │   ├── Core/             # GameTime, DevLog, ServiceLocator
│   │   ├── Crafting/         # Crafting stations & recipes
│   │   ├── Dialogue/         # NPC dialogue trees
│   │   ├── Dungeon/          # Procedural dungeon generation
│   │   ├── ECS/              # Entity Component System (Entity, World, Components, Systems)
│   │   ├── Environment/      # Day/night cycle, sky, water, vegetation
│   │   ├── Input/            # Input handling
│   │   ├── Inventory/        # Items, equipment, loot
│   │   ├── Leveling/         # XP, levels, skill trees
│   │   ├── Magic/            # Spells & mana
│   │   ├── Procedural/       # Mesh builders & texture generation
│   │   ├── Quest/            # Quest system
│   │   ├── Rendering/        # OpenGL rendering, shaders, camera
│   │   ├── SaveLoad/         # JSON save/load system
│   │   ├── Terrain/          # World terrain generation (FastNoiseLite)
│   │   ├── UI/               # HUD, health bars, minimap
│   │   ├── VFX/              # Particle effects, torches, point lights
│   │   └── Weather/          # Weather system
│   └── CongCraft.Game/       # Executable entry point (Native AOT)
└── tests/
    └── CongCraft.Engine.Tests/  # xUnit test suite
```

## Architecture
- **ECS (Entity Component System)**: Custom implementation - components hold data, systems hold logic
- **Native AOT**: Compiled to native binaries, no .NET runtime needed
- **AOT-Safe**: All code must be AOT-compatible (no reflection-based serialization)
- **JSON Serialization**: Use `System.Text.Json` with source generators for AOT compatibility

## Code Conventions
- Target framework: `net8.0`
- Nullable reference types: enabled
- Unsafe code: allowed where needed for native interop
- Test framework: xUnit 2.x
- Dependencies: Silk.NET 2.21 for OpenGL/GLFW/OpenAL
- Test naming: `MethodName_Scenario_ExpectedResult`

## CI/CD
- **CI**: Runs on every push/PR to main - build, test, code coverage
- **Release**: Tag `v*` triggers multi-platform release (Windows/macOS/Linux)
