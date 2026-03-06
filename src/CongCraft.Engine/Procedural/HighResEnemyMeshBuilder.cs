using Silk.NET.OpenGL;
using CongCraft.Engine.Rendering;
using static CongCraft.Engine.Procedural.MeshLofter;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates high-resolution enemy character meshes with detailed armor,
/// anatomical skeleton structure, and wolf/troll variants.
/// Uses 24 segments for smooth silhouettes and extensive cross-sections.
/// </summary>
public static class HighResEnemyMeshBuilder
{
    private const int Seg = 24;
    private const int LimbSeg = 14;
    private const int DetailSeg = 12;

    // ── Dark knight armor ──
    private const float AR = 0.26f, AG = 0.23f, AB = 0.22f;    // dark steel base
    private const float ADR = 0.18f, ADG = 0.16f, ADB = 0.15f; // shadow steel
    private const float MR = 0.34f, MG = 0.30f, MB = 0.28f;    // metal accent
    private const float RdR = 0.65f, RdG = 0.12f, RdB = 0.08f; // red accent
    private const float SR = 0.52f, SG = 0.40f, SB = 0.36f;    // skin

    /// <summary>
    /// High-resolution dark knight warrior with layered plate armor.
    /// </summary>
    public static MeshData GenerateData()
    {
        var v = new List<float>(28000);
        var idx = new List<uint>(42000);

        // ═══ TORSO — heavy dark plate armor ═══
        Loft(v, idx, new[]
        {
            RE(0.36f, 0.215f, 0.150f, AR, AG, AB),          // waist armor
            RE(0.40f, 0.230f, 0.160f, AR, AG, AB),
            RE(0.44f, 0.245f, 0.170f, AR, AG, AB),
            RE(0.48f, 0.258f, 0.178f, AR, AG, AB),          // lower chest
            RE(0.52f, 0.268f, 0.185f, AR, AG, AB),
            RE(0.56f, 0.275f, 0.190f, AR, AG, AB),          // mid chest
            RE(0.60f, 0.280f, 0.192f, AR, AG, AB),          // chest widest
            RE(0.64f, 0.278f, 0.190f, AR, AG, AB),
            RE(0.68f, 0.272f, 0.186f, AR, AG, AB),
            RE(0.72f, 0.262f, 0.180f, AR, AG, AB),          // upper chest
            RE(0.76f, 0.250f, 0.172f, AR, AG, AB),
            RE(0.80f, 0.236f, 0.162f, ADR, ADG, ADB),
            RE(0.84f, 0.222f, 0.150f, ADR, ADG, ADB),       // shoulder base
            RE(0.88f, 0.205f, 0.138f, ADR, ADG, ADB),
            RE(0.92f, 0.180f, 0.122f, ADR, ADG, ADB),       // neck transition
            RE(0.96f, 0.140f, 0.100f, AR, AG, AB),           // gorget
            RE(1.00f, 0.100f, 0.085f, AR, AG, AB),           // gorget top
            RE(1.04f, 0.082f, 0.072f, SR, SG, SB),           // neck
            RE(1.08f, 0.075f, 0.068f, SR, SG, SB),
            RE(1.12f, 0.078f, 0.070f, SR, SG, SB),           // neck top
        }, Seg, capBottom: true, capTop: false);

        // Chest emblem — red skull motif
        AddSphere(v, idx, 0, 0.62f, 0.195f, 0.028f, DetailSeg, RdR, RdG, RdB);
        AddSphere(v, idx, -0.018f, 0.63f, 0.198f, 0.008f, 6, 0.08f, 0.06f, 0.06f); // eye
        AddSphere(v, idx, 0.018f, 0.63f, 0.198f, 0.008f, 6, 0.08f, 0.06f, 0.06f);  // eye

        // ═══ HEAD + HELMET — full-face dark helm with crest ═══
        Loft(v, idx, new[]
        {
            R(1.12f, 0.082f, SR, SG, SB),                    // chin
            RE(1.15f, 0.105f, 0.098f, SR, SG, SB),           // jaw
            RE(1.18f, 0.120f, 0.112f, ADR, ADG, ADB),        // helmet starts
            RE(1.21f, 0.135f, 0.128f, ADR, ADG, ADB),        // helmet lower
            RE(1.24f, 0.142f, 0.138f, AR, AG, AB),            // helmet mid
            RE(1.27f, 0.145f, 0.142f, AR, AG, AB),            // helmet widest
            RE(1.30f, 0.142f, 0.140f, AR, AG, AB),
            RE(1.33f, 0.136f, 0.136f, ADR, ADG, ADB),        // helmet upper
            RE(1.36f, 0.125f, 0.128f, ADR, ADG, ADB),
            R(1.39f, 0.100f, ADR, ADG, ADB),                  // dome
            R(1.42f, 0.070f, AR, AG, AB),                      // crown
        }, Seg, capBottom: false, capTop: false);

        // Helmet crest
        Loft(v, idx, new[]
        {
            R(1.42f, 0.035f, RdR, RdG, RdB),
            RE(1.46f, 0.032f, 0.040f, RdR, RdG, RdB),
            RE(1.50f, 0.028f, 0.038f, RdR, RdG, RdB),
            RE(1.54f, 0.022f, 0.032f, RdR * 0.9f, RdG, RdB),
            R(1.58f, 0.012f, RdR * 0.85f, RdG, RdB),
            R(1.60f, 0.000f, RdR * 0.8f, RdG, RdB),
        }, 10, capBottom: false, capTop: false);

        // Visor slit
        AddCylinder(v, idx, 0, 1.25f, 0.145f, 0.070f, 0.014f, 8, 0.04f, 0.03f, 0.03f);

        // Helmet horns
        AddCylinder(v, idx, -0.10f, 1.35f, 0, 0.018f, 0.10f, 8, MR, MG, MB);
        AddCylinder(v, idx, 0.10f, 1.35f, 0, 0.018f, 0.10f, 8, MR, MG, MB);

        // ═══ LEFT ARM — heavy gauntlet ═══
        LoftLimb(v, idx, -0.28f, 0.88f, 0, new[]
        {
            R(0.00f, 0.115f, MR, MG, MB),                   // shoulder cap
            R(-0.04f, 0.105f, AR, AG, AB),
            R(-0.08f, 0.095f, AR, AG, AB),                   // upper arm
            R(-0.14f, 0.085f, AR, AG, AB),
            R(-0.20f, 0.078f, AR, AG, AB),
            R(-0.26f, 0.072f, AR, AG, AB),                   // elbow plate
            R(-0.28f, 0.076f, MR, MG, MB),                   // elbow spike base
            R(-0.30f, 0.072f, AR, AG, AB),
            R(-0.36f, 0.068f, ADR, ADG, ADB),                // forearm
            R(-0.42f, 0.072f, ADR, ADG, ADB),                // gauntlet
            R(-0.48f, 0.075f, ADR, ADG, ADB),                // gauntlet flare
            R(-0.52f, 0.068f, ADR, ADG, ADB),
            R(-0.56f, 0.055f, MR, MG, MB),                   // wrist
            R(-0.60f, 0.040f, MR, MG, MB),                   // fist
            R(-0.63f, 0.020f, MR, MG, MB),
            R(-0.65f, 0.000f, MR, MG, MB),
        }, LimbSeg, angleX: -0.22f, angleZ: 0.08f);

        // ═══ RIGHT ARM — mirrored ═══
        LoftLimb(v, idx, 0.28f, 0.88f, 0, new[]
        {
            R(0.00f, 0.115f, MR, MG, MB),
            R(-0.04f, 0.105f, AR, AG, AB),
            R(-0.08f, 0.095f, AR, AG, AB),
            R(-0.14f, 0.085f, AR, AG, AB),
            R(-0.20f, 0.078f, AR, AG, AB),
            R(-0.26f, 0.072f, AR, AG, AB),
            R(-0.28f, 0.076f, MR, MG, MB),
            R(-0.30f, 0.072f, AR, AG, AB),
            R(-0.36f, 0.068f, ADR, ADG, ADB),
            R(-0.42f, 0.072f, ADR, ADG, ADB),
            R(-0.48f, 0.075f, ADR, ADG, ADB),
            R(-0.52f, 0.068f, ADR, ADG, ADB),
            R(-0.56f, 0.055f, MR, MG, MB),
            R(-0.60f, 0.040f, MR, MG, MB),
            R(-0.63f, 0.020f, MR, MG, MB),
            R(-0.65f, 0.000f, MR, MG, MB),
        }, LimbSeg, angleX: 0.22f, angleZ: 0.08f);

        // Shoulder spikes
        AddCylinder(v, idx, -0.32f, 1.06f, 0.04f, 0.016f, 0.12f, 8, MR, MG, MB);
        AddCylinder(v, idx, -0.32f, 1.06f, -0.04f, 0.016f, 0.10f, 8, MR, MG, MB);
        AddCylinder(v, idx, 0.32f, 1.06f, 0.04f, 0.016f, 0.12f, 8, MR, MG, MB);
        AddCylinder(v, idx, 0.32f, 1.06f, -0.04f, 0.016f, 0.10f, 8, MR, MG, MB);

        // Massive pauldrons
        AddSphere(v, idx, -0.30f, 0.98f, 0, 0.115f, DetailSeg, AR + 0.04f, AG + 0.03f, AB + 0.02f);
        AddSphere(v, idx, 0.30f, 0.98f, 0, 0.115f, DetailSeg, AR + 0.04f, AG + 0.03f, AB + 0.02f);

        // Elbow spikes
        AddCylinder(v, idx, -0.32f, 0.62f, -0.06f, 0.010f, 0.06f, 6, MR, MG, MB);
        AddCylinder(v, idx, 0.32f, 0.62f, -0.06f, 0.010f, 0.06f, 6, MR, MG, MB);

        // ═══ LEFT LEG — dark armored greaves ═══
        LoftLimb(v, idx, -0.12f, 0.36f, 0, new[]
        {
            R(0.00f, 0.108f, ADR, ADG, ADB),                // hip
            R(-0.04f, 0.102f, ADR, ADG, ADB),
            R(-0.08f, 0.096f, ADR, ADG, ADB),
            R(-0.12f, 0.090f, ADR, ADG, ADB),               // thigh
            R(-0.16f, 0.086f, ADR, ADG, ADB),
            R(-0.20f, 0.082f, ADR, ADG, ADB),
            R(-0.24f, 0.078f, ADR, ADG, ADB),
            R(-0.28f, 0.074f, MR, MG, MB),                  // knee plate
            R(-0.30f, 0.078f, MR, MG, MB),                  // knee bulge
            R(-0.32f, 0.074f, MR, MG, MB),
            R(-0.36f, 0.068f, ADR, ADG, ADB),               // shin
            R(-0.40f, 0.065f, ADR, ADG, ADB),
            R(-0.44f, 0.062f, ADR, ADG, ADB),
            R(-0.48f, 0.060f, ADR, ADG, ADB),
            R(-0.52f, 0.064f, ADR - 0.02f, ADG - 0.02f, ADB - 0.02f), // boot
            R(-0.56f, 0.068f, ADR - 0.02f, ADG - 0.02f, ADB - 0.02f),
            RE(-0.62f, 0.066f, 0.082f, ADR - 0.04f, ADG - 0.04f, ADB - 0.04f),
            RE(-0.66f, 0.060f, 0.086f, ADR - 0.06f, ADG - 0.06f, ADB - 0.06f),
            RE(-0.68f, 0.000f, 0.000f, ADR - 0.06f, ADG - 0.06f, ADB - 0.06f),
        }, LimbSeg);

        // ═══ RIGHT LEG — mirrored ═══
        LoftLimb(v, idx, 0.12f, 0.36f, 0, new[]
        {
            R(0.00f, 0.108f, ADR, ADG, ADB),
            R(-0.04f, 0.102f, ADR, ADG, ADB),
            R(-0.08f, 0.096f, ADR, ADG, ADB),
            R(-0.12f, 0.090f, ADR, ADG, ADB),
            R(-0.16f, 0.086f, ADR, ADG, ADB),
            R(-0.20f, 0.082f, ADR, ADG, ADB),
            R(-0.24f, 0.078f, ADR, ADG, ADB),
            R(-0.28f, 0.074f, MR, MG, MB),
            R(-0.30f, 0.078f, MR, MG, MB),
            R(-0.32f, 0.074f, MR, MG, MB),
            R(-0.36f, 0.068f, ADR, ADG, ADB),
            R(-0.40f, 0.065f, ADR, ADG, ADB),
            R(-0.44f, 0.062f, ADR, ADG, ADB),
            R(-0.48f, 0.060f, ADR, ADG, ADB),
            R(-0.52f, 0.064f, ADR - 0.02f, ADG - 0.02f, ADB - 0.02f),
            R(-0.56f, 0.068f, ADR - 0.02f, ADG - 0.02f, ADB - 0.02f),
            RE(-0.62f, 0.066f, 0.082f, ADR - 0.04f, ADG - 0.04f, ADB - 0.04f),
            RE(-0.66f, 0.060f, 0.086f, ADR - 0.06f, ADG - 0.06f, ADB - 0.06f),
            RE(-0.68f, 0.000f, 0.000f, ADR - 0.06f, ADG - 0.06f, ADB - 0.06f),
        }, LimbSeg);

        // Belt
        Loft(v, idx, new[]
        {
            R(0.40f, 0.252f, 0.42f, 0.28f, 0.12f),
            R(0.43f, 0.260f, 0.42f, 0.28f, 0.12f),
            R(0.46f, 0.258f, 0.42f, 0.28f, 0.12f),
            R(0.48f, 0.252f, 0.42f, 0.28f, 0.12f),
        }, Seg, capBottom: false, capTop: false);

        // Belt skull buckle
        AddSphere(v, idx, 0, 0.44f, 0.265f, 0.025f, 8, MR + 0.1f, MG + 0.08f, MB + 0.06f);

        return new MeshData(v.ToArray(), idx.ToArray());
    }

