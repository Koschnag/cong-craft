using Silk.NET.OpenGL;
using CongCraft.Engine.Rendering;
using static CongCraft.Engine.Procedural.MeshLofter;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates a SpellForce/Gothic-style player warrior mesh.
/// Uses lofted cross-sections for smooth, connected body surfaces
/// instead of separate primitives. Small details (buckles, studs) still use primitives.
/// </summary>
public static class PlayerMeshBuilder
{
    private const int Seg = 14; // Segments per ring

    // Colors
    private const float SkinR = 0.72f, SkinG = 0.58f, SkinB = 0.48f;
    private const float LeatherR = 0.42f, LeatherG = 0.30f, LeatherB = 0.18f;
    private const float ArmorR = 0.48f, ArmorG = 0.35f, ArmorB = 0.20f;
    private const float MetalR = 0.55f, MetalG = 0.50f, MetalB = 0.45f;
    private const float BootR = 0.25f, BootG = 0.17f, BootB = 0.10f;
    private const float PantsR = 0.32f, PantsG = 0.24f, PantsB = 0.15f;
    private const float HairR = 0.22f, HairG = 0.14f, HairB = 0.08f;
    private const float CloakR = 0.18f, CloakG = 0.22f, CloakB = 0.12f;

    public static MeshData GenerateData()
    {
        var v = new List<float>();
        var idx = new List<uint>();

        // ═══════════════════════════════════════════════
        // TORSO — single connected lofted surface
        // From waist up through chest to neck base
        // ═══════════════════════════════════════════════
        Loft(v, idx, new[]
        {
            RE(0.42f, 0.18f, 0.12f, LeatherR, LeatherG, LeatherB),   // waist
            RE(0.50f, 0.20f, 0.14f, LeatherR, LeatherG, LeatherB),   // lower abdomen
            RE(0.60f, 0.22f, 0.15f, ArmorR, ArmorG, ArmorB),         // upper abdomen
            RE(0.72f, 0.24f, 0.16f, ArmorR, ArmorG, ArmorB),         // chest (widest)
            RE(0.85f, 0.23f, 0.15f, ArmorR, ArmorG, ArmorB),         // upper chest
            RE(0.95f, 0.20f, 0.13f, ArmorR, ArmorG, ArmorB),         // shoulder line
            RE(1.02f, 0.16f, 0.10f, LeatherR, LeatherG, LeatherB),   // neck base
            RE(1.08f, 0.08f, 0.07f, SkinR, SkinG, SkinB),            // neck bottom
            RE(1.14f, 0.065f, 0.06f, SkinR, SkinG, SkinB),           // neck mid
            RE(1.20f, 0.07f, 0.065f, SkinR, SkinG, SkinB),           // neck top
        }, Seg, capBottom: true, capTop: false);

        // ═══════════════════════════════════════════════
        // HEAD — connected lofted sphere-like shape
        // ═══════════════════════════════════════════════
        Loft(v, idx, new[]
        {
            R(1.20f, 0.08f, SkinR, SkinG, SkinB),            // jaw base (connects to neck)
            RE(1.24f, 0.11f, 0.10f, SkinR, SkinG, SkinB),    // jaw
            RE(1.28f, 0.12f, 0.11f, SkinR, SkinG, SkinB),    // cheeks
            RE(1.32f, 0.12f, 0.12f, SkinR, SkinG, SkinB),    // mid face
            RE(1.36f, 0.115f, 0.115f, SkinR, SkinG, SkinB),  // eyes level
            RE(1.40f, 0.10f, 0.11f, HairR, HairG, HairB),    // forehead / hair start
            RE(1.44f, 0.08f, 0.10f, HairR, HairG, HairB),    // top hair
            R(1.47f, 0.04f, HairR, HairG, HairB),             // crown
            R(1.48f, 0.00f, HairR, HairG, HairB),             // top pole
        }, Seg, capBottom: false, capTop: false);

        // Nose
        AddSphere(v, idx, 0, 1.30f, 0.12f, 0.022f, 8, SkinR - 0.02f, SkinG - 0.02f, SkinB - 0.02f);
        // Ears
        AddSphere(v, idx, -0.12f, 1.33f, 0, 0.025f, 6, SkinR, SkinG, SkinB);
        AddSphere(v, idx, 0.12f, 1.33f, 0, 0.025f, 6, SkinR, SkinG, SkinB);

        // ═══════════════════════════════════════════════
        // LEFT ARM — connected loft from shoulder to hand
        // ═══════════════════════════════════════════════
        LoftLimb(v, idx, -0.24f, 0.95f, 0, new[]
        {
            R(0.00f, 0.09f, LeatherR, LeatherG, LeatherB),    // shoulder cap
            R(-0.06f, 0.08f, SkinR, SkinG, SkinB),            // upper arm top
            R(-0.20f, 0.065f, SkinR, SkinG, SkinB),           // upper arm mid
            R(-0.34f, 0.06f, SkinR, SkinG, SkinB),            // elbow area
            R(-0.36f, 0.055f, LeatherR, LeatherG, LeatherB),  // bracer start
            R(-0.46f, 0.055f, LeatherR, LeatherG, LeatherB),  // forearm
            R(-0.54f, 0.05f, LeatherR, LeatherG, LeatherB),   // bracer end
            R(-0.58f, 0.04f, SkinR, SkinG, SkinB),            // wrist
            R(-0.64f, 0.035f, SkinR, SkinG, SkinB),           // hand base
            R(-0.70f, 0.02f, SkinR, SkinG, SkinB),            // fingers
            R(-0.73f, 0.00f, SkinR, SkinG, SkinB),            // fingertip
        }, Seg / 2 + 2, angleX: -0.15f, angleZ: 0.1f);

        // RIGHT ARM — mirrored
        LoftLimb(v, idx, 0.24f, 0.95f, 0, new[]
        {
            R(0.00f, 0.09f, LeatherR, LeatherG, LeatherB),
            R(-0.06f, 0.08f, SkinR, SkinG, SkinB),
            R(-0.20f, 0.065f, SkinR, SkinG, SkinB),
            R(-0.34f, 0.06f, SkinR, SkinG, SkinB),
            R(-0.36f, 0.055f, LeatherR, LeatherG, LeatherB),
            R(-0.46f, 0.055f, LeatherR, LeatherG, LeatherB),
            R(-0.54f, 0.05f, LeatherR, LeatherG, LeatherB),
            R(-0.58f, 0.04f, SkinR, SkinG, SkinB),
            R(-0.64f, 0.035f, SkinR, SkinG, SkinB),
            R(-0.70f, 0.02f, SkinR, SkinG, SkinB),
            R(-0.73f, 0.00f, SkinR, SkinG, SkinB),
        }, Seg / 2 + 2, angleX: 0.15f, angleZ: 0.1f);

        // Shoulder guards (armor overlay on top of arm loft)
        AddSphere(v, idx, -0.26f, 1.02f, 0, 0.09f, 10, MetalR, MetalG, MetalB);
        AddSphere(v, idx, 0.26f, 1.02f, 0, 0.09f, 10, MetalR, MetalG, MetalB);
        // Shoulder studs
        AddSphere(v, idx, -0.28f, 1.08f, 0.04f, 0.025f, 6, MetalR + 0.1f, MetalG + 0.08f, MetalB + 0.05f);
        AddSphere(v, idx, 0.28f, 1.08f, 0.04f, 0.025f, 6, MetalR + 0.1f, MetalG + 0.08f, MetalB + 0.05f);

        // ═══════════════════════════════════════════════
        // LEFT LEG — connected loft from hip to foot
        // ═══════════════════════════════════════════════
        LoftLimb(v, idx, -0.10f, 0.42f, 0, new[]
        {
            R(0.00f, 0.09f, PantsR, PantsG, PantsB),          // hip joint
            R(-0.08f, 0.085f, PantsR, PantsG, PantsB),        // upper thigh
            R(-0.18f, 0.075f, PantsR, PantsG, PantsB),        // mid thigh
            R(-0.28f, 0.065f, PantsR, PantsG, PantsB),        // lower thigh
            R(-0.32f, 0.058f, MetalR, MetalG, MetalB),        // knee guard
            R(-0.36f, 0.055f, PantsR, PantsG, PantsB),        // below knee
            R(-0.46f, 0.055f, PantsR, PantsG, PantsB),        // shin
            R(-0.54f, 0.052f, BootR, BootG, BootB),           // boot top
            R(-0.62f, 0.058f, BootR, BootG, BootB),           // boot shaft
            RE(-0.68f, 0.06f, 0.07f, BootR, BootG, BootB),   // ankle
            RE(-0.72f, 0.055f, 0.08f, BootR - 0.05f, BootG - 0.03f, BootB - 0.02f), // sole
            RE(-0.73f, 0.00f, 0.00f, BootR - 0.05f, BootG - 0.03f, BootB - 0.02f), // bottom
        }, Seg / 2 + 2, angleX: 0f, angleZ: 0f);

        // RIGHT LEG — mirrored
        LoftLimb(v, idx, 0.10f, 0.42f, 0, new[]
        {
            R(0.00f, 0.09f, PantsR, PantsG, PantsB),
            R(-0.08f, 0.085f, PantsR, PantsG, PantsB),
            R(-0.18f, 0.075f, PantsR, PantsG, PantsB),
            R(-0.28f, 0.065f, PantsR, PantsG, PantsB),
            R(-0.32f, 0.058f, MetalR, MetalG, MetalB),
            R(-0.36f, 0.055f, PantsR, PantsG, PantsB),
            R(-0.46f, 0.055f, PantsR, PantsG, PantsB),
            R(-0.54f, 0.052f, BootR, BootG, BootB),
            R(-0.62f, 0.058f, BootR, BootG, BootB),
            RE(-0.68f, 0.06f, 0.07f, BootR, BootG, BootB),
            RE(-0.72f, 0.055f, 0.08f, BootR - 0.05f, BootG - 0.03f, BootB - 0.02f),
            RE(-0.73f, 0.00f, 0.00f, BootR - 0.05f, BootG - 0.03f, BootB - 0.02f),
        }, Seg / 2 + 2, angleX: 0f, angleZ: 0f);

        // Knee guards (overlay)
        AddSphere(v, idx, -0.10f, 0.10f, 0.04f, 0.04f, 8, MetalR, MetalG, MetalB);
        AddSphere(v, idx, 0.10f, 0.10f, 0.04f, 0.04f, 8, MetalR, MetalG, MetalB);

        // ═══════════════════════════════════════════════
        // BELT — lofted ring around waist
        // ═══════════════════════════════════════════════
        Loft(v, idx, new[]
        {
            R(0.44f, 0.21f, 0.28f, 0.18f, 0.10f),
            R(0.47f, 0.22f, 0.28f, 0.18f, 0.10f),
            R(0.50f, 0.21f, 0.28f, 0.18f, 0.10f),
        }, Seg, capBottom: false, capTop: false);

        // Belt buckle
        AddSphere(v, idx, 0, 0.47f, 0.22f, 0.025f, 6, 0.82f, 0.62f, 0.15f);
        // Belt pouch
        AddSphere(v, idx, 0.18f, 0.46f, 0.10f, 0.035f, 6, 0.32f, 0.22f, 0.12f);
        // Dagger scabbard
        AddCylinder(v, idx, -0.19f, 0.36f, 0.08f, 0.018f, 0.16f, 6, 0.22f, 0.16f, 0.10f);

        // ═══════════════════════════════════════════════
        // CLOAK — lofted drape from shoulders down back
        // ═══════════════════════════════════════════════
        Loft(v, idx, new[]
        {
            RE(0.98f, 0.18f, 0.02f, CloakR, CloakG, CloakB, offsetZ: -0.12f),
            RE(0.90f, 0.22f, 0.03f, CloakR, CloakG, CloakB, offsetZ: -0.14f),
            RE(0.78f, 0.24f, 0.04f, CloakR, CloakG, CloakB, offsetZ: -0.16f),
            RE(0.65f, 0.25f, 0.04f, CloakR, CloakG, CloakB, offsetZ: -0.17f),
            RE(0.52f, 0.24f, 0.04f, CloakR - 0.02f, CloakG - 0.02f, CloakB - 0.01f, offsetZ: -0.18f),
            RE(0.42f, 0.22f, 0.03f, CloakR - 0.02f, CloakG - 0.02f, CloakB - 0.01f, offsetZ: -0.17f),
        }, Seg / 2, capBottom: true, capTop: true);

        // Cloak clasp
        AddSphere(v, idx, 0, 1.10f, -0.10f, 0.02f, 6, 0.78f, 0.58f, 0.12f);

        return new MeshData(v.ToArray(), idx.ToArray());
    }

    public static Mesh Create(GL gl)
    {
        var data = GenerateData();
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
    }
}
