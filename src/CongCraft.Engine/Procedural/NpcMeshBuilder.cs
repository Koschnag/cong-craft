using Silk.NET.OpenGL;
using CongCraft.Engine.Rendering;
using static CongCraft.Engine.Procedural.MeshLofter;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates NPC meshes using lofted connected surfaces.
/// Three distinct NPC types: Blacksmith, Elder, Merchant.
/// Gothic/SpellForce style with smooth body transitions.
/// </summary>
public static class NpcMeshBuilder
{
    private const int Seg = 12;

    public static MeshData GenerateBlacksmith()
    {
        var v = new List<float>();
        var idx = new List<uint>();

        const float sk = 0.68f, sg = 0.52f, sb = 0.42f;  // skin
        const float lr = 0.45f, lg = 0.30f, lb = 0.15f;    // leather apron
        const float pr = 0.38f, pg = 0.28f, pb = 0.18f;    // pants

        // ═══ TORSO — stocky build ═══
        Loft(v, idx, new[]
        {
            RE(0.40f, 0.22f, 0.16f, lr, lg, lb),
            RE(0.50f, 0.26f, 0.18f, lr, lg, lb),
            RE(0.62f, 0.28f, 0.20f, lr, lg, lb),    // wide chest
            RE(0.76f, 0.27f, 0.19f, lr, lg, lb),
            RE(0.88f, 0.24f, 0.16f, lr, lg, lb),
            RE(0.98f, 0.18f, 0.12f, sk, sg, sb),     // neck base
            R(1.06f, 0.09f, sk, sg, sb),               // neck
            R(1.14f, 0.085f, sk, sg, sb),
        }, Seg, capBottom: true, capTop: false);

        // ═══ HEAD — bald, beefy ═══
        Loft(v, idx, new[]
        {
            R(1.14f, 0.09f, sk, sg, sb),
            R(1.20f, 0.12f, sk, sg, sb),
            RE(1.26f, 0.14f, 0.13f, sk, sg, sb),
            RE(1.32f, 0.14f, 0.13f, sk, sg, sb),
            R(1.38f, 0.12f, sk, sg, sb),
            R(1.42f, 0.08f, sk - 0.02f, sg - 0.02f, sb),
            R(1.45f, 0.03f, sk - 0.02f, sg - 0.02f, sb),
            R(1.46f, 0.00f, sk - 0.02f, sg - 0.02f, sb),
        }, Seg, capBottom: false, capTop: false);

        // Beard
        Loft(v, idx, new[]
        {
            R(1.24f, 0.04f, 0.30f, 0.20f, 0.12f, offsetZ: 0.10f),
            RE(1.18f, 0.08f, 0.06f, 0.30f, 0.20f, 0.12f, offsetZ: 0.08f),
            RE(1.10f, 0.06f, 0.04f, 0.28f, 0.18f, 0.10f, offsetZ: 0.06f),
        }, 8, capBottom: true, capTop: true);

        // ═══ ARMS — muscular, connected ═══
        LoftLimb(v, idx, -0.28f, 0.88f, 0, new[]
        {
            R(0.00f, 0.10f, sk, sg, sb),
            R(-0.08f, 0.09f, sk, sg, sb),       // thick upper arm
            R(-0.20f, 0.08f, sk, sg, sb),
            R(-0.30f, 0.075f, lr, lg, lb),       // bracer
            R(-0.40f, 0.07f, lr, lg, lb),
            R(-0.48f, 0.06f, sk, sg, sb),         // wrist
            R(-0.54f, 0.05f, sk, sg, sb),
            R(-0.58f, 0.03f, sk, sg, sb),
            R(-0.60f, 0.00f, sk, sg, sb),
        }, 8, angleX: -0.2f, angleZ: 0.1f);

        LoftLimb(v, idx, 0.28f, 0.88f, 0, new[]
        {
            R(0.00f, 0.10f, sk, sg, sb),
            R(-0.08f, 0.09f, sk, sg, sb),
            R(-0.20f, 0.08f, sk, sg, sb),
            R(-0.30f, 0.075f, lr, lg, lb),
            R(-0.40f, 0.07f, lr, lg, lb),
            R(-0.48f, 0.06f, sk, sg, sb),
            R(-0.54f, 0.05f, sk, sg, sb),
            R(-0.58f, 0.03f, sk, sg, sb),
            R(-0.60f, 0.00f, sk, sg, sb),
        }, 8, angleX: 0.2f, angleZ: 0.1f);

        // ═══ LEGS — sturdy ═══
        LoftLimb(v, idx, -0.12f, 0.40f, 0, new[]
        {
            R(0.00f, 0.10f, pr, pg, pb),
            R(-0.12f, 0.09f, pr, pg, pb),
            R(-0.24f, 0.08f, pr, pg, pb),
            R(-0.36f, 0.07f, pr, pg, pb),
            R(-0.46f, 0.065f, 0.28f, 0.18f, 0.09f),  // boot
            R(-0.54f, 0.07f, 0.28f, 0.18f, 0.09f),
            RE(-0.60f, 0.065f, 0.08f, 0.26f, 0.17f, 0.08f),
            RE(-0.62f, 0.00f, 0.00f, 0.26f, 0.17f, 0.08f),
        }, 8);

        LoftLimb(v, idx, 0.12f, 0.40f, 0, new[]
        {
            R(0.00f, 0.10f, pr, pg, pb),
            R(-0.12f, 0.09f, pr, pg, pb),
            R(-0.24f, 0.08f, pr, pg, pb),
            R(-0.36f, 0.07f, pr, pg, pb),
            R(-0.46f, 0.065f, 0.28f, 0.18f, 0.09f),
            R(-0.54f, 0.07f, 0.28f, 0.18f, 0.09f),
            RE(-0.60f, 0.065f, 0.08f, 0.26f, 0.17f, 0.08f),
            RE(-0.62f, 0.00f, 0.00f, 0.26f, 0.17f, 0.08f),
        }, 8);

        // Apron (lofted drape)
        Loft(v, idx, new[]
        {
            RE(0.78f, 0.20f, 0.02f, 0.32f, 0.20f, 0.09f, offsetZ: 0.06f),
            RE(0.60f, 0.26f, 0.03f, 0.32f, 0.20f, 0.09f, offsetZ: 0.08f),
            RE(0.44f, 0.24f, 0.03f, 0.30f, 0.18f, 0.08f, offsetZ: 0.06f),
            RE(0.34f, 0.20f, 0.02f, 0.30f, 0.18f, 0.08f, offsetZ: 0.04f),
        }, 8, capBottom: true, capTop: true);

        // Belt + hammer
        MeshLofter.AddCylinder(v, idx, -0.22f, 0.38f, 0.10f, 0.015f, 0.24f, 6, 0.48f, 0.36f, 0.20f);
        MeshLofter.AddSphere(v, idx, -0.22f, 0.63f, 0.10f, 0.038f, 8, 0.50f, 0.48f, 0.45f);

        return new MeshData(v.ToArray(), idx.ToArray());
    }

