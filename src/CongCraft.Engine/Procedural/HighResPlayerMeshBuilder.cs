using Silk.NET.OpenGL;
using CongCraft.Engine.Rendering;
using static CongCraft.Engine.Procedural.MeshLofter;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates a high-resolution player warrior mesh with detailed anatomy,
/// layered plate armor, chainmail underlayer, and ornamental accessories.
/// Uses 24 segments per ring for smooth silhouettes and many more cross-sections
/// for accurate anatomical proportions (Gothic 2 / Dark Souls quality target).
/// </summary>
public static class HighResPlayerMeshBuilder
{
    private const int Seg = 24;       // High-res ring segments
    private const int LimbSeg = 16;   // Limb segments
    private const int DetailSeg = 12; // Detail elements

    // ── Skin tones ──
    private const float SkR = 0.72f, SkG = 0.58f, SkB = 0.48f;
    private const float SkDR = 0.68f, SkDG = 0.52f, SkDB = 0.42f; // shadow skin

    // ── Armor layers ──
    private const float PlR = 0.52f, PlG = 0.48f, PlB = 0.44f;   // plate steel
    private const float PlDR = 0.42f, PlDG = 0.38f, PlDB = 0.34f; // dark steel
    private const float ChR = 0.38f, ChG = 0.36f, ChB = 0.34f;   // chainmail
    private const float GdR = 0.78f, GdG = 0.62f, GdB = 0.18f;   // gold trim

    // ── Leather ──
    private const float LtR = 0.42f, LtG = 0.30f, LtB = 0.18f;
    private const float LtDR = 0.34f, LtDG = 0.24f, LtDB = 0.14f; // dark leather

    // ── Fabric / Cloth ──
    private const float CkR = 0.14f, CkG = 0.18f, CkB = 0.10f;   // cloak green
    private const float PnR = 0.30f, PnG = 0.22f, PnB = 0.14f;   // pants

    // ── Boots ──
    private const float BtR = 0.24f, BtG = 0.16f, BtB = 0.10f;

    // ── Hair ──
    private const float HrR = 0.22f, HrG = 0.14f, HrB = 0.08f;

    public static MeshData GenerateData()
    {
        var v = new List<float>(32000);
        var idx = new List<uint>(48000);

        BuildTorso(v, idx);
        BuildHead(v, idx);
        BuildLeftArm(v, idx);
        BuildRightArm(v, idx);
        BuildLeftLeg(v, idx);
        BuildRightLeg(v, idx);
        BuildArmorOverlays(v, idx);
        BuildBeltAndAccessories(v, idx);
        BuildCloak(v, idx);

        return new MeshData(v.ToArray(), idx.ToArray());
    }

