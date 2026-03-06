using System.Numerics;

namespace CongCraft.Engine.Level;

/// <summary>
/// Level 1: "Ashvale" — a medieval frontier settlement at the edge of dark wilderness.
///
/// Layout inspired by SpellForce 1 (Greydusk Vale), Gothic 2 (Khorinis outskirts),
/// and Two Worlds 1 (starting village area).
///
/// Geography (256x256 world units):
/// ┌────────────────────────────────────────────┐
/// │  MOUNTAINS (north, impassable cliffs)       │
/// │  ┌──────────┐                               │
/// │  │ SKELETON │    DARK FOREST                │
/// │  │  RUINS   │   (wolves, bandits)           │
/// │  └────┬─────┘        ┌──────────┐           │
/// │       │ path         │  TROLL   │           │
/// │       │              │  CAVE    │           │
/// │  ─────┴──────────────┴──┬───────┘           │
/// │       CROSSROADS        │                    │
/// │  ┌──────────┐   ┌──────┴──────┐             │
/// │  │  ASHVALE │   │  FARMLANDS  │             │
/// │  │ VILLAGE  │   │   (fields)  │             │
/// │  │ ☼ spawn  │   └─────────────┘             │
/// │  └────┬─────┘                                │
/// │       │ road south                           │
/// │  ─────┴──── RIVER (east-west) ──────────    │
/// │            ┌──────────┐                      │
/// │     LAKE   │  BANDIT  │                      │
/// │            │  CAMP    │                      │
/// │            └──────────┘                      │
/// │  SOUTHERN HILLS (gentle, scattered rocks)    │
/// └────────────────────────────────────────────┘
/// </summary>
public static class LevelOneData
{
    public static LevelData Create()
    {
        var level = new LevelData
        {
            Name = "Ashvale - The Frontier",
            WorldSize = 256f,
            WaterLevel = 1.8f,
            PlayerSpawn = new Vector3(0, 10, 8),

            // ════════════════════════════════════════════
            // HEIGHT FEATURES — terrain sculpting
            // ════════════════════════════════════════════
            HeightFeatures = new List<HeightFeature>
            {
                // === NORTHERN MOUNTAINS (impassable barrier) ===
                new() { Type = HeightFeature.FeatureType.Ridge, CenterX = 0, CenterZ = -100, Radius = 130, Height = 35f, Falloff = 2.5f, WidthX = 2.5f, WidthZ = 0.6f },
                new() { Type = HeightFeature.FeatureType.Hill, CenterX = -40, CenterZ = -95, Radius = 35, Height = 28f, Falloff = 2f },
                new() { Type = HeightFeature.FeatureType.Hill, CenterX = 50, CenterZ = -90, Radius = 30, Height = 25f, Falloff = 2f },
                new() { Type = HeightFeature.FeatureType.Cliff, CenterX = -20, CenterZ = -80, Radius = 25, Height = 22f, Falloff = 3f, Rotation = 0.3f },

                // === VILLAGE PLATEAU (flat area for buildings) ===
                new() { Type = HeightFeature.FeatureType.Plateau, CenterX = 0, CenterZ = 10, Radius = 28, Height = 5f, Falloff = 1.5f },

                // === GENTLE HILLS around village ===
                new() { Type = HeightFeature.FeatureType.Hill, CenterX = -35, CenterZ = 5, Radius = 20, Height = 8f, Falloff = 1.8f },
                new() { Type = HeightFeature.FeatureType.Hill, CenterX = 40, CenterZ = 15, Radius = 25, Height = 7f, Falloff = 1.6f },
                new() { Type = HeightFeature.FeatureType.Hill, CenterX = 25, CenterZ = -10, Radius = 15, Height = 6f, Falloff = 1.5f },

                // === FARMLANDS — gently rolling terrain east ===
                new() { Type = HeightFeature.FeatureType.Plateau, CenterX = 50, CenterZ = 5, Radius = 35, Height = 4f, Falloff = 1.3f },
                new() { Type = HeightFeature.FeatureType.Hill, CenterX = 60, CenterZ = 20, Radius = 12, Height = 3f, Falloff = 1.2f },

                // === DARK FOREST AREA — elevated, hilly ===
                new() { Type = HeightFeature.FeatureType.Hill, CenterX = -15, CenterZ = -45, Radius = 40, Height = 12f, Falloff = 1.6f },
                new() { Type = HeightFeature.FeatureType.Hill, CenterX = 20, CenterZ = -55, Radius = 30, Height = 14f, Falloff = 1.8f },
                new() { Type = HeightFeature.FeatureType.Hill, CenterX = -40, CenterZ = -50, Radius = 20, Height = 10f, Falloff = 1.5f },

                // === SKELETON RUINS — elevated stone outcrop ===
                new() { Type = HeightFeature.FeatureType.Plateau, CenterX = -50, CenterZ = -55, Radius = 18, Height = 15f, Falloff = 2.5f },

                // === TROLL CAVE — rocky outcrop with bowl ===
                new() { Type = HeightFeature.FeatureType.Hill, CenterX = 55, CenterZ = -45, Radius = 20, Height = 18f, Falloff = 2.2f },
                new() { Type = HeightFeature.FeatureType.Bowl, CenterX = 55, CenterZ = -45, Radius = 10, Height = -4f, Falloff = 1.5f },

                // === RIVER VALLEY (east-west depression) ===
                new() { Type = HeightFeature.FeatureType.Valley, CenterX = 0, CenterZ = 45, Radius = 120, Height = -3f, Falloff = 1.2f, WidthX = 3f, WidthZ = 0.3f },

                // === LAKE (south-west depression) ===
                new() { Type = HeightFeature.FeatureType.Bowl, CenterX = -40, CenterZ = 55, Radius = 25, Height = -5f, Falloff = 1.8f },

                // === SOUTHERN HILLS ===
                new() { Type = HeightFeature.FeatureType.Hill, CenterX = -20, CenterZ = 80, Radius = 25, Height = 9f, Falloff = 1.5f },
                new() { Type = HeightFeature.FeatureType.Hill, CenterX = 30, CenterZ = 75, Radius = 20, Height = 7f, Falloff = 1.4f },
                new() { Type = HeightFeature.FeatureType.Hill, CenterX = 60, CenterZ = 85, Radius = 22, Height = 8f, Falloff = 1.6f },

                // === BANDIT CAMP — slight elevated area south ===
                new() { Type = HeightFeature.FeatureType.Plateau, CenterX = 25, CenterZ = 60, Radius = 15, Height = 4f, Falloff = 1.5f },

                // === CROSSROADS area — flat junction ===
                new() { Type = HeightFeature.FeatureType.Plateau, CenterX = 0, CenterZ = -20, Radius = 12, Height = 6f, Falloff = 1.3f },
            },

            // ════════════════════════════════════════════
            // TERRAIN ZONES — texture blending areas
            // ════════════════════════════════════════════
            Zones = new List<TerrainZone>
            {
                // Village green
                new() { Type = "village", CenterX = 0, CenterZ = 10, Radius = 30 },
                // Farmlands
                new() { Type = "grass", CenterX = 50, CenterZ = 5, Radius = 40, Strength = 0.8f },
                // Dark forest floor
                new() { Type = "forest", CenterX = -15, CenterZ = -45, Radius = 45 },
                new() { Type = "forest", CenterX = 20, CenterZ = -55, Radius = 35 },
                // Rocky ruins
                new() { Type = "stone", CenterX = -50, CenterZ = -55, Radius = 22 },
                // Troll cave rocks
                new() { Type = "stone", CenterX = 55, CenterZ = -45, Radius = 22 },
                // Mountains — snow and stone
                new() { Type = "stone", CenterX = 0, CenterZ = -95, Radius = 80, Strength = 0.9f },
                new() { Type = "snow", CenterX = 0, CenterZ = -110, Radius = 60, Strength = 0.7f },
                // Southern hills
                new() { Type = "grass", CenterX = 0, CenterZ = 80, Radius = 50, Strength = 0.6f },
                // Lake shore — swamp
                new() { Type = "swamp", CenterX = -40, CenterZ = 55, Radius = 30, Strength = 0.5f },
            },

            // ════════════════════════════════════════════
            // PATHS / ROADS
            // ════════════════════════════════════════════
            Paths = new List<PathSegment>
            {
                // Main road through village
                new() { StartX = 0, StartZ = 30, EndX = 0, EndZ = -5, Width = 4f },
                // Village to crossroads
                new() { StartX = 0, StartZ = -5, EndX = 0, EndZ = -20, Width = 3.5f },
                // Crossroads to forest (north)
                new() { StartX = 0, StartZ = -20, EndX = -10, EndZ = -40, Width = 3f },
                // Forest to ruins
                new() { StartX = -10, StartZ = -40, EndX = -50, EndZ = -50, Width = 2.5f },
                // Crossroads east to farmlands
                new() { StartX = 0, StartZ = -20, EndX = 40, EndZ = -10, Width = 3f },
                // Farmlands to troll area
                new() { StartX = 40, StartZ = -10, EndX = 55, EndZ = -35, Width = 2f },
                // Village south to river
                new() { StartX = 0, StartZ = 30, EndX = 0, EndZ = 48, Width = 3f },
                // River crossing to bandit camp
                new() { StartX = 0, StartZ = 48, EndX = 25, EndZ = 55, Width = 2.5f },
                // Village east to farms
                new() { StartX = 15, StartZ = 10, EndX = 40, EndZ = 5, Width = 3f },
            },

            // ════════════════════════════════════════════
            // TREES — hand-placed clusters
            // ════════════════════════════════════════════
            Trees = CreateTrees(),

            // ════════════════════════════════════════════
            // ROCKS — placed at cliffs, paths, rivers
            // ════════════════════════════════════════════
            Rocks = CreateRocks(),

            // ════════════════════════════════════════════
            // BUSHES / GROUND COVER
            // ════════════════════════════════════════════
            Bushes = CreateBushes(),

            // ════════════════════════════════════════════
            // RUINS — skeleton ruins area + scattered
            // ════════════════════════════════════════════
            Ruins = CreateRuins(),

            // ════════════════════════════════════════════
            // NPCs — in village
            // ════════════════════════════════════════════
            Npcs = new List<NpcPlacement>
            {
                new() { X = 8, Z = 12, NpcType = "blacksmith", Name = "Blacksmith Aldric", FacingAngle = MathF.PI * 0.5f },
                new() { X = -6, Z = 15, NpcType = "elder", Name = "Elder Maren", FacingAngle = 0 },
                new() { X = 12, Z = 5, NpcType = "merchant", Name = "Merchant Gregor", FacingAngle = MathF.PI },
            },

            // ════════════════════════════════════════════
            // ENEMY ZONES
            // ════════════════════════════════════════════
            EnemyZones = new List<EnemyZone>
            {
                // Wolves near forest edge (easy)
                new() { CenterX = -20, CenterZ = -30, Radius = 15, EnemyType = "wolf", Count = 3, MinLevel = 1 },
                new() { CenterX = 10, CenterZ = -35, Radius = 12, EnemyType = "wolf", Count = 2, MinLevel = 1 },

                // Bandits in forest (medium)
                new() { CenterX = -10, CenterZ = -50, Radius = 18, EnemyType = "bandit", Count = 3, MinLevel = 2 },
                new() { CenterX = 15, CenterZ = -55, Radius = 14, EnemyType = "bandit", Count = 2, MinLevel = 2 },

                // Bandit camp south (medium)
                new() { CenterX = 25, CenterZ = 60, Radius = 12, EnemyType = "bandit", Count = 4, MinLevel = 3 },

                // Skeletons at ruins (hard)
                new() { CenterX = -50, CenterZ = -55, Radius = 16, EnemyType = "skeleton", Count = 5, MinLevel = 4 },

                // Troll at cave (boss-like)
                new() { CenterX = 55, CenterZ = -45, Radius = 10, EnemyType = "troll", Count = 1, MinLevel = 5 },
            },

            // ════════════════════════════════════════════
            // TORCHES — village and path lighting
            // ════════════════════════════════════════════
            Torches = new List<TorchPlacement>
            {
                // Village torches
                new() { X = 5, Y = 7.5f, Z = 8 },
                new() { X = -5, Y = 7.5f, Z = 8 },
                new() { X = 5, Y = 7.5f, Z = 18 },
                new() { X = -5, Y = 7.5f, Z = 18 },
                new() { X = 15, Y = 7f, Z = 5 },
                new() { X = -10, Y = 7f, Z = 15 },
                // Crossroads
                new() { X = 2, Y = 8f, Z = -20 },
                new() { X = -2, Y = 8f, Z = -20 },
                // Ruins (eerie atmosphere)
                new() { X = -48, Y = 17f, Z = -52 },
                new() { X = -52, Y = 17f, Z = -58 },
            },
        };

        return level;
    }

