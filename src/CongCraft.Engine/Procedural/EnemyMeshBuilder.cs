using Silk.NET.OpenGL;
using CongCraft.Engine.Rendering;
using static CongCraft.Engine.Procedural.MeshLofter;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates Gothic-style enemy warrior mesh using lofted connected surfaces.
/// Dark steel armor, heavy build. Also includes skeleton variant.
/// </summary>
public static class EnemyMeshBuilder
{
    private const int Seg = 14;

    // Dark steel armor colors
    private const float AR = 0.28f, AG = 0.25f, AB = 0.24f;   // armor base
    private const float DR = 0.22f, DG = 0.20f, DB = 0.18f;   // dark armor
    private const float MR = 0.35f, MG = 0.30f, MB = 0.28f;   // metal accent
    private const float SR = 0.55f, SG = 0.42f, SB = 0.38f;   // skin

    public static MeshData GenerateData()
    {
        var v = new List<float>();
        var idx = new List<uint>();

        // ═══ TORSO — heavy dark armor, connected loft ═══
        Loft(v, idx, new[]
        {
            RE(0.38f, 0.20f, 0.14f, AR, AG, AB),       // waist
            RE(0.48f, 0.23f, 0.16f, AR, AG, AB),       // lower abdomen
            RE(0.60f, 0.26f, 0.18f, AR, AG, AB),       // upper abdomen
            RE(0.72f, 0.28f, 0.19f, AR, AG, AB),       // chest (wide, armored)
            RE(0.84f, 0.26f, 0.17f, AR, AG, AB),       // upper chest
            RE(0.94f, 0.22f, 0.14f, AR, AG, AB),       // shoulder line
            RE(1.02f, 0.16f, 0.10f, DR, DG, DB),       // neck base
            RE(1.08f, 0.08f, 0.07f, SR, SG, SB),       // neck
            RE(1.14f, 0.07f, 0.065f, SR, SG, SB),      // neck top
        }, Seg, capBottom: true, capTop: false);

        // ═══ HEAD + HELMET — connected loft ═══
        Loft(v, idx, new[]
        {
            R(1.14f, 0.08f, SR, SG, SB),                // chin
            RE(1.18f, 0.11f, 0.10f, SR, SG, SB),        // jaw
            RE(1.22f, 0.13f, 0.12f, DR, DG, DB),        // helmet lower
            RE(1.28f, 0.14f, 0.14f, DR, DG, DB),        // helmet mid
            RE(1.34f, 0.135f, 0.14f, DR, DG, DB),       // helmet upper
            RE(1.38f, 0.12f, 0.13f, AR, AG, AB),        // helmet top
            R(1.42f, 0.08f, AR, AG, AB),                 // helmet crown
            R(1.46f, 0.03f, 0.6f, 0.15f, 0.1f),         // crest base (red)
            R(1.54f, 0.02f, 0.6f, 0.15f, 0.1f),         // crest tip
            R(1.56f, 0.00f, 0.6f, 0.15f, 0.1f),         // crest end
        }, Seg, capBottom: false, capTop: false);

        // Visor slit
        MeshLofter.AddCylinder(v, idx, 0, 1.27f, 0.14f, 0.06f, 0.012f, 6, 0.06f, 0.05f, 0.05f);

        // ═══ LEFT ARM — heavy armor ═══
        LoftLimb(v, idx, -0.26f, 0.94f, 0, new[]
        {
            R(0.00f, 0.11f, MR, MG, MB),      // shoulder cap
            R(-0.08f, 0.09f, AR, AG, AB),      // upper arm top
            R(-0.22f, 0.07f, AR, AG, AB),      // upper arm mid
            R(-0.34f, 0.065f, AR, AG, AB),     // elbow
            R(-0.44f, 0.06f, DR, DG, DB),      // forearm
            R(-0.52f, 0.065f, DR, DG, DB),     // gauntlet
            R(-0.58f, 0.06f, DR, DG, DB),      // gauntlet end
            R(-0.62f, 0.04f, MR, MG, MB),      // wrist
            R(-0.66f, 0.02f, MR, MG, MB),      // fist
            R(-0.68f, 0.00f, MR, MG, MB),
        }, Seg / 2 + 2, angleX: -0.2f, angleZ: 0.08f);

        // ═══ RIGHT ARM — mirrored ═══
        LoftLimb(v, idx, 0.26f, 0.94f, 0, new[]
        {
            R(0.00f, 0.11f, MR, MG, MB),
            R(-0.08f, 0.09f, AR, AG, AB),
            R(-0.22f, 0.07f, AR, AG, AB),
            R(-0.34f, 0.065f, AR, AG, AB),
            R(-0.44f, 0.06f, DR, DG, DB),
            R(-0.52f, 0.065f, DR, DG, DB),
            R(-0.58f, 0.06f, DR, DG, DB),
            R(-0.62f, 0.04f, MR, MG, MB),
            R(-0.66f, 0.02f, MR, MG, MB),
            R(-0.68f, 0.00f, MR, MG, MB),
        }, Seg / 2 + 2, angleX: 0.2f, angleZ: 0.08f);

        // Shoulder spikes
        MeshLofter.AddCylinder(v, idx, -0.30f, 1.06f, 0, 0.02f, 0.10f, 6, MR, MG, MB);
        MeshLofter.AddCylinder(v, idx, 0.30f, 1.06f, 0, 0.02f, 0.10f, 6, MR, MG, MB);
        // Shoulder guards
        MeshLofter.AddSphere(v, idx, -0.28f, 1.00f, 0, 0.10f, 10, AR + 0.04f, AG + 0.03f, AB + 0.02f);
        MeshLofter.AddSphere(v, idx, 0.28f, 1.00f, 0, 0.10f, 10, AR + 0.04f, AG + 0.03f, AB + 0.02f);

        // ═══ LEFT LEG — heavy armor ═══
        LoftLimb(v, idx, -0.11f, 0.38f, 0, new[]
        {
            R(0.00f, 0.10f, DR, DG, DB),       // hip
            R(-0.10f, 0.09f, DR, DG, DB),      // upper thigh
            R(-0.20f, 0.08f, DR, DG, DB),      // mid thigh
            R(-0.30f, 0.07f, DR, DG, DB),      // lower thigh
            R(-0.34f, 0.065f, MR, MG, MB),     // knee plate
            R(-0.38f, 0.06f, DR, DG, DB),      // shin top
            R(-0.48f, 0.058f, DR, DG, DB),     // shin
            R(-0.56f, 0.06f, DR - 0.02f, DG - 0.02f, DB - 0.02f), // boot top
            R(-0.64f, 0.065f, DR - 0.02f, DG - 0.02f, DB - 0.02f), // boot shaft
            RE(-0.70f, 0.06f, 0.08f, DR - 0.04f, DG - 0.04f, DB - 0.04f), // sole
            RE(-0.72f, 0.00f, 0.00f, DR - 0.04f, DG - 0.04f, DB - 0.04f),
        }, Seg / 2 + 2);

        // ═══ RIGHT LEG — mirrored ═══
        LoftLimb(v, idx, 0.11f, 0.38f, 0, new[]
        {
            R(0.00f, 0.10f, DR, DG, DB),
            R(-0.10f, 0.09f, DR, DG, DB),
            R(-0.20f, 0.08f, DR, DG, DB),
            R(-0.30f, 0.07f, DR, DG, DB),
            R(-0.34f, 0.065f, MR, MG, MB),
            R(-0.38f, 0.06f, DR, DG, DB),
            R(-0.48f, 0.058f, DR, DG, DB),
            R(-0.56f, 0.06f, DR - 0.02f, DG - 0.02f, DB - 0.02f),
            R(-0.64f, 0.065f, DR - 0.02f, DG - 0.02f, DB - 0.02f),
            RE(-0.70f, 0.06f, 0.08f, DR - 0.04f, DG - 0.04f, DB - 0.04f),
            RE(-0.72f, 0.00f, 0.00f, DR - 0.04f, DG - 0.04f, DB - 0.04f),
        }, Seg / 2 + 2);

        // Belt
        Loft(v, idx, new[]
        {
            R(0.42f, 0.24f, 0.45f, 0.30f, 0.15f),
            R(0.46f, 0.25f, 0.45f, 0.30f, 0.15f),
            R(0.49f, 0.24f, 0.45f, 0.30f, 0.15f),
        }, Seg, capBottom: false, capTop: false);
        MeshLofter.AddSphere(v, idx, 0, 0.46f, 0.25f, 0.02f, 6, 0.52f, 0.45f, 0.38f);

        return new MeshData(v.ToArray(), idx.ToArray());
    }