    private static void BuildTorso(List<float> v, List<uint> idx)
    {
        // ═══ CHAINMAIL UNDERLAYER — visible at waist and neck ═══
        Loft(v, idx, new[]
        {
            RE(0.40f, 0.185f, 0.125f, ChR, ChG, ChB),
            RE(0.44f, 0.195f, 0.135f, ChR, ChG, ChB),
            RE(0.48f, 0.205f, 0.140f, ChR, ChG, ChB),
        }, Seg, capBottom: true, capTop: false);

        // ═══ PLATE ARMOR TORSO — detailed cuirass with muscle contours ═══
        Loft(v, idx, new[]
        {
            RE(0.48f, 0.210f, 0.145f, PlDR, PlDG, PlDB),     // armor waist edge
            RE(0.52f, 0.220f, 0.152f, PlR, PlG, PlB),         // lower abdomen plate
            RE(0.55f, 0.225f, 0.155f, PlR, PlG, PlB),         // abdomen
            RE(0.58f, 0.230f, 0.158f, PlR, PlG, PlB),         // upper abdomen
            RE(0.62f, 0.240f, 0.162f, PlR, PlG, PlB),         // ab ridge
            RE(0.65f, 0.245f, 0.164f, PlR, PlG, PlB),         // sternum area
            RE(0.68f, 0.250f, 0.168f, PlR, PlG, PlB),         // lower chest
            RE(0.72f, 0.258f, 0.172f, PlR, PlG, PlB),         // chest (widest)
            RE(0.76f, 0.255f, 0.170f, PlR, PlG, PlB),         // upper chest
            RE(0.80f, 0.248f, 0.166f, PlR, PlG, PlB),         // pectoral line
            RE(0.84f, 0.238f, 0.160f, PlR, PlG, PlB),         // upper pec
            RE(0.88f, 0.225f, 0.152f, PlDR, PlDG, PlDB),      // collar plate
            RE(0.92f, 0.210f, 0.142f, PlDR, PlDG, PlDB),      // shoulder base
            RE(0.96f, 0.190f, 0.130f, PlDR, PlDG, PlDB),      // gorget
            RE(1.00f, 0.160f, 0.110f, ChR, ChG, ChB),         // neck base chainmail
            RE(1.04f, 0.095f, 0.085f, ChR, ChG, ChB),         // neck chainmail coif
            RE(1.08f, 0.080f, 0.072f, SkR, SkG, SkB),         // neck skin
            RE(1.11f, 0.072f, 0.066f, SkR, SkG, SkB),         // neck mid
            RE(1.14f, 0.068f, 0.064f, SkR, SkG, SkB),         // neck upper
            RE(1.18f, 0.074f, 0.068f, SkR, SkG, SkB),         // jaw transition
        }, Seg, capBottom: false, capTop: false);

        // ═══ ARMOR RIDGE — central breastplate crest ═══
        Loft(v, idx, new[]
        {
            RE(0.55f, 0.03f, 0.015f, GdR, GdG, GdB, offsetZ: 0.16f),
            RE(0.65f, 0.035f, 0.018f, GdR, GdG, GdB, offsetZ: 0.168f),
            RE(0.75f, 0.035f, 0.018f, GdR, GdG, GdB, offsetZ: 0.175f),
            RE(0.82f, 0.025f, 0.012f, GdR, GdG, GdB, offsetZ: 0.168f),
        }, 8, capBottom: true, capTop: true);
    }

