using Silk.NET.OpenGL;
using CongCraft.Engine.Rendering;
using static CongCraft.Engine.Procedural.MeshLofter;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates high-resolution NPC meshes with detailed facial features,
/// clothing folds, and unique character accessories.
/// Three variants: Blacksmith, Elder, Merchant — each with distinct
/// body proportions and equipment.
/// </summary>
public static class HighResNpcMeshBuilder
{
    private const int Seg = 24;
    private const int LimbSeg = 14;
    private const int DetailSeg = 12;

    /// <summary>
    /// High-resolution blacksmith — stocky, muscular build with leather apron,
    /// soot-stained skin, heavy arms, bald head with thick beard.
    /// </summary>
    public static MeshData GenerateBlacksmith()
    {
        var v = new List<float>(26000);
        var idx = new List<uint>(39000);

        const float sk = 0.66f, sg = 0.50f, sb = 0.40f;    // soot-tanned skin
        const float skd = 0.58f, sgd = 0.44f, sbd = 0.34f;  // dark skin
        const float lr = 0.44f, lg = 0.30f, lb = 0.15f;      // leather
        const float ldr = 0.36f, ldg = 0.24f, ldb = 0.12f;   // dark leather
        const float pr = 0.36f, pg = 0.26f, pb = 0.16f;       // pants
        const float mr = 0.50f, mg = 0.48f, mb = 0.45f;       // metal
        const float br = 0.28f, bg = 0.18f, bb = 0.10f;       // beard brown

        // ═══ TORSO — stocky, barrel-chested build ═══
        Loft(v, idx, new[]
        {
            RE(0.38f, 0.24f, 0.18f, lr, lg, lb),
            RE(0.42f, 0.26f, 0.20f, lr, lg, lb),
            RE(0.46f, 0.28f, 0.22f, lr, lg, lb),
            RE(0.50f, 0.30f, 0.24f, lr, lg, lb),             // belly
            RE(0.54f, 0.30f, 0.24f, lr, lg, lb),
            RE(0.58f, 0.29f, 0.22f, lr, lg, lb),
            RE(0.62f, 0.30f, 0.22f, sk, sg, sb),             // chest
            RE(0.66f, 0.31f, 0.22f, sk, sg, sb),             // pecs
            RE(0.70f, 0.30f, 0.21f, sk, sg, sb),
            RE(0.74f, 0.28f, 0.20f, sk, sg, sb),
            RE(0.78f, 0.26f, 0.18f, sk, sg, sb),
            RE(0.82f, 0.24f, 0.17f, sk, sg, sb),
            RE(0.88f, 0.22f, 0.15f, sk, sg, sb),             // shoulders
            RE(0.94f, 0.18f, 0.13f, sk, sg, sb),             // traps
            RE(1.00f, 0.12f, 0.10f, sk, sg, sb),             // neck base
            R(1.04f, 0.10f, sk, sg, sb),                       // thick neck
            R(1.08f, 0.095f, sk, sg, sb),
            R(1.12f, 0.092f, sk, sg, sb),                     // neck top
        }, Seg, capBottom: true, capTop: false);

        // ═══ HEAD — bald, weathered ═══
        Loft(v, idx, new[]
        {
            R(1.12f, 0.095f, sk, sg, sb),                     // jaw base
            RE(1.15f, 0.115f, 0.105f, sk, sg, sb),            // jaw
            RE(1.18f, 0.130f, 0.120f, sk, sg, sb),            // cheeks
            RE(1.21f, 0.140f, 0.130f, sk, sg, sb),
            RE(1.24f, 0.145f, 0.135f, sk, sg, sb),            // mid face
            RE(1.27f, 0.142f, 0.136f, sk, sg, sb),            // brow
            RE(1.30f, 0.138f, 0.134f, skd, sgd, sbd),         // forehead
            RE(1.33f, 0.130f, 0.128f, skd, sgd, sbd),         // bald dome
            RE(1.36f, 0.118f, 0.118f, skd, sgd, sbd),
            RE(1.39f, 0.095f, 0.100f, skd, sgd, sbd),
            R(1.42f, 0.060f, skd, sgd, sbd),
            R(1.44f, 0.025f, skd, sgd, sbd),
            R(1.45f, 0.000f, skd, sgd, sbd),
        }, Seg, capBottom: false, capTop: false);

        // Face details
        AddSphere(v, idx, 0, 1.22f, 0.13f, 0.024f, 10, sk - 0.02f, sg - 0.02f, sb - 0.02f); // nose
        AddSphere(v, idx, -0.05f, 1.25f, 0.12f, 0.016f, 8, 0.90f, 0.88f, 0.84f); // eye white L
        AddSphere(v, idx, 0.05f, 1.25f, 0.12f, 0.016f, 8, 0.90f, 0.88f, 0.84f);  // eye white R
        AddSphere(v, idx, -0.05f, 1.25f, 0.13f, 0.008f, 6, 0.32f, 0.24f, 0.12f); // iris L
        AddSphere(v, idx, 0.05f, 1.25f, 0.13f, 0.008f, 6, 0.32f, 0.24f, 0.12f);  // iris R

        // Thick bushy beard
        Loft(v, idx, new[]
        {
            RE(1.18f, 0.06f, 0.04f, br, bg, bb, offsetZ: 0.12f),
            RE(1.14f, 0.10f, 0.07f, br, bg, bb, offsetZ: 0.10f),
            RE(1.08f, 0.12f, 0.08f, br, bg, bb, offsetZ: 0.08f),
            RE(1.02f, 0.10f, 0.06f, br - 0.02f, bg - 0.02f, bb, offsetZ: 0.06f),
            RE(0.96f, 0.06f, 0.04f, br - 0.04f, bg - 0.02f, bb, offsetZ: 0.04f),
        }, 10, capBottom: true, capTop: true);

        // Scar on cheek
        AddCylinder(v, idx, 0.08f, 1.22f, 0.13f, 0.002f, 0.04f, 4, sk + 0.10f, sg - 0.08f, sb - 0.10f);

        // ═══ ARMS — massive, muscular ═══
        LoftLimb(v, idx, -0.28f, 0.86f, 0, new[]
        {
            R(0.00f, 0.115f, sk, sg, sb),                    // shoulder
            R(-0.04f, 0.108f, sk, sg, sb),
            R(-0.08f, 0.105f, sk, sg, sb),                   // bulging bicep
            R(-0.12f, 0.102f, sk, sg, sb),
            R(-0.16f, 0.098f, sk, sg, sb),
            R(-0.20f, 0.090f, sk, sg, sb),
            R(-0.24f, 0.082f, sk, sg, sb),                   // elbow
            R(-0.28f, 0.078f, lr, lg, lb),                   // bracer
            R(-0.32f, 0.080f, lr, lg, lb),
            R(-0.36f, 0.078f, lr, lg, lb),
            R(-0.40f, 0.072f, ldr, ldg, ldb),                // bracer end
            R(-0.44f, 0.060f, sk, sg, sb),                   // wrist
            R(-0.48f, 0.055f, sk, sg, sb),                   // hand
            R(-0.52f, 0.045f, sk, sg, sb),
            R(-0.56f, 0.030f, skd, sgd, sbd),
            R(-0.58f, 0.000f, skd, sgd, sbd),
        }, LimbSeg, angleX: -0.22f, angleZ: 0.1f);

        LoftLimb(v, idx, 0.28f, 0.86f, 0, new[]
        {
            R(0.00f, 0.115f, sk, sg, sb),
            R(-0.04f, 0.108f, sk, sg, sb),
            R(-0.08f, 0.105f, sk, sg, sb),
            R(-0.12f, 0.102f, sk, sg, sb),
            R(-0.16f, 0.098f, sk, sg, sb),
            R(-0.20f, 0.090f, sk, sg, sb),
            R(-0.24f, 0.082f, sk, sg, sb),
            R(-0.28f, 0.078f, lr, lg, lb),
            R(-0.32f, 0.080f, lr, lg, lb),
            R(-0.36f, 0.078f, lr, lg, lb),
            R(-0.40f, 0.072f, ldr, ldg, ldb),
            R(-0.44f, 0.060f, sk, sg, sb),
            R(-0.48f, 0.055f, sk, sg, sb),
            R(-0.52f, 0.045f, sk, sg, sb),
            R(-0.56f, 0.030f, skd, sgd, sbd),
            R(-0.58f, 0.000f, skd, sgd, sbd),
        }, LimbSeg, angleX: 0.22f, angleZ: 0.1f);

        // ═══ LEGS — sturdy ═══
        LoftLimb(v, idx, -0.13f, 0.38f, 0, new[]
        {
            R(0.00f, 0.105f, pr, pg, pb),
            R(-0.06f, 0.100f, pr, pg, pb),
            R(-0.12f, 0.095f, pr, pg, pb),
            R(-0.18f, 0.090f, pr, pg, pb),
            R(-0.24f, 0.085f, pr, pg, pb),
            R(-0.30f, 0.078f, pr, pg, pb),
            R(-0.36f, 0.072f, pr, pg, pb),
            R(-0.42f, 0.070f, 0.26f, 0.17f, 0.08f),         // boot
            R(-0.48f, 0.074f, 0.26f, 0.17f, 0.08f),
            R(-0.54f, 0.076f, 0.26f, 0.17f, 0.08f),
            RE(-0.58f, 0.070f, 0.085f, 0.24f, 0.15f, 0.07f),
            RE(-0.60f, 0.000f, 0.000f, 0.24f, 0.15f, 0.07f),
        }, LimbSeg);

        LoftLimb(v, idx, 0.13f, 0.38f, 0, new[]
        {
            R(0.00f, 0.105f, pr, pg, pb),
            R(-0.06f, 0.100f, pr, pg, pb),
            R(-0.12f, 0.095f, pr, pg, pb),
            R(-0.18f, 0.090f, pr, pg, pb),
            R(-0.24f, 0.085f, pr, pg, pb),
            R(-0.30f, 0.078f, pr, pg, pb),
            R(-0.36f, 0.072f, pr, pg, pb),
            R(-0.42f, 0.070f, 0.26f, 0.17f, 0.08f),
            R(-0.48f, 0.074f, 0.26f, 0.17f, 0.08f),
            R(-0.54f, 0.076f, 0.26f, 0.17f, 0.08f),
            RE(-0.58f, 0.070f, 0.085f, 0.24f, 0.15f, 0.07f),
            RE(-0.60f, 0.000f, 0.000f, 0.24f, 0.15f, 0.07f),
        }, LimbSeg);

        // ═══ LEATHER APRON — detailed with burn marks ═══
        Loft(v, idx, new[]
        {
            RE(0.78f, 0.22f, 0.025f, ldr, ldg, ldb, offsetZ: 0.08f),
            RE(0.70f, 0.26f, 0.030f, lr, lg, lb, offsetZ: 0.10f),
            RE(0.60f, 0.28f, 0.035f, lr, lg, lb, offsetZ: 0.12f),
            RE(0.50f, 0.27f, 0.035f, lr, lg, lb, offsetZ: 0.10f),
            RE(0.42f, 0.25f, 0.030f, ldr, ldg, ldb, offsetZ: 0.08f),
            RE(0.34f, 0.22f, 0.025f, ldr - 0.02f, ldg - 0.02f, ldb, offsetZ: 0.06f),
        }, DetailSeg, capBottom: true, capTop: true);

        // Apron straps
        AddCylinder(v, idx, -0.10f, 0.88f, 0.08f, 0.012f, 0.16f, 6, ldr, ldg, ldb);
        AddCylinder(v, idx, 0.10f, 0.88f, 0.08f, 0.012f, 0.16f, 6, ldr, ldg, ldb);

        // Belt + hammer
        Loft(v, idx, new[]
        {
            R(0.42f, 0.26f, 0.42f, 0.28f, 0.12f),
            R(0.44f, 0.27f, 0.42f, 0.28f, 0.12f),
            R(0.46f, 0.26f, 0.42f, 0.28f, 0.12f),
        }, Seg, capBottom: false, capTop: false);

        // Hammer handle
        AddCylinder(v, idx, -0.24f, 0.40f, 0.10f, 0.016f, 0.28f, 8, 0.46f, 0.34f, 0.18f);
        // Hammer head
        AddBox(v, idx, -0.24f, 0.56f, 0.10f, 0.05f, 0.06f, 0.04f, mr, mg, mb);
        AddSphere(v, idx, -0.24f, 0.56f, 0.10f, 0.032f, 8, mr + 0.05f, mg + 0.04f, mb + 0.03f);

        // Tongs at belt
        AddCylinder(v, idx, 0.22f, 0.38f, 0.12f, 0.008f, 0.20f, 4, mr - 0.05f, mg - 0.05f, mb - 0.05f);
        AddCylinder(v, idx, 0.24f, 0.38f, 0.12f, 0.008f, 0.18f, 4, mr - 0.05f, mg - 0.05f, mb - 0.05f);

        return new MeshData(v.ToArray(), idx.ToArray());
    }