    public static Mesh Create(GL gl)
    {
        var data = GenerateData();
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
    }

    /// <summary>
    /// High-resolution skeleton with anatomically detailed bone structure,
    /// glowing eye sockets, ribcage with individual ribs, and tattered robes.
    /// </summary>
    public static MeshData GenerateSkeletonData()
    {
        var v = new List<float>(24000);
        var idx = new List<uint>(36000);

        const float br = 0.88f, bg = 0.84f, bb = 0.72f;     // bone
        const float bdr = 0.78f, bdg = 0.74f, bdb = 0.62f;  // dark bone
        const float dr = 0.10f, dg = 0.08f, db = 0.07f;      // dark void
        const float gr = 0.25f, gg = 0.85f, gb = 0.35f;      // green glow

        // ═══ PELVIS — wide bone basin ═══
        Loft(v, idx, new[]
        {
            RE(0.30f, 0.14f, 0.10f, bdr, bdg, bdb),
            RE(0.34f, 0.15f, 0.11f, br, bg, bb),
            RE(0.38f, 0.14f, 0.10f, br, bg, bb),            // pelvis top
            R(0.42f, 0.10f, bdr, bdg, bdb),                   // waist narrow
        }, Seg / 2, capBottom: true, capTop: false);

        // ═══ SPINE — segmented vertebrae ═══
        for (int i = 0; i < 6; i++)
        {
            float y = 0.42f + i * 0.08f;
            float rad = 0.04f + MathF.Sin(i * 0.5f) * 0.005f;
            Loft(v, idx, new[]
            {
                R(y, rad * 0.8f, br, bg, bb),
                R(y + 0.02f, rad, br, bg, bb),
                R(y + 0.04f, rad * 1.05f, bdr, bdg, bdb),    // vertebra bulge
                R(y + 0.06f, rad, br, bg, bb),
                R(y + 0.08f, rad * 0.8f, br, bg, bb),
            }, 8, capBottom: false, capTop: false);
        }

        // ═══ RIBCAGE — individual rib pairs ═══
        Loft(v, idx, new[]
        {
            R(0.56f, 0.08f, br, bg, bb),
            RE(0.62f, 0.12f, 0.09f, br, bg, bb),
            RE(0.68f, 0.16f, 0.11f, br, bg, bb),             // ribs widest
            RE(0.74f, 0.17f, 0.12f, br, bg, bb),
            RE(0.80f, 0.15f, 0.11f, br, bg, bb),
            RE(0.86f, 0.12f, 0.09f, br, bg, bb),
            R(0.90f, 0.08f, bdr, bdg, bdb),                   // upper ribcage
        }, Seg / 2, capBottom: false, capTop: false);

        // Individual rib struts
        for (int i = 0; i < 5; i++)
        {
            float ry = 0.58f + i * 0.065f;
            float ribW = 0.17f - i * 0.010f;
            float ribD = 0.012f + i * 0.001f;
            AddCylinder(v, idx, 0, ry, 0, ribW, ribD, 8, br * 0.94f, bg * 0.92f, bb * 0.88f);
            // Rib tips
            AddSphere(v, idx, -ribW, ry, 0, ribD * 0.8f, 4, bdr, bdg, bdb);
            AddSphere(v, idx, ribW, ry, 0, ribD * 0.8f, 4, bdr, bdg, bdb);
        }

        // ═══ SHOULDER GIRDLE ═══
        Loft(v, idx, new[]
        {
            R(0.88f, 0.10f, br, bg, bb),
            RE(0.92f, 0.14f, 0.08f, br, bg, bb),             // clavicle area
            R(0.96f, 0.08f, bdr, bdg, bdb),                   // neck base
        }, 10, capBottom: false, capTop: false);

        // ═══ NECK VERTEBRAE ═══
        Loft(v, idx, new[]
        {
            R(0.96f, 0.06f, br, bg, bb),
            R(0.99f, 0.048f, bdr, bdg, bdb),
            R(1.02f, 0.042f, br, bg, bb),
            R(1.05f, 0.038f, bdr, bdg, bdb),
            R(1.08f, 0.042f, br, bg, bb),
            R(1.12f, 0.055f, br, bg, bb),                    // atlas
        }, 8, capBottom: false, capTop: false);

        // ═══ SKULL — detailed cranium ═══
        Loft(v, idx, new[]
        {
            R(1.12f, 0.065f, br, bg, bb),                    // foramen magnum
            RE(1.15f, 0.085f, 0.080f, br, bg, bb),           // occipital
            RE(1.18f, 0.105f, 0.100f, br, bg, bb),           // jaw
            RE(1.21f, 0.118f, 0.112f, br, bg, bb),           // zygomatic
            RE(1.24f, 0.132f, 0.125f, br, bg, bb),           // temporal
            RE(1.27f, 0.140f, 0.134f, br, bg, bb),           // mid skull
            RE(1.30f, 0.145f, 0.140f, br, bg, bb),           // parietal
            RE(1.33f, 0.142f, 0.138f, br, bg, bb),
            RE(1.36f, 0.135f, 0.132f, br, bg, bb),           // upper skull
            R(1.39f, 0.115f, bdr, bdg, bdb),                  // dome
            R(1.42f, 0.085f, bdr, bdg, bdb),
            R(1.44f, 0.050f, br, bg, bb),                     // crown
            R(1.46f, 0.020f, br, bg, bb),
            R(1.47f, 0.000f, br, bg, bb),
        }, Seg / 2, capBottom: false, capTop: false);

        // Eye sockets — deep with green glow
        AddSphere(v, idx, -0.055f, 1.27f, 0.12f, 0.032f, 10, dr, dg, db);
        AddSphere(v, idx, 0.055f, 1.27f, 0.12f, 0.032f, 10, dr, dg, db);
        // Eerie glow
        AddSphere(v, idx, -0.055f, 1.27f, 0.125f, 0.018f, 8, gr, gg, gb);
        AddSphere(v, idx, 0.055f, 1.27f, 0.125f, 0.018f, 8, gr, gg, gb);
        // Glow core
        AddSphere(v, idx, -0.055f, 1.27f, 0.128f, 0.008f, 6, gr + 0.2f, gg + 0.1f, gb + 0.15f);
        AddSphere(v, idx, 0.055f, 1.27f, 0.128f, 0.008f, 6, gr + 0.2f, gg + 0.1f, gb + 0.15f);

        // Nasal cavity
        AddSphere(v, idx, 0, 1.24f, 0.13f, 0.018f, 6, dr, dg, db);

        // Jaw — separate loft
        Loft(v, idx, new[]
        {
            RE(1.12f, 0.08f, 0.06f, br, bg, bb, offsetZ: 0.04f),
            RE(1.15f, 0.09f, 0.06f, bdr, bdg, bdb, offsetZ: 0.02f),
            RE(1.18f, 0.06f, 0.04f, br, bg, bb, offsetZ: 0.06f),
        }, 8, capBottom: true, capTop: true);

        // Teeth
        for (int t = -2; t <= 2; t++)
        {
            float tx = t * 0.018f;
            AddBox(v, idx, tx, 1.18f, 0.11f, 0.008f, 0.015f, 0.005f, 0.92f, 0.90f, 0.82f);
        }

        // ═══ ARMS — skeletal bones ═══
        LoftLimb(v, idx, -0.22f, 0.92f, 0, new[]
        {
            R(0.00f, 0.055f, br, bg, bb),                    // shoulder joint
            R(-0.04f, 0.042f, br, bg, bb),                   // humerus head
            R(-0.08f, 0.035f, bdr, bdg, bdb),
            R(-0.14f, 0.032f, br, bg, bb),
            R(-0.20f, 0.030f, br, bg, bb),                   // mid humerus
            R(-0.24f, 0.034f, bdr, bdg, bdb),                // near elbow
            R(-0.26f, 0.038f, br, bg, bb),                   // elbow knob
            R(-0.28f, 0.030f, br, bg, bb),
            R(-0.32f, 0.025f, bdr, bdg, bdb),                // radius/ulna
            R(-0.38f, 0.022f, br, bg, bb),
            R(-0.44f, 0.020f, bdr, bdg, bdb),
            R(-0.48f, 0.015f, br, bg, bb),                   // wrist
            R(-0.52f, 0.012f, br, bg, bb),                   // metacarpals
            R(-0.55f, 0.008f, bdr, bdg, bdb),
            R(-0.57f, 0.000f, bdr, bdg, bdb),
        }, 10, angleX: -0.15f, angleZ: 0.1f);

        LoftLimb(v, idx, 0.22f, 0.92f, 0, new[]
        {
            R(0.00f, 0.055f, br, bg, bb),
            R(-0.04f, 0.042f, br, bg, bb),
            R(-0.08f, 0.035f, bdr, bdg, bdb),
            R(-0.14f, 0.032f, br, bg, bb),
            R(-0.20f, 0.030f, br, bg, bb),
            R(-0.24f, 0.034f, bdr, bdg, bdb),
            R(-0.26f, 0.038f, br, bg, bb),
            R(-0.28f, 0.030f, br, bg, bb),
            R(-0.32f, 0.025f, bdr, bdg, bdb),
            R(-0.38f, 0.022f, br, bg, bb),
            R(-0.44f, 0.020f, bdr, bdg, bdb),
            R(-0.48f, 0.015f, br, bg, bb),
            R(-0.52f, 0.012f, br, bg, bb),
            R(-0.55f, 0.008f, bdr, bdg, bdb),
            R(-0.57f, 0.000f, bdr, bdg, bdb),
        }, 10, angleX: 0.15f, angleZ: 0.1f);

        // ═══ LEGS — femur/tibia bone structure ═══
        LoftLimb(v, idx, -0.10f, 0.32f, 0, new[]
        {
            R(0.00f, 0.058f, br, bg, bb),                    // hip joint
            R(-0.04f, 0.048f, br, bg, bb),
            R(-0.08f, 0.042f, bdr, bdg, bdb),                // femur
            R(-0.14f, 0.038f, br, bg, bb),
            R(-0.20f, 0.036f, br, bg, bb),
            R(-0.24f, 0.040f, bdr, bdg, bdb),                // near knee
            R(-0.26f, 0.048f, br, bg, bb),                   // knee knob
            R(-0.28f, 0.040f, br, bg, bb),
            R(-0.32f, 0.034f, bdr, bdg, bdb),                // tibia
            R(-0.38f, 0.030f, br, bg, bb),
            R(-0.44f, 0.028f, br, bg, bb),
            R(-0.48f, 0.026f, bdr, bdg, bdb),                // ankle
            RE(-0.52f, 0.030f, 0.048f, bdr, bdg, bdb),       // foot
            RE(-0.54f, 0.028f, 0.055f, br * 0.92f, bg * 0.90f, bb * 0.86f),
            RE(-0.56f, 0.000f, 0.000f, br * 0.92f, bg * 0.90f, bb * 0.86f),
        }, 10);

        LoftLimb(v, idx, 0.10f, 0.32f, 0, new[]
        {
            R(0.00f, 0.058f, br, bg, bb),
            R(-0.04f, 0.048f, br, bg, bb),
            R(-0.08f, 0.042f, bdr, bdg, bdb),
            R(-0.14f, 0.038f, br, bg, bb),
            R(-0.20f, 0.036f, br, bg, bb),
            R(-0.24f, 0.040f, bdr, bdg, bdb),
            R(-0.26f, 0.048f, br, bg, bb),
            R(-0.28f, 0.040f, br, bg, bb),
            R(-0.32f, 0.034f, bdr, bdg, bdb),
            R(-0.38f, 0.030f, br, bg, bb),
            R(-0.44f, 0.028f, br, bg, bb),
            R(-0.48f, 0.026f, bdr, bdg, bdb),
            RE(-0.52f, 0.030f, 0.048f, bdr, bdg, bdb),
            RE(-0.54f, 0.028f, 0.055f, br * 0.92f, bg * 0.90f, bb * 0.86f),
            RE(-0.56f, 0.000f, 0.000f, br * 0.92f, bg * 0.90f, bb * 0.86f),
        }, 10);

        // ═══ TATTERED ROBE — flowing cloth remnants ═══
        Loft(v, idx, new[]
        {
            RE(0.60f, 0.18f, 0.12f, 0.16f, 0.09f, 0.09f, offsetZ: 0.02f),
            RE(0.52f, 0.20f, 0.14f, 0.16f, 0.09f, 0.09f, offsetZ: 0.04f),
            RE(0.44f, 0.18f, 0.12f, 0.14f, 0.08f, 0.08f, offsetZ: 0.03f),
            RE(0.36f, 0.15f, 0.08f, 0.12f, 0.07f, 0.07f, offsetZ: 0.02f),
            RE(0.30f, 0.10f, 0.05f, 0.10f, 0.06f, 0.06f, offsetZ: 0.01f),
        }, 10, capBottom: true, capTop: true);

        // Second robe strip (back)
        Loft(v, idx, new[]
        {
            RE(0.58f, 0.14f, 0.08f, 0.14f, 0.08f, 0.08f, offsetZ: -0.05f),
            RE(0.48f, 0.16f, 0.10f, 0.13f, 0.07f, 0.07f, offsetZ: -0.06f),
            RE(0.38f, 0.12f, 0.06f, 0.12f, 0.06f, 0.06f, offsetZ: -0.04f),
        }, 8, capBottom: true, capTop: true);

        return new MeshData(v.ToArray(), idx.ToArray());
    }