    public static Mesh Create(GL gl)
    {
        var data = GenerateData();
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
    }

    /// <summary>
    /// Skeleton enemy with lofted bone structure and tattered cloth.
    /// </summary>
    public static MeshData GenerateSkeletonData()
    {
        var v = new List<float>();
        var idx = new List<uint>();

        const float br = 0.88f, bg = 0.84f, bb = 0.72f; // bone
        const float dr = 0.12f, dg = 0.10f, db = 0.09f;  // dark

        // ═══ SPINE + TORSO — gaunt lofted body ═══
        Loft(v, idx, new[]
        {
            R(0.34f, 0.12f, br, bg, bb),         // pelvis
            R(0.42f, 0.10f, br * 0.9f, bg * 0.88f, bb * 0.84f), // lower spine
            R(0.50f, 0.08f, br, bg, bb),          // spine narrow
            R(0.60f, 0.10f, br, bg, bb),          // ribs start
            RE(0.70f, 0.14f, 0.10f, br, bg, bb),  // ribcage wide
            RE(0.80f, 0.13f, 0.09f, br, bg, bb),  // upper ribs
            RE(0.90f, 0.10f, 0.07f, br, bg, bb),  // shoulder area
            R(0.98f, 0.06f, br, bg, bb),           // neck base
            R(1.06f, 0.04f, br * 0.96f, bg * 0.94f, bb * 0.90f), // neck
            R(1.12f, 0.05f, br, bg, bb),           // skull base
        }, Seg, capBottom: true, capTop: false);

        // Ribcage struts
        for (int i = 0; i < 4; i++)
        {
            float ry = 0.60f + i * 0.08f;
            float ribW = 0.16f - i * 0.012f;
            MeshLofter.AddCylinder(v, idx, 0, ry, 0, ribW, 0.018f, 6, br * 0.92f, bg * 0.90f, bb * 0.85f);
        }

        // ═══ SKULL — lofted bone shape ═══
        Loft(v, idx, new[]
        {
            R(1.12f, 0.06f, br, bg, bb),
            RE(1.18f, 0.10f, 0.10f, br, bg, bb),    // jaw area
            RE(1.24f, 0.13f, 0.12f, br, bg, bb),    // cheekbones
            RE(1.30f, 0.14f, 0.13f, br, bg, bb),    // mid skull
            RE(1.36f, 0.13f, 0.13f, br, bg, bb),    // upper skull
            R(1.40f, 0.10f, br, bg, bb),              // dome
            R(1.44f, 0.05f, br, bg, bb),              // crown
            R(1.46f, 0.00f, br, bg, bb),              // top
        }, Seg, capBottom: false, capTop: false);

        // Eye sockets
        MeshLofter.AddSphere(v, idx, -0.06f, 1.28f, 0.11f, 0.03f, 6, dr, dg, db);
        MeshLofter.AddSphere(v, idx, 0.06f, 1.28f, 0.11f, 0.03f, 6, dr, dg, db);
        // Jaw
        MeshLofter.AddCylinder(v, idx, 0, 1.12f, 0.05f, 0.06f, 0.03f, 6, br * 0.95f, bg * 0.93f, bb * 0.88f);

        // ═══ ARMS — gaunt bone lofts ═══
        LoftLimb(v, idx, -0.22f, 0.92f, 0, new[]
        {
            R(0.00f, 0.06f, br, bg, bb),
            R(-0.10f, 0.04f, br, bg, bb),
            R(-0.24f, 0.035f, br, bg, bb),     // elbow
            R(-0.26f, 0.04f, br, bg, bb),      // knob
            R(-0.28f, 0.03f, br * 0.96f, bg * 0.94f, bb * 0.90f),
            R(-0.42f, 0.025f, br * 0.96f, bg * 0.94f, bb * 0.90f),
            R(-0.50f, 0.015f, br, bg, bb),
            R(-0.54f, 0.00f, br, bg, bb),
        }, 8, angleX: -0.15f, angleZ: 0.1f);

        LoftLimb(v, idx, 0.22f, 0.92f, 0, new[]
        {
            R(0.00f, 0.06f, br, bg, bb),
            R(-0.10f, 0.04f, br, bg, bb),
            R(-0.24f, 0.035f, br, bg, bb),
            R(-0.26f, 0.04f, br, bg, bb),
            R(-0.28f, 0.03f, br * 0.96f, bg * 0.94f, bb * 0.90f),
            R(-0.42f, 0.025f, br * 0.96f, bg * 0.94f, bb * 0.90f),
            R(-0.50f, 0.015f, br, bg, bb),
            R(-0.54f, 0.00f, br, bg, bb),
        }, 8, angleX: 0.15f, angleZ: 0.1f);

        // ═══ LEGS — thin bone lofts ═══
        LoftLimb(v, idx, -0.10f, 0.34f, 0, new[]
        {
            R(0.00f, 0.06f, br, bg, bb),
            R(-0.12f, 0.045f, br, bg, bb),
            R(-0.24f, 0.04f, br, bg, bb),     // knee
            R(-0.26f, 0.048f, br, bg, bb),    // knob
            R(-0.28f, 0.038f, br, bg, bb),
            R(-0.40f, 0.035f, br, bg, bb),
            R(-0.50f, 0.03f, br * 0.9f, bg * 0.88f, bb * 0.85f),
            RE(-0.54f, 0.035f, 0.05f, br * 0.9f, bg * 0.88f, bb * 0.85f), // foot
            RE(-0.56f, 0.00f, 0.00f, br * 0.9f, bg * 0.88f, bb * 0.85f),
        }, 8);

        LoftLimb(v, idx, 0.10f, 0.34f, 0, new[]
        {
            R(0.00f, 0.06f, br, bg, bb),
            R(-0.12f, 0.045f, br, bg, bb),
            R(-0.24f, 0.04f, br, bg, bb),
            R(-0.26f, 0.048f, br, bg, bb),
            R(-0.28f, 0.038f, br, bg, bb),
            R(-0.40f, 0.035f, br, bg, bb),
            R(-0.50f, 0.03f, br * 0.9f, bg * 0.88f, bb * 0.85f),
            RE(-0.54f, 0.035f, 0.05f, br * 0.9f, bg * 0.88f, bb * 0.85f),
            RE(-0.56f, 0.00f, 0.00f, br * 0.9f, bg * 0.88f, bb * 0.85f),
        }, 8);

        // Tattered robe wisps
        Loft(v, idx, new[]
        {
            RE(0.56f, 0.14f, 0.10f, 0.18f, 0.10f, 0.10f, offsetZ: 0.02f),
            RE(0.48f, 0.16f, 0.12f, 0.18f, 0.10f, 0.10f, offsetZ: 0.04f),
            RE(0.38f, 0.14f, 0.10f, 0.16f, 0.09f, 0.09f, offsetZ: 0.03f),
            RE(0.32f, 0.10f, 0.06f, 0.14f, 0.08f, 0.08f, offsetZ: 0.02f),
        }, 8, capBottom: true, capTop: true);

        return new MeshData(v.ToArray(), idx.ToArray());
    }

    public static Mesh CreateSkeleton(GL gl)
    {
        var data = GenerateSkeletonData();
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
    }
}
