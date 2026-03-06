using Silk.NET.OpenGL;
using CongCraft.Engine.Rendering;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates procedural ruin meshes: broken stone pillars, crumbling wall segments, arch fragments.
/// Inspired by Gothic 2/3 and SpellForce 1 world aesthetics — dark weathered stone with moss highlights.
/// </summary>
public static class RuinMeshBuilder
{
    public enum RuinType { Pillar, BrokenPillar, WallSegment, ArchFragment }

    public static Mesh Create(GL gl, RuinType type = RuinType.Pillar, int seed = 42)
    {
        var data = GenerateData(type, seed);
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
    }

    public static (float[] Vertices, uint[] Indices) GenerateData(RuinType type = RuinType.Pillar, int seed = 42)
    {
        var verts = new List<float>();
        var inds = new List<uint>();
        var rng = new Random(seed);

        switch (type)
        {
            case RuinType.Pillar:
                BuildPillar(verts, inds, rng, broken: false);
                break;
            case RuinType.BrokenPillar:
                BuildPillar(verts, inds, rng, broken: true);
                break;
            case RuinType.WallSegment:
                BuildWallSegment(verts, inds, rng);
                break;
            case RuinType.ArchFragment:
                BuildArchFragment(verts, inds, rng);
                break;
        }

        return (verts.ToArray(), inds.ToArray());
    }

    // Full or broken stone column
    private static void BuildPillar(List<float> verts, List<uint> inds, Random rng, bool broken)
    {
        const int segs = 8;
        float baseR = 0.30f, capR = 0.26f;
        float fullH = broken ? 1.8f + (float)rng.NextDouble() * 1.2f : 4.0f;

        // Stone base (slightly wider cylinder)
        AddCylinder(verts, inds, 0, 0, 0,
            baseR + 0.06f, 0.25f, segs,
            StoneR(rng), StoneG(rng), StoneB(rng));

        // Column shaft
        AddCylinder(verts, inds, 0, 0.25f, 0,
            baseR, fullH, segs,
            StoneR(rng), StoneG(rng), StoneB(rng));

        if (!broken)
        {
            // Abacus cap (wide flat top slab)
            AddBox(verts, inds, 0, 0.25f + fullH, 0,
                0.80f, 0.18f, 0.80f,
                StoneR(rng), StoneG(rng), StoneB(rng));

            // Echinus (slightly rounded upper shaft)
            AddCylinder(verts, inds, 0, 0.25f + fullH - 0.12f, 0,
                capR + 0.04f, 0.18f, segs,
                StoneR(rng), StoneG(rng), StoneB(rng));
        }
        else
        {
            // Rough broken top — debris block, offset
            float debrisX = (float)(rng.NextDouble() - 0.5) * 0.3f;
            float debrisZ = (float)(rng.NextDouble() - 0.5) * 0.3f;
            AddBox(verts, inds, debrisX, 0.25f + fullH, debrisZ,
                0.35f + (float)rng.NextDouble() * 0.2f,
                0.20f + (float)rng.NextDouble() * 0.15f,
                0.35f + (float)rng.NextDouble() * 0.2f,
                StoneR(rng), StoneG(rng), StoneB(rng));

            // Second smaller debris chunk
            AddBox(verts, inds,
                -debrisX * 0.6f, 0.25f + fullH - 0.12f, debrisZ * 1.2f,
                0.18f, 0.12f, 0.20f,
                StoneR(rng) * 0.85f, StoneG(rng) * 0.85f, StoneB(rng) * 0.85f);
        }

        // Moss patches — slightly greenish flat slabs along shaft base
        for (int i = 0; i < 3; i++)
        {
            float angle = (float)(rng.NextDouble() * MathF.Tau);
            float mosX = MathF.Cos(angle) * baseR * 0.85f;
            float mosZ = MathF.Sin(angle) * baseR * 0.85f;
            float mosH = 0.25f + (float)(rng.NextDouble() * 0.5f);
            AddBox(verts, inds, mosX, mosH, mosZ,
                0.10f, 0.04f, 0.12f,
                0.18f + (float)rng.NextDouble() * 0.08f,  // mossy green-grey
                0.22f + (float)rng.NextDouble() * 0.08f,
                0.14f + (float)rng.NextDouble() * 0.06f);
        }
    }