    private static List<ObjectPlacement> CreateTrees()
    {
        var trees = new List<ObjectPlacement>();

        // === DARK FOREST (dense) ===
        AddCluster(trees, -20, -45, 35, 50, 0.8f, 1.4f);
        AddCluster(trees, 15, -55, 25, 35, 0.9f, 1.3f);
        AddCluster(trees, -40, -50, 15, 20, 0.7f, 1.2f);

        // === VILLAGE surroundings (scattered, ornamental) ===
        trees.Add(new() { X = -20, Z = 5, Scale = 1.2f });
        trees.Add(new() { X = -22, Z = 8, Scale = 1.0f });
        trees.Add(new() { X = 25, Z = 18, Scale = 1.1f });
        trees.Add(new() { X = 22, Z = 15, Scale = 0.9f });
        trees.Add(new() { X = -15, Z = 22, Scale = 1.3f });
        trees.Add(new() { X = 18, Z = -5, Scale = 1.0f });

        // === FARMLAND (scattered shade trees) ===
        trees.Add(new() { X = 45, Z = 10, Scale = 1.4f });
        trees.Add(new() { X = 55, Z = 0, Scale = 1.2f });
        trees.Add(new() { X = 60, Z = 15, Scale = 1.1f });
        trees.Add(new() { X = 50, Z = -5, Scale = 1.3f });

        // === RIVER BANK ===
        AddLine(trees, -30, 42, 30, 48, 10, 0.8f, 1.2f);

        // === SOUTHERN HILLS ===
        AddCluster(trees, -20, 80, 20, 15, 0.7f, 1.1f);
        AddCluster(trees, 30, 75, 15, 10, 0.8f, 1.0f);

        // === Lake shore ===
        AddLine(trees, -55, 45, -25, 50, 8, 0.7f, 1.0f);

        return trees;
    }

