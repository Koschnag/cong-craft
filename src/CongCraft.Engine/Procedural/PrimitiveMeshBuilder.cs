using Silk.NET.OpenGL;
using CongCraft.Engine.Rendering;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates basic geometric meshes: planes, cubes, cylinders, spheres, capsules.
/// All meshes use PositionNormalColor layout (9 floats per vertex).
/// </summary>
public static class PrimitiveMeshBuilder
{
    public static Mesh CreatePlane(GL gl, float width, float depth, float r, float g, float b)
    {
        float hw = width / 2f, hd = depth / 2f;
        float[] vertices =
        {
            -hw, 0, -hd,  0, 1, 0,  r, g, b,
             hw, 0, -hd,  0, 1, 0,  r, g, b,
             hw, 0,  hd,  0, 1, 0,  r, g, b,
            -hw, 0,  hd,  0, 1, 0,  r, g, b,
        };
        uint[] indices = { 0, 1, 2, 0, 2, 3 };
        return new Mesh(gl, vertices, indices, VertexLayout.PositionNormalColor);
    }

    public static Mesh CreateCube(GL gl, float size, float r, float g, float b)
    {
        float h = size / 2f;
        var verts = new List<float>();
        var inds = new List<uint>();

        void AddFace(float nx, float ny, float nz,
            float ax, float ay, float az, float bx, float by, float bz)
        {
            uint baseIdx = (uint)(verts.Count / 9);
            float cx = nx * h, cy = ny * h, cz = nz * h;

            for (int i = -1; i <= 1; i += 2)
            for (int j = -1; j <= 1; j += 2)
            {
                verts.AddRange(new[]
                {
                    cx + ax * i * h + bx * j * h,
                    cy + ay * i * h + by * j * h,
                    cz + az * i * h + bz * j * h,
                    nx, ny, nz, r, g, b
                });
            }
            inds.AddRange(new[] { baseIdx, baseIdx + 1, baseIdx + 3, baseIdx, baseIdx + 3, baseIdx + 2 });
        }

        AddFace( 0,  1,  0,  1, 0, 0,  0, 0, 1); // top
        AddFace( 0, -1,  0,  1, 0, 0,  0, 0, 1); // bottom
        AddFace( 1,  0,  0,  0, 1, 0,  0, 0, 1); // right
        AddFace(-1,  0,  0,  0, 1, 0,  0, 0, 1); // left
        AddFace( 0,  0,  1,  1, 0, 0,  0, 1, 0); // front
        AddFace( 0,  0, -1,  1, 0, 0,  0, 1, 0); // back

        return new Mesh(gl, verts.ToArray(), inds.ToArray(), VertexLayout.PositionNormalColor);
    }

    public static Mesh CreateCylinder(GL gl, float radius, float height, int segments,
        float r, float g, float b)
    {
        var verts = new List<float>();
        var inds = new List<uint>();
        float halfH = height / 2f;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * MathF.Tau / segments;
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            float nx = cos, nz = sin;

            // Bottom vertex
            verts.AddRange(new[] { cos * radius, -halfH, sin * radius, nx, 0f, nz, r, g, b });
            // Top vertex
            verts.AddRange(new[] { cos * radius,  halfH, sin * radius, nx, 0f, nz, r, g, b });
        }

        for (uint i = 0; i < (uint)segments; i++)
        {
            uint b0 = i * 2, b1 = (i + 1) * 2;
            inds.AddRange(new[] { b0, b1, b1 + 1, b0, b1 + 1, b0 + 1 });
        }

