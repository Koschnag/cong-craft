using Silk.NET.OpenGL;
using CongCraft.Engine.Rendering;
using static CongCraft.Engine.Procedural.MeshLofter;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates tree meshes with lofted trunk, branches, and layered canopy.
/// Connected, smooth surfaces — Gothic 2 / SpellForce 1 style organic trees.
/// </summary>
public static class TreeMeshBuilder
{
    public record TreeParams(
        float TrunkRadius = 0.15f,
        float TrunkHeight = 2.5f,
        float CrownRadius = 1.5f,
        float CrownHeight = 3.5f,
        int Segments = 14
    );

    // Bark colors (warm brown, darker at base)
    private const float BarkDR = 0.28f, BarkDG = 0.17f, BarkDB = 0.07f; // dark bark (base)
    private const float BarkR = 0.35f, BarkG = 0.22f, BarkB = 0.10f;    // main bark
    private const float BarkLR = 0.38f, BarkLG = 0.25f, BarkLB = 0.12f; // light bark (top)

    // Foliage colors (varied greens)
    private const float LeafDR = 0.07f, LeafDG = 0.22f, LeafDB = 0.04f; // dark leaves (interior/bottom)
    private const float LeafR = 0.10f, LeafG = 0.30f, LeafB = 0.06f;    // main leaves
    private const float LeafLR = 0.14f, LeafLG = 0.38f, LeafLB = 0.10f; // light leaves (top/sun)

