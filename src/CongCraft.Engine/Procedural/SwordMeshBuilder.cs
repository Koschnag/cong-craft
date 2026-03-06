using Silk.NET.OpenGL;
using CongCraft.Engine.Rendering;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates a detailed medieval sword mesh with pommel, wrapped grip,
/// crossguard, fuller groove, and tapered double-edged blade.
/// ~2010 RPG aesthetic with smooth capsule/sphere/cylinder primitives.
/// </summary>
public static class SwordMeshBuilder
{
    private const int Segments = 12;

    public static MeshData GenerateData()
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        // ─── Pommel (spherical, brass/bronze) ───
        AddSphere(verts, inds, 0, -0.04f, 0, 0.035f, 10, 0.65f, 0.50f, 0.18f);

        // Pommel cap (small disk on bottom)
        AddCylinder(verts, inds, 0, -0.07f, 0, 0.025f, 0.02f, 8, 0.58f, 0.44f, 0.15f);

        // ─── Grip (wrapped leather, dark brown) ───
        // Main grip cylinder
        AddCapsule(verts, inds, 0, 0.10f, 0, 0.025f, 0.22f, 10, 0.28f, 0.18f, 0.10f);

        // Leather wrap rings (lighter brown ridges)
        for (int i = 0; i < 5; i++)
        {
            float y = 0.02f + i * 0.04f;
            AddCylinder(verts, inds, 0, y, 0, 0.028f, 0.008f, 8, 0.34f, 0.22f, 0.12f);
        }

        // ─── Crossguard (steel, wider) ───
        // Main guard bar
        AddCapsule(verts, inds, 0, 0.23f, 0, 0.018f, 0.22f, 10, 0.52f, 0.50f, 0.48f);
        // Rotate 90 degrees: horizontal guard
        AddBox(verts, inds, 0, 0.23f, 0, 0.14f, 0.018f, 0.025f, 0.52f, 0.50f, 0.48f);

        // Guard tips (slightly rounded, gold accent)
        AddSphere(verts, inds, -0.14f, 0.23f, 0, 0.018f, 8, 0.60f, 0.48f, 0.16f);
        AddSphere(verts, inds, 0.14f, 0.23f, 0, 0.018f, 8, 0.60f, 0.48f, 0.16f);

        // Rain guard (small widened section above crossguard)
        AddCylinder(verts, inds, 0, 0.25f, 0, 0.032f, 0.03f, 8, 0.55f, 0.52f, 0.50f);

        // ─── Blade (tapered steel with fuller groove) ───
        // Main blade body (elongated tapered box, polished steel)
        AddBlade(verts, inds, 0, 0.28f, 0, 0.028f, 0.75f, 0.012f, 0.72f, 0.70f, 0.68f);

        // Fuller (blood groove) — darker inset line on blade face
        AddBox(verts, inds, 0, 0.52f, 0.005f, 0.008f, 0.28f, 0.002f, 0.45f, 0.43f, 0.42f);
        AddBox(verts, inds, 0, 0.52f, -0.005f, 0.008f, 0.28f, 0.002f, 0.45f, 0.43f, 0.42f);

        // Blade tip (sharper point)
        AddBlade(verts, inds, 0, 1.03f, 0, 0.012f, 0.12f, 0.006f, 0.78f, 0.76f, 0.74f);

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    public static Mesh Create(GL gl)
    {
        var data = GenerateData();
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
    }

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

