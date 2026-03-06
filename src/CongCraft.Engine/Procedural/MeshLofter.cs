namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates connected mesh surfaces by lofting between cross-section profiles.
/// This produces smooth, continuous character meshes like Gothic 2 / SpellForce 1 / Warcraft 3,
/// instead of separate geometric primitives stuck together.
///
/// Each cross-section (ring) defines a shape at a Y-height, and the lofter connects
/// adjacent rings with smooth triangle strips to form a seamless surface.
/// </summary>
public static class MeshLofter
{
    /// <summary>
    /// Defines a single cross-section ring of a lofted mesh.
    /// </summary>
    public struct Ring
    {
        public float Y;          // Height along the body
        public float RadiusX;    // Horizontal radius (left-right)
        public float RadiusZ;    // Depth radius (front-back)
        public float OffsetX;    // Ring center offset X
        public float OffsetZ;    // Ring center offset Z
        public float R, G, B;    // Vertex color
    }

    /// <summary>
    /// Generate a lofted mesh from an array of rings. Rings are connected top-to-bottom
    /// with smooth triangle strips. Normals are computed per-vertex for smooth shading.
    /// </summary>
    public static void Loft(List<float> verts, List<uint> inds, Ring[] rings, int segments,
        bool capBottom = true, bool capTop = true)
    {
        if (rings.Length < 2) return;
        uint baseIdx = (uint)(verts.Count / 9);

        // Generate vertices for each ring
        for (int r = 0; r < rings.Length; r++)
        {
            var ring = rings[r];
            for (int s = 0; s <= segments; s++)
            {
                float angle = s * MathF.Tau / segments;
                float cos = MathF.Cos(angle);
                float sin = MathF.Sin(angle);

                float px = ring.OffsetX + cos * ring.RadiusX;
                float py = ring.Y;
                float pz = ring.OffsetZ + sin * ring.RadiusZ;

                // Compute smooth normal by considering adjacent rings for Y-component
                float nx, ny, nz;
                if (ring.RadiusX < 0.001f && ring.RadiusZ < 0.001f)
                {
                    // Degenerate ring (pole) - point normal outward based on neighbors
                    ny = r == 0 ? -1f : 1f;
                    nx = cos * 0.3f;
                    nz = sin * 0.3f;
                }
                else
                {
                    // Normal from ellipse shape
                    nx = cos / MathF.Max(ring.RadiusX, 0.001f);
                    nz = sin / MathF.Max(ring.RadiusZ, 0.001f);

                    // Add Y-component from slope between adjacent rings
                    ny = 0f;
                    if (r > 0 && r < rings.Length - 1)
                    {
                        float drPrev = (ring.RadiusX + ring.RadiusZ) * 0.5f -
                                       (rings[r - 1].RadiusX + rings[r - 1].RadiusZ) * 0.5f;
                        float dyPrev = ring.Y - rings[r - 1].Y;
                        float drNext = (rings[r + 1].RadiusX + rings[r + 1].RadiusZ) * 0.5f -
                                       (ring.RadiusX + ring.RadiusZ) * 0.5f;
                        float dyNext = rings[r + 1].Y - ring.Y;

                        float slopePrev = dyPrev > 0.0001f ? -drPrev / dyPrev : 0f;
                        float slopeNext = dyNext > 0.0001f ? -drNext / dyNext : 0f;
                        ny = (slopePrev + slopeNext) * 0.5f;
                    }
                    else if (r == 0 && rings.Length > 1)
                    {
                        float dr = (rings[1].RadiusX + rings[1].RadiusZ) * 0.5f -
                                   (ring.RadiusX + ring.RadiusZ) * 0.5f;
                        float dy = rings[1].Y - ring.Y;
                        ny = dy > 0.0001f ? -dr / dy : -0.5f;
                    }
                    else if (r == rings.Length - 1 && rings.Length > 1)
                    {
                        float dr = (ring.RadiusX + ring.RadiusZ) * 0.5f -
                                   (rings[r - 1].RadiusX + rings[r - 1].RadiusZ) * 0.5f;
                        float dy = ring.Y - rings[r - 1].Y;
                        ny = dy > 0.0001f ? -dr / dy : 0.5f;
                    }
                }

                float len = MathF.Sqrt(nx * nx + ny * ny + nz * nz);
                if (len > 0.0001f) { nx /= len; ny /= len; nz /= len; }
                else { nx = cos; ny = 0; nz = sin; }

                verts.AddRange(new[] { px, py, pz, nx, ny, nz, ring.R, ring.G, ring.B });
            }
        }

        // Connect adjacent rings with triangle strips
        int vertsPerRing = segments + 1;
        for (int r = 0; r < rings.Length - 1; r++)
        {
            for (int s = 0; s < segments; s++)
            {
                uint tl = baseIdx + (uint)(r * vertsPerRing + s);
                uint tr = tl + 1;
                uint bl = baseIdx + (uint)((r + 1) * vertsPerRing + s);
                uint br = bl + 1;
                inds.AddRange(new[] { tl, bl, br, tl, br, tr });
            }
        }

        // Optional caps
        if (capBottom && rings[0].RadiusX > 0.001f)
        {
            AddCap(verts, inds, rings[0], segments, false);
        }
        if (capTop && rings[^1].RadiusX > 0.001f)
        {
            AddCap(verts, inds, rings[^1], segments, true);
        }
    }