    private static void BuildHead(List<float> v, List<uint> idx)
    {
        // ═══ HEAD — detailed cranial structure ═══
        Loft(v, idx, new[]
        {
            RE(1.18f, 0.080f, 0.072f, SkR, SkG, SkB),        // jaw base
            RE(1.21f, 0.100f, 0.088f, SkR, SkG, SkB),        // chin
            RE(1.23f, 0.110f, 0.098f, SkR, SkG, SkB),        // jaw angle
            RE(1.25f, 0.118f, 0.105f, SkR, SkG, SkB),        // lower cheek
            RE(1.27f, 0.124f, 0.112f, SkR, SkG, SkB),        // cheekbone
            RE(1.29f, 0.126f, 0.116f, SkR, SkG, SkB),        // mid face
            RE(1.31f, 0.124f, 0.118f, SkR, SkG, SkB),        // eye level
            RE(1.33f, 0.122f, 0.118f, SkR, SkG, SkB),        // brow ridge
            RE(1.35f, 0.118f, 0.116f, SkDR, SkDG, SkDB),     // forehead
            RE(1.37f, 0.112f, 0.114f, HrR, HrG, HrB),        // hairline
            RE(1.39f, 0.105f, 0.112f, HrR, HrG, HrB),        // hair front
            RE(1.41f, 0.095f, 0.108f, HrR, HrG, HrB),        // hair mid
            RE(1.43f, 0.080f, 0.098f, HrR, HrG, HrB),        // hair top
            RE(1.45f, 0.055f, 0.070f, HrR, HrG, HrB),        // crown
            R(1.47f, 0.025f, HrR, HrG, HrB),                  // top
            R(1.48f, 0.000f, HrR, HrG, HrB),                  // pole
        }, Seg, capBottom: false, capTop: false);

        // Nose — multi-part for definition
        AddSphere(v, idx, 0, 1.29f, 0.125f, 0.022f, DetailSeg, SkR - 0.02f, SkG - 0.02f, SkB - 0.02f);
        AddSphere(v, idx, 0, 1.27f, 0.128f, 0.016f, 8, SkR - 0.01f, SkG - 0.01f, SkB - 0.01f); // tip

        // Eyes — inset spheres with irises
        AddSphere(v, idx, -0.045f, 1.31f, 0.108f, 0.016f, 10, 0.92f, 0.92f, 0.90f); // left white
        AddSphere(v, idx, 0.045f, 1.31f, 0.108f, 0.016f, 10, 0.92f, 0.92f, 0.90f);  // right white
        AddSphere(v, idx, -0.045f, 1.31f, 0.118f, 0.010f, 8, 0.28f, 0.42f, 0.22f);  // left iris
        AddSphere(v, idx, 0.045f, 1.31f, 0.118f, 0.010f, 8, 0.28f, 0.42f, 0.22f);   // right iris
        AddSphere(v, idx, -0.045f, 1.31f, 0.124f, 0.005f, 6, 0.05f, 0.05f, 0.05f);  // left pupil
        AddSphere(v, idx, 0.045f, 1.31f, 0.124f, 0.005f, 6, 0.05f, 0.05f, 0.05f);   // right pupil

        // Brow ridge detail
        AddCylinder(v, idx, -0.04f, 1.335f, 0.112f, 0.035f, 0.008f, 8, SkDR, SkDG, SkDB);
        AddCylinder(v, idx, 0.04f, 1.335f, 0.112f, 0.035f, 0.008f, 8, SkDR, SkDG, SkDB);

        // Ears — detailed
        AddSphere(v, idx, -0.125f, 1.30f, 0, 0.028f, 10, SkR, SkG, SkB);
        AddSphere(v, idx, 0.125f, 1.30f, 0, 0.028f, 10, SkR, SkG, SkB);
        AddSphere(v, idx, -0.130f, 1.30f, 0, 0.015f, 6, SkDR, SkDG, SkDB); // ear lobe
        AddSphere(v, idx, 0.130f, 1.30f, 0, 0.015f, 6, SkDR, SkDG, SkDB);

        // Lips
        AddCylinder(v, idx, 0, 1.24f, 0.11f, 0.028f, 0.006f, 8, SkR + 0.05f, SkG - 0.05f, SkB - 0.08f);

        // Chin detail
        AddSphere(v, idx, 0, 1.215f, 0.095f, 0.018f, 8, SkR, SkG, SkB);

        // Short beard stubble — subtle jawline detail
        Loft(v, idx, new[]
        {
            RE(1.21f, 0.035f, 0.02f, HrR + 0.08f, HrG + 0.06f, HrB + 0.04f, offsetZ: 0.09f),
            RE(1.18f, 0.06f, 0.03f, HrR + 0.06f, HrG + 0.04f, HrB + 0.02f, offsetZ: 0.07f),
            RE(1.15f, 0.04f, 0.02f, HrR + 0.04f, HrG + 0.02f, HrB),
        }, 8, capBottom: true, capTop: true);
    }