    private static void AddCapsule(List<float> verts, List<uint> inds,
        float cx, float cy, float cz, float radius, float height, int segments,
        float r, float g, float b)
    {
        uint baseIdx = (uint)(verts.Count / 9);
        float bodyH = height - 2 * radius;
        int rings = 6;
        int totalRings = rings * 2 + 2;
        for (int ring = 0; ring <= totalRings; ring++)
        {
            float t = (float)ring / totalRings;
            float y, ringRadius;
            if (t < 0.25f)
            {
                float angle = (t / 0.25f) * MathF.PI / 2f;
                y = cy - bodyH / 2f - radius * MathF.Cos(angle);
                ringRadius = radius * MathF.Sin(angle);
            }
            else if (t < 0.75f)
            {
                y = cy - bodyH / 2f + bodyH * ((t - 0.25f) / 0.5f);
                ringRadius = radius;
            }
            else
            {
                float angle = ((t - 0.75f) / 0.25f) * MathF.PI / 2f;
                y = cy + bodyH / 2f + radius * MathF.Sin(angle);
                ringRadius = radius * MathF.Cos(angle);
            }
            for (int seg = 0; seg <= segments; seg++)
            {
                float a = seg * MathF.Tau / segments;
                float cos = MathF.Cos(a), sin = MathF.Sin(a);
                float px = cx + cos * ringRadius;
                float pz = cz + sin * ringRadius;
                float nx = cos, nz = sin, ny = 0f;
                if (t < 0.25f || t > 0.75f)
                {
                    float lat = t < 0.25f ? -(1f - t / 0.25f) * MathF.PI / 2f : ((t - 0.75f) / 0.25f) * MathF.PI / 2f;
                    ny = MathF.Sin(lat);
                    float cosLat = MathF.Cos(lat);
                    nx = cos * cosLat; nz = sin * cosLat;
                }
                verts.AddRange(new[] { px, y, pz, nx, ny, nz, r, g, b });
            }
        }
        int vertsPerRing = segments + 1;
        for (int ring = 0; ring < totalRings; ring++)
        for (int seg = 0; seg < segments; seg++)
        {
            uint tl = baseIdx + (uint)(ring * vertsPerRing + seg);
            uint tr = tl + 1;
            uint bl = (uint)(tl + vertsPerRing);
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

    private static void AddBox(List<float> verts, List<uint> inds,
        float cx, float cy, float cz,
        float hw, float hh, float hd,
        float r, float g, float b)
    {
        float[][] corners = {
            new[] { cx-hw, cy-hh, cz-hd }, new[] { cx+hw, cy-hh, cz-hd },
            new[] { cx+hw, cy+hh, cz-hd }, new[] { cx-hw, cy+hh, cz-hd },
            new[] { cx-hw, cy-hh, cz+hd }, new[] { cx+hw, cy-hh, cz+hd },
            new[] { cx+hw, cy+hh, cz+hd }, new[] { cx-hw, cy+hh, cz+hd },
        };
        (int[] face, float[] normal)[] faces = {
            (new[]{0,1,2,3}, new[]{0f,0f,-1f}),
            (new[]{5,4,7,6}, new[]{0f,0f,1f}),
            (new[]{4,0,3,7}, new[]{-1f,0f,0f}),
            (new[]{1,5,6,2}, new[]{1f,0f,0f}),
            (new[]{3,2,6,7}, new[]{0f,1f,0f}),
            (new[]{4,5,1,0}, new[]{0f,-1f,0f}),
        };
        foreach (var (face, normal) in faces)
        {
            uint fi = (uint)(verts.Count / 9);
            foreach (int idx in face)
            {
                verts.AddRange(corners[idx]);
                verts.AddRange(normal);
                verts.AddRange(new[] { r, g, b });
            }
            inds.AddRange(new[] { fi, fi+1, fi+2, fi, fi+2, fi+3 });
        }
    }

    private static void AddBlade(List<float> verts, List<uint> inds,
        float cx, float cy, float cz,
        float baseWidth, float height, float depth,
        float r, float g, float b)
    {
        float tipWidth = baseWidth * 0.08f;
        float bw = baseWidth, tw = tipWidth;
        float hd = depth;
        float[] bl = { cx-bw, cy, cz-hd };
        float[] br = { cx+bw, cy, cz-hd };
        float[] fr = { cx+bw, cy, cz+hd };
        float[] fl = { cx-bw, cy, cz+hd };
        float[] tbl = { cx-tw, cy+height, cz-hd*0.3f };
        float[] tbr = { cx+tw, cy+height, cz-hd*0.3f };
        float[] tfr = { cx+tw, cy+height, cz+hd*0.3f };
        float[] tfl = { cx-tw, cy+height, cz+hd*0.3f };
        float[][] corners = { bl, br, fr, fl, tbl, tbr, tfr, tfl };
        (int[] face, float[] normal)[] faces = {
            (new[]{0,1,5,4}, new[]{0f,0f,-1f}),
            (new[]{2,3,7,6}, new[]{0f,0f,1f}),
            (new[]{3,0,4,7}, new[]{-1f,0.15f,0f}),
            (new[]{1,2,6,5}, new[]{1f,0.15f,0f}),
            (new[]{4,5,6,7}, new[]{0f,1f,0f}),
            (new[]{3,2,1,0}, new[]{0f,-1f,0f}),
        };
        foreach (var (face, normal) in faces)
        {
            uint fi = (uint)(verts.Count / 9);
            foreach (int idx in face)
            {
                verts.AddRange(corners[idx]);
                verts.AddRange(normal);
                float brightFactor = idx >= 4 ? 1.12f : 1f;
                verts.AddRange(new[] {
                    Math.Min(1f, r * brightFactor),
                    Math.Min(1f, g * brightFactor),
                    Math.Min(1f, b * brightFactor)
                });
            }
            inds.AddRange(new[] { fi, fi+1, fi+2, fi, fi+2, fi+3 });
        }
    }
}
