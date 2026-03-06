using Silk.NET.OpenGL;
using CongCraft.Engine.Rendering;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates humanoid enemy mesh from smooth rounded primitives.
/// Dark armor look with capsule/cylinder body parts for a SpellForce-like aesthetic.
/// </summary>
public static class EnemyMeshBuilder
{
    private const int Segments = 12;

    public static MeshData GenerateData()
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        // Torso (capsule, dark steel armor)
        AddCapsule(verts, inds, 0, 0.7f, 0, 0.22f, 0.7f, Segments, 0.28f, 0.25f, 0.24f);

        // Head (sphere, skin with dark helmet tint)
        AddSphere(verts, inds, 0, 1.28f, 0, 0.13f, Segments, 0.55f, 0.42f, 0.38f);

        // Helmet crest (thin tapered cylinder on top)
        AddCylinder(verts, inds, 0, 1.38f, 0, 0.03f, 0.18f, 8, 0.6f, 0.15f, 0.1f);

        // Shoulder pads (spheres)
        AddSphere(verts, inds, -0.3f, 1.0f, 0, 0.1f, 8, 0.32f, 0.28f, 0.26f);
        AddSphere(verts, inds, 0.3f, 1.0f, 0, 0.1f, 8, 0.32f, 0.28f, 0.26f);

        // Left arm (capsule)
        AddCapsule(verts, inds, -0.32f, 0.7f, 0, 0.06f, 0.55f, 8, 0.30f, 0.26f, 0.24f);

        // Right arm (capsule, sword arm)
        AddCapsule(verts, inds, 0.32f, 0.7f, 0, 0.06f, 0.55f, 8, 0.30f, 0.26f, 0.24f);

        // Gauntlets (slightly thicker wrist spheres)
        AddSphere(verts, inds, -0.32f, 0.42f, 0, 0.07f, 8, 0.25f, 0.22f, 0.2f);
        AddSphere(verts, inds, 0.32f, 0.42f, 0, 0.07f, 8, 0.25f, 0.22f, 0.2f);

        // Left leg (capsule)
        AddCapsule(verts, inds, -0.1f, 0.22f, 0, 0.07f, 0.44f, 8, 0.22f, 0.2f, 0.18f);

        // Right leg (capsule)
        AddCapsule(verts, inds, 0.1f, 0.22f, 0, 0.07f, 0.44f, 8, 0.22f, 0.2f, 0.18f);

        // Boots (spheres at feet)
        AddSphere(verts, inds, -0.1f, 0.02f, 0.02f, 0.08f, 8, 0.2f, 0.18f, 0.16f);
        AddSphere(verts, inds, 0.1f, 0.02f, 0.02f, 0.08f, 8, 0.2f, 0.18f, 0.16f);

        // Belt (ring around waist)
        AddCylinder(verts, inds, 0, 0.48f, 0, 0.24f, 0.04f, Segments, 0.45f, 0.3f, 0.15f);

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    public static Mesh Create(GL gl)
    {
        var data = GenerateData();
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
    }

    /// <summary>
    /// Generates a skeleton enemy mesh: gaunt bone-white proportions, no armor,
    /// hollow eye sockets, visible rib suggestions, and tattered cloth wisps.
    /// </summary>
    public static MeshData GenerateSkeletonData()
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        // Bone-white base color (slightly yellowish aged bone)
        const float br = 0.88f, bg = 0.84f, bb = 0.72f;
        // Dark shadow tone for sockets / gaps
        const float dr = 0.12f, dg = 0.10f, db = 0.09f;

        // Spine / torso (narrow capsule, gaunt)
        AddCapsule(verts, inds, 0, 0.72f, 0, 0.14f, 0.62f, Segments, br, bg, bb);

        // Ribcage suggestion — thin horizontal cylinders across chest
        for (int i = 0; i < 4; i++)
        {
            float ry = 0.62f + i * 0.11f;
            float ribW = 0.18f - i * 0.015f; // Slightly narrowing upward
            AddCylinder(verts, inds, 0, ry, 0, ribW, 0.025f, 8, br * 0.92f, bg * 0.90f, bb * 0.85f);
        }

        // Skull (sphere, slightly larger than normal — hollow look)
        AddSphere(verts, inds, 0, 1.30f, 0, 0.14f, Segments, br, bg, bb);

