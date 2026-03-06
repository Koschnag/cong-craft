using System.Numerics;
using CongCraft.Engine.Terrain;

namespace CongCraft.Engine.Level;

/// <summary>
/// Generates terrain heightmap data from fixed level design data.
/// Replaces noise-based TerrainGenerator with sculpted terrain from HeightFeatures.
/// Uses the same HeightmapData output format for compatibility with TerrainSystem.
///
/// Also adds subtle noise detail on top of the designed heightmap so the terrain
/// doesn't look unnaturally smooth — like Gothic 2 / SpellForce terrain.
/// </summary>
public sealed class LevelTerrainGenerator
{
    private readonly LevelData _level;
    private readonly FastNoiseLite _detailNoise;
    private readonly FastNoiseLite _microNoise;
    private readonly float _chunkSize;

    public LevelData Level => _level;

    public LevelTerrainGenerator(LevelData level, float chunkSize = 64f)
    {
        _level = level;
        _chunkSize = chunkSize;

        // Subtle detail noise — adds fine bumps without changing macro shape
        _detailNoise = new FastNoiseLite(54321);
        _detailNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _detailNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _detailNoise.SetFractalOctaves(3);
        _detailNoise.SetFrequency(0.015f);

        // Micro noise — grass-level variation
        _microNoise = new FastNoiseLite(98765);
        _microNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _microNoise.SetFrequency(0.08f);
    }

    /// <summary>
    /// Compute the designed height at any world position.
    /// Used by character movement, object placement, etc.
    /// </summary>
    public float GetHeightAt(float worldX, float worldZ)
    {
        float height = ComputeDesignedHeight(worldX, worldZ);

        // Apply path flattening
        height = ApplyPathFlattening(worldX, worldZ, height);

        // Add subtle detail noise
        height += _detailNoise.GetNoise(worldX, worldZ) * 0.5f;
        height += _microNoise.GetNoise(worldX, worldZ) * 0.15f;

        return height;
    }

    /// <summary>
    /// Generate a terrain chunk compatible with TerrainSystem.
    /// </summary>
    public HeightmapData GenerateChunk(int chunkX, int chunkZ, int resolution = 64)
    {
        int vertCount = (resolution + 1) * (resolution + 1);
        var heights = new float[vertCount];
        var normals = new Vector3[vertCount];
        var positions = new Vector3[vertCount];

        float step = _chunkSize / resolution;
        float originX = chunkX * _chunkSize;
        float originZ = chunkZ * _chunkSize;

        // Generate heights
        for (int z = 0; z <= resolution; z++)
        for (int x = 0; x <= resolution; x++)
        {
            int idx = z * (resolution + 1) + x;
            float worldX = originX + x * step;
            float worldZ = originZ + z * step;
            float height = GetHeightAt(worldX, worldZ);

            heights[idx] = height;
            positions[idx] = new Vector3(worldX, height, worldZ);
        }

        // Compute normals from adjacent heights
        for (int z = 0; z <= resolution; z++)
        for (int x = 0; x <= resolution; x++)
        {
            int idx = z * (resolution + 1) + x;
            float hL = GetH(heights, resolution, x - 1, z);
            float hR = GetH(heights, resolution, x + 1, z);
            float hD = GetH(heights, resolution, x, z - 1);
            float hU = GetH(heights, resolution, x, z + 1);

            normals[idx] = Vector3.Normalize(new Vector3(hL - hR, 2f * step, hD - hU));
        }

        return new HeightmapData(heights, normals, positions, resolution, chunkX, chunkZ, _chunkSize);
    }

    /// <summary>
    /// Get the path influence at a world position (0 = off path, 1 = on path center).
    /// Used by terrain shader for path texture blending.
    /// </summary>
    public float GetPathInfluence(float worldX, float worldZ)
    {
        float maxInfluence = 0f;
        foreach (var path in _level.Paths)
        {
            float dist = DistanceToSegment(worldX, worldZ, path.StartX, path.StartZ, path.EndX, path.EndZ);
            float influence = 1f - MathF.Min(1f, dist / path.Width);
            influence = MathF.Max(0f, influence);
            maxInfluence = MathF.Max(maxInfluence, influence);
        }
        return maxInfluence;
    }

    /// <summary>
    /// Get the dominant terrain zone type at a world position.
    /// Returns (type, strength) for texture blending.
    /// </summary>
    public (string type, float strength) GetDominantZone(float worldX, float worldZ)
    {
        string bestType = "grass";
        float bestStrength = 0f;

        foreach (var zone in _level.Zones)
        {
            float dx = worldX - zone.CenterX;
            float dz = worldZ - zone.CenterZ;
            float dist = MathF.Sqrt(dx * dx + dz * dz);
            float influence = MathF.Max(0f, 1f - dist / zone.Radius) * zone.Strength;

            if (influence > bestStrength)
            {
                bestStrength = influence;
                bestType = zone.Type;
            }
        }

        return (bestType, bestStrength);
    }

    // ── Private implementation ──