    private static void BuildLeftArm(List<float> v, List<uint> idx)
    {
        // ═══ LEFT ARM — armored upper, bare forearm with bracer ═══
        LoftLimb(v, idx, -0.26f, 0.92f, 0, new[]
        {
            R(0.00f, 0.105f, PlR, PlG, PlB),                // shoulder plate
            R(-0.03f, 0.098f, PlR, PlG, PlB),               // plate top
            R(-0.06f, 0.092f, PlDR, PlDG, PlDB),            // plate ridge
            R(-0.10f, 0.085f, ChR, ChG, ChB),               // chainmail upper arm
            R(-0.14f, 0.080f, SkR, SkG, SkB),               // bicep
            R(-0.18f, 0.078f, SkR, SkG, SkB),               // mid upper arm
            R(-0.22f, 0.074f, SkR, SkG, SkB),               // lower bicep
            R(-0.26f, 0.068f, SkR, SkG, SkB),               // above elbow
            R(-0.30f, 0.062f, SkR, SkG, SkB),               // elbow
            R(-0.32f, 0.058f, SkR, SkG, SkB),               // below elbow
            R(-0.34f, 0.060f, LtR, LtG, LtB),              // bracer start
            R(-0.38f, 0.062f, LtR, LtG, LtB),              // bracer mid
            R(-0.42f, 0.060f, LtR, LtG, LtB),              // bracer lower
            R(-0.46f, 0.055f, LtDR, LtDG, LtDB),           // bracer end
            R(-0.48f, 0.045f, SkR, SkG, SkB),               // wrist
            R(-0.52f, 0.042f, SkR, SkG, SkB),               // hand base
            R(-0.56f, 0.038f, SkR, SkG, SkB),               // palm
            R(-0.60f, 0.030f, SkR, SkG, SkB),               // fingers base
            R(-0.64f, 0.020f, SkR, SkG, SkB),               // fingers mid
            R(-0.67f, 0.010f, SkDR, SkDG, SkDB),            // fingertips
            R(-0.68f, 0.000f, SkDR, SkDG, SkDB),            // tip
        }, LimbSeg, angleX: -0.15f, angleZ: 0.1f);
    }

    private static void BuildRightArm(List<float> v, List<uint> idx)
    {
        // ═══ RIGHT ARM — mirrored ═══
        LoftLimb(v, idx, 0.26f, 0.92f, 0, new[]
        {
            R(0.00f, 0.105f, PlR, PlG, PlB),
            R(-0.03f, 0.098f, PlR, PlG, PlB),
            R(-0.06f, 0.092f, PlDR, PlDG, PlDB),
            R(-0.10f, 0.085f, ChR, ChG, ChB),
            R(-0.14f, 0.080f, SkR, SkG, SkB),
            R(-0.18f, 0.078f, SkR, SkG, SkB),
            R(-0.22f, 0.074f, SkR, SkG, SkB),
            R(-0.26f, 0.068f, SkR, SkG, SkB),
            R(-0.30f, 0.062f, SkR, SkG, SkB),
            R(-0.32f, 0.058f, SkR, SkG, SkB),
            R(-0.34f, 0.060f, LtR, LtG, LtB),
            R(-0.38f, 0.062f, LtR, LtG, LtB),
            R(-0.42f, 0.060f, LtR, LtG, LtB),
            R(-0.46f, 0.055f, LtDR, LtDG, LtDB),
            R(-0.48f, 0.045f, SkR, SkG, SkB),
            R(-0.52f, 0.042f, SkR, SkG, SkB),
            R(-0.56f, 0.038f, SkR, SkG, SkB),
            R(-0.60f, 0.030f, SkR, SkG, SkB),
            R(-0.64f, 0.020f, SkR, SkG, SkB),
            R(-0.67f, 0.010f, SkDR, SkDG, SkDB),
            R(-0.68f, 0.000f, SkDR, SkDG, SkDB),
        }, LimbSeg, angleX: 0.15f, angleZ: 0.1f);
    }

