# CongCraft - Medieval RPG

[![CI](https://github.com/Koschnag/cong-craft/actions/workflows/ci.yml/badge.svg)](https://github.com/Koschnag/cong-craft/actions/workflows/ci.yml)
[![Build & Release](https://github.com/Koschnag/cong-craft/actions/workflows/release.yml/badge.svg)](https://github.com/Koschnag/cong-craft/actions/workflows/release.yml)
[![Nightly](https://github.com/Koschnag/cong-craft/actions/workflows/nightly.yml/badge.svg)](https://github.com/Koschnag/cong-craft/actions/workflows/nightly.yml)
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

Grab the latest release from the [Releases page](../../releases). Self-contained builds available for:

- **macOS ARM64** (M1/M2/M3) — `CongCraft-macOS-arm64.tar.gz`
- **macOS x64** (Intel) — `CongCraft-macOS-x64.tar.gz`
- **Linux x64** — `CongCraft-linux-x64.tar.gz`
- **Windows x64** — `CongCraft-windows-x64.zip`

No .NET SDK required — just download, extract, and run.

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

## Tech Stack

- **.NET 8** — Cross-platform runtime
- **Silk.NET 2.21** — OpenGL 3.3 Core, GLFW windowing, OpenAL audio
- **xUnit** — 338 unit tests
- **System.Text.Json** — Save/load serialization

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for development setup and guidelines.

## Project Management

- **Issues**: Use [issue templates](https://github.com/Koschnag/cong-craft/issues/new/choose) for bugs, features, tasks, and epics
- **PRs**: Auto-labeled by files changed and size, with CI/CD and pre-release builds
- **Releases**: Automated multi-platform builds on version tags
- **Monitoring**: Nightly health checks, weekly metrics, dependency updates via Dependabot

### Vision System

Mit dem Vision System kannst du mehrere Feature-Ideen auf einmal einreichen. Der Vision Parser erstellt automatisch einzelne Issues daraus.

**Neues Vision-Issue erstellen:**

1. Gehe zu [Issues > New Issue](https://github.com/Koschnag/cong-craft/issues/new)
2. Titel: `[VISION] Dein Visions-Titel`
3. Body mit Bullet Points:
   ```
   - Groessere Spielwelt mit mehreren Regionen
   - Reittiere zum schnelleren Reisen
   - NPC-Siedlungen mit Wirtschaftssystem
   ```
4. Der Vision Parser erstellt automatisch ein Feature-Issue pro Bullet Point

**Vision Parser manuell triggern (z.B. fuer Issue #34):**

Falls der automatische Trigger nicht greift (z.B. bei Bot-erstellten Issues), kannst du den Parser manuell starten:

1. Gehe zu [Actions > Vision Parser](https://github.com/Koschnag/cong-craft/actions/workflows/vision-parser.yml)
2. Klicke **"Run workflow"**
3. Gib die Issue-Nummer ein (z.B. `34`)
4. Klicke **"Run workflow"** zur Bestaetigung

Der Parser erkennt Duplikate automatisch und ueberspringt bereits verarbeitete Vision-Issues.

### Inline-Befehle in Issue-Kommentaren

| Befehl | Aktion |
|--------|--------|
| `Bug: Beschreibung` | Erstellt ein Bug-Issue |
| `Fehler: Beschreibung` | Erstellt ein Bug-Issue |
| `Idee: Beschreibung` | Erstellt ein Feature-Issue |
| `Feature: Beschreibung` | Erstellt ein Feature-Issue |
| `/priority critical\|high\|medium\|low` | Aendert die Prioritaet |
| `/label name` | Fuegt ein Label hinzu |
| `/close` oder `/done` | Schliesst das Issue |
| `/reopen` | Oeffnet das Issue wieder |
| `/duplicate #N` | Markiert als Duplikat |

### Release-Feedback

Nach einem Release bekommst du ein Feedback-Issue. Genehmige mit:
- Thumbs-up Reaktion, oder
- Kommentar: `passt`, `genehmigt`, `approved`, `lgtm`, `weiter`

Bugs und Features kannst du direkt im selben Kommentar melden (jeweils eigene Zeile):
```
Bug: Spieler bleibt an Waenden haengen
Idee: Schnellreise zwischen Lagerfeuern
passt
```

## License

MIT
