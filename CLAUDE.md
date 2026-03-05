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
- **Auto-Version**: After merge to main, auto-creates `v*` tag (semantic versioning)
- **Auto-Fix**: Formatting issues auto-fixed via PR when self-test detects them
- **Release**: Tag `v*` triggers multi-platform release (Windows/macOS/Linux)
- **Pre-release**: PR builds create testing pre-releases (macOS ARM64)
- **Nightly**: Daily health check builds across all platforms
- **Self-Test Loop**: Every 6 hours - build, test, code health analysis
- **Daily Report**: Every day at 20:00 UTC - full status report as GitHub Issue
- **Weekly Metrics**: Every Sunday - codebase statistics
- **Dependabot**: Weekly NuGet and GitHub Actions dependency updates

## Fully Autonomous System
Everything runs automatically. Zero manual intervention needed.

### Bootstrap (runs once on merge to main)
- Labels, variables, branch protection all auto-configured
- Welcome issue with instructions auto-created
- No manual workflow triggers needed

### Autonomous Development Cycle
1. Code pushed to feature branch -> PR auto-created
2. CI + Quality Gate run automatically
3. All checks pass -> Auto-approved and auto-merged
4. Merge to main -> Auto-version tag created (vX.Y.Z)
5. Tag triggers multi-platform release (Windows/macOS/Linux)
6. Release published -> Feedback issue created for @Koschnag
7. User tests, gives feedback, approves (thumbs-up or "passt")
8. Feedback parsed -> Bug/Feature issues auto-created
9. Next cycle begins

### Daily Report (Tagesbericht)
- Runs every day at 20:00 UTC (21:00 CET)
- Posted as comment to pinned "Tagesbericht" issue
- Contains: build status, test results, coverage, open issues, recent commits, workflow activity
- No action needed - just read it

### Self-Healing
- Self-test loop runs every 6 hours
- Formatting issues -> Auto-fix PR created
- Build failures -> Issue auto-created
- When fixed -> Failure issues auto-closed

## User Interface (Issues only)

### Vision Issues
Create issue with title `[VISION] Your Vision Title`:
```
- Groessere Spielwelt mit mehreren Regionen
- Reittiere zum schnelleren Reisen
- NPC-Siedlungen mit Wirtschaftssystem
```
Each bullet point becomes a separate feature issue, auto-categorized.

### Inline Commands (in any issue comment)
- `Bug: <description>` -> Creates bug issue
- `Fehler: <description>` -> Creates bug issue
- `Idee: <description>` -> Creates feature issue
- `Feature: <description>` -> Creates feature issue
- `Vision: <description>` -> Creates feature issue
- `/priority critical|high|medium|low` -> Changes issue priority
- `/label <name>` -> Adds label
- `/close` or `/done` -> Closes issue
- `/reopen` -> Reopens issue
- `/duplicate #N` -> Marks as duplicate

### Release Feedback
After each release, approve with:
- Thumbs-up reaction
- Or comment: "passt", "genehmigt", "approved", "lgtm", "weiter"

## Labels
PRs are auto-labeled by:
- **Files changed**: `area: engine`, `area: combat`, `area: rendering`, etc.
- **PR size**: `size: XS` to `size: XL`
- Issues are auto-triaged by keywords in title/body