    public static MeshData GenerateData(TreeParams? p = null)
    {
        p ??= new TreeParams();
        var verts = new List<float>();
        var inds = new List<uint>();
        int seg = p.Segments;
        int bSeg = Math.Max(8, seg - 2); // branch segments (slightly fewer)

        float tr = p.TrunkRadius;
        float th = p.TrunkHeight;
        float cr = p.CrownRadius;

        // === TRUNK: Lofted from root flare through tapered shaft to branch point ===
        var trunkRings = new Ring[]
        {
            // Root flare (wide base for stability)
            R(0.0f, tr * 1.8f, BarkDR, BarkDG, BarkDB),
            R(th * 0.03f, tr * 1.5f, BarkDR, BarkDG, BarkDB),
            R(th * 0.08f, tr * 1.2f, BarkR - 0.03f, BarkG - 0.02f, BarkB),
            // Main trunk shaft
            R(th * 0.15f, tr * 1.05f, BarkR, BarkG, BarkB),
            R(th * 0.35f, tr * 1.0f, BarkR, BarkG, BarkB),
            // Slight bulge mid-trunk (natural growth ring)
            R(th * 0.50f, tr * 1.05f, BarkR + 0.01f, BarkG + 0.01f, BarkB),
            R(th * 0.65f, tr * 0.98f, BarkR, BarkG, BarkB),
            // Taper toward crown
            R(th * 0.80f, tr * 0.90f, BarkLR, BarkLG, BarkLB),
            R(th * 0.90f, tr * 0.80f, BarkLR, BarkLG, BarkLB),
            // Top of trunk (where branches start)
            R(th * 1.0f, tr * 0.65f, BarkLR, BarkLG, BarkLB),
        };
        Loft(verts, inds, trunkRings, seg, capBottom: true, capTop: false);

        // === ROOT BUMPS: Small lofted root protrusions ===
        float[] rootAngles = { 0f, MathF.Tau * 0.33f, MathF.Tau * 0.7f };
        foreach (float angle in rootAngles)
        {
            float rx = MathF.Cos(angle) * tr * 1.1f;
            float rz = MathF.Sin(angle) * tr * 1.1f;
            var rootRings = new Ring[]
            {
                R(0.0f, tr * 0.45f, BarkDR, BarkDG, BarkDB, rx * 0.5f, rz * 0.5f),
                R(-0.02f, tr * 0.35f, BarkDR, BarkDG, BarkDB, rx * 0.8f, rz * 0.8f),
                R(0.02f, tr * 0.20f, BarkDR, BarkDG, BarkDB, rx * 1.2f, rz * 1.2f),
                R(0.05f, tr * 0.08f, BarkDR, BarkDG, BarkDB, rx * 1.5f, rz * 1.5f),
            };
            Loft(verts, inds, rootRings, 6, capBottom: true, capTop: true);
        }

        // === MAIN BRANCHES: Lofted limbs extending into the canopy ===
        float branchY = th * 0.78f;

        // Branch 1: Forward-right, angled up
        var branch1 = new Ring[]
        {
            R(0.0f, tr * 0.40f, BarkR, BarkG, BarkB),
            R(th * 0.12f, tr * 0.35f, BarkR, BarkG, BarkB),
            R(th * 0.25f, tr * 0.28f, BarkLR, BarkLG, BarkLB),
            R(th * 0.38f, tr * 0.20f, BarkLR, BarkLG, BarkLB),
            R(th * 0.48f, tr * 0.12f, BarkLR, BarkLG, BarkLB),
        };
        LoftLimb(verts, inds, tr * 0.3f, branchY, tr * 0.1f,
            branch1, bSeg, angleX: -0.3f, angleZ: 0.6f);

        // Branch 2: Left-back, angled up
        var branch2 = new Ring[]
        {
            R(0.0f, tr * 0.38f, BarkR, BarkG, BarkB),
            R(th * 0.10f, tr * 0.32f, BarkR, BarkG, BarkB),
            R(th * 0.22f, tr * 0.25f, BarkLR, BarkLG, BarkLB),
            R(th * 0.35f, tr * 0.18f, BarkLR, BarkLG, BarkLB),
            R(th * 0.42f, tr * 0.10f, BarkLR, BarkLG, BarkLB),
        };
        LoftLimb(verts, inds, -tr * 0.4f, branchY + 0.15f, -tr * 0.2f,
            branch2, bSeg, angleX: 0.35f, angleZ: 0.55f);

        // Branch 3: Right-back, shorter
        var branch3 = new Ring[]
        {
            R(0.0f, tr * 0.30f, BarkR, BarkG, BarkB),
            R(th * 0.08f, tr * 0.25f, BarkR, BarkG, BarkB),
            R(th * 0.18f, tr * 0.18f, BarkLR, BarkLG, BarkLB),
            R(th * 0.28f, tr * 0.10f, BarkLR, BarkLG, BarkLB),
        };
        LoftLimb(verts, inds, tr * 0.2f, branchY + 0.3f, -tr * 0.3f,
            branch3, bSeg, angleX: 0.2f, angleZ: 0.7f);

        // === CANOPY: Overlapping lofted leaf clusters for organic volume ===
        float baseY = th;

        // Main central canopy mass — large lofted sphere-like shape
        AddCanopyCluster(verts, inds, 0f, baseY + cr * 0.7f, 0f,
            cr * 0.85f, cr * 1.0f, seg,
            LeafR, LeafG, LeafB, LeafDR, LeafDG, LeafDB);

        // Upper crown (lighter green, catches sunlight)
        AddCanopyCluster(verts, inds, 0f, baseY + cr * 1.2f, 0f,
            cr * 0.55f, cr * 0.65f, seg - 2,
            LeafLR, LeafLG, LeafLB, LeafR, LeafG, LeafB);

        // Side clusters for volume
        AddCanopyCluster(verts, inds, cr * 0.45f, baseY + cr * 0.5f, 0.05f,
            cr * 0.50f, cr * 0.55f, bSeg,
            LeafDR + 0.01f, LeafDG + 0.04f, LeafDB + 0.01f,
            LeafDR, LeafDG, LeafDB);

        AddCanopyCluster(verts, inds, -cr * 0.35f, baseY + cr * 0.55f, cr * 0.3f,
            cr * 0.48f, cr * 0.50f, bSeg,
            LeafR, LeafG + 0.02f, LeafB,
            LeafDR + 0.01f, LeafDG + 0.02f, LeafDB);

        AddCanopyCluster(verts, inds, 0.05f, baseY + cr * 0.4f, -cr * 0.38f,
            cr * 0.45f, cr * 0.50f, bSeg,
            LeafDR + 0.02f, LeafDG + 0.03f, LeafDB + 0.01f,
            LeafDR, LeafDG, LeafDB);

        // Lower hanging foliage (darker, denser)
        AddCanopyCluster(verts, inds, cr * 0.25f, baseY + cr * 0.2f, cr * 0.3f,
            cr * 0.35f, cr * 0.32f, 8,
            LeafDR, LeafDG, LeafDB, LeafDR, LeafDG - 0.02f, LeafDB);

        AddCanopyCluster(verts, inds, -cr * 0.3f, baseY + cr * 0.25f, -cr * 0.25f,
            cr * 0.32f, cr * 0.30f, 8,
            LeafDR + 0.01f, LeafDG + 0.01f, LeafDB,
            LeafDR, LeafDG - 0.01f, LeafDB);

        // Topmost tuft (bright green accent)
        AddCanopyCluster(verts, inds, 0f, baseY + cr * 1.5f, 0f,
            cr * 0.30f, cr * 0.28f, 8,
            LeafLR, LeafLG, LeafLB, LeafR, LeafG, LeafB);

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    /// <summary>
    /// Generates a canopy cluster as a lofted oblate/prolate shape — connected surface
    /// rather than a primitive sphere. Uses ring cross-sections from bottom to top.
    /// </summary>
    private static void AddCanopyCluster(List<float> verts, List<uint> inds,
        float cx, float cy, float cz,
        float radiusH, float radiusV, int segments,
        float topR, float topG, float topB,
        float bottomR, float bottomG, float bottomB)
    {
        int stacks = 6; // number of ring cross-sections
        var rings = new Ring[stacks + 1];

        for (int i = 0; i <= stacks; i++)
        {
            float t = (float)i / stacks; // 0 = bottom, 1 = top
            float phi = MathF.PI * t;
            float sinPhi = MathF.Sin(phi);
            float cosPhi = MathF.Cos(phi);

            float radius = radiusH * sinPhi;
            float y = cy + radiusV * cosPhi; // from +V (bottom) to -V (top) but we flip

            // Blend color from bottom to top
            float cr = bottomR + (topR - bottomR) * t;
            float cg = bottomG + (topG - bottomG) * t;
            float cb = bottomB + (topB - bottomB) * t;

            // Slight irregular shape — flatten bottom, round top
            float irregularity = 1.0f + 0.08f * MathF.Sin(t * MathF.PI * 3f);
            radius *= irregularity;

            rings[i] = R(y, Math.Max(radius, 0.001f), cr, cg, cb, cx, cz);
        }

        // Make poles very small for clean cap
        rings[0] = R(cy + radiusV, 0.01f, topR, topG, topB, cx, cz);
        rings[stacks] = R(cy - radiusV, 0.01f, bottomR, bottomG, bottomB, cx, cz);

        Loft(verts, inds, rings, segments, capBottom: false, capTop: false);
    }

    public static Mesh Create(GL gl, TreeParams? p = null)
    {
        var data = GenerateData(p);
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
    }
}

public record MeshData(float[] Vertices, uint[] Indices)
{
    public int VertexCount => Vertices.Length / 9; // 9 floats per vertex (pos + normal + color)
}
