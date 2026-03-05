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
    private const int Segments = 12;

    public static MeshData GenerateBlacksmith()
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        // Torso (stocky capsule, brown leather apron)
        AddCapsule(verts, inds, 0, 0.75f, 0, 0.26f, 0.72f, Segments, 0.45f, 0.3f, 0.15f);
        // Head (sphere, skin tone)
        AddSphere(verts, inds, 0, 1.32f, 0, 0.14f, Segments, 0.68f, 0.52f, 0.42f);
        // Left arm (muscular)
        AddCapsule(verts, inds, -0.36f, 0.75f, 0, 0.08f, 0.58f, 8, 0.52f, 0.38f, 0.22f);
        // Right arm
        AddCapsule(verts, inds, 0.36f, 0.75f, 0, 0.08f, 0.58f, 8, 0.52f, 0.38f, 0.22f);
        // Left leg
        AddCapsule(verts, inds, -0.12f, 0.2f, 0, 0.08f, 0.42f, 8, 0.38f, 0.28f, 0.18f);
        // Right leg
        AddCapsule(verts, inds, 0.12f, 0.2f, 0, 0.08f, 0.42f, 8, 0.38f, 0.28f, 0.18f);
        // Apron (cylinder around waist)
        AddCylinder(verts, inds, 0, 0.45f, 0, 0.28f, 0.25f, Segments, 0.35f, 0.22f, 0.1f);
        // Boots
        AddSphere(verts, inds, -0.12f, 0.02f, 0.02f, 0.09f, 8, 0.3f, 0.2f, 0.1f);
        AddSphere(verts, inds, 0.12f, 0.02f, 0.02f, 0.09f, 8, 0.3f, 0.2f, 0.1f);

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    public static MeshData GenerateElder()
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        // Body (slender capsule, purple robes)
        AddCapsule(verts, inds, 0, 0.7f, 0, 0.22f, 0.72f, Segments, 0.38f, 0.22f, 0.48f);
        // Head (sphere, gray hair)
        AddSphere(verts, inds, 0, 1.28f, 0, 0.13f, Segments, 0.62f, 0.58f, 0.54f);
        // Left arm (wide sleeves)
        AddCapsule(verts, inds, -0.34f, 0.7f, 0, 0.09f, 0.56f, 8, 0.42f, 0.28f, 0.52f);
        // Right arm
        AddCapsule(verts, inds, 0.34f, 0.7f, 0, 0.09f, 0.56f, 8, 0.42f, 0.28f, 0.52f);
        // Robe skirt (wider cylinder)
        AddCylinder(verts, inds, 0, 0.1f, 0, 0.26f, 0.38f, Segments, 0.32f, 0.2f, 0.42f);
        // Staff (thin tall cylinder)
        AddCylinder(verts, inds, 0.42f, 0.1f, 0, 0.025f, 1.5f, 6, 0.52f, 0.42f, 0.28f);
        // Staff orb
        AddSphere(verts, inds, 0.42f, 1.62f, 0, 0.06f, 8, 0.6f, 0.5f, 0.8f);

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    public static MeshData GenerateMerchant()
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        // Body (rounder capsule, green tunic)
        AddCapsule(verts, inds, 0, 0.75f, 0, 0.26f, 0.68f, Segments, 0.28f, 0.48f, 0.22f);
        // Head (sphere, skin)
        AddSphere(verts, inds, 0, 1.3f, 0, 0.13f, Segments, 0.62f, 0.52f, 0.42f);
        // Hat (cone)
        AddCone(verts, inds, 0, 1.42f, 0, 0.14f, 0.18f, 8, 0.52f, 0.38f, 0.18f);
        // Hat brim (flat cylinder)
        AddCylinder(verts, inds, 0, 1.4f, 0, 0.18f, 0.03f, 10, 0.5f, 0.36f, 0.16f);
        // Left arm
        AddCapsule(verts, inds, -0.36f, 0.75f, 0, 0.06f, 0.54f, 8, 0.32f, 0.52f, 0.28f);
        // Right arm
        AddCapsule(verts, inds, 0.36f, 0.75f, 0, 0.06f, 0.54f, 8, 0.32f, 0.52f, 0.28f);
        // Left leg
        AddCapsule(verts, inds, -0.11f, 0.2f, 0, 0.07f, 0.42f, 8, 0.32f, 0.22f, 0.14f);
        // Right leg
        AddCapsule(verts, inds, 0.11f, 0.2f, 0, 0.07f, 0.42f, 8, 0.32f, 0.22f, 0.14f);
        // Boots
        AddSphere(verts, inds, -0.11f, 0.02f, 0.02f, 0.08f, 8, 0.28f, 0.18f, 0.1f);
        AddSphere(verts, inds, 0.11f, 0.02f, 0.02f, 0.08f, 8, 0.28f, 0.18f, 0.1f);
        // Belt pouch (sphere)
        AddSphere(verts, inds, 0.2f, 0.5f, 0.12f, 0.06f, 6, 0.45f, 0.3f, 0.12f);

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
