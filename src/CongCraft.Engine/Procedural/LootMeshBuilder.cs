using Silk.NET.OpenGL;
using CongCraft.Engine.Inventory;
using CongCraft.Engine.Rendering;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates visually distinct loot drop meshes for different item types.
/// Each item type has a unique 3D shape so players can identify drops at a glance.
/// ~2010 RPG aesthetic: small detailed objects with recognizable silhouettes.
/// </summary>
public static class LootMeshBuilder
{
    /// <summary>
    /// Creates a mesh appropriate for the given item type and color.
    /// </summary>
    public static Mesh Create(GL gl, ItemType type, float r, float g, float b)
    {
        var data = type switch
        {
            ItemType.Weapon => GenerateWeaponDrop(r, g, b),
            ItemType.Armor => GenerateArmorDrop(r, g, b),
            ItemType.Consumable => GeneratePotionDrop(r, g, b),
            ItemType.Material => GenerateMaterialDrop(r, g, b),
            _ => GenerateMaterialDrop(r, g, b),
        };
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
    }

    /// <summary>Weapon drop: small sword/dagger shape lying on ground.</summary>
    public static MeshData GenerateWeaponDrop(float r, float g, float b)
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        // Small blade (metallic highlight)
        AddCapsule(verts, inds, 0, 0.15f, 0, 0.02f, 0.25f, 8, r * 1.1f, g * 1.1f, b * 1.1f);
        // Handle
        AddCapsule(verts, inds, 0, -0.05f, 0, 0.018f, 0.12f, 8, r * 0.6f, g * 0.5f, b * 0.4f);
        // Crossguard
        AddCapsule(verts, inds, 0, 0.05f, 0, 0.015f, 0.10f, 6, r * 0.8f, g * 0.75f, b * 0.7f);
        // Pommel
        AddSphere(verts, inds, 0, -0.10f, 0, 0.022f, 6, r * 0.7f, g * 0.6f, b * 0.3f);

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    /// <summary>Armor drop: small chest/shield shape.</summary>
    public static MeshData GenerateArmorDrop(float r, float g, float b)
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        // Shield/chest plate body (rounded)
        AddSphere(verts, inds, 0, 0.08f, 0, 0.10f, 10, r, g, b);
        // Metal rim around edge
        AddCylinder(verts, inds, 0, 0.02f, 0, 0.11f, 0.02f, 10, r * 0.7f, g * 0.7f, b * 0.7f);
        // Boss/stud in center
        AddSphere(verts, inds, 0, 0.12f, 0.06f, 0.03f, 6, r * 1.2f, g * 1.1f, b * 0.9f);
        // Small buckle detail
        AddCylinder(verts, inds, 0, 0.0f, 0, 0.04f, 0.015f, 8, r * 0.5f, g * 0.45f, b * 0.4f);

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    /// <summary>Potion drop: small bottle/flask shape.</summary>
    public static MeshData GeneratePotionDrop(float r, float g, float b)
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        // Flask body (round bottom)
        AddSphere(verts, inds, 0, 0.06f, 0, 0.06f, 10, r * 0.6f, g * 0.6f, b * 0.6f);
        // Glass body (transparent-ish, lighter)
        AddCapsule(verts, inds, 0, 0.06f, 0, 0.055f, 0.10f, 10, r, g, b);
        // Neck
        AddCylinder(verts, inds, 0, 0.12f, 0, 0.025f, 0.06f, 8, r * 0.7f, g * 0.7f, b * 0.7f);
        // Cork (warm brown)
        AddSphere(verts, inds, 0, 0.18f, 0, 0.028f, 6, 0.55f, 0.38f, 0.18f);
        // Liquid glow (inner sphere, brighter)
        AddSphere(verts, inds, 0, 0.04f, 0, 0.04f, 8, r * 1.3f, g * 1.3f, b * 1.3f);

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    /// <summary>Material drop: small sack/bundle shape.</summary>
    public static MeshData GenerateMaterialDrop(float r, float g, float b)
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        // Sack body (rounded, burlap-colored base)
        AddSphere(verts, inds, 0, 0.06f, 0, 0.07f, 10, r, g, b);
        // Tied top (narrower)
        AddCylinder(verts, inds, 0, 0.10f, 0, 0.03f, 0.04f, 8, r * 0.85f, g * 0.82f, b * 0.78f);
        // Rope tie
        AddCylinder(verts, inds, 0, 0.12f, 0, 0.035f, 0.01f, 8, 0.50f, 0.40f, 0.25f);
        // Bottom flat
        AddCylinder(verts, inds, 0, -0.01f, 0, 0.06f, 0.01f, 8, r * 0.75f, g * 0.72f, b * 0.68f);

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    // ─── Primitive helpers ───

    private static void AddSphere(List<float> verts, List<uint> inds,
        float cx, float cy, float cz, float radius, int segments,
        float r, float g, float b)
    {
        r = Math.Clamp(r, 0f, 1f); g = Math.Clamp(g, 0f, 1f); b = Math.Clamp(b, 0f, 1f);
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
        r = Math.Clamp(r, 0f, 1f); g = Math.Clamp(g, 0f, 1f); b = Math.Clamp(b, 0f, 1f);
        uint baseIdx = (uint)(verts.Count / 9);
        float bodyH = height - 2 * radius;
        int rings = 4;
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
        r = Math.Clamp(r, 0f, 1f); g = Math.Clamp(g, 0f, 1f); b = Math.Clamp(b, 0f, 1f);
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
