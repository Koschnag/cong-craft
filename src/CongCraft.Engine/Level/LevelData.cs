using System.Numerics;

namespace CongCraft.Engine.Level;

/// <summary>
/// Defines a fixed game level with designed terrain, zones, and object placements.
/// Inspired by SpellForce 1 maps / Gothic 2 Khorinis / Two Worlds 1 open areas.
/// No procedural randomness — every hill, path, and tree is intentionally placed.
/// </summary>
public sealed class LevelData
{
    public string Name { get; init; } = "";
    public float WorldSize { get; init; } = 256f;
    public float WaterLevel { get; init; } = 1.5f;

    /// <summary>Zone definitions that control terrain material blending.</summary>
    public List<TerrainZone> Zones { get; init; } = new();

    /// <summary>Fixed heightmap control points — terrain is sculpted from these.</summary>
    public List<HeightFeature> HeightFeatures { get; init; } = new();

    /// <summary>Paths/roads carved into the terrain.</summary>
    public List<PathSegment> Paths { get; init; } = new();

    /// <summary>Fixed tree placements.</summary>
    public List<ObjectPlacement> Trees { get; init; } = new();

    /// <summary>Fixed rock placements.</summary>
    public List<ObjectPlacement> Rocks { get; init; } = new();

    /// <summary>Fixed bush/vegetation placements.</summary>
    public List<ObjectPlacement> Bushes { get; init; } = new();

    /// <summary>Fixed ruin placements.</summary>
    public List<RuinPlacement> Ruins { get; init; } = new();

    /// <summary>NPC spawn points with type and name.</summary>
    public List<NpcPlacement> Npcs { get; init; } = new();

    /// <summary>Enemy spawn zones with type and count.</summary>
    public List<EnemyZone> EnemyZones { get; init; } = new();

    /// <summary>Point light / torch positions.</summary>
    public List<TorchPlacement> Torches { get; init; } = new();

    /// <summary>Player spawn position.</summary>
    public Vector3 PlayerSpawn { get; init; } = new(0, 0, 0);
}

/// <summary>
/// Terrain zone: a circular region that influences texture blending and height.
/// </summary>
public sealed class TerrainZone
{
    public string Type { get; init; } = "grass"; // grass, forest, stone, snow, path, village, swamp
    public float CenterX { get; init; }
    public float CenterZ { get; init; }
    public float Radius { get; init; }
    public float Strength { get; init; } = 1f;
}

/// <summary>
/// Height feature: shapes the terrain — hills, valleys, plateaus, cliffs.
/// </summary>
public sealed class HeightFeature
{
    public enum FeatureType { Hill, Valley, Plateau, Ridge, Cliff, Bowl }
    public FeatureType Type { get; init; }
    public float CenterX { get; init; }
    public float CenterZ { get; init; }
    public float Radius { get; init; }
    public float Height { get; init; }
    public float Falloff { get; init; } = 2f; // Power of falloff curve
    public float Rotation { get; init; } // For ridges/cliffs
    public float WidthX { get; init; } = 1f; // Elliptical scaling
    public float WidthZ { get; init; } = 1f;
}

/// <summary>
/// A road/path segment between two points, flattened and textured.
/// </summary>
public sealed class PathSegment
{
    public float StartX { get; init; }
    public float StartZ { get; init; }
    public float EndX { get; init; }
    public float EndZ { get; init; }
    public float Width { get; init; } = 3f;
}

/// <summary>Generic object placement (tree, rock, bush).</summary>
public sealed class ObjectPlacement
{
    public float X { get; init; }
    public float Z { get; init; }
    public float Scale { get; init; } = 1f;
    public float RotationY { get; init; }
    public string Variant { get; init; } = "default";
}

/// <summary>Ruin placement with type.</summary>
public sealed class RuinPlacement
{
    public float X { get; init; }
    public float Z { get; init; }
    public float Scale { get; init; } = 1f;
    public float RotationY { get; init; }
    public string Type { get; init; } = "pillar"; // pillar, broken_pillar, wall, arch
}

/// <summary>NPC placement with identity.</summary>
public sealed class NpcPlacement
{
    public float X { get; init; }
    public float Z { get; init; }
    public string NpcType { get; init; } = "blacksmith";
    public string Name { get; init; } = "NPC";
    public float FacingAngle { get; init; }
}

/// <summary>Enemy spawn zone — enemies patrol within this area.</summary>
public sealed class EnemyZone
{
    public float CenterX { get; init; }
    public float CenterZ { get; init; }
    public float Radius { get; init; }
    public string EnemyType { get; init; } = "bandit"; // bandit, wolf, skeleton, troll
    public int Count { get; init; } = 3;
    public int MinLevel { get; init; } = 1;
}

/// <summary>Torch/light placement.</summary>
public sealed class TorchPlacement
{
    public float X { get; init; }
    public float Y { get; init; }
    public float Z { get; init; }
}
