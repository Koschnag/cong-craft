using CongCraft.Engine.Rendering;
using Silk.NET.OpenGL;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates NPC meshes from boxes. Different color schemes for different NPC types.
/// </summary>
public static class NpcMeshBuilder
{
    public static MeshData GenerateBlacksmith()
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        // Body (brown leather apron)
        AddBox(verts, inds, 0, 0.75f, 0, 0.28f, 0.38f, 0.16f, 0.45f, 0.3f, 0.15f);
        // Head
        AddBox(verts, inds, 0, 1.3f, 0, 0.13f, 0.13f, 0.13f, 0.65f, 0.5f, 0.4f);
        // Left arm
        AddBox(verts, inds, -0.38f, 0.75f, 0, 0.08f, 0.32f, 0.08f, 0.5f, 0.35f, 0.2f);
        // Right arm
        AddBox(verts, inds, 0.38f, 0.75f, 0, 0.08f, 0.32f, 0.08f, 0.5f, 0.35f, 0.2f);
        // Left leg
        AddBox(verts, inds, -0.12f, 0.2f, 0, 0.09f, 0.22f, 0.09f, 0.35f, 0.25f, 0.15f);
        // Right leg
        AddBox(verts, inds, 0.12f, 0.2f, 0, 0.09f, 0.22f, 0.09f, 0.35f, 0.25f, 0.15f);

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    public static MeshData GenerateElder()
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        // Body (purple robes)
        AddBox(verts, inds, 0, 0.7f, 0, 0.26f, 0.4f, 0.15f, 0.35f, 0.2f, 0.45f);
        // Head (gray hair)
        AddBox(verts, inds, 0, 1.28f, 0, 0.12f, 0.12f, 0.12f, 0.6f, 0.55f, 0.5f);
        // Left arm (wide sleeves)
        AddBox(verts, inds, -0.36f, 0.7f, 0, 0.1f, 0.3f, 0.1f, 0.4f, 0.25f, 0.5f);
        // Right arm
        AddBox(verts, inds, 0.36f, 0.7f, 0, 0.1f, 0.3f, 0.1f, 0.4f, 0.25f, 0.5f);
        // Skirt (longer robe)
        AddBox(verts, inds, 0, 0.2f, 0, 0.22f, 0.22f, 0.12f, 0.3f, 0.18f, 0.4f);
        // Staff
        AddBox(verts, inds, 0.45f, 0.8f, 0, 0.02f, 0.7f, 0.02f, 0.5f, 0.4f, 0.25f);

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    public static MeshData GenerateMerchant()
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        // Body (green tunic)
        AddBox(verts, inds, 0, 0.75f, 0, 0.3f, 0.36f, 0.18f, 0.25f, 0.45f, 0.2f);
        // Head
        AddBox(verts, inds, 0, 1.28f, 0, 0.12f, 0.12f, 0.12f, 0.6f, 0.5f, 0.4f);
        // Hat
        AddBox(verts, inds, 0, 1.45f, 0, 0.15f, 0.06f, 0.15f, 0.5f, 0.35f, 0.15f);
        // Left arm
        AddBox(verts, inds, -0.4f, 0.75f, 0, 0.07f, 0.3f, 0.07f, 0.3f, 0.5f, 0.25f);
        // Right arm
        AddBox(verts, inds, 0.4f, 0.75f, 0, 0.07f, 0.3f, 0.07f, 0.3f, 0.5f, 0.25f);
        // Left leg
        AddBox(verts, inds, -0.11f, 0.2f, 0, 0.08f, 0.22f, 0.08f, 0.3f, 0.2f, 0.12f);
        // Right leg
        AddBox(verts, inds, 0.11f, 0.2f, 0, 0.08f, 0.22f, 0.08f, 0.3f, 0.2f, 0.12f);

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

    private static void AddBox(List<float> verts, List<uint> inds,
        float cx, float cy, float cz,
        float hw, float hh, float hd,
        float r, float g, float b)
    {
        float[][] corners =
        {
            new[] { cx-hw, cy-hh, cz-hd }, new[] { cx+hw, cy-hh, cz-hd },
            new[] { cx+hw, cy+hh, cz-hd }, new[] { cx-hw, cy+hh, cz-hd },
            new[] { cx-hw, cy-hh, cz+hd }, new[] { cx+hw, cy-hh, cz+hd },
            new[] { cx+hw, cy+hh, cz+hd }, new[] { cx-hw, cy+hh, cz+hd },
        };

        (int[] face, float[] normal)[] faces =
        {
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
            inds.AddRange(new[] { fi, fi + 1, fi + 2, fi, fi + 2, fi + 3 });
        }
    }
}