    /// <summary>
    /// High-resolution elder — tall, slender mage with flowing robes,
    /// ornate staff, long white beard, and mystical crystal.
    /// </summary>
    public static MeshData GenerateElder()
    {
        var v = new List<float>(28000);
        var idx = new List<uint>(42000);

        const float rr = 0.36f, rg = 0.20f, rb = 0.46f;     // purple robe
        const float rdr = 0.28f, rdg = 0.14f, rdb = 0.38f;   // dark robe
        const float sk = 0.64f, skg = 0.58f, skb = 0.52f;     // aged skin
        const float wr = 0.84f, wg = 0.82f, wb = 0.78f;       // white hair
        const float gr = 0.72f, gg = 0.55f, gb = 0.15f;        // gold
        const float cr = 0.48f, cg = 0.28f, cb = 0.78f;        // crystal purple

        // ═══ BODY — slender robed figure ═══
        Loft(v, idx, new[]
        {
            R(0.04f, 0.30f, rdr, rdg, rdb),                  // robe hem
            R(0.08f, 0.29f, rr, rg, rb),
            R(0.12f, 0.28f, rr, rg, rb),
            R(0.16f, 0.27f, rr, rg, rb),
            R(0.22f, 0.25f, rr, rg, rb),
            R(0.28f, 0.23f, rr, rg, rb),                      // lower robe
            R(0.34f, 0.22f, rr, rg, rb),
            R(0.40f, 0.21f, rr, rg, rb),
            R(0.46f, 0.215f, rr, rg, rb),
            R(0.52f, 0.22f, rr, rg, rb),
            R(0.58f, 0.225f, rr, rg, rb),                     // chest
            R(0.64f, 0.23f, rr, rg, rb),
            R(0.70f, 0.225f, rr, rg, rb),
            R(0.76f, 0.22f, rr, rg, rb),
            R(0.82f, 0.21f, rr, rg, rb),
            R(0.88f, 0.19f, rr, rg, rb),
            R(0.94f, 0.16f, rdr, rdg, rdb),                   // shoulders
            R(1.00f, 0.12f, rdr, rdg, rdb),
            R(1.04f, 0.08f, sk, skg, skb),                    // neck
            R(1.08f, 0.072f, sk, skg, skb),
            R(1.12f, 0.068f, sk, skg, skb),
        }, Seg, capBottom: true, capTop: false);

        // Gold hem trim
        Loft(v, idx, new[]
        {
            R(0.04f, 0.31f, gr, gg, gb),
            R(0.06f, 0.30f, gr, gg, gb),
            R(0.08f, 0.29f, gr, gg, gb),
        }, Seg, capBottom: false, capTop: false);

        // Robe V-neckline detail
        Loft(v, idx, new[]
        {
            RE(0.94f, 0.08f, 0.01f, rdr - 0.04f, rdg - 0.02f, rdb - 0.04f, offsetZ: 0.12f),
            RE(0.82f, 0.06f, 0.008f, rdr - 0.04f, rdg - 0.02f, rdb - 0.04f, offsetZ: 0.14f),
            RE(0.72f, 0.04f, 0.006f, rdr - 0.04f, rdg - 0.02f, rdb - 0.04f, offsetZ: 0.15f),
        }, 6, capBottom: true, capTop: true);

        // ═══ HEAD — wise elder with wrinkles ═══
        Loft(v, idx, new[]
        {
            R(1.12f, 0.072f, sk, skg, skb),
            RE(1.15f, 0.095f, 0.088f, sk, skg, skb),          // chin
            RE(1.18f, 0.112f, 0.104f, sk, skg, skb),
            RE(1.21f, 0.125f, 0.116f, sk, skg, skb),
            RE(1.24f, 0.132f, 0.124f, sk, skg, skb),          // cheekbones
            RE(1.27f, 0.134f, 0.128f, sk, skg, skb),          // mid face
            RE(1.30f, 0.130f, 0.126f, sk, skg, skb),          // brow
            RE(1.33f, 0.125f, 0.124f, wr, wg, wb),            // hair starts
            RE(1.36f, 0.118f, 0.120f, wr, wg, wb),
            RE(1.39f, 0.105f, 0.110f, wr, wg, wb),
            R(1.42f, 0.080f, wr, wg, wb),
            R(1.44f, 0.050f, wr, wg, wb),
            R(1.46f, 0.020f, wr, wg, wb),
            R(1.47f, 0.000f, wr, wg, wb),
        }, Seg, capBottom: false, capTop: false);

        // Eyes — kind, old
        AddSphere(v, idx, -0.04f, 1.28f, 0.12f, 0.014f, 8, 0.90f, 0.88f, 0.84f);
        AddSphere(v, idx, 0.04f, 1.28f, 0.12f, 0.014f, 8, 0.90f, 0.88f, 0.84f);
        AddSphere(v, idx, -0.04f, 1.28f, 0.128f, 0.008f, 6, 0.32f, 0.42f, 0.58f); // blue iris
        AddSphere(v, idx, 0.04f, 1.28f, 0.128f, 0.008f, 6, 0.32f, 0.42f, 0.58f);

        // Nose
        AddSphere(v, idx, 0, 1.24f, 0.13f, 0.022f, 8, sk - 0.01f, skg - 0.01f, skb - 0.01f);

        // Bushy white eyebrows
        AddCylinder(v, idx, -0.04f, 1.30f, 0.12f, 0.025f, 0.010f, 6, wr, wg, wb);
        AddCylinder(v, idx, 0.04f, 1.30f, 0.12f, 0.025f, 0.010f, 6, wr, wg, wb);

        // Long flowing beard
        Loft(v, idx, new[]
        {
            RE(1.20f, 0.05f, 0.03f, wr, wg, wb, offsetZ: 0.12f),
            RE(1.14f, 0.08f, 0.05f, wr, wg, wb, offsetZ: 0.10f),
            RE(1.06f, 0.09f, 0.06f, wr, wg, wb, offsetZ: 0.09f),
            RE(0.96f, 0.08f, 0.05f, wr - 0.02f, wg - 0.02f, wb - 0.02f, offsetZ: 0.08f),
            RE(0.86f, 0.06f, 0.04f, wr - 0.02f, wg - 0.02f, wb - 0.02f, offsetZ: 0.07f),
            RE(0.78f, 0.04f, 0.03f, wr - 0.04f, wg - 0.04f, wb - 0.04f, offsetZ: 0.06f),
            R(0.72f, 0.02f, wr - 0.04f, wg - 0.04f, wb - 0.04f, offsetZ: 0.05f),
        }, 10, capBottom: true, capTop: true);

        // ═══ ARMS — wide flowing sleeves ═══
        LoftLimb(v, idx, -0.20f, 0.86f, 0, new[]
        {
            R(0.00f, 0.11f, rr, rg, rb),
            R(-0.05f, 0.12f, rr, rg, rb),                    // wide sleeve
            R(-0.10f, 0.125f, rr, rg, rb),
            R(-0.16f, 0.12f, rr, rg, rb),
            R(-0.22f, 0.11f, rr, rg, rb),
            R(-0.28f, 0.10f, rr, rg, rb),
            R(-0.34f, 0.085f, rr, rg, rb),
            R(-0.38f, 0.070f, rr, rg, rb),
            R(-0.40f, 0.060f, gr, gg, gb),                   // gold cuff
            R(-0.42f, 0.055f, gr, gg, gb),
            R(-0.44f, 0.045f, sk, skg, skb),                 // hand
            R(-0.48f, 0.038f, sk, skg, skb),
            R(-0.52f, 0.025f, sk, skg, skb),
            R(-0.55f, 0.010f, sk, skg, skb),
            R(-0.56f, 0.000f, sk, skg, skb),
        }, 10, angleX: -0.22f, angleZ: 0.12f);

        LoftLimb(v, idx, 0.20f, 0.86f, 0, new[]
        {
            R(0.00f, 0.11f, rr, rg, rb),
            R(-0.05f, 0.12f, rr, rg, rb),
            R(-0.10f, 0.125f, rr, rg, rb),
            R(-0.16f, 0.12f, rr, rg, rb),
            R(-0.22f, 0.11f, rr, rg, rb),
            R(-0.28f, 0.10f, rr, rg, rb),
            R(-0.34f, 0.085f, rr, rg, rb),
            R(-0.38f, 0.070f, rr, rg, rb),
            R(-0.40f, 0.060f, gr, gg, gb),
            R(-0.42f, 0.055f, gr, gg, gb),
            R(-0.44f, 0.045f, sk, skg, skb),
            R(-0.48f, 0.038f, sk, skg, skb),
            R(-0.52f, 0.025f, sk, skg, skb),
            R(-0.55f, 0.010f, sk, skg, skb),
            R(-0.56f, 0.000f, sk, skg, skb),
        }, 10, angleX: 0.22f, angleZ: 0.12f);

        // ═══ GOLD SASH BELT ═══
        Loft(v, idx, new[]
        {
            R(0.48f, 0.23f, gr, gg, gb),
            R(0.50f, 0.235f, gr, gg, gb),
            R(0.52f, 0.24f, gr, gg, gb),
            R(0.54f, 0.235f, gr, gg, gb),
            R(0.56f, 0.23f, gr, gg, gb),
        }, Seg, capBottom: false, capTop: false);

        // Pendant on chain
        AddCylinder(v, idx, 0, 0.90f, 0.15f, 0.04f, 0.004f, 6, gr * 0.8f, gg * 0.8f, gb * 0.8f); // chain
        AddSphere(v, idx, 0, 0.84f, 0.16f, 0.025f, 8, gr, gg, gb);

        // ═══ STAFF — ornate with crystal ═══
        Loft(v, idx, new[]
        {
            R(0.04f, 0.026f, 0.46f, 0.36f, 0.22f, offsetX: 0.44f),
            R(0.20f, 0.026f, 0.46f, 0.36f, 0.22f, offsetX: 0.44f),
            R(0.40f, 0.028f, 0.44f, 0.34f, 0.20f, offsetX: 0.44f),
            R(0.60f, 0.028f, 0.46f, 0.36f, 0.22f, offsetX: 0.44f),
            R(0.76f, 0.032f, 0.42f, 0.32f, 0.18f, offsetX: 0.44f),  // knot
            R(0.80f, 0.028f, 0.46f, 0.36f, 0.22f, offsetX: 0.44f),
            R(1.00f, 0.026f, 0.46f, 0.36f, 0.22f, offsetX: 0.44f),
            R(1.20f, 0.024f, 0.44f, 0.34f, 0.20f, offsetX: 0.44f),
            R(1.40f, 0.022f, 0.44f, 0.34f, 0.20f, offsetX: 0.44f),
            R(1.50f, 0.018f, 0.42f, 0.32f, 0.18f, offsetX: 0.44f),
        }, 8, capBottom: true, capTop: true);

        // Staff crown (holds crystal)
        AddCylinder(v, idx, 0.44f, 1.54f, 0, 0.025f, 0.04f, 8, gr, gg, gb);

        // Crystal orb — layered for glow effect
        AddSphere(v, idx, 0.44f, 1.60f, 0, 0.060f, DetailSeg, cr * 0.7f, cg * 0.7f, cb * 0.7f);
        AddSphere(v, idx, 0.44f, 1.60f, 0, 0.045f, 10, cr, cg, cb);
        AddSphere(v, idx, 0.44f, 1.60f, 0, 0.028f, 8, cr + 0.15f, cg + 0.15f, cb + 0.10f); // inner glow

        // Rune symbols on robe (small gold details)
        AddSphere(v, idx, 0, 0.70f, 0.23f, 0.012f, 4, gr, gg, gb);
        AddSphere(v, idx, 0, 0.60f, 0.225f, 0.010f, 4, gr, gg, gb);
        AddSphere(v, idx, 0, 0.50f, 0.22f, 0.010f, 4, gr, gg, gb);

        return new MeshData(v.ToArray(), idx.ToArray());
    }

