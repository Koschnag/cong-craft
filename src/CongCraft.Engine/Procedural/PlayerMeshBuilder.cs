using Silk.NET.OpenGL;
using CongCraft.Engine.Rendering;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates a SpellForce-style player warrior mesh.
/// Smooth capsule/sphere/cylinder humanoid with light armor, cloak, and boots.
/// Uses PositionNormalColor layout (9 floats per vertex).
/// </summary>
public static class PlayerMeshBuilder
{
    private const int Segments = 16;

    public static MeshData GenerateData()
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        // ─── Body ───

        // Torso — light leather armor (warm brown)
        AddCapsule(verts, inds, 0, 0.75f, 0, 0.20f, 0.65f, Segments, 0.42f, 0.30f, 0.18f);

        // Chest plate overlay (slightly brighter, thinner)
        AddCapsule(verts, inds, 0, 0.82f, 0.02f, 0.21f, 0.40f, 12, 0.48f, 0.35f, 0.20f);

        // ─── Head ───

        // Head (skin tone)
        AddSphere(verts, inds, 0, 1.32f, 0, 0.12f, Segments, 0.72f, 0.58f, 0.48f);

        // Hair (dark brown, slightly larger than head at back)
        AddSphere(verts, inds, 0, 1.36f, -0.04f, 0.11f, 12, 0.22f, 0.14f, 0.08f);

        // Nose bridge (small detail)
        AddSphere(verts, inds, 0, 1.30f, 0.11f, 0.02f, 8, 0.70f, 0.56f, 0.46f);

        // Ears
        AddSphere(verts, inds, -0.11f, 1.32f, 0, 0.025f, 8, 0.70f, 0.56f, 0.46f);
        AddSphere(verts, inds, 0.11f, 1.32f, 0, 0.025f, 8, 0.70f, 0.56f, 0.46f);

        // Neck
        AddCylinder(verts, inds, 0, 1.16f, 0, 0.06f, 0.08f, 12, 0.68f, 0.55f, 0.44f);

        // ─── Shoulders ───

        // Shoulder guards (leather pauldrons)
        AddSphere(verts, inds, -0.28f, 1.05f, 0, 0.10f, 12, 0.38f, 0.28f, 0.16f);
        AddSphere(verts, inds, 0.28f, 1.05f, 0, 0.10f, 12, 0.38f, 0.28f, 0.16f);

        // Metal studs on shoulders (3 per side)
        AddSphere(verts, inds, -0.28f, 1.10f, 0.05f, 0.03f, 8, 0.60f, 0.55f, 0.50f);
        AddSphere(verts, inds, 0.28f, 1.10f, 0.05f, 0.03f, 8, 0.60f, 0.55f, 0.50f);
        AddSphere(verts, inds, -0.24f, 1.08f, 0.08f, 0.02f, 8, 0.55f, 0.50f, 0.45f);
        AddSphere(verts, inds, 0.24f, 1.08f, 0.08f, 0.02f, 8, 0.55f, 0.50f, 0.45f);

        // ─── Arms ───

        // Upper arms (skin + leather wraps)
        AddCapsule(verts, inds, -0.30f, 0.75f, 0, 0.06f, 0.48f, 12, 0.65f, 0.52f, 0.42f);
        AddCapsule(verts, inds, 0.30f, 0.75f, 0, 0.06f, 0.48f, 12, 0.65f, 0.52f, 0.42f);

        // Elbow guards
        AddSphere(verts, inds, -0.31f, 0.52f, 0.03f, 0.035f, 8, 0.45f, 0.38f, 0.30f);
        AddSphere(verts, inds, 0.31f, 0.52f, 0.03f, 0.035f, 8, 0.45f, 0.38f, 0.30f);

        // Forearm bracers (leather with metal trim)
        AddCapsule(verts, inds, -0.32f, 0.35f, 0.02f, 0.055f, 0.26f, 12, 0.36f, 0.26f, 0.15f);
        AddCapsule(verts, inds, 0.32f, 0.35f, 0.02f, 0.055f, 0.26f, 12, 0.36f, 0.26f, 0.15f);

        // Hands (skin)
        AddSphere(verts, inds, -0.33f, 0.20f, 0.02f, 0.04f, 8, 0.70f, 0.56f, 0.46f);
        AddSphere(verts, inds, 0.33f, 0.20f, 0.02f, 0.04f, 8, 0.70f, 0.56f, 0.46f);