    private static void BuildLeftLeg(List<float> v, List<uint> idx)
    {
        // ═══ LEFT LEG — armored thigh, plated knee, leather boot ═══
        LoftLimb(v, idx, -0.10f, 0.40f, 0, new[]
        {
            R(0.00f, 0.095f, PnR, PnG, PnB),               // hip
            R(-0.04f, 0.092f, PnR, PnG, PnB),              // upper thigh
            R(-0.08f, 0.088f, ChR, ChG, ChB),               // chainmail thigh
            R(-0.12f, 0.084f, ChR, ChG, ChB),
            R(-0.16f, 0.080f, PnR, PnG, PnB),              // mid thigh
            R(-0.20f, 0.076f, PnR, PnG, PnB),
            R(-0.24f, 0.072f, PnR, PnG, PnB),              // above knee
            R(-0.28f, 0.068f, PlDR, PlDG, PlDB),            // knee plate top
            R(-0.30f, 0.072f, PlR, PlG, PlB),               // knee plate
            R(-0.32f, 0.068f, PlDR, PlDG, PlDB),            // knee plate bottom
            R(-0.36f, 0.062f, PnR, PnG, PnB),              // below knee
            R(-0.40f, 0.060f, PnR, PnG, PnB),              // shin upper
            R(-0.44f, 0.058f, PnR, PnG, PnB),              // shin mid
            R(-0.48f, 0.056f, BtR, BtG, BtB),              // boot top cuff
            R(-0.50f, 0.060f, BtR, BtG, BtB),              // boot shaft
            R(-0.54f, 0.062f, BtR, BtG, BtB),              // boot mid
            R(-0.58f, 0.064f, BtR, BtG, BtB),              // boot lower
            RE(-0.62f, 0.062f, 0.072f, BtR, BtG, BtB),     // ankle
            RE(-0.66f, 0.060f, 0.080f, BtR - 0.04f, BtG - 0.03f, BtB - 0.02f), // sole
            RE(-0.68f, 0.055f, 0.085f, BtR - 0.06f, BtG - 0.04f, BtB - 0.03f), // sole edge
            RE(-0.69f, 0.000f, 0.000f, BtR - 0.06f, BtG - 0.04f, BtB - 0.03f), // bottom
        }, LimbSeg);
    }

    private static void BuildRightLeg(List<float> v, List<uint> idx)
    {
        // ═══ RIGHT LEG — mirrored ═══
        LoftLimb(v, idx, 0.10f, 0.40f, 0, new[]
        {
            R(0.00f, 0.095f, PnR, PnG, PnB),
            R(-0.04f, 0.092f, PnR, PnG, PnB),
            R(-0.08f, 0.088f, ChR, ChG, ChB),
            R(-0.12f, 0.084f, ChR, ChG, ChB),
            R(-0.16f, 0.080f, PnR, PnG, PnB),
            R(-0.20f, 0.076f, PnR, PnG, PnB),
            R(-0.24f, 0.072f, PnR, PnG, PnB),
            R(-0.28f, 0.068f, PlDR, PlDG, PlDB),
            R(-0.30f, 0.072f, PlR, PlG, PlB),
            R(-0.32f, 0.068f, PlDR, PlDG, PlDB),
            R(-0.36f, 0.062f, PnR, PnG, PnB),
            R(-0.40f, 0.060f, PnR, PnG, PnB),
            R(-0.44f, 0.058f, PnR, PnG, PnB),
            R(-0.48f, 0.056f, BtR, BtG, BtB),
            R(-0.50f, 0.060f, BtR, BtG, BtB),
            R(-0.54f, 0.062f, BtR, BtG, BtB),
            R(-0.58f, 0.064f, BtR, BtG, BtB),
            RE(-0.62f, 0.062f, 0.072f, BtR, BtG, BtB),
            RE(-0.66f, 0.060f, 0.080f, BtR - 0.04f, BtG - 0.03f, BtB - 0.02f),
            RE(-0.68f, 0.055f, 0.085f, BtR - 0.06f, BtG - 0.04f, BtB - 0.03f),
            RE(-0.69f, 0.000f, 0.000f, BtR - 0.06f, BtG - 0.04f, BtB - 0.03f),
        }, LimbSeg);
    }