    public static MeshData GenerateElder()
    {
        var v = new List<float>();
        var idx = new List<uint>();

        const float rr = 0.38f, rg = 0.22f, rb = 0.48f;  // robe purple
        const float sk = 0.65f, skg = 0.58f, skb = 0.52f;  // aged skin
        const float wr = 0.82f, wg = 0.80f, wb = 0.76f;    // white hair/beard
        const float gr = 0.72f, gg = 0.55f, gb = 0.15f;     // gold trim

        // ═══ BODY — slender robed figure, single loft from hem to neck ═══
        Loft(v, idx, new[]
        {
            R(0.06f, 0.28f, rr - 0.06f, rg - 0.02f, rb - 0.06f), // robe hem
            R(0.14f, 0.26f, rr, rg, rb),
            R(0.26f, 0.22f, rr, rg, rb),                           // robe lower
            R(0.38f, 0.20f, rr, rg, rb),
            R(0.50f, 0.21f, rr, rg, rb),
            R(0.62f, 0.22f, rr, rg, rb),                           // chest
            R(0.74f, 0.21f, rr, rg, rb),
            R(0.86f, 0.19f, rr, rg, rb),
            R(0.96f, 0.15f, rr - 0.03f, rg, rb - 0.03f),         // shoulders
            R(1.04f, 0.10f, rr - 0.03f, rg, rb - 0.03f),
            R(1.10f, 0.07f, sk, skg, skb),                         // neck
            R(1.16f, 0.065f, sk, skg, skb),
        }, Seg, capBottom: true, capTop: false);

        // Gold hem trim
        Loft(v, idx, new[]
        {
            R(0.06f, 0.29f, gr, gg, gb),
            R(0.08f, 0.28f, gr, gg, gb),
        }, Seg, capBottom: false, capTop: false);

        // ═══ HEAD — wise elder ═══
        Loft(v, idx, new[]
        {
            R(1.16f, 0.07f, sk, skg, skb),
            R(1.22f, 0.11f, sk, skg, skb),
            RE(1.28f, 0.13f, 0.12f, sk, skg, skb),
            RE(1.32f, 0.13f, 0.12f, wr, wg, wb),       // hair starts
            R(1.38f, 0.12f, wr, wg, wb),
            R(1.42f, 0.08f, wr, wg, wb),
            R(1.45f, 0.03f, wr, wg, wb),
            R(1.46f, 0.00f, wr, wg, wb),
        }, Seg, capBottom: false, capTop: false);

        // Long beard (lofted)
        Loft(v, idx, new[]
        {
            R(1.22f, 0.04f, wr, wg, wb, offsetZ: 0.10f),
            RE(1.14f, 0.06f, 0.04f, wr, wg, wb, offsetZ: 0.08f),
            RE(1.04f, 0.05f, 0.035f, wr - 0.02f, wg - 0.02f, wb - 0.02f, offsetZ: 0.07f),
            RE(0.92f, 0.04f, 0.03f, wr - 0.02f, wg - 0.02f, wb - 0.02f, offsetZ: 0.06f),
        }, 8, capBottom: true, capTop: true);

        // Bushy eyebrows
        MeshLofter.AddCylinder(v, idx, -0.05f, 1.30f, 0.11f, 0.02f, 0.008f, 6, wr, wg, wb);
        MeshLofter.AddCylinder(v, idx, 0.05f, 1.30f, 0.11f, 0.02f, 0.008f, 6, wr, wg, wb);

        // ═══ ARMS — wide sleeves ═══
        LoftLimb(v, idx, -0.20f, 0.86f, 0, new[]
        {
            R(0.00f, 0.10f, rr, rg, rb),            // sleeve top
            R(-0.10f, 0.11f, rr, rg, rb),           // wide sleeve
            R(-0.22f, 0.10f, rr, rg, rb),
            R(-0.32f, 0.09f, rr, rg, rb),
            R(-0.40f, 0.07f, rr, rg, rb),
            R(-0.42f, 0.06f, gr, gg, gb),            // gold cuff
            R(-0.44f, 0.04f, sk, skg, skb),          // hand
            R(-0.50f, 0.025f, sk, skg, skb),
            R(-0.54f, 0.00f, sk, skg, skb),
        }, 8, angleX: -0.2f, angleZ: 0.12f);

        LoftLimb(v, idx, 0.20f, 0.86f, 0, new[]
        {
            R(0.00f, 0.10f, rr, rg, rb),
            R(-0.10f, 0.11f, rr, rg, rb),
            R(-0.22f, 0.10f, rr, rg, rb),
            R(-0.32f, 0.09f, rr, rg, rb),
            R(-0.40f, 0.07f, rr, rg, rb),
            R(-0.42f, 0.06f, gr, gg, gb),
            R(-0.44f, 0.04f, sk, skg, skb),
            R(-0.50f, 0.025f, sk, skg, skb),
            R(-0.54f, 0.00f, sk, skg, skb),
        }, 8, angleX: 0.2f, angleZ: 0.12f);

        // Gold belt sash
        Loft(v, idx, new[]
        {
            R(0.48f, 0.22f, gr, gg, gb),
            R(0.51f, 0.225f, gr, gg, gb),
            R(0.54f, 0.22f, gr, gg, gb),
        }, Seg, capBottom: false, capTop: false);

        // Pendant
        MeshLofter.AddSphere(v, idx, 0, 0.88f, 0.14f, 0.022f, 6, gr, gg, gb);

        // Staff (lofted cylinder for smooth wood)
        Loft(v, idx, new[]
        {
            R(0.06f, 0.025f, 0.48f, 0.38f, 0.24f, offsetX: 0.42f),
            R(0.50f, 0.025f, 0.48f, 0.38f, 0.24f, offsetX: 0.42f),
            R(0.78f, 0.028f, 0.44f, 0.34f, 0.20f, offsetX: 0.42f),  // knot
            R(0.82f, 0.025f, 0.48f, 0.38f, 0.24f, offsetX: 0.42f),
            R(1.40f, 0.022f, 0.48f, 0.38f, 0.24f, offsetX: 0.42f),
            R(1.52f, 0.015f, 0.45f, 0.35f, 0.22f, offsetX: 0.42f),
        }, 6, capBottom: true, capTop: true);

        // Crystal orb
        MeshLofter.AddSphere(v, idx, 0.42f, 1.58f, 0, 0.055f, 10, 0.55f, 0.35f, 0.80f);
        MeshLofter.AddSphere(v, idx, 0.42f, 1.58f, 0, 0.035f, 8, 0.65f, 0.45f, 0.90f); // inner glow

        return new MeshData(v.ToArray(), idx.ToArray());
    }

