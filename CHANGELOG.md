# Changelog

All notable changes to CongCraft will be documented in this file.

## [1.0.0] - 2025-03-05

### Initial Release

First playable release of CongCraft — a medieval action RPG built from scratch in .NET 8 with OpenGL 3.3.

### Game Systems
- **Entity Component System (ECS)** — Custom implementation with priority-based system execution
- **Procedural World Generation** — Terrain with FastNoiseLite, vegetation, rocks, water planes, and sky rendering
- **Combat System** — Melee attacks, blocking (right click), dodging (Q) with damage calculations and cooldowns
- **Enemy AI** — Multiple enemy types with AI behavior, spawning system, and procedurally generated enemy meshes
- **Boss Fights** — Multi-phase boss encounters with special attacks, enrage mechanics, and phase transitions
- **Magic System** — 4 spells (Fireball, Heal, Shield, Ice Nova) with mana management and cooldowns
- **Crafting System** — 3 station types (Anvil, Alchemy Table, Workbench) with recipe database
- **Leveling & Skills** — XP from kills, level-up system, skill tree with Strength/Endurance/Agility
- **Quest System** — Quest journal with objectives, quest database, and quest state tracking
- **Dialogue System** — NPC dialogue trees with branching choices and dialogue database
- **Inventory & Equipment** — Item database, inventory management, equipment slots (weapon/armor)
- **Dungeon Generation** — Procedurally generated dungeon rooms with enemies and loot
- **Weather System** — Dynamic transitions between Clear, Cloudy, Rain, Storm, and Fog
- **Day/Night Cycle** — Sun movement, ambient lighting changes, torch lighting at night
- **Save/Load System** — JSON-based with AOT-safe source-generated serialization (F5/F9)
- **Minimap** — Real-time terrain minimap with markers for NPCs, enemies, bosses, and stations
- **Particle VFX** — Combat hits, spell effects, boss attacks, rain, lightning
- **Procedural Audio** — Generated sound effects and ambient music via OpenAL

### Technical
- .NET 8 with Native AOT compilation — no runtime required
- OpenGL 3.3 Core via Silk.NET 2.21
- Cross-platform builds: Windows x64, macOS ARM64/x64, Linux x64
- 53 test files with comprehensive unit test coverage
- AOT-safe JSON serialization using System.Text.Json source generators