        // Eye sockets (dark small spheres embedded in skull)
        AddSphere(verts, inds, -0.055f, 1.32f, 0.10f, 0.032f, 6, dr, dg, db);
        AddSphere(verts, inds,  0.055f, 1.32f, 0.10f, 0.032f, 6, dr, dg, db);

        // Jaw (small flat cylinder under skull)
        AddCylinder(verts, inds, 0, 1.16f, 0.05f, 0.07f, 0.04f, 8, br * 0.95f, bg * 0.93f, bb * 0.88f);

        // Shoulder bones (bare knobby spheres)
        AddSphere(verts, inds, -0.25f, 1.06f, 0, 0.065f, 8, br, bg, bb);
        AddSphere(verts, inds,  0.25f, 1.06f, 0, 0.065f, 8, br, bg, bb);

        // Left arm (thin capsule, gaunt)
        AddCapsule(verts, inds, -0.28f, 0.72f, 0, 0.038f, 0.52f, 8, br, bg, bb);

        // Right arm (slightly raised, sword arm)
        AddCapsule(verts, inds, 0.28f, 0.80f, 0.06f, 0.038f, 0.52f, 8, br, bg, bb);

        // Elbow knobs
        AddSphere(verts, inds, -0.28f, 0.48f, 0, 0.045f, 6, br, bg, bb);
        AddSphere(verts, inds,  0.28f, 0.56f, 0.06f, 0.045f, 6, br, bg, bb);

        // Forearms (thinner)
        AddCapsule(verts, inds, -0.30f, 0.28f, 0.02f, 0.028f, 0.30f, 8, br * 0.96f, bg * 0.94f, bb * 0.90f);
        AddCapsule(verts, inds,  0.30f, 0.36f, 0.06f, 0.028f, 0.30f, 8, br * 0.96f, bg * 0.94f, bb * 0.90f);

        // Pelvis (flat cylinder, wider than torso)
        AddCylinder(verts, inds, 0, 0.36f, 0, 0.18f, 0.06f, Segments, br * 0.9f, bg * 0.88f, bb * 0.84f);

        // Left leg (narrow capsule)
        AddCapsule(verts, inds, -0.10f, 0.18f, 0, 0.042f, 0.40f, 8, br, bg, bb);

        // Right leg
        AddCapsule(verts, inds,  0.10f, 0.18f, 0, 0.042f, 0.40f, 8, br, bg, bb);

        // Knee knobs
        AddSphere(verts, inds, -0.10f, 0.00f, 0.02f, 0.048f, 6, br, bg, bb);
        AddSphere(verts, inds,  0.10f, 0.00f, 0.02f, 0.048f, 6, br, bg, bb);

        // Foot bones (flat elongated)
        AddCylinder(verts, inds, -0.10f, -0.18f, 0.04f, 0.045f, 0.05f, 6, br * 0.9f, bg * 0.88f, bb * 0.85f);
        AddCylinder(verts, inds,  0.10f, -0.18f, 0.04f, 0.045f, 0.05f, 6, br * 0.9f, bg * 0.88f, bb * 0.85f);

        // Tattered robe wisps — very dark reddish cloth remnants
        AddCylinder(verts, inds, -0.05f, 0.50f, 0.12f, 0.12f, 0.28f, 6, 0.18f, 0.10f, 0.10f);
        AddCylinder(verts, inds,  0.08f, 0.45f, -0.10f, 0.09f, 0.22f, 6, 0.16f, 0.09f, 0.09f);

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    public static Mesh CreateSkeleton(GL gl)
    {
        var data = GenerateSkeletonData();
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
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
                float nx = cos, nz = sin;
                float ny = 0f;
                if (t < 0.25f || t > 0.75f)
                {
                    float lat = t < 0.25f ? -(1f - t / 0.25f) * MathF.PI / 2f : ((t - 0.75f) / 0.25f) * MathF.PI / 2f;
                    ny = MathF.Sin(lat);
                    float cosLat = MathF.Cos(lat);
                    nx = cos * cosLat;
                    nz = sin * cosLat;
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
                float px = cx + radius * nx, py = cy + radius * ny, pz = cz + radius * nz;
                verts.AddRange(new[] { px, py, pz, nx, ny, nz, r, g, b });
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
