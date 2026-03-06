using Silk.NET.OpenGL;
using CongCraft.Engine.Rendering;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates NPC meshes from smooth rounded primitives.
/// Different body types and color schemes for different NPC roles.
/// SpellForce-style rounded humanoids instead of blocky boxes.
/// </summary>
public static class NpcMeshBuilder
{
    private const int Segments = 16;

    public static MeshData GenerateBlacksmith()
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        // Torso (stocky capsule, brown leather apron)
        AddCapsule(verts, inds, 0, 0.75f, 0, 0.26f, 0.72f, Segments, 0.45f, 0.30f, 0.15f);
        // Head (sphere, skin tone)
        AddSphere(verts, inds, 0, 1.32f, 0, 0.14f, Segments, 0.68f, 0.52f, 0.42f);
        // Beard (dark, bushy)
        AddSphere(verts, inds, 0, 1.22f, 0.08f, 0.09f, 10, 0.30f, 0.20f, 0.12f);
        AddSphere(verts, inds, 0, 1.18f, 0.06f, 0.07f, 8, 0.28f, 0.18f, 0.10f);
        // Bald head (slightly wider skull)
        AddSphere(verts, inds, 0, 1.38f, -0.02f, 0.12f, 10, 0.66f, 0.50f, 0.40f);
        // Neck (thick)
        AddCylinder(verts, inds, 0, 1.14f, 0, 0.08f, 0.08f, 10, 0.65f, 0.50f, 0.40f);
        // Left arm (muscular, thick)
        AddCapsule(verts, inds, -0.36f, 0.75f, 0, 0.09f, 0.58f, 12, 0.62f, 0.48f, 0.38f);
        // Right arm
        AddCapsule(verts, inds, 0.36f, 0.75f, 0, 0.09f, 0.58f, 12, 0.62f, 0.48f, 0.38f);
        // Forearm bracers (leather work gloves)
        AddCapsule(verts, inds, -0.38f, 0.38f, 0, 0.065f, 0.20f, 8, 0.40f, 0.28f, 0.14f);
        AddCapsule(verts, inds, 0.38f, 0.38f, 0, 0.065f, 0.20f, 8, 0.40f, 0.28f, 0.14f);
        // Hands (big, strong)
        AddSphere(verts, inds, -0.38f, 0.28f, 0, 0.05f, 8, 0.64f, 0.50f, 0.40f);
        AddSphere(verts, inds, 0.38f, 0.28f, 0, 0.05f, 8, 0.64f, 0.50f, 0.40f);
        // Left leg
        AddCapsule(verts, inds, -0.12f, 0.2f, 0, 0.085f, 0.42f, 12, 0.38f, 0.28f, 0.18f);
        // Right leg
        AddCapsule(verts, inds, 0.12f, 0.2f, 0, 0.085f, 0.42f, 12, 0.38f, 0.28f, 0.18f);
        // Apron (cylinder around waist, sooty leather)
        AddCylinder(verts, inds, 0, 0.45f, 0.02f, 0.28f, 0.30f, Segments, 0.32f, 0.20f, 0.09f);
        // Apron strap
        AddCylinder(verts, inds, 0, 0.75f, 0.10f, 0.025f, 0.30f, 6, 0.30f, 0.18f, 0.08f);
        // Belt with tools
        AddCylinder(verts, inds, 0, 0.46f, 0, 0.27f, 0.04f, Segments, 0.35f, 0.22f, 0.10f);
        // Hammer at belt (handle)
        AddCylinder(verts, inds, -0.24f, 0.38f, 0.10f, 0.015f, 0.25f, 6, 0.48f, 0.36f, 0.20f);
        // Hammer head (metal)
        AddSphere(verts, inds, -0.24f, 0.64f, 0.10f, 0.04f, 8, 0.50f, 0.48f, 0.45f);
        // Boots (heavy work boots)
        AddCapsule(verts, inds, -0.12f, -0.02f, 0, 0.075f, 0.10f, 10, 0.28f, 0.18f, 0.09f);
        AddCapsule(verts, inds, 0.12f, -0.02f, 0, 0.075f, 0.10f, 10, 0.28f, 0.18f, 0.09f);
        AddSphere(verts, inds, -0.12f, -0.06f, 0.03f, 0.08f, 8, 0.26f, 0.17f, 0.08f);
        AddSphere(verts, inds, 0.12f, -0.06f, 0.03f, 0.08f, 8, 0.26f, 0.17f, 0.08f);

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    public static MeshData GenerateElder()
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        // Body (slender capsule, deep purple robes)
        AddCapsule(verts, inds, 0, 0.7f, 0, 0.22f, 0.72f, Segments, 0.38f, 0.22f, 0.48f);
        // Head (sphere, aged skin)
        AddSphere(verts, inds, 0, 1.28f, 0, 0.13f, Segments, 0.65f, 0.58f, 0.52f);
        // Long white beard
        AddCapsule(verts, inds, 0, 1.08f, 0.06f, 0.06f, 0.28f, 10, 0.82f, 0.80f, 0.76f);
        AddSphere(verts, inds, 0, 0.92f, 0.08f, 0.05f, 8, 0.80f, 0.78f, 0.74f);
        // White hair (back of head)
        AddSphere(verts, inds, 0, 1.32f, -0.06f, 0.12f, 10, 0.80f, 0.78f, 0.74f);
        // Bushy eyebrows
        AddCylinder(verts, inds, -0.05f, 1.33f, 0.11f, 0.025f, 0.01f, 6, 0.78f, 0.76f, 0.72f);
        AddCylinder(verts, inds, 0.05f, 1.33f, 0.11f, 0.025f, 0.01f, 6, 0.78f, 0.76f, 0.72f);
        // Left arm (wide sleeves, flowing)
        AddCapsule(verts, inds, -0.34f, 0.7f, 0, 0.10f, 0.56f, 12, 0.42f, 0.28f, 0.52f);
        // Right arm
        AddCapsule(verts, inds, 0.34f, 0.7f, 0, 0.10f, 0.56f, 12, 0.42f, 0.28f, 0.52f);
        // Sleeve cuffs (gold trim)
        AddCylinder(verts, inds, -0.34f, 0.44f, 0, 0.08f, 0.02f, 8, 0.72f, 0.55f, 0.15f);
        AddCylinder(verts, inds, 0.34f, 0.44f, 0, 0.08f, 0.02f, 8, 0.72f, 0.55f, 0.15f);
        // Robe skirt (wider, flowing)
        AddCylinder(verts, inds, 0, 0.08f, 0, 0.28f, 0.42f, Segments, 0.32f, 0.20f, 0.42f);
        // Robe hem (gold trim)
        AddCylinder(verts, inds, 0, 0.06f, 0, 0.29f, 0.02f, Segments, 0.68f, 0.52f, 0.14f);
        // Robe collar
        AddCylinder(verts, inds, 0, 1.10f, 0, 0.12f, 0.05f, 10, 0.35f, 0.20f, 0.45f);
        // Belt sash
        AddCylinder(verts, inds, 0, 0.50f, 0, 0.23f, 0.04f, Segments, 0.65f, 0.50f, 0.12f);
        // Pendant/amulet
        AddSphere(verts, inds, 0, 0.92f, 0.14f, 0.025f, 8, 0.70f, 0.55f, 0.15f);
        // Staff (gnarled wooden)
        AddCylinder(verts, inds, 0.42f, 0.06f, 0, 0.025f, 1.55f, 8, 0.48f, 0.38f, 0.24f);
        // Staff knot (wood grain detail)
        AddSphere(verts, inds, 0.42f, 0.80f, 0, 0.032f, 6, 0.44f, 0.34f, 0.20f);
        // Staff crystal/orb (glowing purple)
        AddSphere(verts, inds, 0.42f, 1.64f, 0, 0.065f, 10, 0.55f, 0.35f, 0.80f);
        // Crystal glow ring
        AddCylinder(verts, inds, 0.42f, 1.60f, 0, 0.035f, 0.01f, 8, 0.60f, 0.40f, 0.85f);
        // Staff top fork (two prongs holding crystal)
        AddCylinder(verts, inds, 0.40f, 1.52f, 0, 0.012f, 0.12f, 6, 0.45f, 0.35f, 0.22f);
        AddCylinder(verts, inds, 0.44f, 1.52f, 0, 0.012f, 0.12f, 6, 0.45f, 0.35f, 0.22f);

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    public static MeshData GenerateMerchant()
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        // Body (rounder capsule, rich green tunic)
        AddCapsule(verts, inds, 0, 0.75f, 0, 0.26f, 0.68f, Segments, 0.28f, 0.48f, 0.22f);
        // Head (sphere, skin)
        AddSphere(verts, inds, 0, 1.30f, 0, 0.13f, Segments, 0.66f, 0.54f, 0.44f);
        // Mustache
        AddCylinder(verts, inds, 0, 1.24f, 0.12f, 0.05f, 0.01f, 8, 0.32f, 0.22f, 0.14f);
        // Brown hair
        AddSphere(verts, inds, 0, 1.35f, -0.03f, 0.12f, 10, 0.36f, 0.24f, 0.14f);
        // Neck
        AddCylinder(verts, inds, 0, 1.14f, 0, 0.06f, 0.06f, 10, 0.64f, 0.52f, 0.42f);
        // Hat (cone, merchant cap)
        AddCone(verts, inds, 0, 1.42f, 0, 0.14f, 0.20f, 10, 0.52f, 0.38f, 0.18f);
        // Hat brim (flat cylinder, wider)
        AddCylinder(verts, inds, 0, 1.40f, 0, 0.19f, 0.025f, 12, 0.50f, 0.36f, 0.16f);
        // Hat feather (small accent)
        AddCylinder(verts, inds, 0.10f, 1.52f, -0.05f, 0.008f, 0.12f, 6, 0.80f, 0.20f, 0.15f);
        // Left arm (green sleeve)
        AddCapsule(verts, inds, -0.36f, 0.75f, 0, 0.065f, 0.54f, 12, 0.30f, 0.50f, 0.26f);
        // Right arm
        AddCapsule(verts, inds, 0.36f, 0.75f, 0, 0.065f, 0.54f, 12, 0.30f, 0.50f, 0.26f);
        // Hands
        AddSphere(verts, inds, -0.36f, 0.25f, 0, 0.04f, 8, 0.64f, 0.52f, 0.42f);
        AddSphere(verts, inds, 0.36f, 0.25f, 0, 0.04f, 8, 0.64f, 0.52f, 0.42f);
        // Left leg (brown pants)
        AddCapsule(verts, inds, -0.11f, 0.2f, 0, 0.072f, 0.42f, 12, 0.32f, 0.22f, 0.14f);
        // Right leg
        AddCapsule(verts, inds, 0.11f, 0.2f, 0, 0.072f, 0.42f, 12, 0.32f, 0.22f, 0.14f);
        // Belt (gold-trimmed)
        AddCylinder(verts, inds, 0, 0.48f, 0, 0.27f, 0.035f, Segments, 0.62f, 0.48f, 0.14f);
        // Belt pouch (large, leather)
        AddSphere(verts, inds, 0.20f, 0.46f, 0.12f, 0.06f, 8, 0.42f, 0.28f, 0.12f);
        // Second pouch (left side)
        AddSphere(verts, inds, -0.18f, 0.45f, 0.10f, 0.045f, 8, 0.40f, 0.26f, 0.10f);
        // Gold coin purse
        AddSphere(verts, inds, 0.12f, 0.44f, 0.14f, 0.03f, 6, 0.80f, 0.65f, 0.15f);
        // Boots (fine leather)
        AddCapsule(verts, inds, -0.11f, -0.02f, 0, 0.068f, 0.10f, 10, 0.30f, 0.20f, 0.10f);
        AddCapsule(verts, inds, 0.11f, -0.02f, 0, 0.068f, 0.10f, 10, 0.30f, 0.20f, 0.10f);
        AddSphere(verts, inds, -0.11f, -0.06f, 0.03f, 0.07f, 8, 0.28f, 0.18f, 0.09f);
        AddSphere(verts, inds, 0.11f, -0.06f, 0.03f, 0.07f, 8, 0.28f, 0.18f, 0.09f);
        // Collar detail
        AddCylinder(verts, inds, 0, 1.08f, 0, 0.10f, 0.04f, 10, 0.25f, 0.42f, 0.20f);

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    public static Mesh Create(GL gl, string npcType)
    {
        var data = npcType switch
        {
            "blacksmith" => GenerateBlacksmith(),
            "elder" => GenerateElder(),
            "merchant" => GenerateMerchant(),
            _ => GenerateBlacksmith()
        };
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

    private static void AddCone(List<float> verts, List<uint> inds,
        float cx, float cy, float cz, float radius, float height, int segments,
        float r, float g, float b)
    {
        uint baseIdx = (uint)(verts.Count / 9);
        verts.AddRange(new[] { cx, cy + height, cz, 0f, 1f, 0f, r * 0.9f, g * 0.9f, b * 0.9f });

        float ny = radius / MathF.Sqrt(radius * radius + height * height);
        float nxz = height / MathF.Sqrt(radius * radius + height * height);

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * MathF.Tau / segments;
            float cos = MathF.Cos(angle), sin = MathF.Sin(angle);
            verts.AddRange(new[] { cx + cos * radius, cy, cz + sin * radius, cos * nxz, ny, sin * nxz, r, g, b });
        }

        for (uint i = 1; i <= (uint)segments; i++)
            inds.AddRange(new[] { baseIdx, baseIdx + i, baseIdx + i + 1 });
    }
}