    public static Mesh CreateSkeleton(GL gl)
    {
        var data = GenerateSkeletonData();
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
    }

    /// <summary>
    /// High-resolution wolf mesh — quadruped with fur-textured body.
    /// </summary>
    public static MeshData GenerateWolfData()
    {
        var v = new List<float>(18000);
        var idx = new List<uint>(27000);

        const float fr = 0.38f, fg = 0.34f, fb = 0.28f;     // fur dark
        const float flr = 0.48f, flg = 0.44f, flb = 0.36f;  // fur light
        const float fbr = 0.32f, fbg = 0.26f, fbb = 0.20f;  // fur belly

        // ═══ BODY — muscular wolf torso ═══
        Loft(v, idx, new[]
        {
            RE(0.00f, 0.06f, 0.08f, fr, fg, fb),             // tail base
            RE(0.08f, 0.12f, 0.14f, fr, fg, fb),             // haunches start
            RE(0.16f, 0.16f, 0.18f, fr, fg, fb),             // haunches
            RE(0.24f, 0.17f, 0.19f, flr, flg, flb),          // hip widest
            RE(0.32f, 0.15f, 0.16f, fr, fg, fb),             // waist
            RE(0.40f, 0.14f, 0.15f, fr, fg, fb),             // mid body
            RE(0.48f, 0.16f, 0.17f, fr, fg, fb),             // ribcage
            RE(0.56f, 0.18f, 0.19f, flr, flg, flb),          // chest
            RE(0.64f, 0.17f, 0.18f, fr, fg, fb),             // upper chest
            RE(0.70f, 0.14f, 0.15f, fr, fg, fb),             // shoulder
            RE(0.76f, 0.12f, 0.13f, fr, fg, fb),             // neck base
        }, 16, capBottom: true, capTop: false);

        // Belly (lighter underside)
        Loft(v, idx, new[]
        {
            RE(0.20f, 0.12f, 0.03f, fbr, fbg, fbb, offsetZ: 0.14f),
            RE(0.35f, 0.10f, 0.02f, fbr, fbg, fbb, offsetZ: 0.12f),
            RE(0.50f, 0.12f, 0.02f, fbr, fbg, fbb, offsetZ: 0.14f),
        }, 8, capBottom: true, capTop: true);

        // ═══ NECK + HEAD ═══
        Loft(v, idx, new[]
        {
            RE(0.76f, 0.10f, 0.11f, fr, fg, fb),
            RE(0.82f, 0.08f, 0.09f, flr, flg, flb),          // neck mid
            RE(0.88f, 0.07f, 0.08f, fr, fg, fb),              // neck upper
            RE(0.92f, 0.08f, 0.09f, fr, fg, fb),              // skull base
            RE(0.96f, 0.09f, 0.085f, flr, flg, flb),          // skull
            RE(1.00f, 0.08f, 0.075f, fr, fg, fb),             // forehead
            RE(1.04f, 0.06f, 0.055f, fr, fg, fb),             // snout
            RE(1.08f, 0.04f, 0.040f, fr, fg, fb),             // snout mid
            RE(1.12f, 0.028f, 0.025f, 0.18f, 0.14f, 0.12f),  // nose
            R(1.14f, 0.000f, 0.12f, 0.10f, 0.08f),            // nose tip
        }, 14, capBottom: false, capTop: false);

        // Eyes
        AddSphere(v, idx, -0.06f, 0.38f, 0.97f, 0.018f, 8, 0.82f, 0.65f, 0.15f);
        AddSphere(v, idx, 0.06f, 0.38f, 0.97f, 0.018f, 8, 0.82f, 0.65f, 0.15f);

        // Ears
        Loft(v, idx, new[]
        {
            R(0.96f, 0.025f, fr, fg, fb, offsetX: -0.07f),
            R(1.02f, 0.018f, flr, flg, flb, offsetX: -0.08f),
            R(1.06f, 0.008f, fr, fg, fb, offsetX: -0.085f),
            R(1.08f, 0.000f, fr, fg, fb, offsetX: -0.085f),
        }, 6, capBottom: true, capTop: false);

        Loft(v, idx, new[]
        {
            R(0.96f, 0.025f, fr, fg, fb, offsetX: 0.07f),
            R(1.02f, 0.018f, flr, flg, flb, offsetX: 0.08f),
            R(1.06f, 0.008f, fr, fg, fb, offsetX: 0.085f),
            R(1.08f, 0.000f, fr, fg, fb, offsetX: 0.085f),
        }, 6, capBottom: true, capTop: false);

        // ═══ LEGS — four muscular wolf legs ═══
        // Front left
        LoftLimb(v, idx, -0.10f, 0.70f, 0.10f, new[]
        {
            R(0.00f, 0.055f, fr, fg, fb),
            R(-0.08f, 0.048f, fr, fg, fb),
            R(-0.16f, 0.040f, flr, flg, flb),
            R(-0.24f, 0.035f, fr, fg, fb),
            R(-0.32f, 0.032f, fr, fg, fb),
            R(-0.38f, 0.030f, fr, fg, fb),
            RE(-0.42f, 0.028f, 0.035f, fbr, fbg, fbb),
            RE(-0.44f, 0.000f, 0.000f, fbr, fbg, fbb),
        }, 8, angleX: 0f, angleZ: -0.08f);

        // Front right
        LoftLimb(v, idx, 0.10f, 0.70f, 0.10f, new[]
        {
            R(0.00f, 0.055f, fr, fg, fb),
            R(-0.08f, 0.048f, fr, fg, fb),
            R(-0.16f, 0.040f, flr, flg, flb),
            R(-0.24f, 0.035f, fr, fg, fb),
            R(-0.32f, 0.032f, fr, fg, fb),
            R(-0.38f, 0.030f, fr, fg, fb),
            RE(-0.42f, 0.028f, 0.035f, fbr, fbg, fbb),
            RE(-0.44f, 0.000f, 0.000f, fbr, fbg, fbb),
        }, 8, angleX: 0f, angleZ: -0.08f);

        // Rear left
        LoftLimb(v, idx, -0.12f, 0.22f, 0.06f, new[]
        {
            R(0.00f, 0.065f, fr, fg, fb),
            R(-0.06f, 0.058f, flr, flg, flb),
            R(-0.14f, 0.048f, fr, fg, fb),
            R(-0.22f, 0.042f, fr, fg, fb),
            R(-0.28f, 0.038f, fr, fg, fb),
            R(-0.34f, 0.034f, fr, fg, fb),
            RE(-0.38f, 0.030f, 0.038f, fbr, fbg, fbb),
            RE(-0.40f, 0.000f, 0.000f, fbr, fbg, fbb),
        }, 8, angleX: 0f, angleZ: 0.06f);

        // Rear right
        LoftLimb(v, idx, 0.12f, 0.22f, 0.06f, new[]
        {
            R(0.00f, 0.065f, fr, fg, fb),
            R(-0.06f, 0.058f, flr, flg, flb),
            R(-0.14f, 0.048f, fr, fg, fb),
            R(-0.22f, 0.042f, fr, fg, fb),
            R(-0.28f, 0.038f, fr, fg, fb),
            R(-0.34f, 0.034f, fr, fg, fb),
            RE(-0.38f, 0.030f, 0.038f, fbr, fbg, fbb),
            RE(-0.40f, 0.000f, 0.000f, fbr, fbg, fbb),
        }, 8, angleX: 0f, angleZ: 0.06f);

        // Tail
        Loft(v, idx, new[]
        {
            R(-0.02f, 0.04f, fr, fg, fb, offsetZ: -0.04f),
            R(-0.08f, 0.035f, flr, flg, flb, offsetZ: -0.08f),
            R(-0.14f, 0.028f, fr, fg, fb, offsetZ: -0.10f),
            R(-0.20f, 0.020f, fr, fg, fb, offsetZ: -0.12f),
            R(-0.26f, 0.012f, flr, flg, flb, offsetZ: -0.11f),
            R(-0.30f, 0.000f, fr, fg, fb, offsetZ: -0.10f),
        }, 8, capBottom: true, capTop: false);

        return new MeshData(v.ToArray(), idx.ToArray());
    }