    /// <summary>
    /// Loft a limb (arm/leg) that branches from a body at a given position.
    /// The limb is a smooth tube defined by rings, attached at an angle.
    /// </summary>
    public static void LoftLimb(List<float> verts, List<uint> inds,
        float attachX, float attachY, float attachZ,
        Ring[] rings, int segments, float angleX = 0f, float angleZ = 0f)
    {
        if (rings.Length < 2) return;

        // Rotate and translate rings
        float cosX = MathF.Cos(angleX);
        float sinX = MathF.Sin(angleX);
        float cosZ = MathF.Cos(angleZ);
        float sinZ = MathF.Sin(angleZ);

        var transformed = new Ring[rings.Length];
        for (int i = 0; i < rings.Length; i++)
        {
            var r = rings[i];
            // Local position along limb axis
            float ly = r.Y;
            float lx = r.OffsetX;
            float lz = r.OffsetZ;

            // Rotate around X (for spreading arms out)
            float ry = ly * cosX - lz * sinX;
            float rz = ly * sinX + lz * cosX;
            float rx = lx;

            // Rotate around Z (for angling forward/back)
            float fy = ry * cosZ - rx * sinZ;
            float fx = ry * sinZ + rx * cosZ;

            transformed[i] = new Ring
            {
                Y = attachY + fy,
                OffsetX = attachX + fx,
                OffsetZ = attachZ + rz,
                RadiusX = r.RadiusX,
                RadiusZ = r.RadiusZ,
                R = r.R, G = r.G, B = r.B
            };
        }

        Loft(verts, inds, transformed, segments, capBottom: true, capTop: true);
    }

    private static void AddCap(List<float> verts, List<uint> inds, Ring ring, int segments, bool top)
    {
        uint centerIdx = (uint)(verts.Count / 9);
        float ny = top ? 1f : -1f;

        // Center vertex
        verts.AddRange(new[] { ring.OffsetX, ring.Y, ring.OffsetZ, 0f, ny, 0f, ring.R, ring.G, ring.B });

        // Edge vertices
        for (int s = 0; s <= segments; s++)
        {
            float angle = s * MathF.Tau / segments;
            float px = ring.OffsetX + MathF.Cos(angle) * ring.RadiusX;
            float pz = ring.OffsetZ + MathF.Sin(angle) * ring.RadiusZ;
            verts.AddRange(new[] { px, ring.Y, pz, 0f, ny, 0f, ring.R, ring.G, ring.B });
        }

        // Fan triangles
        for (uint s = 0; s < (uint)segments; s++)
        {
            if (top)
                inds.AddRange(new[] { centerIdx, centerIdx + 1 + s, centerIdx + 2 + s });
            else
                inds.AddRange(new[] { centerIdx, centerIdx + 2 + s, centerIdx + 1 + s });
        }
    }

    /// <summary>
    /// Helper to build a ring with equal X/Z radius.
    /// </summary>
    public static Ring R(float y, float radius, float r, float g, float b,
        float offsetX = 0, float offsetZ = 0)
    {
        return new Ring
        {
            Y = y, RadiusX = radius, RadiusZ = radius,
            OffsetX = offsetX, OffsetZ = offsetZ,
            R = r, G = g, B = b
        };
    }

    /// <summary>
    /// Helper to build a ring with different X/Z radii (elliptical).
    /// </summary>
    public static Ring RE(float y, float radiusX, float radiusZ, float r, float g, float b,
        float offsetX = 0, float offsetZ = 0)
    {
        return new Ring
        {
            Y = y, RadiusX = radiusX, RadiusZ = radiusZ,
            OffsetX = offsetX, OffsetZ = offsetZ,
            R = r, G = g, B = b
        };
    }

