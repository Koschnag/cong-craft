using CongCraft.Engine.Rendering;
using Silk.NET.OpenGL;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates a simple humanoid enemy mesh from primitives.
/// Dark armor look: dark gray/red color scheme.
/// </summary>
public static class EnemyMeshBuilder
{
    public static MeshData GenerateData()
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        // Body (dark gray torso - box)
        AddBox(verts, inds, 0, 0.7f, 0, 0.25f, 0.35f, 0.15f, 0.25f, 0.22f, 0.2f);

        // Head (dark sphere approximation - small box)
        AddBox(verts, inds, 0, 1.25f, 0, 0.12f, 0.12f, 0.12f, 0.5f, 0.4f, 0.35f);

        // Left arm
        AddBox(verts, inds, -0.35f, 0.7f, 0, 0.07f, 0.3f, 0.07f, 0.3f, 0.25f, 0.22f);

        // Right arm (sword arm)
        AddBox(verts, inds, 0.35f, 0.7f, 0, 0.07f, 0.3f, 0.07f, 0.3f, 0.25f, 0.22f);

        // Left leg
        AddBox(verts, inds, -0.1f, 0.2f, 0, 0.08f, 0.2f, 0.08f, 0.2f, 0.18f, 0.15f);

        // Right leg
        AddBox(verts, inds, 0.1f, 0.2f, 0, 0.08f, 0.2f, 0.08f, 0.2f, 0.18f, 0.15f);

        // Helmet detail (red accent on top)
        AddBox(verts, inds, 0, 1.4f, 0, 0.05f, 0.04f, 0.14f, 0.6f, 0.15f, 0.1f);

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    public static Mesh Create(GL gl)
    {
        var data = GenerateData();
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
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
            inds.AddRange(new[] { fi, fi + 1, fi + 2, fi, fi + 2, fi + 3 });
        }
    }
}