    private static void BuildArmorOverlays(List<float> v, List<uint> idx)
    {
        // ═══ SHOULDER PAULDRONS — layered plate armor ═══
        // Left pauldron - main dome
        AddSphere(v, idx, -0.28f, 1.00f, 0, 0.105f, DetailSeg, PlR, PlG, PlB);
        // Left pauldron - top ridge
        AddSphere(v, idx, -0.28f, 1.06f, 0, 0.06f, 10, PlDR, PlDG, PlDB);
        // Left pauldron - gold studs
        AddSphere(v, idx, -0.30f, 1.08f, 0.05f, 0.018f, 6, GdR, GdG, GdB);
        AddSphere(v, idx, -0.30f, 1.08f, -0.05f, 0.018f, 6, GdR, GdG, GdB);
        AddSphere(v, idx, -0.26f, 1.09f, 0, 0.018f, 6, GdR, GdG, GdB);
        // Left pauldron - edge rim
        AddCylinder(v, idx, -0.28f, 0.95f, 0, 0.10f, 0.012f, DetailSeg, GdR * 0.8f, GdG * 0.8f, GdB * 0.8f);

        // Right pauldron - mirror
        AddSphere(v, idx, 0.28f, 1.00f, 0, 0.105f, DetailSeg, PlR, PlG, PlB);
        AddSphere(v, idx, 0.28f, 1.06f, 0, 0.06f, 10, PlDR, PlDG, PlDB);
        AddSphere(v, idx, 0.30f, 1.08f, 0.05f, 0.018f, 6, GdR, GdG, GdB);
        AddSphere(v, idx, 0.30f, 1.08f, -0.05f, 0.018f, 6, GdR, GdG, GdB);
        AddSphere(v, idx, 0.26f, 1.09f, 0, 0.018f, 6, GdR, GdG, GdB);
        AddCylinder(v, idx, 0.28f, 0.95f, 0, 0.10f, 0.012f, DetailSeg, GdR * 0.8f, GdG * 0.8f, GdB * 0.8f);

        // ═══ KNEE GUARDS — plate + stud ═══
        AddSphere(v, idx, -0.10f, 0.10f, 0.05f, 0.042f, 10, PlR, PlG, PlB);
        AddSphere(v, idx, -0.10f, 0.10f, 0.065f, 0.015f, 6, GdR, GdG, GdB);
        AddSphere(v, idx, 0.10f, 0.10f, 0.05f, 0.042f, 10, PlR, PlG, PlB);
        AddSphere(v, idx, 0.10f, 0.10f, 0.065f, 0.015f, 6, GdR, GdG, GdB);

        // ═══ GORGET — neck armor collar ═══
        Loft(v, idx, new[]
        {
            RE(0.98f, 0.175f, 0.120f, PlDR, PlDG, PlDB),
            RE(1.01f, 0.165f, 0.115f, PlR, PlG, PlB),
            RE(1.03f, 0.150f, 0.105f, PlDR, PlDG, PlDB),
        }, Seg, capBottom: false, capTop: false);
    }

