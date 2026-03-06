using Silk.NET.OpenGL;
using CongCraft.Engine.Rendering;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates bush/shrub meshes for ground cover vegetation.
/// Two types: low scrub bushes and flowering berry bushes.
/// Adds ~2010 RPG ground detail (Gothic 3, Two Worlds 1 style).
/// </summary>
public static class BushMeshBuilder
{
    /// <summary>Low green scrub bush - ground cover.</summary>
    public static MeshData GenerateScrub(int seed = 0)
    {
        var verts = new List<float>();
        var inds = new List<uint>();
        var rng = new Random(seed);

        // 3-5 overlapping green spheres for organic scrub shape
        int count = 3 + rng.Next(3);
        for (int i = 0; i < count; i++)
        {
            float ox = (float)(rng.NextDouble() * 0.5 - 0.25);
            float oz = (float)(rng.NextDouble() * 0.5 - 0.25);
            float radius = 0.25f + (float)rng.NextDouble() * 0.2f;
            float oy = radius * 0.5f;

            // Varied green tones
            float gr = 0.06f + (float)rng.NextDouble() * 0.06f;
            float gg = 0.20f + (float)rng.NextDouble() * 0.12f;
            float gb = 0.03f + (float)rng.NextDouble() * 0.04f;

            AddSphere(verts, inds, ox, oy, oz, radius, 8, gr, gg, gb);
        }

        // Small stem/trunk at base
        AddCylinder(verts, inds, 0, 0, 0, 0.04f, 0.15f, 6, 0.30f, 0.20f, 0.10f);

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    /// <summary>Berry bush - bush with small colored berries.</summary>
    public static MeshData GenerateBerry(int seed = 0)
    {
        var verts = new List<float>();
        var inds = new List<uint>();
        var rng = new Random(seed);

        // Main bush body (darker green)
        AddSphere(verts, inds, 0, 0.28f, 0, 0.32f, 10, 0.08f, 0.22f, 0.05f);
        AddSphere(verts, inds, 0.12f, 0.22f, 0.10f, 0.24f, 8, 0.07f, 0.20f, 0.04f);
        AddSphere(verts, inds, -0.10f, 0.25f, -0.08f, 0.22f, 8, 0.09f, 0.24f, 0.06f);

        // Berry clusters (small bright spheres)
        for (int i = 0; i < 6; i++)
        {
            float bx = (float)(rng.NextDouble() * 0.4 - 0.2);
            float bz = (float)(rng.NextDouble() * 0.4 - 0.2);
            float by = 0.20f + (float)rng.NextDouble() * 0.20f;

            // Red/purple berries
            float br = 0.55f + (float)rng.NextDouble() * 0.30f;
            float bg = 0.08f + (float)rng.NextDouble() * 0.10f;
            float bb = 0.10f + (float)rng.NextDouble() * 0.15f;

            AddSphere(verts, inds, bx, by, bz, 0.025f, 6, br, bg, bb);
        }

        // Woody stem
        AddCylinder(verts, inds, 0, 0, 0, 0.035f, 0.18f, 6, 0.32f, 0.22f, 0.12f);

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    /// <summary>Tall grass tuft - clump of grass blades.</summary>
    public static MeshData GenerateGrassTuft(int seed = 0)
    {
        var verts = new List<float>();
        var inds = new List<uint>();
        var rng = new Random(seed);

        // Multiple thin tall cylinders for grass blade appearance
        int bladeCount = 5 + rng.Next(4);
        for (int i = 0; i < bladeCount; i++)
        {
            float ox = (float)(rng.NextDouble() * 0.2 - 0.1);
            float oz = (float)(rng.NextDouble() * 0.2 - 0.1);
            float height = 0.3f + (float)rng.NextDouble() * 0.25f;
            float radius = 0.012f + (float)rng.NextDouble() * 0.008f;

            float gr = 0.10f + (float)rng.NextDouble() * 0.08f;
            float gg = 0.28f + (float)rng.NextDouble() * 0.15f;
            float gb = 0.04f + (float)rng.NextDouble() * 0.04f;

            AddCylinder(verts, inds, ox, 0, oz, radius, height, 4, gr, gg, gb);
        }

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    // ─── Primitive helpers ───

    private static void AddSphere(List<float> verts, List<uint> inds,
        float cx, float cy, float cz, float radius, int segments,
        float r, float g, float b)
    {
        uint baseIdx = (uint)(verts.Count / 9);
        int stacks = segments / 2;
        for (int stack = 0; stack <= stacks; stack++)
        {
            float phi = MathF.PI * stack / stacks;
            float sinPhi = MathF.Sin(phi), cosPhi = MathF.Cos(phi);
            for (int seg = 0; seg <= segments; seg++)
            {
                float theta = MathF.Tau * seg / segments;
                float cosT = MathF.Cos(theta), sinT = MathF.Sin(theta);
                float nx = sinPhi * cosT, ny = cosPhi, nz = sinPhi * sinT;
                verts.AddRange(new[] { cx + radius * nx, cy + radius * ny, cz + radius * nz, nx, ny, nz, r, g, b });
            }
        }
        int vertsPerRow = segments + 1;
        for (int stack = 0; stack < stacks; stack++)
        for (int seg = 0; seg < segments; seg++)
        {
            uint tl = baseIdx + (uint)(stack * vertsPerRow + seg);
            uint tr = tl + 1;
            uint bl = (uint)(tl + vertsPerRow);
            uint br = bl + 1;
            inds.AddRange(new[] { tl, bl, br, tl, br, tr });
        }
    }

    private static void AddCylinder(List<float> verts, List<uint> inds,
        float cx, float cy, float cz, float radius, float height, int segments,
        float r, float g, float b)
    {
        uint baseIdx = (uint)(verts.Count / 9);
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * MathF.Tau / segments;
            float cos = MathF.Cos(angle), sin = MathF.Sin(angle);
            verts.AddRange(new[] { cx + cos * radius, cy, cz + sin * radius, cos, 0f, sin, r, g, b });
            verts.AddRange(new[] { cx + cos * radius, cy + height, cz + sin * radius, cos, 0f, sin, r, g, b });
        }
        for (uint i = 0; i < (uint)segments; i++)
        {
            uint a = baseIdx + i * 2, c = baseIdx + (i + 1) * 2;
            inds.AddRange(new[] { a, c, c + 1, a, c + 1, a + 1 });
        }
    }
}
