using Silk.NET.OpenGL;
using CongCraft.Engine.Rendering;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates a deformed icosphere to create natural-looking rocks.
/// </summary>
public static class RockMeshBuilder
{
    public record RockParams(
        float BaseRadius = 0.8f,
        float Deformation = 0.3f,
        int Subdivisions = 3,
        int Seed = 42
    );

    public static MeshData GenerateData(RockParams? p = null)
    {
        p ??= new RockParams();
        var verts = new List<float>();
        var inds = new List<uint>();

        // Start with icosahedron vertices
        float t = (1f + MathF.Sqrt(5f)) / 2f;
        var positions = new List<float[]>
        {
            V(-1, t, 0), V(1, t, 0), V(-1, -t, 0), V(1, -t, 0),
            V(0, -1, t), V(0, 1, t), V(0, -1, -t), V(0, 1, -t),
            V(t, 0, -1), V(t, 0, 1), V(-t, 0, -1), V(-t, 0, 1),
        };

        var triangles = new List<int[]>
        {
            new[]{0,11,5}, new[]{0,5,1}, new[]{0,1,7}, new[]{0,7,10}, new[]{0,10,11},
            new[]{1,5,9}, new[]{5,11,4}, new[]{11,10,2}, new[]{10,7,6}, new[]{7,1,8},
            new[]{3,9,4}, new[]{3,4,2}, new[]{3,2,6}, new[]{3,6,8}, new[]{3,8,9},
            new[]{4,9,5}, new[]{2,4,11}, new[]{6,2,10}, new[]{8,6,7}, new[]{9,8,1},
        };

        // Subdivide
        var midpointCache = new Dictionary<long, int>();
        for (int s = 0; s < p.Subdivisions; s++)
        {
            var newTriangles = new List<int[]>();
            foreach (var tri in triangles)
            {
                int a = GetMidpoint(positions, midpointCache, tri[0], tri[1]);
                int b = GetMidpoint(positions, midpointCache, tri[1], tri[2]);
                int c = GetMidpoint(positions, midpointCache, tri[2], tri[0]);
                newTriangles.Add(new[] { tri[0], a, c });
                newTriangles.Add(new[] { tri[1], b, a });
                newTriangles.Add(new[] { tri[2], c, b });
                newTriangles.Add(new[] { a, b, c });
            }
            triangles = newTriangles;
            midpointCache.Clear();
        }

        // Deform and build vertex data
        var rng = new Random(p.Seed);
        foreach (var pos in positions)
        {
            float len = MathF.Sqrt(pos[0] * pos[0] + pos[1] * pos[1] + pos[2] * pos[2]);
            float nx = pos[0] / len, ny = pos[1] / len, nz = pos[2] / len;
            float deform = p.BaseRadius + (float)(rng.NextDouble() * 2 - 1) * p.Deformation;

            float px = nx * deform;
            float py = ny * deform * 0.6f; // flatten vertically
            float pz = nz * deform;

            // Natural stone coloring with weathering, strata, and moss
            float colorVar = (float)rng.NextDouble() * 0.12f;
            float strataVar = MathF.Sin(py * 8f) * 0.04f; // horizontal strata lines
            float r, g, b;
            // Moss/lichen on upward-facing surfaces
            if (ny > 0.25f && rng.NextDouble() > 0.35f)
            {
                // Rich moss/lichen patches with variation
                float mossVar = (float)rng.NextDouble() * 0.06f;
                r = 0.12f + colorVar * 0.4f + mossVar;
                g = 0.22f + colorVar * 1.2f + mossVar * 0.5f;
                b = 0.08f + colorVar * 0.2f;
            }
            else if (ny < -0.2f)
            {
                // Darker underside with earthy tones
                r = 0.30f + colorVar * 0.8f;
                g = 0.28f + colorVar * 0.7f;
                b = 0.24f + colorVar * 0.6f;
            }
            else
            {
                // Main stone with warm-cool variation and strata
                r = 0.38f + colorVar + strataVar;
                g = 0.36f + colorVar * 0.9f + strataVar * 0.8f;
                b = 0.33f + colorVar * 0.8f + strataVar * 0.6f;
            }

            verts.AddRange(new[] { px, py, pz, nx, ny, nz, r, g, b });
        }

        foreach (var tri in triangles)
            inds.AddRange(new[] { (uint)tri[0], (uint)tri[1], (uint)tri[2] });

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    public static Mesh Create(GL gl, RockParams? p = null)
    {
        var data = GenerateData(p);
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
    }

    private static float[] V(float x, float y, float z)
    {
        float len = MathF.Sqrt(x * x + y * y + z * z);
        return new[] { x / len, y / len, z / len };
    }

    private static int GetMidpoint(List<float[]> positions, Dictionary<long, int> cache, int i1, int i2)
    {
        long key = (long)Math.Min(i1, i2) << 32 | (long)Math.Max(i1, i2);
        if (cache.TryGetValue(key, out int idx))
            return idx;

        var p1 = positions[i1];
        var p2 = positions[i2];
        float mx = (p1[0] + p2[0]) / 2f;
        float my = (p1[1] + p2[1]) / 2f;
        float mz = (p1[2] + p2[2]) / 2f;

        // Normalize to unit sphere
        float len = MathF.Sqrt(mx * mx + my * my + mz * mz);
        positions.Add(new[] { mx / len, my / len, mz / len });
        idx = positions.Count - 1;
        cache[key] = idx;
        return idx;
    }
}
