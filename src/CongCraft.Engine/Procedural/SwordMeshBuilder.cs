using Silk.NET.OpenGL;
using CongCraft.Engine.Rendering;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates a simple medieval sword mesh from primitives.
/// Blade (elongated box) + guard (flat box) + handle (thin cylinder).
/// </summary>
public static class SwordMeshBuilder
{
    public static MeshData GenerateData()
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        // Handle (dark brown cylinder)
        AddBox(verts, inds, 0, 0, 0, 0.04f, 0.25f, 0.04f, 0.3f, 0.2f, 0.1f);

        // Guard (dark gray flat box)
        AddBox(verts, inds, 0, 0.25f, 0, 0.15f, 0.03f, 0.04f, 0.35f, 0.35f, 0.3f);

        // Blade (light gray elongated box, tapers slightly)
        AddBlade(verts, inds, 0, 0.28f, 0, 0.03f, 0.8f, 0.015f, 0.7f, 0.7f, 0.75f);

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
        uint baseIdx = (uint)(verts.Count / 9);

        // 8 corners
        float[][] corners = {
            new[] { cx-hw, cy-hh, cz-hd }, new[] { cx+hw, cy-hh, cz-hd },
            new[] { cx+hw, cy+hh, cz-hd }, new[] { cx-hw, cy+hh, cz-hd },
            new[] { cx-hw, cy-hh, cz+hd }, new[] { cx+hw, cy-hh, cz+hd },
            new[] { cx+hw, cy+hh, cz+hd }, new[] { cx-hw, cy+hh, cz+hd },
        };

        // 6 faces with normals
        (int[] face, float[] normal)[] faces = {
            (new[]{0,1,2,3}, new[]{0f,0f,-1f}), // front
            (new[]{5,4,7,6}, new[]{0f,0f,1f}),  // back
            (new[]{4,0,3,7}, new[]{-1f,0f,0f}), // left
            (new[]{1,5,6,2}, new[]{1f,0f,0f}),  // right
            (new[]{3,2,6,7}, new[]{0f,1f,0f}),  // top
            (new[]{4,5,1,0}, new[]{0f,-1f,0f}), // bottom
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
        uint baseIdx = (uint)(verts.Count / 9);
        float tipWidth = baseWidth * 0.1f;

        // Bottom quad (wider)
        // Top point (narrower, tip)
        // Build as tapered box: 8 vertices

        float bw = baseWidth, tw = tipWidth;
        float hd = depth;
        float[] bl = { cx-bw, cy, cz-hd };
        float[] br = { cx+bw, cy, cz-hd };
        float[] fr = { cx+bw, cy, cz+hd };
        float[] fl = { cx-bw, cy, cz+hd };
        float[] tbl = { cx-tw, cy+height, cz-hd*0.5f };
        float[] tbr = { cx+tw, cy+height, cz-hd*0.5f };
        float[] tfr = { cx+tw, cy+height, cz+hd*0.5f };
        float[] tfl = { cx-tw, cy+height, cz+hd*0.5f };

        float[][] corners = { bl, br, fr, fl, tbl, tbr, tfr, tfl };

        (int[] face, float[] normal)[] faces = {
            (new[]{0,1,5,4}, new[]{0f,0f,-1f}),
            (new[]{2,3,7,6}, new[]{0f,0f,1f}),
            (new[]{3,0,4,7}, new[]{-1f,0.2f,0f}),
            (new[]{1,2,6,5}, new[]{1f,0.2f,0f}),
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
                // Slightly lighter at tip
                float brightFactor = idx >= 4 ? 1.1f : 1f;
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
