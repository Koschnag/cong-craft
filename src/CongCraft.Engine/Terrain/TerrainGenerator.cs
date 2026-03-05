using System.Numerics;

namespace CongCraft.Engine.Terrain;

/// <summary>
/// Generates terrain heightmap data from noise functions.
/// Pure data generation — no GL dependencies.
/// </summary>
public sealed class TerrainGenerator
{
    private readonly FastNoiseLite _noise;
    private readonly float _amplitude;
    private readonly float _chunkSize;

    public TerrainGenerator(int seed = 12345, float amplitude = 20f, float chunkSize = 64f)
    {
        _amplitude = amplitude;
        _chunkSize = chunkSize;
        _noise = new FastNoiseLite(seed);
        _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _noise.SetFractalOctaves(5);
        _noise.SetFrequency(0.003f);
    }

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
                float height = _noise.GetNoise(worldX, worldZ) * _amplitude;

                heights[idx] = height;
                positions[idx] = new Vector3(worldX, height, worldZ);
            }

        // Compute normals from adjacent heights
        for (int z = 0; z <= resolution; z++)
            for (int x = 0; x <= resolution; x++)
            {
                int idx = z * (resolution + 1) + x;
                float hL = GetHeight(heights, resolution, x - 1, z);
                float hR = GetHeight(heights, resolution, x + 1, z);
                float hD = GetHeight(heights, resolution, x, z - 1);
                float hU = GetHeight(heights, resolution, x, z + 1);

                normals[idx] = Vector3.Normalize(new Vector3(hL - hR, 2f * step, hD - hU));
            }

        return new HeightmapData(heights, normals, positions, resolution, chunkX, chunkZ, _chunkSize);
    }

    public float GetHeightAt(float worldX, float worldZ)
    {
        return _noise.GetNoise(worldX, worldZ) * _amplitude;
    }

    private static float GetHeight(float[] heights, int resolution, int x, int z)
    {
        x = Math.Clamp(x, 0, resolution);
        z = Math.Clamp(z, 0, resolution);
        return heights[z * (resolution + 1) + x];
    }
}

public sealed class HeightmapData
{
    public float[] Heights { get; }
    public Vector3[] Normals { get; }
    public Vector3[] Positions { get; }
    public int Resolution { get; }
    public int ChunkX { get; }
    public int ChunkZ { get; }
    public float ChunkSize { get; }

    public HeightmapData(float[] heights, Vector3[] normals, Vector3[] positions,
        int resolution, int chunkX, int chunkZ, float chunkSize)
    {
        Heights = heights;
        Normals = normals;
        Positions = positions;
        Resolution = resolution;
        ChunkX = chunkX;
        ChunkZ = chunkZ;
        ChunkSize = chunkSize;
    }

    /// <summary>
    /// Build vertex array with position + normal + texcoord (8 floats per vertex).
    /// </summary>
    public float[] BuildVertices()
    {
        int vertCount = (Resolution + 1) * (Resolution + 1);
        var verts = new float[vertCount * 8];

        for (int z = 0; z <= Resolution; z++)
            for (int x = 0; x <= Resolution; x++)
            {
                int idx = z * (Resolution + 1) + x;
                int vi = idx * 8;
                var pos = Positions[idx];
                var norm = Normals[idx];

                verts[vi + 0] = pos.X;
                verts[vi + 1] = pos.Y;
                verts[vi + 2] = pos.Z;
                verts[vi + 3] = norm.X;
                verts[vi + 4] = norm.Y;
                verts[vi + 5] = norm.Z;
                verts[vi + 6] = (float)x / Resolution;
                verts[vi + 7] = (float)z / Resolution;
            }

        return verts;
    }

    /// <summary>
    /// Build index array for the terrain mesh.
    /// </summary>
    public uint[] BuildIndices()
    {
        var indices = new uint[Resolution * Resolution * 6];
        int idx = 0;

        for (int z = 0; z < Resolution; z++)
            for (int x = 0; x < Resolution; x++)
            {
                uint tl = (uint)(z * (Resolution + 1) + x);
                uint tr = tl + 1;
                uint bl = tl + (uint)(Resolution + 1);
                uint br = bl + 1;

                indices[idx++] = tl;
                indices[idx++] = bl;
                indices[idx++] = br;
                indices[idx++] = tl;
                indices[idx++] = br;
                indices[idx++] = tr;
            }

        return indices;
    }
}