    private static List<ObjectPlacement> CreateRocks()
    {
        var rocks = new List<ObjectPlacement>();

        // === Mountain foothills ===
        AddCluster(rocks, -30, -75, 50, 12, 0.5f, 1.5f);
        AddCluster(rocks, 20, -70, 40, 10, 0.6f, 1.3f);

        // === Ruin area (rubble) ===
        AddCluster(rocks, -50, -55, 12, 15, 0.3f, 0.8f);

        // === Troll cave entrance ===
        rocks.Add(new() { X = 50, Z = -40, Scale = 2.0f, RotationY = 0.5f });
        rocks.Add(new() { X = 58, Z = -42, Scale = 1.8f, RotationY = 1.2f });
        rocks.Add(new() { X = 53, Z = -48, Scale = 1.5f, RotationY = 2.1f });
        rocks.Add(new() { X = 60, Z = -50, Scale = 1.2f });

        // === River boulders ===
        rocks.Add(new() { X = -10, Z = 45, Scale = 0.8f });
        rocks.Add(new() { X = 5, Z = 47, Scale = 0.6f });
        rocks.Add(new() { X = 15, Z = 44, Scale = 0.7f });

        // === Path-side accent rocks ===
        rocks.Add(new() { X = 3, Z = -15, Scale = 0.4f });
        rocks.Add(new() { X = -2, Z = -22, Scale = 0.5f });

        // === Southern hills ===
        AddCluster(rocks, 60, 85, 15, 8, 0.4f, 1.0f);

        return rocks;
    }