        return new Mesh(gl, verts.ToArray(), inds.ToArray(), VertexLayout.PositionNormalColor);
    }

    public static Mesh CreateCone(GL gl, float radius, float height, int segments,
        float r, float g, float b)
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        // Apex
        verts.AddRange(new[] { 0f, height, 0f, 0f, 1f, 0f, r, g, b });

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * MathF.Tau / segments;
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);

            // Normal points outward and up
            float ny = radius / MathF.Sqrt(radius * radius + height * height);
            float nxz = height / MathF.Sqrt(radius * radius + height * height);

            verts.AddRange(new[] { cos * radius, 0f, sin * radius, cos * nxz, ny, sin * nxz, r, g, b });
        }

        for (uint i = 1; i <= (uint)segments; i++)
        {
            inds.AddRange(new[] { 0u, i, i + 1 });
        }

        return new Mesh(gl, verts.ToArray(), inds.ToArray(), VertexLayout.PositionNormalColor);
    }

    public static Mesh CreateCapsule(GL gl, float radius, float height, int segments,
        float r, float g, float b)
    {
        // Simplified: cylinder body + hemisphere top/bottom approximated as extra rings
        var verts = new List<float>();
        var inds = new List<uint>();
        float bodyH = height - 2 * radius;
        int rings = 8;

        // Generate rings from bottom hemisphere through cylinder to top hemisphere
        int totalRings = rings * 2 + 2; // hemisphere bottom + cylinder + hemisphere top
        for (int ring = 0; ring <= totalRings; ring++)
        {
            float t = (float)ring / totalRings;
            float y, ringRadius;

            if (t < 0.25f) // bottom hemisphere
            {
                float angle = (t / 0.25f) * MathF.PI / 2f;
                y = -bodyH / 2f - radius * MathF.Cos(angle);
                ringRadius = radius * MathF.Sin(angle);
            }
            else if (t < 0.75f) // cylinder
            {
                y = -bodyH / 2f + bodyH * ((t - 0.25f) / 0.5f);
                ringRadius = radius;
            }
            else // top hemisphere
            {
                float angle = ((t - 0.75f) / 0.25f) * MathF.PI / 2f;
                y = bodyH / 2f + radius * MathF.Sin(angle);
                ringRadius = radius * MathF.Cos(angle);
            }

            for (int seg = 0; seg <= segments; seg++)
            {
                float angle = seg * MathF.Tau / segments;
                float cos = MathF.Cos(angle);
                float sin = MathF.Sin(angle);

                float px = cos * ringRadius;
                float pz = sin * ringRadius;
                float nx = cos, nz = sin;

                verts.AddRange(new[] { px, y, pz, nx, 0f, nz, r, g, b });
            }
        }

        int vertsPerRing = segments + 1;
        for (int ring = 0; ring < totalRings; ring++)
        {
            for (int seg = 0; seg < segments; seg++)
            {
                uint tl = (uint)(ring * vertsPerRing + seg);
                uint tr = tl + 1;
                uint bl = (uint)((ring + 1) * vertsPerRing + seg);
                uint br = bl + 1;
                inds.AddRange(new[] { tl, bl, br, tl, br, tr });
            }
        }

        return new Mesh(gl, verts.ToArray(), inds.ToArray(), VertexLayout.PositionNormalColor);
    }

    /// <summary>
    /// Returns capsule vertex/index data without uploading to GPU.
    /// </summary>
    public static MeshData CreateCapsuleData(float radius, float height, int segments,
        float r, float g, float b)
    {
        var verts = new List<float>();
        var inds = new List<uint>();
        float bodyH = height - 2 * radius;
        int rings = 8;
        int totalRings = rings * 2 + 2;

        for (int ring = 0; ring <= totalRings; ring++)
        {
            float t = (float)ring / totalRings;
            float y, ringRadius;

            if (t < 0.25f)
            {
                float angle = (t / 0.25f) * MathF.PI / 2f;
                y = -bodyH / 2f - radius * MathF.Cos(angle);
                ringRadius = radius * MathF.Sin(angle);
            }
            else if (t < 0.75f)
            {
                y = -bodyH / 2f + bodyH * ((t - 0.25f) / 0.5f);
                ringRadius = radius;
            }
            else
            {
                float angle = ((t - 0.75f) / 0.25f) * MathF.PI / 2f;
                y = bodyH / 2f + radius * MathF.Sin(angle);
                ringRadius = radius * MathF.Cos(angle);
            }

            for (int seg = 0; seg <= segments; seg++)
            {
                float angle2 = seg * MathF.Tau / segments;
                float cos = MathF.Cos(angle2);
                float sin = MathF.Sin(angle2);
                verts.AddRange(new[] { cos * ringRadius, y, sin * ringRadius, cos, 0f, sin, r, g, b });
            }
        }

        int vertsPerRing = segments + 1;
        for (int ring = 0; ring < totalRings; ring++)
        {
            for (int seg = 0; seg < segments; seg++)
            {
                uint tl = (uint)(ring * vertsPerRing + seg);
                uint tr = tl + 1;
                uint bl = (uint)((ring + 1) * vertsPerRing + seg);
                uint br = bl + 1;
                inds.AddRange(new[] { tl, bl, br, tl, br, tr });
            }
        }

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    /// <summary>
    /// Returns cube vertex/index data without uploading to GPU.
    /// </summary>
    public static MeshData CreateCubeData(float r, float g, float b)
    {
        float h = 0.5f;
        var verts = new List<float>();
        var inds = new List<uint>();

        void AddFace(float nx, float ny, float nz,
            float ax, float ay, float az, float bx, float by, float bz)
        {
            uint baseIdx = (uint)(verts.Count / 9);
            float cx = nx * h, cy = ny * h, cz = nz * h;
            for (int i = -1; i <= 1; i += 2)
            for (int j = -1; j <= 1; j += 2)
            {
                verts.AddRange(new[]
                {
                    cx + ax * i * h + bx * j * h,
                    cy + ay * i * h + by * j * h,
                    cz + az * i * h + bz * j * h,
                    nx, ny, nz, r, g, b
                });
            }
            inds.AddRange(new[] { baseIdx, baseIdx + 1, baseIdx + 3, baseIdx, baseIdx + 3, baseIdx + 2 });
        }

        AddFace( 0,  1,  0,  1, 0, 0,  0, 0, 1);
        AddFace( 0, -1,  0,  1, 0, 0,  0, 0, 1);
        AddFace( 1,  0,  0,  0, 1, 0,  0, 0, 1);
        AddFace(-1,  0,  0,  0, 1, 0,  0, 0, 1);
        AddFace( 0,  0,  1,  1, 0, 0,  0, 1, 0);
        AddFace( 0,  0, -1,  1, 0, 0,  0, 1, 0);

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    /// <summary>
    /// Full-screen quad for sky rendering (2D positions in clip space).
    /// </summary>
    public static Mesh CreateFullScreenQuad(GL gl)
    {
        float[] vertices = { -1, -1,  1, -1,  1, 1,  -1, 1 };
        uint[] indices = { 0, 1, 2, 0, 2, 3 };
        return new Mesh(gl, vertices, indices, VertexLayout.Position2D);
    }

    /// <summary>
    /// Unit quad for HUD rendering (0,0 to 1,1).
    /// </summary>
    public static Mesh CreateUnitQuad(GL gl)
    {
        float[] vertices = { 0, 0,  1, 0,  1, 1,  0, 1 };
        uint[] indices = { 0, 1, 2, 0, 2, 3 };
        return new Mesh(gl, vertices, indices, VertexLayout.Position2D);
    }
}