    private static void BuildBeltAndAccessories(List<float> v, List<uint> idx)
    {
        // ═══ BELT — ornate leather with gold buckle ═══
        Loft(v, idx, new[]
        {
            R(0.44f, 0.225f, LtDR, LtDG, LtDB),
            R(0.46f, 0.232f, LtR, LtG, LtB),
            R(0.48f, 0.235f, LtR, LtG, LtB),
            R(0.50f, 0.232f, LtR, LtG, LtB),
            R(0.52f, 0.225f, LtDR, LtDG, LtDB),
        }, Seg, capBottom: false, capTop: false);

        // Belt buckle — detailed gold
        AddBox(v, idx, 0, 0.48f, 0.24f, 0.04f, 0.06f, 0.015f, GdR, GdG, GdB);
        AddSphere(v, idx, 0, 0.48f, 0.252f, 0.012f, 6, GdR + 0.08f, GdG + 0.06f, GdB + 0.04f);

        // Belt pouch (right)
        AddSphere(v, idx, 0.18f, 0.47f, 0.12f, 0.038f, 8, LtR - 0.05f, LtG - 0.03f, LtB - 0.02f);
        AddSphere(v, idx, 0.18f, 0.49f, 0.13f, 0.016f, 6, GdR * 0.7f, GdG * 0.7f, GdB * 0.7f); // clasp

        // Belt pouch (left, smaller)
        AddSphere(v, idx, -0.16f, 0.46f, 0.10f, 0.030f, 6, LtR - 0.04f, LtG - 0.02f, LtB - 0.01f);

        // Dagger scabbard
        AddCylinder(v, idx, -0.20f, 0.36f, 0.09f, 0.020f, 0.18f, 8, LtDR, LtDG, LtDB);
        // Dagger pommel
        AddSphere(v, idx, -0.20f, 0.46f, 0.09f, 0.016f, 6, GdR, GdG, GdB);
        // Dagger guard
        AddCylinder(v, idx, -0.20f, 0.44f, 0.09f, 0.022f, 0.008f, 6, GdR * 0.8f, GdG * 0.8f, GdB * 0.8f);

        // Coin purse
        AddSphere(v, idx, 0.10f, 0.45f, 0.14f, 0.022f, 6, GdR * 0.6f, GdG * 0.6f, GdB * 0.6f);
    }

    private static void BuildCloak(List<float> v, List<uint> idx)
    {
        // ═══ CLOAK — flowing cape from shoulders ═══
        Loft(v, idx, new[]
        {
            RE(0.98f, 0.20f, 0.020f, CkR, CkG, CkB, offsetZ: -0.13f),
            RE(0.94f, 0.22f, 0.025f, CkR, CkG, CkB, offsetZ: -0.14f),
            RE(0.88f, 0.24f, 0.030f, CkR, CkG, CkB, offsetZ: -0.15f),
            RE(0.80f, 0.26f, 0.035f, CkR, CkG, CkB, offsetZ: -0.16f),
            RE(0.72f, 0.27f, 0.038f, CkR, CkG, CkB, offsetZ: -0.17f),
            RE(0.64f, 0.28f, 0.040f, CkR, CkG, CkB, offsetZ: -0.18f),
            RE(0.56f, 0.27f, 0.042f, CkR - 0.01f, CkG - 0.01f, CkB, offsetZ: -0.19f),
            RE(0.48f, 0.26f, 0.040f, CkR - 0.02f, CkG - 0.02f, CkB - 0.01f, offsetZ: -0.18f),
            RE(0.40f, 0.24f, 0.035f, CkR - 0.02f, CkG - 0.02f, CkB - 0.01f, offsetZ: -0.17f),
            RE(0.34f, 0.20f, 0.025f, CkR - 0.03f, CkG - 0.03f, CkB - 0.02f, offsetZ: -0.16f),
        }, Seg / 2, capBottom: true, capTop: true);

        // Cloak clasp — ornate gold
        AddSphere(v, idx, -0.08f, 1.02f, -0.11f, 0.022f, 8, GdR, GdG, GdB);
        AddSphere(v, idx, 0.08f, 1.02f, -0.11f, 0.022f, 8, GdR, GdG, GdB);
        // Chain between clasps
        AddCylinder(v, idx, 0, 1.02f, -0.10f, 0.05f, 0.006f, 6, GdR * 0.8f, GdG * 0.8f, GdB * 0.8f);

        // Cloak trim — gold hem
        Loft(v, idx, new[]
        {
            RE(0.36f, 0.22f, 0.028f, GdR * 0.7f, GdG * 0.7f, GdB * 0.7f, offsetZ: -0.165f),
            RE(0.34f, 0.205f, 0.027f, GdR * 0.7f, GdG * 0.7f, GdB * 0.7f, offsetZ: -0.160f),
        }, 8, capBottom: false, capTop: false);
    }

    public static Mesh Create(GL gl)
    {
        var data = GenerateData();
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
    }
}
