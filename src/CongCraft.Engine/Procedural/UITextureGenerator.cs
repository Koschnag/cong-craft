using CongCraft.Engine.Terrain;
using Silk.NET.OpenGL;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates procedural textures for UI elements: parchment, gold, leather, ornamental borders.
/// All textures are warm medieval palette — no external files needed.
/// Inspired by illuminated manuscript aesthetics.
/// </summary>
public static class UITextureGenerator
{
    /// <summary>
    /// Warm parchment/vellum background texture for UI panels.
    /// Cream/tan base with subtle fiber patterns and age spots.
    /// </summary>
    public static byte[] GenerateParchment(int size = 128, int seed = 1337)
    {
        var noise = new FastNoiseLite(seed);
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFractalOctaves(5);
        noise.SetFrequency(0.03f);

        var noise2 = new FastNoiseLite(seed + 100);
        noise2.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise2.SetFrequency(0.08f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n1 = noise.GetNoise(x, y) * 0.5f + 0.5f;
            float n2 = noise2.GetNoise(x, y) * 0.5f + 0.5f;

            // Parchment base: warm cream/tan
            float r = 0.82f + n1 * 0.12f - n2 * 0.05f;
            float g = 0.72f + n1 * 0.10f - n2 * 0.04f;
            float b = 0.55f + n1 * 0.08f - n2 * 0.03f;

            // Edge darkening (vignette)
            float dx = (x / (float)size - 0.5f) * 2f;
            float dy = (y / (float)size - 0.5f) * 2f;
            float vignette = 1f - (dx * dx + dy * dy) * 0.15f;
            r *= vignette;
            g *= vignette;
            b *= vignette;

            int i = (y * size + x) * 4;
            pixels[i + 0] = ToByte(r);
            pixels[i + 1] = ToByte(g);
            pixels[i + 2] = ToByte(b);
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    /// <summary>
    /// Gold leaf texture for ornamental borders and decorations.
    /// Metallic sheen with hammered surface variation.
    /// </summary>
    public static byte[] GenerateGoldLeaf(int size = 64, int seed = 2000)
    {
        var noise = new FastNoiseLite(seed);
        noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        noise.SetFrequency(0.06f);

        var noise2 = new FastNoiseLite(seed + 50);
        noise2.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise2.SetFrequency(0.12f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n1 = noise.GetNoise(x, y) * 0.5f + 0.5f;
            float n2 = noise2.GetNoise(x, y) * 0.5f + 0.5f;

            // Gold base with metallic variation
            float r = 0.85f + n1 * 0.15f;
            float g = 0.68f + n1 * 0.12f + n2 * 0.05f;
            float b = 0.20f + n1 * 0.10f;

            // Specular highlights
            float highlight = MathF.Pow(n2, 3f) * 0.3f;
            r = MathF.Min(1f, r + highlight);
            g = MathF.Min(1f, g + highlight * 0.8f);

            int i = (y * size + x) * 4;
            pixels[i + 0] = ToByte(r);
            pixels[i + 1] = ToByte(g);
            pixels[i + 2] = ToByte(b);
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    /// <summary>
    /// Dark leather texture for button backgrounds.
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

            // Dark brown leather
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
    /// Wood grain texture for frames.
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
            // Ring pattern
            float ring = MathF.Sin((n * 10f + y * 0.1f) * MathF.PI) * 0.5f + 0.5f;

            float r = 0.40f + ring * 0.15f;
            float g = 0.28f + ring * 0.10f;
            float b = 0.15f + ring * 0.05f;

            int i = (y * size + x) * 4;
            pixels[i + 0] = ToByte(r);
            pixels[i + 1] = ToByte(g);
            pixels[i + 2] = ToByte(b);
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    /// <summary>
    /// Ornamental vine/floral border pattern for illuminated manuscript style HUD.
    /// Generates a 1D border texture: a horizontal strip with vine patterns.
    /// </summary>
    public static byte[] GenerateVineBorder(int width = 256, int height = 32, int seed = 5000)
    {
        var pixels = new byte[width * height * 4];

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            float tx = (float)x / width;
            float ty = (float)y / height;

            // Vine wave pattern
            float vine = MathF.Sin(tx * MathF.PI * 8f + MathF.Sin(tx * MathF.PI * 3f) * 1.5f) * 0.3f + 0.5f;
            float dist = MathF.Abs(ty - vine);

            // Vine stem
            float stem = MathF.Exp(-dist * dist * 80f);

            // Leaf/flourish shapes at regular intervals
            float leafPhase = tx * MathF.PI * 4f;
            float leaf = MathF.Max(0f,
                MathF.Cos(leafPhase) * MathF.Exp(-MathF.Pow(MathF.Abs(ty - vine + MathF.Sin(leafPhase) * 0.15f), 2f) * 40f));

            // Small berries/dots
            float berry = 0f;
            for (int bi = 0; bi < 6; bi++)
            {
                float bx = (bi + 0.5f) / 6f;
                float by = MathF.Sin(bx * MathF.PI * 8f + MathF.Sin(bx * MathF.PI * 3f) * 1.5f) * 0.3f + 0.5f;
                by += (bi % 2 == 0 ? 0.12f : -0.12f);
                float bdist = MathF.Sqrt((tx - bx) * (tx - bx) * 16f + (ty - by) * (ty - by));
                berry = MathF.Max(berry, MathF.Exp(-bdist * bdist * 200f));
            }

            float total = MathF.Min(1f, stem * 0.8f + leaf * 0.6f + berry * 0.9f);

            // Gold color for ornaments
            float r = total * 0.90f;
            float g = total * 0.72f;
            float b = total * 0.25f;

            int i = (y * width + x) * 4;
            pixels[i + 0] = ToByte(r);
            pixels[i + 1] = ToByte(g);
            pixels[i + 2] = ToByte(b);
            pixels[i + 3] = ToByte(total);
        }
        return pixels;
    }

    /// <summary>
    /// Ornate corner decoration for UI panels. Quadrant pattern: place at corners with rotation.
    /// </summary>
    public static byte[] GenerateCornerOrnament(int size = 64)
    {
        var pixels = new byte[size * size * 4];

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float tx = (float)x / size;
            float ty = (float)y / size;

            // Spiral/flourish from corner
            float angle = MathF.Atan2(ty, tx);
            float radius = MathF.Sqrt(tx * tx + ty * ty);

            // Spiral arm
            float spiral = MathF.Sin(angle * 3f + radius * 8f) * 0.5f + 0.5f;
            float mask = MathF.Exp(-radius * 2.5f);
            float flourish = spiral * mask;

            // Circular dots along the spiral
            float dots = 0f;
            for (int di = 1; di <= 4; di++)
            {
                float dr = di * 0.18f;
                float da = di * 1.2f;
                float ddist = MathF.Sqrt((radius - dr) * (radius - dr) + MathF.Pow(angle - da, 2f) * 0.02f);
                dots = MathF.Max(dots, MathF.Exp(-ddist * ddist * 100f));
            }

            float total = MathF.Min(1f, flourish * 0.7f + dots * 0.8f);

            // Gold
            float r = total * 0.90f;
            float g = total * 0.72f;
            float b = total * 0.25f;

            int i = (y * size + x) * 4;
            pixels[i + 0] = ToByte(r);
            pixels[i + 1] = ToByte(g);
            pixels[i + 2] = ToByte(b);
            pixels[i + 3] = ToByte(total * 0.9f);
        }
        return pixels;
    }

    /// <summary>
    /// Simple dark atmospheric background for menu - night sky with stars.
    /// </summary>
    public static byte[] GenerateMenuBackground(int width = 512, int height = 256, int seed = 6000)
    {
        var noise = new FastNoiseLite(seed);
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFractalOctaves(4);
        noise.SetFrequency(0.008f);

        var starNoise = new FastNoiseLite(seed + 999);
        starNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        starNoise.SetFrequency(0.15f);

        var pixels = new byte[width * height * 4];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            float tx = (float)x / width;
            float ty = (float)y / height;

            // Night sky gradient (darker at top)
            float skyR = 0.04f + ty * 0.06f;
            float skyG = 0.04f + ty * 0.04f;
            float skyB = 0.08f + ty * 0.08f;

            // Clouds/mist
            float cloud = noise.GetNoise(x, y) * 0.5f + 0.5f;
            cloud = MathF.Pow(cloud, 2f) * 0.15f;
            skyR += cloud * 0.5f;
            skyG += cloud * 0.3f;
            skyB += cloud * 0.6f;

            // Landscape silhouette at bottom
            float mountainNoise = noise.GetNoise(x * 2, 0) * 0.5f + 0.5f;
            float mountainLine = 0.15f + mountainNoise * 0.15f;
            if (ty < mountainLine)
            {
                // Dark mountain
                float depth = (mountainLine - ty) * 4f;
                skyR = MathF.Max(0, skyR - depth * 0.1f);
                skyG = MathF.Max(0, skyG - depth * 0.1f);
                skyB = MathF.Max(0, skyB - depth * 0.08f);
            }

            // Stars (only above mountains)
            if (ty > mountainLine + 0.05f)
            {
                float star = starNoise.GetNoise(x, y) * 0.5f + 0.5f;
                if (star > 0.92f)
                {
                    float brightness = (star - 0.92f) * 12f;
                    skyR = MathF.Min(1f, skyR + brightness * 0.9f);
                    skyG = MathF.Min(1f, skyG + brightness * 0.85f);
                    skyB = MathF.Min(1f, skyB + brightness * 0.7f);
                }
            }

            // Warm glow on horizon
            float horizonGlow = MathF.Exp(-MathF.Pow((ty - mountainLine) * 6f, 2f)) * 0.2f;
            skyR += horizonGlow * 0.8f;
            skyG += horizonGlow * 0.4f;
            skyB += horizonGlow * 0.2f;

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