    public static MeshData GenerateMerchant()
    {
        var v = new List<float>();
        var idx = new List<uint>();

        const float tr = 0.28f, tg = 0.48f, tb = 0.22f;  // green tunic
        const float sk = 0.66f, skg = 0.54f, skb = 0.44f;  // skin
        const float pr = 0.32f, pg = 0.22f, pb = 0.14f;     // brown pants
        const float hr = 0.36f, hg = 0.24f, hb = 0.14f;     // brown hair
        const float gr = 0.62f, gg = 0.48f, gb = 0.14f;      // gold belt

        // ═══ BODY — round merchant build ═══
        Loft(v, idx, new[]
        {
            RE(0.40f, 0.20f, 0.16f, tr, tg, tb),
            RE(0.50f, 0.26f, 0.20f, tr, tg, tb),     // belly!
            RE(0.62f, 0.28f, 0.22f, tr, tg, tb),      // round middle
            RE(0.74f, 0.26f, 0.18f, tr, tg, tb),
            RE(0.86f, 0.22f, 0.15f, tr, tg, tb),
            RE(0.96f, 0.16f, 0.11f, tr + 0.02f, tg + 0.02f, tb + 0.02f),
            R(1.04f, 0.08f, sk, skg, skb),             // neck
            R(1.10f, 0.07f, sk, skg, skb),
        }, Seg, capBottom: true, capTop: false);

        // Collar detail
        Loft(v, idx, new[]
        {
            R(1.02f, 0.12f, tr - 0.03f, tg - 0.06f, tb - 0.02f),
            R(1.06f, 0.11f, tr - 0.03f, tg - 0.06f, tb - 0.02f),
        }, Seg, capBottom: false, capTop: false);

        // ═══ HEAD + HAT ═══
        Loft(v, idx, new[]
        {
            R(1.10f, 0.08f, sk, skg, skb),
            R(1.16f, 0.11f, sk, skg, skb),
            RE(1.22f, 0.13f, 0.12f, sk, skg, skb),
            RE(1.28f, 0.13f, 0.12f, sk, skg, skb),
            R(1.32f, 0.12f, hr, hg, hb),              // hair
            R(1.36f, 0.10f, hr, hg, hb),
        }, Seg, capBottom: false, capTop: false);

        // Hat (lofted cone)
        Loft(v, idx, new[]
        {
            R(1.36f, 0.17f, 0.50f, 0.36f, 0.16f),    // brim
            R(1.38f, 0.18f, 0.50f, 0.36f, 0.16f),    // brim top
            R(1.40f, 0.14f, 0.52f, 0.38f, 0.18f),    // hat base
            R(1.48f, 0.10f, 0.52f, 0.38f, 0.18f),    // hat mid
            R(1.56f, 0.06f, 0.50f, 0.36f, 0.16f),    // hat top
            R(1.60f, 0.02f, 0.48f, 0.34f, 0.14f),    // hat tip
            R(1.62f, 0.00f, 0.48f, 0.34f, 0.14f),
        }, Seg, capBottom: false, capTop: false);

        // Feather
        MeshLofter.AddCylinder(v, idx, 0.10f, 1.48f, -0.04f, 0.006f, 0.14f, 4, 0.80f, 0.20f, 0.15f);

        // Mustache
        MeshLofter.AddCylinder(v, idx, 0, 1.20f, 0.12f, 0.04f, 0.008f, 6, hr, hg, hb);

        // ═══ ARMS ═══
        LoftLimb(v, idx, -0.22f, 0.86f, 0, new[]
        {
            R(0.00f, 0.08f, tr, tg, tb),
            R(-0.10f, 0.07f, tr, tg, tb),
            R(-0.24f, 0.06f, tr, tg, tb),
            R(-0.36f, 0.055f, tr, tg, tb),
            R(-0.44f, 0.04f, sk, skg, skb),
            R(-0.50f, 0.035f, sk, skg, skb),
            R(-0.54f, 0.02f, sk, skg, skb),
            R(-0.56f, 0.00f, sk, skg, skb),
        }, 8, angleX: -0.18f, angleZ: 0.1f);

        LoftLimb(v, idx, 0.22f, 0.86f, 0, new[]
        {
            R(0.00f, 0.08f, tr, tg, tb),
            R(-0.10f, 0.07f, tr, tg, tb),
            R(-0.24f, 0.06f, tr, tg, tb),
            R(-0.36f, 0.055f, tr, tg, tb),
            R(-0.44f, 0.04f, sk, skg, skb),
            R(-0.50f, 0.035f, sk, skg, skb),
            R(-0.54f, 0.02f, sk, skg, skb),
            R(-0.56f, 0.00f, sk, skg, skb),
        }, 8, angleX: 0.18f, angleZ: 0.1f);

        // ═══ LEGS ═══
        LoftLimb(v, idx, -0.11f, 0.40f, 0, new[]
        {
            R(0.00f, 0.09f, pr, pg, pb),
            R(-0.12f, 0.08f, pr, pg, pb),
            R(-0.24f, 0.07f, pr, pg, pb),
            R(-0.36f, 0.065f, pr, pg, pb),
            R(-0.46f, 0.06f, 0.30f, 0.20f, 0.10f),   // boot
            R(-0.54f, 0.065f, 0.30f, 0.20f, 0.10f),
            RE(-0.60f, 0.06f, 0.075f, 0.28f, 0.18f, 0.09f),
            RE(-0.62f, 0.00f, 0.00f, 0.28f, 0.18f, 0.09f),
        }, 8);

        LoftLimb(v, idx, 0.11f, 0.40f, 0, new[]
        {
            R(0.00f, 0.09f, pr, pg, pb),
            R(-0.12f, 0.08f, pr, pg, pb),
            R(-0.24f, 0.07f, pr, pg, pb),
            R(-0.36f, 0.065f, pr, pg, pb),
            R(-0.46f, 0.06f, 0.30f, 0.20f, 0.10f),
            R(-0.54f, 0.065f, 0.30f, 0.20f, 0.10f),
            RE(-0.60f, 0.06f, 0.075f, 0.28f, 0.18f, 0.09f),
            RE(-0.62f, 0.00f, 0.00f, 0.28f, 0.18f, 0.09f),
        }, 8);

        // Gold belt
        Loft(v, idx, new[]
        {
            R(0.46f, 0.26f, gr, gg, gb),
            R(0.49f, 0.27f, gr, gg, gb),
            R(0.52f, 0.26f, gr, gg, gb),
        }, Seg, capBottom: false, capTop: false);

        // Pouches
        MeshLofter.AddSphere(v, idx, 0.20f, 0.46f, 0.12f, 0.05f, 8, 0.42f, 0.28f, 0.12f);
        MeshLofter.AddSphere(v, idx, -0.16f, 0.45f, 0.10f, 0.04f, 6, 0.40f, 0.26f, 0.10f);
        MeshLofter.AddSphere(v, idx, 0.10f, 0.44f, 0.14f, 0.025f, 6, 0.80f, 0.65f, 0.15f); // coin purse

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