    private static List<ObjectPlacement> CreateBushes()
    {
        var bushes = new List<ObjectPlacement>();

        // === Forest undergrowth ===
        AddCluster(bushes, -20, -45, 30, 40, 0.4f, 0.8f, "scrub");
        AddCluster(bushes, 15, -50, 20, 25, 0.4f, 0.7f, "scrub");

        // === Village gardens ===
        bushes.Add(new() { X = 10, Z = 10, Scale = 0.6f, Variant = "berry" });
        bushes.Add(new() { X = -8, Z = 18, Scale = 0.7f, Variant = "berry" });
        bushes.Add(new() { X = 14, Z = 8, Scale = 0.5f, Variant = "berry" });

        // === Farmland grass tufts ===
        AddCluster(bushes, 50, 5, 30, 30, 0.3f, 0.6f, "grass");

        // === Path borders ===
        AddLine(bushes, 2, 0, 2, 25, 8, 0.3f, 0.5f, "grass");
        AddLine(bushes, -2, 0, -2, 25, 8, 0.3f, 0.5f, "grass");

        // === Lake reeds ===
        AddCluster(bushes, -40, 50, 18, 12, 0.4f, 0.7f, "grass");

        // === Southern hills grass ===
        AddCluster(bushes, 0, 80, 40, 25, 0.3f, 0.6f, "grass");

        return bushes;
    }