    private float ComputeDesignedHeight(float worldX, float worldZ)
    {
        // Start with a base gentle rolling terrain
        float height = 3f;

        foreach (var feature in _level.HeightFeatures)
        {
            float dx = (worldX - feature.CenterX) / feature.WidthX;
            float dz = (worldZ - feature.CenterZ) / feature.WidthZ;

            // Apply rotation for ridges/cliffs
            if (feature.Rotation != 0)
            {
                float cos = MathF.Cos(feature.Rotation);
                float sin = MathF.Sin(feature.Rotation);
                float rx = dx * cos - dz * sin;
                float rz = dx * sin + dz * cos;
                dx = rx;
                dz = rz;
            }

            float dist = MathF.Sqrt(dx * dx + dz * dz);
            float t = dist / feature.Radius;

            if (t >= 1f) continue; // Outside feature range

            switch (feature.Type)
            {
                case HeightFeature.FeatureType.Hill:
                {
                    // Smooth bell curve
                    float influence = MathF.Pow(1f - t, feature.Falloff);
                    height += feature.Height * influence;
                    break;
                }
                case HeightFeature.FeatureType.Valley:
                {
                    // Inverted bell curve
                    float influence = MathF.Pow(1f - t, feature.Falloff);
                    height += feature.Height * influence; // Height is negative for valleys
                    break;
                }
                case HeightFeature.FeatureType.Plateau:
                {
                    // Flat top with smooth edges
                    float edge = MathF.Min(1f, t * feature.Falloff);
                    float influence = 1f - SmoothStep(0.6f, 1f, edge);
                    height = MathF.Max(height, feature.Height * influence + 3f * (1f - influence));
                    break;
                }
                case HeightFeature.FeatureType.Ridge:
                {
                    // Elongated mountain
                    float influence = MathF.Pow(1f - t, feature.Falloff);
                    height += feature.Height * influence;
                    break;
                }
                case HeightFeature.FeatureType.Cliff:
                {
                    // Sharp falloff on one side
                    float influence = MathF.Pow(1f - t, feature.Falloff);
                    // Steeper on the positive Z side
                    float asymmetry = dz > 0 ? 0.5f : 1f;
                    height += feature.Height * influence * asymmetry;
                    break;
                }
                case HeightFeature.FeatureType.Bowl:
                {
                    // Depression (like a crater or cave entrance)
                    float influence = MathF.Pow(1f - t, feature.Falloff);
                    height += feature.Height * influence; // Height is negative
                    break;
                }
            }
        }

        return height;
    }

    private float ApplyPathFlattening(float worldX, float worldZ, float height)
    {
        float pathInfluence = GetPathInfluence(worldX, worldZ);
        if (pathInfluence <= 0) return height;

        // Flatten towards the average height along the path
        // This creates natural-looking roads that follow contours
        float flatHeight = height;

        foreach (var path in _level.Paths)
        {
            float dist = DistanceToSegment(worldX, worldZ, path.StartX, path.StartZ, path.EndX, path.EndZ);
            if (dist > path.Width) continue;

            float t = 1f - dist / path.Width;
            t = SmoothStep(0f, 1f, t);

            // Get the height at the nearest point on the path centerline
            float closestT = ClosestTOnSegment(worldX, worldZ, path.StartX, path.StartZ, path.EndX, path.EndZ);
            float px = path.StartX + (path.EndX - path.StartX) * closestT;
            float pz = path.StartZ + (path.EndZ - path.StartZ) * closestT;
            float pathCenterHeight = ComputeDesignedHeight(px, pz);

            // Blend towards path height (slight depression for worn path feel)
            flatHeight = MathF.Max(flatHeight, height * (1f - t * 0.3f) + (pathCenterHeight - 0.15f) * t * 0.3f);
            flatHeight = height * (1f - t * 0.4f) + pathCenterHeight * t * 0.4f - t * 0.1f;
        }

        return flatHeight;
    }

    private static float DistanceToSegment(float px, float pz, float ax, float az, float bx, float bz)
    {
        float t = ClosestTOnSegment(px, pz, ax, az, bx, bz);
        float closestX = ax + (bx - ax) * t;
        float closestZ = az + (bz - az) * t;
        float dx = px - closestX;
        float dz = pz - closestZ;
        return MathF.Sqrt(dx * dx + dz * dz);
    }

    private static float ClosestTOnSegment(float px, float pz, float ax, float az, float bx, float bz)
    {
        float abx = bx - ax, abz = bz - az;
        float apx = px - ax, apz = pz - az;
        float dot = apx * abx + apz * abz;
        float lenSq = abx * abx + abz * abz;
        if (lenSq < 0.0001f) return 0f;
        return MathF.Min(1f, MathF.Max(0f, dot / lenSq));
    }

    private static float SmoothStep(float edge0, float edge1, float x)
    {
        float t = MathF.Min(1f, MathF.Max(0f, (x - edge0) / (edge1 - edge0)));
        return t * t * (3f - 2f * t);
    }

    private static float GetH(float[] heights, int resolution, int x, int z)
    {
        x = Math.Clamp(x, 0, resolution);
        z = Math.Clamp(z, 0, resolution);
        return heights[z * (resolution + 1) + x];
    }
}
