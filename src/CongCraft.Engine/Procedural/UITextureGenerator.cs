using CongCraft.Engine.Terrain;
using Silk.NET.OpenGL;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates procedural textures for UI elements.
/// SpellForce 1-style UI — carved dark stone panels with warm amber-gold ornaments.
/// Secondary vibes: Gothic 2/3, Risen. Dark stone background, gold trim, warm cream text.
/// </summary>
public static class UITextureGenerator
{
    /// <summary>
    /// Dark carved stone panel texture for Gothic-style UI backgrounds.
    /// Deep gray-brown with chisel marks, surface variation and subtle vignette.
    /// </summary>
    public static byte[] GenerateParchment(int size = 128, int seed = 1337)
    {
        var noise = new FastNoiseLite(seed);
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFractalOctaves(5);
        noise.SetFrequency(0.03f);

        var crackNoise = new FastNoiseLite(seed + 100);
        crackNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        crackNoise.SetFrequency(0.05f);

        var chisNoise = new FastNoiseLite(seed + 200);
        chisNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        chisNoise.SetFrequency(0.18f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n1 = noise.GetNoise(x, y) * 0.5f + 0.5f;
            float n2 = crackNoise.GetNoise(x, y) * 0.5f + 0.5f;
            float n3 = chisNoise.GetNoise(x, y) * 0.5f + 0.5f;

            // Dark stone base: brownish-gray with warm undertone (R > G > B)
            float base_ = 0.15f + n1 * 0.10f + n3 * 0.04f;
            float crack = MathF.Pow(n2, 3f) * 0.06f; // crack highlight
            float r = base_ + crack + 0.025f; // slight warm tint
            float g = base_ + crack;
            float b = base_ + crack * 0.5f - 0.01f;

            // Vignette — panels feel grounded at edges
            float dx = (x / (float)size - 0.5f) * 2f;
            float dy = (y / (float)size - 0.5f) * 2f;
            float vignette = 1f - (dx * dx + dy * dy) * 0.25f;
            r = Math.Clamp(r * vignette, 0f, 1f);
            g = Math.Clamp(g * vignette, 0f, 1f);
            b = Math.Clamp(b * vignette, 0f, 1f);

            int i = (y * size + x) * 4;
            pixels[i + 0] = ToByte(r);
            pixels[i + 1] = ToByte(g);
            pixels[i + 2] = ToByte(b);
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    /// <summary>
    /// Tarnished iron/dark copper border texture.
    /// Replaces bright gold with a weathered medieval metal look.
    /// </summary>
    public static byte[] GenerateGoldLeaf(int size = 64, int seed = 2000)
    {
        var noise = new FastNoiseLite(seed);
        noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        noise.SetFrequency(0.06f);

        var rustNoise = new FastNoiseLite(seed + 50);
        rustNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        rustNoise.SetFrequency(0.15f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n1 = noise.GetNoise(x, y) * 0.5f + 0.5f;
            float n2 = rustNoise.GetNoise(x, y) * 0.5f + 0.5f;

            // Tarnished iron: dark steel with rust-orange highlights
            float rust = MathF.Pow(n2, 2.5f);
            float r = 0.32f + rust * 0.18f + n1 * 0.08f;
            float g = 0.28f + rust * 0.06f + n1 * 0.05f;
            float b = 0.24f + n1 * 0.04f;

            // Scratched metal sheen
            float highlight = MathF.Pow(n1, 4f) * 0.15f;
            r = MathF.Min(1f, r + highlight);
            g = MathF.Min(1f, g + highlight * 0.7f);
            b = MathF.Min(1f, b + highlight * 0.5f);

            int i = (y * size + x) * 4;
            pixels[i + 0] = ToByte(r);
            pixels[i + 1] = ToByte(g);
            pixels[i + 2] = ToByte(b);
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    /// <summary>
    /// Dark worn leather texture.
    /// </summary>
    public static byte[] GenerateLeather(int size = 64, int seed = 3000)
    {
        var noise = new FastNoiseLite(seed);
        noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        noise.SetFrequency(0.08f);

        var noise2 = new FastNoiseLite(seed + 77);
        noise2.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise2.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise2.SetFractalOctaves(3);
        noise2.SetFrequency(0.05f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n1 = noise.GetNoise(x, y) * 0.5f + 0.5f;
            float n2 = noise2.GetNoise(x, y) * 0.5f + 0.5f;

            // Dark brown leather (R > G > B)
            float r = 0.30f + n1 * 0.12f + n2 * 0.05f;
            float g = 0.20f + n1 * 0.08f + n2 * 0.04f;
            float b = 0.10f + n1 * 0.05f + n2 * 0.02f;

            int i = (y * size + x) * 4;
            pixels[i + 0] = ToByte(r);
            pixels[i + 1] = ToByte(g);
            pixels[i + 2] = ToByte(b);
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    /// <summary>
    /// Dark wood texture.
    /// </summary>
    public static byte[] GenerateWoodGrain(int size = 128, int seed = 4000)
    {
        var noise = new FastNoiseLite(seed);
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFrequency(0.02f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n = noise.GetNoise(x, y * 4);
            float ring = MathF.Sin((n * 10f + y * 0.1f) * MathF.PI) * 0.5f + 0.5f;

            float r = 0.25f + ring * 0.10f;
            float g = 0.18f + ring * 0.07f;
            float b = 0.10f + ring * 0.03f;

            int i = (y * size + x) * 4;
            pixels[i + 0] = ToByte(r);
            pixels[i + 1] = ToByte(g);
            pixels[i + 2] = ToByte(b);
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    /// <summary>
    /// Carved rune border — dark stone with glowing etched symbols.
    /// Replaces the illuminated-manuscript vine border.
    /// </summary>
    public static byte[] GenerateVineBorder(int width = 256, int height = 32, int seed = 5000)
    {
        var pixels = new byte[width * height * 4];

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            float tx = (float)x / width;
            float ty = (float)y / height;

            // Horizontal divider line
            float centerDist = MathF.Abs(ty - 0.5f);
            float line = MathF.Exp(-centerDist * centerDist * 120f);

            // Rune glyphs: evenly spaced rectangular cutouts
            float runePhase = (tx * 8f) % 1f;
            float runeGlyph = 0f;
            // Vertical bar
            if (runePhase > 0.2f && runePhase < 0.35f && ty > 0.2f && ty < 0.8f)
                runeGlyph = 0.8f;
            // Diagonal slash
            float diag = MathF.Abs((runePhase - 0.55f) * 3f - (ty - 0.5f));
            if (runePhase > 0.45f && runePhase < 0.7f && diag < 0.1f)
                runeGlyph = MathF.Max(runeGlyph, MathF.Exp(-diag * diag * 200f) * 0.7f);
            // Horizontal crossbar
            float crossDist = MathF.Abs(ty - 0.5f);
            if (runePhase > 0.45f && runePhase < 0.7f && crossDist < 0.08f)
                runeGlyph = MathF.Max(runeGlyph, 0.65f);

            // Small dots between glyphs
            float dotPhase = (tx * 8f + 0.5f) % 1f;
            float dot = 0f;
            if (dotPhase > 0.4f && dotPhase < 0.6f)
            {
                float ddist = MathF.Sqrt((dotPhase - 0.5f) * (dotPhase - 0.5f) * 40f + (ty - 0.5f) * (ty - 0.5f) * 40f);
                dot = MathF.Exp(-ddist * ddist * 60f) * 0.5f;
            }

            float total = MathF.Min(1f, line * 0.6f + runeGlyph + dot);

            // SpellForce warm amber-gold color for runes
            float r = total * 0.82f;
            float g = total * 0.58f;
            float b = total * 0.14f;

            int i = (y * width + x) * 4;
            pixels[i + 0] = ToByte(r);
            pixels[i + 1] = ToByte(g);
            pixels[i + 2] = ToByte(b);
            pixels[i + 3] = ToByte(total * 0.85f);
        }
        return pixels;
    }

    /// <summary>
    /// Carved stone corner ornament — spiral with chisel marks.
    /// </summary>
    public static byte[] GenerateCornerOrnament(int size = 64)
    {
        var pixels = new byte[size * size * 4];

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float tx = (float)x / size;
            float ty = (float)y / size;

            float angle = MathF.Atan2(ty, tx);
            float radius = MathF.Sqrt(tx * tx + ty * ty);

            // Carved spiral groove
            float spiral = MathF.Sin(angle * 3f + radius * 8f) * 0.5f + 0.5f;
            float mask = MathF.Exp(-radius * 2.5f);
            float flourish = spiral * mask;

            // Iron dots
            float dots = 0f;
            for (int di = 1; di <= 4; di++)
            {
                float dr = di * 0.18f;
                float da = di * 1.2f;
                float ddist = MathF.Sqrt((radius - dr) * (radius - dr) + MathF.Pow(angle - da, 2f) * 0.02f);
                dots = MathF.Max(dots, MathF.Exp(-ddist * ddist * 100f));
            }

            float total = MathF.Min(1f, flourish * 0.7f + dots * 0.8f);

            // SpellForce amber-gold corner ornament
            float r = total * 0.80f;
            float g = total * 0.55f;
            float b = total * 0.12f;

            int i = (y * size + x) * 4;
            pixels[i + 0] = ToByte(r);
            pixels[i + 1] = ToByte(g);
            pixels[i + 2] = ToByte(b);
            pixels[i + 3] = ToByte(total * 0.9f);
        }
        return pixels;
    }

    /// <summary>
    /// Gothic ruins menu background — crumbling castle arch with torch-fire glow,
    /// dark forest silhouette, blood-orange ember horizon.
    /// </summary>
    public static byte[] GenerateMenuBackground(int width = 512, int height = 256, int seed = 6000)
    {
        var cloudNoise = new FastNoiseLite(seed);
        cloudNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        cloudNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        cloudNoise.SetFractalOctaves(4);
        cloudNoise.SetFrequency(0.008f);

        var starNoise = new FastNoiseLite(seed + 999);
        starNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        starNoise.SetFrequency(0.15f);

        var treeSilNoise = new FastNoiseLite(seed + 111);
        treeSilNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        treeSilNoise.SetFrequency(0.04f);

        var pixels = new byte[width * height * 4];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            float tx = (float)x / width;
            float ty = (float)y / height;

            // Deep night sky: near-black blue-purple
            float skyR = 0.02f + ty * 0.04f;
            float skyG = 0.01f + ty * 0.02f;
            float skyB = 0.04f + ty * 0.06f;

            // Dim cloud wisps (dark, ominous)
            float cloud = cloudNoise.GetNoise(x, y) * 0.5f + 0.5f;
            cloud = MathF.Pow(cloud, 2.5f) * 0.08f;
            skyR += cloud * 0.3f;
            skyG += cloud * 0.1f;
            skyB += cloud * 0.4f;

            // Tree/forest silhouette at bottom (jagged, dense)
            float treeHeight = 0.22f + treeSilNoise.GetNoise(x * 1.5f, 0) * 0.07f;
            treeHeight += MathF.Abs(treeSilNoise.GetNoise(x * 4f, 10)) * 0.06f;

            // Castle/ruin arch silhouette (centered)
            float cx2 = tx - 0.5f;
            float archLeft = 0.3f + MathF.Pow(MathF.Max(0f, MathF.Abs(cx2) - 0.15f), 2f) * 2f;
            float archRight = archLeft;
            bool inArch = MathF.Abs(cx2) < 0.13f && ty < 0.55f && ty > 0.12f;
            bool inTower = (MathF.Abs(cx2 - 0.15f) < 0.05f || MathF.Abs(cx2 + 0.15f) < 0.05f)
                           && ty < 0.65f;
            bool onWall = MathF.Abs(cx2) < 0.22f && ty < 0.28f;

            // Silhouette mask
            float silhouetteHeight = treeHeight;
            if (inTower) silhouetteHeight = MathF.Max(silhouetteHeight, 0.65f);
            if (onWall) silhouetteHeight = MathF.Max(silhouetteHeight, 0.28f);

            if (ty < silhouetteHeight)
            {
                // Very dark silhouette
                float depth = (silhouetteHeight - ty) * 3f;
                skyR = MathF.Max(0.01f, skyR * 0.2f - depth * 0.02f);
                skyG = MathF.Max(0.01f, skyG * 0.15f - depth * 0.02f);
                skyB = MathF.Max(0.02f, skyB * 0.25f - depth * 0.015f);
            }

            // Stars (above silhouette)
            if (ty > silhouetteHeight + 0.04f)
            {
                float star = starNoise.GetNoise(x, y) * 0.5f + 0.5f;
                if (star > 0.93f)
                {
                    float brightness = (star - 0.93f) * 14f;
                    skyR = MathF.Min(1f, skyR + brightness * 0.8f);
                    skyG = MathF.Min(1f, skyG + brightness * 0.75f);
                    skyB = MathF.Min(1f, skyB + brightness * 0.9f);
                }
            }

            // Torch fire glow at horizon center (blood-orange)
            float fireGlow = MathF.Exp(-MathF.Pow((tx - 0.5f) * 4f, 2f)) *
                             MathF.Exp(-MathF.Pow((ty - treeHeight) * 8f, 2f));
            skyR = MathF.Min(1f, skyR + fireGlow * 0.55f);
            skyG = MathF.Min(1f, skyG + fireGlow * 0.18f);
            skyB = MathF.Min(1f, skyB + fireGlow * 0.05f);

            // Arch gateway glow (subtle inner light through the gate)
            if (inArch)
            {
                float archGlow = MathF.Exp(-MathF.Pow((ty - 0.35f) * 4f, 2f));
                skyR = MathF.Min(1f, skyR + archGlow * 0.12f);
                skyG = MathF.Min(1f, skyG + archGlow * 0.06f);
                skyB = MathF.Min(1f, skyB + archGlow * 0.02f);
            }

            int i = (y * width + x) * 4;
            pixels[i + 0] = ToByte(skyR);
            pixels[i + 1] = ToByte(skyG);
            pixels[i + 2] = ToByte(skyB);
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    /// <summary>
    /// Metal/iron texture for weapon/armor icons.
    /// </summary>
    public static byte[] GenerateMetal(int size = 64, int seed = 7000)
    {
        var noise = new FastNoiseLite(seed);
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFrequency(0.1f);

        var scratchNoise = new FastNoiseLite(seed + 300);
        scratchNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        scratchNoise.SetFrequency(0.3f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n = noise.GetNoise(x, y) * 0.5f + 0.5f;
            float scratch = scratchNoise.GetNoise(x * 3, y) * 0.5f + 0.5f;

            float v = 0.45f + n * 0.15f + MathF.Pow(scratch, 4f) * 0.12f;
            float r = v;
            float g = v * 0.97f;
            float b = v * 0.95f;

            int i = (y * size + x) * 4;
            pixels[i + 0] = ToByte(r);
            pixels[i + 1] = ToByte(g);
            pixels[i + 2] = ToByte(b);
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    private static byte ToByte(float v) => (byte)MathF.Min(255f, MathF.Max(0f, v * 255f));
}
