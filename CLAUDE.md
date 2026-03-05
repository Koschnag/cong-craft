# CLAUDE.md - CongCraft Development Guide

## Project Overview
CongCraft is a medieval action RPG built from scratch in .NET 8 with OpenGL 3.3 via Silk.NET. No game engine - all systems hand-crafted.

## Build & Test Commands
```bash
dotnet restore          # Restore NuGet packages
dotnet build            # Build all projects
dotnet test             # Run all unit tests
dotnet run --project src/CongCraft.Game  # Run the game

# Native AOT publish (platform-specific)
dotnet publish src/CongCraft.Game/CongCraft.Game.csproj --configuration Release --runtime linux-x64 --output ./publish
```

## Project Structure
```
CongCraft.sln
├── src/
│   ├── CongCraft.Engine/     # Core game engine (class library, .NET 8)
│   │   ├── Audio/            # Procedural audio & music
│   │   ├── Boss/             # Boss fight mechanics
│   │   ├── Combat/           # Combat system
│   │   ├── Core/             # ECS, GameTime, ServiceLocator
│   │   ├── Crafting/         # Crafting stations & recipes
│   │   ├── Dialogue/         # NPC dialogue trees
│   │   ├── Dungeon/          # Procedural dungeon generation
│   │   ├── Environment/      # Day/night cycle
│   │   ├── Inventory/        # Items & equipment
│   │   ├── Leveling/         # XP, levels, skill trees
│   │   ├── Magic/            # Spells & mana
│   │   ├── Procedural/       # Procedural generation utilities
│   │   ├── Quest/            # Quest system
│   │   ├── Rendering/        # OpenGL rendering, shaders, camera
│   │   ├── SaveLoad/         # JSON save/load system
│   │   ├── Terrain/          # World terrain generation
│   │   ├── UI/               # UI components & HUD
│   │   ├── VFX/              # Particle effects
│   │   └── Weather/          # Weather system
│   └── CongCraft.Game/       # Executable entry point (Native AOT)
└── tests/
    └── CongCraft.Engine.Tests/  # xUnit test suite (52+ test files)
```

## Architecture
- **ECS (Entity Component System)**: Custom implementation with priority-based system execution
- **Native AOT**: Compiled to native binaries, no .NET runtime needed
- **AOT-Safe**: All code must be AOT-compatible (no reflection-based serialization)
- **JSON Serialization**: Use `System.Text.Json` with source generators for AOT compatibility

## Code Conventions
- Target framework: `net8.0`
- Nullable reference types: enabled
- Unsafe code: allowed where needed for native interop
- Test framework: xUnit 2.x
- Dependencies: Silk.NET 2.21 for OpenGL/GLFW/OpenAL

## CI/CD
- **CI**: Runs on every push/PR - build, test, code coverage
- **Quality Gate**: Code formatting, build warnings, test coverage threshold (30%)
- **Auto-Merge**: PRs auto-approved and squash-merged when all quality gates pass
- **Auto-Fix**: Formatting issues auto-fixed via PR when self-test detects them
- **Release**: Tag `v*` triggers multi-platform release (Windows/macOS/Linux)
- **Pre-release**: PR builds create testing pre-releases (macOS ARM64)
- **Nightly**: Daily health check builds across all platforms
- **Self-Test Loop**: Every 6 hours - build, test, code health analysis
- **Dependabot**: Weekly NuGet and GitHub Actions dependency updates

## Autonomous Development Cycle
1. Code changes pushed to feature branch
2. PR created -> CI + Quality Gate run automatically
3. All checks pass -> Auto-approved and auto-merged (no manual review needed)
4. Tag `vX.Y.Z` -> Multi-platform release built automatically
5. Release published -> Feedback issue created for @Koschnag
6. User tests, provides feedback (bugs/ideas), approves with thumbs-up or "passt"
7. Feedback parsed -> Bug/Feature issues auto-created
8. Next development cycle begins

## Feedback Keywords
In release feedback issues, use these prefixes to auto-create issues:
- `Bug: <description>` -> Creates bug issue
- `Fehler: <description>` -> Creates bug issue
- `Idea: <description>` -> Creates feature issue
- `Idee: <description>` -> Creates feature issue
- `Feature: <description>` -> Creates feature issue
- `Vision: <description>` -> Creates feature issue

## Labels
PRs are auto-labeled by:
- **Files changed**: `area: engine`, `area: combat`, `area: rendering`, etc.
- **PR size**: `size: XS` to `size: XL`
- Issues are auto-triaged by keywords in title/body