    private static List<RuinPlacement> CreateRuins()
    {
        return new List<RuinPlacement>
        {
            // === SKELETON RUINS — ancient temple complex ===
            // Entrance arch
            new() { X = -45, Z = -48, Type = "arch", Scale = 1.2f, RotationY = 0.3f },
            // Pillars forming colonnade
            new() { X = -48, Z = -52, Type = "pillar", Scale = 1.0f },
            new() { X = -52, Z = -52, Type = "pillar", Scale = 1.0f },
            new() { X = -48, Z = -58, Type = "pillar", Scale = 1.0f },
            new() { X = -52, Z = -58, Type = "broken_pillar", Scale = 1.0f },
            // Inner walls
            new() { X = -50, Z = -55, Type = "wall", Scale = 1.1f, RotationY = 0f },
            new() { X = -54, Z = -55, Type = "wall", Scale = 0.9f, RotationY = 1.57f },
            // Scattered broken pieces
            new() { X = -46, Z = -56, Type = "broken_pillar", Scale = 0.7f, RotationY = 0.8f },
            new() { X = -55, Z = -50, Type = "broken_pillar", Scale = 0.6f, RotationY = 2.1f },

            // === ROADSIDE RUINS (crossroads area) ===
            new() { X = 5, Z = -18, Type = "broken_pillar", Scale = 0.8f },
            new() { X = -4, Z = -22, Type = "wall", Scale = 0.7f, RotationY = 0.5f },

            // === FOREST RUINS (scattered) ===
            new() { X = -25, Z = -42, Type = "arch", Scale = 0.9f, RotationY = 1.2f },
            new() { X = 10, Z = -48, Type = "pillar", Scale = 0.8f },
            new() { X = 18, Z = -52, Type = "broken_pillar", Scale = 0.7f },

            // === BANDIT CAMP ruins ===
            new() { X = 22, Z = 58, Type = "wall", Scale = 0.8f, RotationY = 0.3f },
            new() { X = 28, Z = 62, Type = "broken_pillar", Scale = 0.6f },
        };
    }

    // ── Placement helpers ──

    private static void AddCluster(List<ObjectPlacement> list, float cx, float cz, float radius,
        int count, float minScale, float maxScale, string variant = "default")
    {
        // Deterministic "random" placement using golden angle spiral
        float goldenAngle = MathF.PI * (3f - MathF.Sqrt(5f));
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float r = radius * MathF.Sqrt(t);
            float angle = i * goldenAngle;
            float x = cx + r * MathF.Cos(angle);
            float z = cz + r * MathF.Sin(angle);
            float scale = minScale + t * (maxScale - minScale);
            float rot = angle * 2.3f; // pseudo-random rotation
            list.Add(new ObjectPlacement { X = x, Z = z, Scale = scale, RotationY = rot, Variant = variant });
        }
    }

    private static void AddLine(List<ObjectPlacement> list, float x1, float z1, float x2, float z2,
        int count, float minScale, float maxScale, string variant = "default")
    {
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / (count - 1);
            float x = x1 + (x2 - x1) * t + MathF.Sin(i * 2.7f) * 2f; // slight offset
            float z = z1 + (z2 - z1) * t + MathF.Cos(i * 3.1f) * 2f;
            float scale = minScale + MathF.Abs(MathF.Sin(i * 1.3f)) * (maxScale - minScale);
            list.Add(new ObjectPlacement { X = x, Z = z, Scale = scale, RotationY = i * 1.7f, Variant = variant });
        }
    }
}