    // A section of crumbling stone wall (2.5m tall, 3m wide)
    private static void BuildWallSegment(List<float> verts, List<uint> inds, Random rng)
    {
        // Main wall body
        AddBox(verts, inds, 0, 1.25f, 0,
            3.0f, 2.5f, 0.45f,
            StoneR(rng), StoneG(rng), StoneB(rng));

        // Crumbled top — missing corner
        AddBox(verts, inds, 1.1f, 2.6f, 0,
            0.8f, 0.5f, 0.45f,
            StoneR(rng), StoneG(rng), StoneB(rng));

        // Crenellation stubs (battlements, mostly broken)
        for (int i = -1; i <= 1; i++)
        {
            if (rng.NextDouble() > 0.5) continue; // Some missing
            float h = 2.5f + (float)rng.NextDouble() * 0.6f;
            AddBox(verts, inds, i * 0.9f, h + 0.2f, 0,
                0.35f, 0.4f + (float)rng.NextDouble() * 0.3f, 0.45f,
                StoneR(rng), StoneG(rng), StoneB(rng));
        }

        // Stone blocks fallen near base
        for (int i = 0; i < 4; i++)
        {
            float bx = (float)(rng.NextDouble() - 0.5) * 2.6f;
            float bz = (float)(rng.NextDouble() > 0.5 ? 1 : -1) * (0.4f + (float)rng.NextDouble() * 0.4f);
            float by = (float)rng.NextDouble() * 0.2f;
            float bw = 0.25f + (float)rng.NextDouble() * 0.3f;
            float bh = 0.15f + (float)rng.NextDouble() * 0.2f;
            AddBox(verts, inds, bx, by + bh * 0.5f, bz,
                bw, bh, bw * 0.85f,
                StoneR(rng) * 0.9f, StoneG(rng) * 0.9f, StoneB(rng) * 0.9f);
        }

        // Moss on top surface
        AddBox(verts, inds, 0, 2.52f, 0,
            2.9f, 0.06f, 0.42f,
            0.20f, 0.26f, 0.16f);
    }

    // A broken arch fragment (one side of a gateway arch)
    private static void BuildArchFragment(List<float> verts, List<uint> inds, Random rng)
    {
        const int archSegs = 8;

        // Left pillar base
        AddBox(verts, inds, -0.8f, 1.25f, 0,
            0.5f, 2.5f, 0.5f,
            StoneR(rng), StoneG(rng), StoneB(rng));

        // Right pillar stump (shorter, broken)
        float rightH = 1.2f + (float)rng.NextDouble() * 0.8f;
        AddBox(verts, inds, 0.8f, rightH * 0.5f, 0,
            0.5f, rightH, 0.5f,
            StoneR(rng), StoneG(rng), StoneB(rng));

        // Arch curve (left quarter only, the right has collapsed)
        float archR = 1.0f;
        float archCx = -0.8f, archCy = 2.5f;
        const int halfSegs = archSegs / 2;
        for (int i = 0; i < halfSegs; i++)
        {
            float a0 = MathF.PI + i * (MathF.PI / 2f / halfSegs);
            float a1 = MathF.PI + (i + 1) * (MathF.PI / 2f / halfSegs);

            float x0 = archCx + MathF.Cos(a0) * archR;
            float y0 = archCy + MathF.Sin(a0) * archR;
            float x1 = archCx + MathF.Cos(a1) * archR;
            float y1 = archCy + MathF.Sin(a1) * archR;

            float midX = (x0 + x1) / 2f;
            float midY = (y0 + y1) / 2f;
            float segLen = MathF.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));