    /// <summary>
    /// High-resolution merchant — rotund, well-dressed with fancy hat,
    /// coin pouches, and a shrewd expression.
    /// </summary>
    public static MeshData GenerateMerchant()
    {
        var v = new List<float>(26000);
        var idx = new List<uint>(39000);

        const float tr = 0.26f, tg = 0.46f, tb = 0.20f;     // rich green tunic
        const float tdr = 0.18f, tdg = 0.36f, tdb = 0.14f;   // dark tunic
        const float sk = 0.64f, skg = 0.52f, skb = 0.42f;     // skin
        const float pr = 0.30f, pg = 0.20f, pb = 0.12f;        // brown pants
        const float hr = 0.34f, hg = 0.22f, hb = 0.12f;        // brown hair
        const float gr = 0.64f, gg = 0.50f, gb = 0.14f;        // gold
        const float htr = 0.48f, htg = 0.34f, htb = 0.14f;     // hat brown

        // ═══ BODY — round merchant build ═══
        Loft(v, idx, new[]
        {
            RE(0.38f, 0.22f, 0.18f, tr, tg, tb),
            RE(0.42f, 0.25f, 0.20f, tr, tg, tb),
            RE(0.46f, 0.28f, 0.24f, tr, tg, tb),             // belly starts
            RE(0.50f, 0.30f, 0.26f, tr, tg, tb),             // big belly
            RE(0.54f, 0.31f, 0.27f, tr, tg, tb),             // belly peak
            RE(0.58f, 0.30f, 0.26f, tr, tg, tb),
            RE(0.62f, 0.28f, 0.24f, tr, tg, tb),
            RE(0.66f, 0.26f, 0.22f, tr, tg, tb),
            RE(0.70f, 0.25f, 0.20f, tr, tg, tb),             // chest
            RE(0.74f, 0.24f, 0.18f, tr, tg, tb),
            RE(0.78f, 0.23f, 0.17f, tr, tg, tb),
            RE(0.82f, 0.22f, 0.16f, tr, tg, tb),
            RE(0.86f, 0.20f, 0.15f, tr, tg, tb),
            RE(0.90f, 0.18f, 0.13f, tdr, tdg, tdb),          // shoulder
            RE(0.94f, 0.15f, 0.11f, tdr, tdg, tdb),
            RE(0.98f, 0.10f, 0.08f, sk, skg, skb),            // neck
            R(1.02f, 0.08f, sk, skg, skb),
            R(1.06f, 0.075f, sk, skg, skb),
        }, Seg, capBottom: true, capTop: false);

        // Ornate collar
        Loft(v, idx, new[]
        {
            RE(0.94f, 0.16f, 0.12f, tdr - 0.04f, tdg - 0.06f, tdb - 0.03f),
            RE(0.97f, 0.14f, 0.11f, tdr - 0.04f, tdg - 0.06f, tdb - 0.03f),
            RE(0.99f, 0.12f, 0.10f, tdr - 0.04f, tdg - 0.06f, tdb - 0.03f),
        }, Seg, capBottom: false, capTop: false);

        // Gold collar trim
        Loft(v, idx, new[]
        {
            R(0.94f, 0.165f, gr, gg, gb),
            R(0.95f, 0.162f, gr, gg, gb),
        }, Seg, capBottom: false, capTop: false);

        // ═══ HEAD + FANCY HAT ═══
        Loft(v, idx, new[]
        {
            R(1.06f, 0.080f, sk, skg, skb),
            RE(1.09f, 0.098f, 0.090f, sk, skg, skb),
            RE(1.12f, 0.115f, 0.108f, sk, skg, skb),
            RE(1.15f, 0.128f, 0.120f, sk, skg, skb),          // cheeks (round)
            RE(1.18f, 0.135f, 0.126f, sk, skg, skb),
            RE(1.21f, 0.138f, 0.130f, sk, skg, skb),          // mid face
            RE(1.24f, 0.134f, 0.128f, sk, skg, skb),          // brow
            RE(1.27f, 0.128f, 0.124f, hr, hg, hb),            // hairline
            RE(1.30f, 0.120f, 0.118f, hr, hg, hb),
            R(1.33f, 0.105f, hr, hg, hb),
        }, Seg, capBottom: false, capTop: false);

        // Face details
        AddSphere(v, idx, 0, 1.17f, 0.13f, 0.022f, 8, sk - 0.01f, skg - 0.02f, skb - 0.02f); // nose
        AddSphere(v, idx, 0, 1.15f, 0.135f, 0.012f, 6, sk + 0.02f, skg - 0.04f, skb - 0.06f); // red nose tip

        // Eyes
        AddSphere(v, idx, -0.04f, 1.22f, 0.115f, 0.014f, 8, 0.90f, 0.88f, 0.84f);
        AddSphere(v, idx, 0.04f, 1.22f, 0.115f, 0.014f, 8, 0.90f, 0.88f, 0.84f);
        AddSphere(v, idx, -0.04f, 1.22f, 0.124f, 0.007f, 6, 0.28f, 0.22f, 0.12f); // brown iris
        AddSphere(v, idx, 0.04f, 1.22f, 0.124f, 0.007f, 6, 0.28f, 0.22f, 0.12f);

        // Curled mustache
        AddCylinder(v, idx, -0.03f, 1.16f, 0.125f, 0.022f, 0.008f, 6, hr, hg, hb);
        AddCylinder(v, idx, 0.03f, 1.16f, 0.125f, 0.022f, 0.008f, 6, hr, hg, hb);
        // Mustache tips (curled outward)
        AddSphere(v, idx, -0.05f, 1.16f, 0.12f, 0.008f, 4, hr, hg, hb);
        AddSphere(v, idx, 0.05f, 1.16f, 0.12f, 0.008f, 4, hr, hg, hb);

        // Small goatee
        Loft(v, idx, new[]
        {
            R(1.12f, 0.02f, hr, hg, hb, offsetZ: 0.10f),
            R(1.08f, 0.03f, hr, hg, hb, offsetZ: 0.08f),
            R(1.04f, 0.02f, hr - 0.02f, hg - 0.02f, hb, offsetZ: 0.06f),
        }, 6, capBottom: true, capTop: true);

        // ═══ FANCY HAT — wide brim with plume ═══
        Loft(v, idx, new[]
        {
            R(1.32f, 0.20f, htr, htg, htb),                  // brim outer
            R(1.33f, 0.21f, htr, htg, htb),                  // brim edge
            R(1.34f, 0.20f, htr + 0.02f, htg + 0.02f, htb + 0.01f), // brim top
            R(1.36f, 0.15f, htr + 0.02f, htg + 0.02f, htb + 0.01f), // hat base
            R(1.40f, 0.12f, htr, htg, htb),
            R(1.44f, 0.11f, htr, htg, htb),
            R(1.48f, 0.10f, htr - 0.02f, htg - 0.02f, htb - 0.01f),
            R(1.52f, 0.08f, htr - 0.02f, htg - 0.02f, htb - 0.01f),
            R(1.56f, 0.05f, htr - 0.04f, htg - 0.04f, htb - 0.02f),
            R(1.58f, 0.02f, htr - 0.04f, htg - 0.04f, htb - 0.02f),
            R(1.60f, 0.00f, htr - 0.04f, htg - 0.04f, htb - 0.02f),
        }, Seg, capBottom: false, capTop: false);

        // Hat band
        Loft(v, idx, new[]
        {
            R(1.36f, 0.155f, gr, gg, gb),
            R(1.37f, 0.155f, gr, gg, gb),
            R(1.38f, 0.152f, gr, gg, gb),
        }, Seg, capBottom: false, capTop: false);

        // Hat brooch
        AddSphere(v, idx, 0, 1.37f, 0.155f, 0.018f, 6, gr + 0.10f, gg + 0.08f, gb + 0.04f);

        // Feather plume
        AddCylinder(v, idx, 0.08f, 1.48f, -0.06f, 0.005f, 0.18f, 4, 0.82f, 0.18f, 0.12f);
        AddCylinder(v, idx, 0.10f, 1.50f, -0.04f, 0.004f, 0.14f, 4, 0.78f, 0.16f, 0.10f);

        // ═══ ARMS ═══
        LoftLimb(v, idx, -0.22f, 0.86f, 0, new[]
        {
            R(0.00f, 0.085f, tr, tg, tb),
            R(-0.05f, 0.080f, tr, tg, tb),
            R(-0.10f, 0.075f, tr, tg, tb),
            R(-0.16f, 0.070f, tr, tg, tb),
            R(-0.22f, 0.065f, tr, tg, tb),
            R(-0.28f, 0.060f, tr, tg, tb),
            R(-0.34f, 0.055f, tr, tg, tb),
            R(-0.38f, 0.048f, tdr, tdg, tdb),                // cuff
            R(-0.40f, 0.042f, sk, skg, skb),                 // hand
            R(-0.44f, 0.038f, sk, skg, skb),
            R(-0.48f, 0.028f, sk, skg, skb),
            R(-0.51f, 0.015f, sk, skg, skb),
            R(-0.52f, 0.000f, sk, skg, skb),
        }, 10, angleX: -0.18f, angleZ: 0.1f);

        LoftLimb(v, idx, 0.22f, 0.86f, 0, new[]
        {
            R(0.00f, 0.085f, tr, tg, tb),
            R(-0.05f, 0.080f, tr, tg, tb),
            R(-0.10f, 0.075f, tr, tg, tb),
            R(-0.16f, 0.070f, tr, tg, tb),
            R(-0.22f, 0.065f, tr, tg, tb),
            R(-0.28f, 0.060f, tr, tg, tb),
            R(-0.34f, 0.055f, tr, tg, tb),
            R(-0.38f, 0.048f, tdr, tdg, tdb),
            R(-0.40f, 0.042f, sk, skg, skb),
            R(-0.44f, 0.038f, sk, skg, skb),
            R(-0.48f, 0.028f, sk, skg, skb),
            R(-0.51f, 0.015f, sk, skg, skb),
            R(-0.52f, 0.000f, sk, skg, skb),
        }, 10, angleX: 0.18f, angleZ: 0.1f);

        // ═══ LEGS ═══
        LoftLimb(v, idx, -0.11f, 0.38f, 0, new[]
        {
            R(0.00f, 0.095f, pr, pg, pb),
            R(-0.06f, 0.090f, pr, pg, pb),
            R(-0.12f, 0.085f, pr, pg, pb),
            R(-0.18f, 0.080f, pr, pg, pb),
            R(-0.24f, 0.075f, pr, pg, pb),
            R(-0.30f, 0.070f, pr, pg, pb),
            R(-0.36f, 0.065f, pr, pg, pb),
            R(-0.42f, 0.062f, 0.28f, 0.18f, 0.09f),         // boot
            R(-0.48f, 0.066f, 0.28f, 0.18f, 0.09f),
            R(-0.52f, 0.068f, 0.28f, 0.18f, 0.09f),
            RE(-0.56f, 0.064f, 0.080f, 0.26f, 0.16f, 0.08f),
            RE(-0.58f, 0.000f, 0.000f, 0.26f, 0.16f, 0.08f),
        }, 10);

        LoftLimb(v, idx, 0.11f, 0.38f, 0, new[]
        {
            R(0.00f, 0.095f, pr, pg, pb),
            R(-0.06f, 0.090f, pr, pg, pb),
            R(-0.12f, 0.085f, pr, pg, pb),
            R(-0.18f, 0.080f, pr, pg, pb),
            R(-0.24f, 0.075f, pr, pg, pb),
            R(-0.30f, 0.070f, pr, pg, pb),
            R(-0.36f, 0.065f, pr, pg, pb),
            R(-0.42f, 0.062f, 0.28f, 0.18f, 0.09f),
            R(-0.48f, 0.066f, 0.28f, 0.18f, 0.09f),
            R(-0.52f, 0.068f, 0.28f, 0.18f, 0.09f),
            RE(-0.56f, 0.064f, 0.080f, 0.26f, 0.16f, 0.08f),
            RE(-0.58f, 0.000f, 0.000f, 0.26f, 0.16f, 0.08f),
        }, 10);

        // ═══ ORNATE GOLD BELT ═══
        Loft(v, idx, new[]
        {
            R(0.44f, 0.28f, gr, gg, gb),
            R(0.46f, 0.29f, gr, gg, gb),
            R(0.48f, 0.295f, gr + 0.04f, gg + 0.02f, gb),
            R(0.50f, 0.29f, gr, gg, gb),
            R(0.52f, 0.28f, gr, gg, gb),
        }, Seg, capBottom: false, capTop: false);

        // Belt buckle — ornate square
        AddBox(v, idx, 0, 0.48f, 0.30f, 0.04f, 0.05f, 0.015f, gr + 0.10f, gg + 0.08f, gb + 0.04f);

        // ═══ POUCHES AND ACCESSORIES ═══
        // Large coin purse
        AddSphere(v, idx, 0.22f, 0.46f, 0.14f, 0.055f, 10, 0.40f, 0.26f, 0.10f);
        AddSphere(v, idx, 0.22f, 0.48f, 0.16f, 0.020f, 6, gr, gg, gb); // clasp

        // Small pouch
        AddSphere(v, idx, -0.18f, 0.45f, 0.12f, 0.042f, 8, 0.38f, 0.24f, 0.10f);
        AddSphere(v, idx, -0.18f, 0.47f, 0.13f, 0.014f, 6, gr, gg, gb); // clasp

        // Coin purse (hip)
        AddSphere(v, idx, 0.10f, 0.44f, 0.16f, 0.028f, 6, gr * 0.7f, gg * 0.7f, gb * 0.7f);

        // Scroll tube at belt
        AddCylinder(v, idx, -0.24f, 0.42f, -0.04f, 0.016f, 0.14f, 8, 0.46f, 0.36f, 0.20f);
        AddSphere(v, idx, -0.24f, 0.50f, -0.04f, 0.018f, 6, 0.48f, 0.38f, 0.22f); // cap

        // Pendant chain with gold coin
        AddCylinder(v, idx, 0, 0.86f, 0.15f, 0.03f, 0.004f, 6, gr * 0.8f, gg * 0.8f, gb * 0.8f);
        AddSphere(v, idx, 0, 0.82f, 0.16f, 0.018f, 6, gr, gg, gb); // gold coin pendant

        return new MeshData(v.ToArray(), idx.ToArray());
    }

    public static Mesh Create(GL gl, string npcType)
    {
        var data = npcType switch
        {
            "blacksmith" => GenerateBlacksmith(),
            "elder" => GenerateElder(),
            "merchant" => GenerateMerchant(),
            _ => GenerateBlacksmith()
        };
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
    }
}
