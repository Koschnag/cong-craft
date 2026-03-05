using CongCraft.Engine.Rendering;
using Silk.NET.OpenGL;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates a simple tree mesh: brown cylinder trunk + dark green cone crown.
/// </summary>
public static class TreeMeshBuilder
{
    public record TreeParams(
        float TrunkRadius = 0.15f,
        float TrunkHeight = 2.5f,
        float CrownRadius = 1.5f,
        float CrownHeight = 3.5f,
        int Segments = 8
    );

    public static MeshData GenerateData(TreeParams? p = null)
    {
        p ??= new TreeParams();
        var verts = new List<float>();
        var inds = new List<uint>();

        // Trunk (brown cylinder)
        AddCylinder(verts, inds, 0, 0, 0,
            p.TrunkRadius, p.TrunkHeight, p.Segments,
            0.35f, 0.22f, 0.1f);

        // Crown (dark green cone)
        AddCone(verts, inds, 0, p.TrunkHeight, 0,
            p.CrownRadius, p.CrownHeight, p.Segments,
            0.08f, 0.25f, 0.05f);

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    public static Mesh Create(GL gl, TreeParams? p = null)
    {
        var data = GenerateData(p);
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
    }

    private static void AddCylinder(List<float> verts, List<uint> inds,
        float cx, float cy, float cz,
        float radius, float height, int segments,
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
        float cx, float cy, float cz,
        float radius, float height, int segments,
        float r, float g, float b)
    {
        uint baseIdx = (uint)(verts.Count / 9);

        // Apex
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

public record MeshData(float[] Vertices, uint[] Indices)
{
    public int VertexCount => Vertices.Length / 9; // 9 floats per vertex (pos + normal + color)
}
