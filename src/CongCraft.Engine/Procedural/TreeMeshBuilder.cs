using Silk.NET.OpenGL;
using CongCraft.Engine.Rendering;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates tree meshes with smooth cylindrical trunk and layered spherical canopy.
/// SpellForce-style organic trees with higher polygon count.
/// </summary>
public static class TreeMeshBuilder
{
    public record TreeParams(
        float TrunkRadius = 0.15f,
        float TrunkHeight = 2.5f,
        float CrownRadius = 1.5f,
        float CrownHeight = 3.5f,
        int Segments = 12
    );

    public static MeshData GenerateData(TreeParams? p = null)
    {
        p ??= new TreeParams();
        var verts = new List<float>();
        var inds = new List<uint>();

        // Trunk (brown cylinder with more segments for smoothness)
        AddCylinder(verts, inds, 0, 0, 0,
            p.TrunkRadius, p.TrunkHeight, p.Segments,
            0.35f, 0.22f, 0.1f);

        // Trunk base flare (wider at bottom)
        AddCylinder(verts, inds, 0, 0, 0,
            p.TrunkRadius * 1.4f, p.TrunkHeight * 0.15f, p.Segments,
            0.32f, 0.2f, 0.09f);

        // Layered canopy using overlapping spheres for organic look
        float baseY = p.TrunkHeight;
        float crR = p.CrownRadius;

        // Main central canopy sphere (largest)
        AddSphere(verts, inds, 0, baseY + crR * 0.7f, 0, crR * 0.85f, p.Segments,
            0.1f, 0.3f, 0.06f);

        // Upper sphere (lighter green, slightly smaller)
        AddSphere(verts, inds, 0, baseY + crR * 1.2f, 0, crR * 0.6f, p.Segments,
            0.12f, 0.35f, 0.08f);

        // Side spheres for volume (darker green)
        AddSphere(verts, inds, crR * 0.5f, baseY + crR * 0.5f, 0, crR * 0.55f, 8,
            0.08f, 0.26f, 0.05f);
        AddSphere(verts, inds, -crR * 0.4f, baseY + crR * 0.55f, crR * 0.3f, crR * 0.5f, 8,
            0.09f, 0.28f, 0.05f);
        AddSphere(verts, inds, 0, baseY + crR * 0.4f, -crR * 0.4f, crR * 0.5f, 8,
            0.08f, 0.25f, 0.04f);

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
}

public record MeshData(float[] Vertices, uint[] Indices)
{
    public int VertexCount => Vertices.Length / 9; // 9 floats per vertex (pos + normal + color)
}