            AddBox(verts, inds, midX, midY, 0,
                segLen + 0.04f, 0.30f, 0.45f,
                StoneR(rng), StoneG(rng), StoneB(rng));
        }

        // Keystone (top of arch, now fallen to ground left side)
        AddBox(verts, inds, -0.2f, 0.12f, 0.1f,
            0.45f, 0.24f, 0.42f,
            StoneR(rng) * 0.92f, StoneG(rng) * 0.92f, StoneB(rng) * 0.92f);
    }

    // ---- Primitive helpers ----

    private static void AddCylinder(List<float> verts, List<uint> inds,
        float cx, float cy, float cz,
        float radius, float height, int segs,
        float r, float g, float b)
    {
        uint baseIdx = (uint)(verts.Count / 9);
        float halfH = height / 2f;

        for (int i = 0; i <= segs; i++)
        {
            float angle = i * MathF.Tau / segs;
            float cos = MathF.Cos(angle), sin = MathF.Sin(angle);
            verts.AddRange(new[] { cx + cos * radius, cy - halfH, cz + sin * radius, cos, 0f, sin, r, g, b });
            verts.AddRange(new[] { cx + cos * radius, cy + halfH, cz + sin * radius, cos, 0f, sin, r, g, b });
        }

        for (uint i = 0; i < (uint)segs; i++)
        {
            uint b0 = baseIdx + i * 2;
            inds.AddRange(new[] { b0, b0 + 1, b0 + 2, b0 + 1, b0 + 3, b0 + 2 });
        }

        // Top and bottom caps
        AddCap(verts, inds, cx, cy + halfH, cz, radius, segs, 0, 1, 0, r, g, b);
        AddCap(verts, inds, cx, cy - halfH, cz, radius, segs, 0, -1, 0, r, g, b);
    }

    private static void AddCap(List<float> verts, List<uint> inds,
        float cx, float cy, float cz, float radius, int segs,
        float nx, float ny, float nz, float r, float g, float b)
    {
        uint center = (uint)(verts.Count / 9);
        verts.AddRange(new[] { cx, cy, cz, nx, ny, nz, r, g, b });

        for (int i = 0; i <= segs; i++)
        {
            float angle = i * MathF.Tau / segs;
            verts.AddRange(new[] {
                cx + MathF.Cos(angle) * radius, cy, cz + MathF.Sin(angle) * radius,
                nx, ny, nz, r, g, b });
        }

        for (uint i = 0; i < (uint)segs; i++)
        {
            if (ny > 0)
                inds.AddRange(new[] { center, center + i + 1, center + i + 2 });
            else
                inds.AddRange(new[] { center, center + i + 2, center + i + 1 });
        }
    }

    private static void AddBox(List<float> verts, List<uint> inds,
        float cx, float cy, float cz,
        float w, float h, float d,
        float r, float g, float b)
    {
        float hw = w / 2f, hh = h / 2f, hd = d / 2f;

        // 6 faces: +Y -Y +X -X +Z -Z
        (float nx, float ny, float nz, float ax, float ay, float az, float bx2, float by2, float bz2)[] faces =
        {
            ( 0,  1,  0,   hw,  0,  0,   0,  0,  hd),
            ( 0, -1,  0,   hw,  0,  0,   0,  0,  hd),
            ( 1,  0,  0,   0,  hh,  0,   0,  0,  hd),
            (-1,  0,  0,   0,  hh,  0,   0,  0,  hd),
            ( 0,  0,  1,   hw,  0,  0,   0,  hh,  0),
            ( 0,  0, -1,   hw,  0,  0,   0,  hh,  0),
        };

        foreach (var (nx, ny, nz, ax, ay, az, bx2, by2, bz2) in faces)
        {
            uint baseIdx = (uint)(verts.Count / 9);
            float faceCx = cx + nx * hw, faceCy = cy + ny * hh, faceCz = cz + nz * hd;

            foreach (var (si, sj) in new[] { (-1, -1), (1, -1), (-1, 1), (1, 1) })
            {
                verts.AddRange(new[] {
                    faceCx + ax * si + bx2 * sj,
                    faceCy + ay * si + by2 * sj,
                    faceCz + az * si + bz2 * sj,
                    nx, ny, nz, r, g, b });
            }

            inds.AddRange(new[] { baseIdx, baseIdx + 1, baseIdx + 3,
                                   baseIdx, baseIdx + 3, baseIdx + 2 });
        }
    }

    // Weathered ancient stone with natural color variation (warm/cool mix)
    private static float StoneR(Random rng) => 0.28f + (float)rng.NextDouble() * 0.10f;
    private static float StoneG(Random rng) => 0.26f + (float)rng.NextDouble() * 0.08f;
    private static float StoneB(Random rng) => 0.24f + (float)rng.NextDouble() * 0.07f;
}