    public static Mesh CreateWolf(GL gl)
    {
        var data = GenerateWolfData();
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
    }

    /// <summary>
    /// High-resolution troll — massive brute with rocky skin and tusks.
    /// </summary>
    public static MeshData GenerateTrollData()
    {
        var v = new List<float>(22000);
        var idx = new List<uint>(33000);

        const float sr = 0.42f, sg = 0.52f, sb = 0.38f;     // green-grey skin
        const float sdr = 0.34f, sdg = 0.42f, sdb = 0.30f;  // dark skin
        const float wr = 0.38f, wg = 0.32f, wb = 0.22f;     // wart/callous
        const float tr = 0.85f, tg = 0.82f, tb = 0.68f;     // tusk

        // ═══ MASSIVE TORSO ═══
        Loft(v, idx, new[]
        {
            RE(0.30f, 0.28f, 0.22f, sdr, sdg, sdb),          // waist
            RE(0.38f, 0.32f, 0.26f, sr, sg, sb),
            RE(0.46f, 0.36f, 0.30f, sr, sg, sb),              // belly
            RE(0.54f, 0.38f, 0.32f, sr, sg, sb),              // belly widest
            RE(0.62f, 0.37f, 0.30f, sr, sg, sb),
            RE(0.70f, 0.36f, 0.28f, sr, sg, sb),              // chest
            RE(0.78f, 0.38f, 0.30f, sdr, sdg, sdb),           // pecs
            RE(0.86f, 0.36f, 0.28f, sr, sg, sb),
            RE(0.94f, 0.32f, 0.24f, sdr, sdg, sdb),           // shoulders
            RE(1.02f, 0.26f, 0.20f, sr, sg, sb),
            RE(1.08f, 0.18f, 0.16f, sdr, sdg, sdb),           // neck base
            R(1.14f, 0.14f, sr, sg, sb),                        // thick neck
            R(1.20f, 0.13f, sr, sg, sb),
            R(1.24f, 0.14f, sr, sg, sb),                        // neck top
        }, Seg, capBottom: true, capTop: false);

        // Warts and bumps on torso
        AddSphere(v, idx, 0.15f, 0.50f, 0.30f, 0.025f, 6, wr, wg, wb);
        AddSphere(v, idx, -0.20f, 0.62f, 0.28f, 0.020f, 6, wr, wg, wb);
        AddSphere(v, idx, 0.10f, 0.75f, 0.25f, 0.022f, 6, wr, wg, wb);

        // ═══ HEAD — brutish, low-browed ═══
        Loft(v, idx, new[]
        {
            R(1.24f, 0.15f, sr, sg, sb),                      // jaw base
            RE(1.28f, 0.18f, 0.16f, sr, sg, sb),              // jaw wide
            RE(1.32f, 0.20f, 0.18f, sr, sg, sb),              // cheeks
            RE(1.36f, 0.19f, 0.18f, sdr, sdg, sdb),           // brow ridge
            RE(1.38f, 0.17f, 0.17f, sr, sg, sb),              // forehead (low)
            R(1.40f, 0.14f, sr, sg, sb),
            R(1.42f, 0.10f, sdr, sdg, sdb),                   // skull top
            R(1.44f, 0.05f, sdr, sdg, sdb),
            R(1.45f, 0.00f, sdr, sdg, sdb),
        }, 16, capBottom: false, capTop: false);

        // Protruding jaw
        Loft(v, idx, new[]
        {
            RE(1.24f, 0.12f, 0.05f, sr, sg, sb, offsetZ: 0.12f),
            RE(1.22f, 0.14f, 0.06f, sdr, sdg, sdb, offsetZ: 0.14f),
            RE(1.20f, 0.10f, 0.04f, sr, sg, sb, offsetZ: 0.10f),
        }, 8, capBottom: true, capTop: true);

        // Tusks
        AddCylinder(v, idx, -0.08f, 1.26f, 0.16f, 0.018f, 0.08f, 8, tr, tg, tb);
        AddCylinder(v, idx, 0.08f, 1.26f, 0.16f, 0.018f, 0.08f, 8, tr, tg, tb);

        // Small beady eyes
        AddSphere(v, idx, -0.08f, 1.35f, 0.16f, 0.022f, 8, 0.72f, 0.55f, 0.15f);
        AddSphere(v, idx, 0.08f, 1.35f, 0.16f, 0.022f, 8, 0.72f, 0.55f, 0.15f);
        AddSphere(v, idx, -0.08f, 1.35f, 0.175f, 0.010f, 6, 0.12f, 0.08f, 0.05f);
        AddSphere(v, idx, 0.08f, 1.35f, 0.175f, 0.010f, 6, 0.12f, 0.08f, 0.05f);

        // Ears — pointed, floppy
        AddSphere(v, idx, -0.19f, 1.33f, 0, 0.040f, 8, sdr, sdg, sdb);
        AddSphere(v, idx, 0.19f, 1.33f, 0, 0.040f, 8, sdr, sdg, sdb);

        // ═══ ARMS — massive, long, gorilla-like ═══
        LoftLimb(v, idx, -0.34f, 0.94f, 0, new[]
        {
            R(0.00f, 0.16f, sr, sg, sb),                     // shoulder
            R(-0.06f, 0.14f, sr, sg, sb),
            R(-0.14f, 0.12f, sdr, sdg, sdb),                 // bicep
            R(-0.22f, 0.11f, sr, sg, sb),
            R(-0.30f, 0.10f, sr, sg, sb),                    // elbow
            R(-0.38f, 0.095f, sdr, sdg, sdb),
            R(-0.46f, 0.10f, sr, sg, sb),                    // forearm
            R(-0.54f, 0.095f, sr, sg, sb),
            R(-0.60f, 0.085f, sdr, sdg, sdb),                // wrist
            R(-0.66f, 0.08f, sr, sg, sb),                    // hand
            R(-0.72f, 0.06f, sdr, sdg, sdb),                 // fingers
            R(-0.78f, 0.03f, sdr, sdg, sdb),
            R(-0.80f, 0.00f, sdr, sdg, sdb),
        }, LimbSeg, angleX: -0.25f, angleZ: 0.05f);

        LoftLimb(v, idx, 0.34f, 0.94f, 0, new[]
        {
            R(0.00f, 0.16f, sr, sg, sb),
            R(-0.06f, 0.14f, sr, sg, sb),
            R(-0.14f, 0.12f, sdr, sdg, sdb),
            R(-0.22f, 0.11f, sr, sg, sb),
            R(-0.30f, 0.10f, sr, sg, sb),
            R(-0.38f, 0.095f, sdr, sdg, sdb),
            R(-0.46f, 0.10f, sr, sg, sb),
            R(-0.54f, 0.095f, sr, sg, sb),
            R(-0.60f, 0.085f, sdr, sdg, sdb),
            R(-0.66f, 0.08f, sr, sg, sb),
            R(-0.72f, 0.06f, sdr, sdg, sdb),
            R(-0.78f, 0.03f, sdr, sdg, sdb),
            R(-0.80f, 0.00f, sdr, sdg, sdb),
        }, LimbSeg, angleX: 0.25f, angleZ: 0.05f);

        // Shoulder humps
        AddSphere(v, idx, -0.36f, 1.02f, 0, 0.14f, DetailSeg, sdr, sdg, sdb);
        AddSphere(v, idx, 0.36f, 1.02f, 0, 0.14f, DetailSeg, sdr, sdg, sdb);

        // ═══ LEGS — stocky, trunk-like ═══
        LoftLimb(v, idx, -0.14f, 0.30f, 0, new[]
        {
            R(0.00f, 0.14f, sdr, sdg, sdb),
            R(-0.06f, 0.13f, sr, sg, sb),
            R(-0.12f, 0.12f, sr, sg, sb),
            R(-0.18f, 0.115f, sdr, sdg, sdb),
            R(-0.24f, 0.11f, sr, sg, sb),
            R(-0.30f, 0.105f, sr, sg, sb),
            R(-0.36f, 0.10f, sdr, sdg, sdb),
            R(-0.42f, 0.10f, sdr, sdg, sdb),
            RE(-0.48f, 0.10f, 0.12f, sdr - 0.04f, sdg - 0.04f, sdb - 0.04f),
            RE(-0.52f, 0.095f, 0.13f, sdr - 0.06f, sdg - 0.06f, sdb - 0.06f),
            RE(-0.54f, 0.00f, 0.00f, sdr - 0.06f, sdg - 0.06f, sdb - 0.06f),
        }, LimbSeg);

        LoftLimb(v, idx, 0.14f, 0.30f, 0, new[]
        {
            R(0.00f, 0.14f, sdr, sdg, sdb),
            R(-0.06f, 0.13f, sr, sg, sb),
            R(-0.12f, 0.12f, sr, sg, sb),
            R(-0.18f, 0.115f, sdr, sdg, sdb),
            R(-0.24f, 0.11f, sr, sg, sb),
            R(-0.30f, 0.105f, sr, sg, sb),
            R(-0.36f, 0.10f, sdr, sdg, sdb),
            R(-0.42f, 0.10f, sdr, sdg, sdb),
            RE(-0.48f, 0.10f, 0.12f, sdr - 0.04f, sdg - 0.04f, sdb - 0.04f),
            RE(-0.52f, 0.095f, 0.13f, sdr - 0.06f, sdg - 0.06f, sdb - 0.06f),
            RE(-0.54f, 0.00f, 0.00f, sdr - 0.06f, sdg - 0.06f, sdb - 0.06f),
        }, LimbSeg);

        // Loincloth
        Loft(v, idx, new[]
        {
            RE(0.34f, 0.30f, 0.03f, 0.32f, 0.24f, 0.14f, offsetZ: 0.10f),
            RE(0.24f, 0.28f, 0.04f, 0.30f, 0.22f, 0.12f, offsetZ: 0.08f),
            RE(0.14f, 0.22f, 0.03f, 0.28f, 0.20f, 0.10f, offsetZ: 0.06f),
        }, 10, capBottom: true, capTop: true);

        return new MeshData(v.ToArray(), idx.ToArray());
    }

    public static Mesh CreateTroll(GL gl)
    {
        var data = GenerateTrollData();
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
    }
}