    /// <summary>
    /// Adds a separate sphere primitive (for small detail parts like eyes, studs, buckles).
    /// </summary>
    public static void AddSphere(List<float> verts, List<uint> inds,
        float cx, float cy, float cz, float radius, int segments,
        float r, float g, float b)
    {
        uint baseIdx = (uint)(verts.Count / 9);
        int rings = segments / 2;

        for (int ring = 0; ring <= rings; ring++)
        {
            float phi = MathF.PI * ring / rings;
            float cosPhi = MathF.Cos(phi);
            float sinPhi = MathF.Sin(phi);

            for (int seg = 0; seg <= segments; seg++)
            {
                float theta = MathF.Tau * seg / segments;
                float cosTheta = MathF.Cos(theta);
                float sinTheta = MathF.Sin(theta);

                float nx = sinPhi * cosTheta;
                float ny = cosPhi;
                float nz = sinPhi * sinTheta;

                float px = cx + nx * radius;
                float py = cy + ny * radius;
                float pz = cz + nz * radius;

                verts.AddRange(new[] { px, py, pz, nx, ny, nz, r, g, b });
            }
        }

        int vertsPerRing = segments + 1;
        for (int ring = 0; ring < rings; ring++)
        {
            for (int seg = 0; seg < segments; seg++)
            {
                uint tl = baseIdx + (uint)(ring * vertsPerRing + seg);
                uint tr = tl + 1;
                uint bl = baseIdx + (uint)((ring + 1) * vertsPerRing + seg);
                uint br = bl + 1;
                inds.AddRange(new[] { tl, bl, br, tl, br, tr });
            }
        }
    }

    /// <summary>
    /// Adds a cylinder primitive for small detail parts.
    /// </summary>
    public static void AddCylinder(List<float> verts, List<uint> inds,
        float cx, float cy, float cz, float radius, float height, int segments,
        float r, float g, float b)
    {
        uint baseIdx = (uint)(verts.Count / 9);
        float halfH = height / 2f;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * MathF.Tau / segments;
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);

            verts.AddRange(new[] { cx + cos * radius, cy - halfH, cz + sin * radius, cos, 0f, sin, r, g, b });
            verts.AddRange(new[] { cx + cos * radius, cy + halfH, cz + sin * radius, cos, 0f, sin, r, g, b });
        }

        for (uint i = 0; i < (uint)segments; i++)
        {
            uint b0 = baseIdx + i * 2;
            uint b1 = baseIdx + (i + 1) * 2;
            inds.AddRange(new[] { b0, b1, b1 + 1, b0, b1 + 1, b0 + 1 });
        }
    }

    /// <summary>
    /// Adds a box primitive for flat details (belt plates, sword blades, etc).
    /// </summary>
    public static void AddBox(List<float> verts, List<uint> inds,
        float cx, float cy, float cz, float sx, float sy, float sz,
        float r, float g, float b)
    {
        uint baseIdx = (uint)(verts.Count / 9);
        float hx = sx / 2f, hy = sy / 2f, hz = sz / 2f;

        // 6 faces, 4 verts each
        void Face(float nx, float ny, float nz,
            float ax, float ay, float az, float bx, float by, float bz)
        {
            uint bi = (uint)(verts.Count / 9);
            for (int i = -1; i <= 1; i += 2)
            for (int j = -1; j <= 1; j += 2)
            {
                verts.AddRange(new[]
                {
                    cx + nx * (nx > 0 ? hx : nx < 0 ? -hx : 0) + ax * i * hx + bx * j * hz,
                    cy + ny * (ny > 0 ? hy : ny < 0 ? -hy : 0) + ay * i * hy + by * j * hz,
                    cz + nz * (nz > 0 ? hz : nz < 0 ? -hz : 0) + az * i * hx + bz * j * hz,
                    nx, ny, nz, r, g, b
                });
            }
            inds.AddRange(new[] { bi, bi + 1, bi + 3, bi, bi + 3, bi + 2 });
        }

        Face(0, 1, 0, 1, 0, 0, 0, 0, 1);   // top
        Face(0, -1, 0, 1, 0, 0, 0, 0, 1);   // bottom
        Face(1, 0, 0, 0, 1, 0, 0, 0, 1);    // right
        Face(-1, 0, 0, 0, 1, 0, 0, 0, 1);   // left
        Face(0, 0, 1, 1, 0, 0, 0, 1, 0);    // front
        Face(0, 0, -1, 1, 0, 0, 0, 1, 0);   // back
    }
}