        // ─── Belt & Waist ───

        // Belt (dark leather with amber buckle)
        AddCylinder(verts, inds, 0, 0.48f, 0, 0.22f, 0.06f, Segments, 0.28f, 0.18f, 0.10f);

        // Belt buckle (gold accent)
        AddSphere(verts, inds, 0, 0.48f, 0.22f, 0.025f, 8, 0.82f, 0.62f, 0.15f);

        // Belt pouch (left side)
        AddSphere(verts, inds, 0.18f, 0.46f, 0.10f, 0.04f, 8, 0.32f, 0.22f, 0.12f);

        // Belt pouch strap
        AddCylinder(verts, inds, 0.18f, 0.46f, 0.08f, 0.015f, 0.06f, 8, 0.26f, 0.18f, 0.10f);

        // Dagger scabbard (right hip)
        AddCapsule(verts, inds, -0.20f, 0.36f, 0.08f, 0.02f, 0.18f, 8, 0.22f, 0.16f, 0.10f);

        // ─── Legs ───

        // Thighs (cloth/leather pants)
        AddCapsule(verts, inds, -0.10f, 0.28f, 0, 0.075f, 0.36f, 12, 0.32f, 0.24f, 0.15f);
        AddCapsule(verts, inds, 0.10f, 0.28f, 0, 0.075f, 0.36f, 12, 0.32f, 0.24f, 0.15f);

        // Knee guards (metal)
        AddSphere(verts, inds, -0.10f, 0.10f, 0.04f, 0.05f, 8, 0.50f, 0.45f, 0.40f);
        AddSphere(verts, inds, 0.10f, 0.10f, 0.04f, 0.05f, 8, 0.50f, 0.45f, 0.40f);

        // Shins with greaves
        AddCapsule(verts, inds, -0.10f, -0.05f, 0, 0.058f, 0.30f, 12, 0.30f, 0.22f, 0.14f);
        AddCapsule(verts, inds, 0.10f, -0.05f, 0, 0.058f, 0.30f, 12, 0.30f, 0.22f, 0.14f);

        // Shin guard plates (metal overlay)
        AddCapsule(verts, inds, -0.10f, -0.02f, 0.04f, 0.03f, 0.20f, 8, 0.48f, 0.42f, 0.38f);
        AddCapsule(verts, inds, 0.10f, -0.02f, 0.04f, 0.03f, 0.20f, 8, 0.48f, 0.42f, 0.38f);

        // ─── Boots ───

        // Boot shafts (heavy leather)
        AddCapsule(verts, inds, -0.10f, -0.18f, 0, 0.065f, 0.10f, 12, 0.25f, 0.17f, 0.10f);
        AddCapsule(verts, inds, 0.10f, -0.18f, 0, 0.065f, 0.10f, 12, 0.25f, 0.17f, 0.10f);

        // Boot soles (slightly extended forward)
        AddSphere(verts, inds, -0.10f, -0.24f, 0.03f, 0.07f, 10, 0.20f, 0.14f, 0.08f);
        AddSphere(verts, inds, 0.10f, -0.24f, 0.03f, 0.07f, 10, 0.20f, 0.14f, 0.08f);

        // Boot heel detail
        AddSphere(verts, inds, -0.10f, -0.24f, -0.03f, 0.04f, 8, 0.22f, 0.15f, 0.09f);
        AddSphere(verts, inds, 0.10f, -0.24f, -0.03f, 0.04f, 8, 0.22f, 0.15f, 0.09f);

        // ─── Cloak (short cape, back only) ───

        // Cloak body (dark green/brown, draping from shoulders)
        AddCylinder(verts, inds, 0, 0.72f, -0.15f, 0.22f, 0.55f, 12, 0.18f, 0.22f, 0.12f);

        // Cloak bottom fringe (slightly wider)
        AddCylinder(verts, inds, 0, 0.44f, -0.16f, 0.24f, 0.08f, 12, 0.16f, 0.20f, 0.11f);

        // Cloak clasp at neck (gold)
        AddSphere(verts, inds, 0, 1.12f, -0.12f, 0.025f, 8, 0.78f, 0.58f, 0.12f);

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    public static Mesh Create(GL gl)
    {
        var data = GenerateData();
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
    }

    // ─── Primitive helpers (same approach as EnemyMeshBuilder) ───

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
