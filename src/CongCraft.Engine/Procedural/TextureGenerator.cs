using Silk.NET.OpenGL;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Generates high-quality procedural textures from noise — no image files needed.
/// 512x512 resolution with multi-octave detail and mipmaps.
/// </summary>
public static class TextureGenerator
{
    private const int DefaultSize = 512;

    public static byte[] GenerateGrassPixels(int size = DefaultSize, int seed = 42)
    {
        var noise = new CongCraft.Engine.Terrain.FastNoiseLite(seed);
        noise.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFrequency(0.04f);

        var detail = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 100);
        detail.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        detail.SetFractalType(CongCraft.Engine.Terrain.FastNoiseLite.FractalType.FBm);
        detail.SetFractalOctaves(5);
        detail.SetFrequency(0.12f);

        var blade = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 200);
        blade.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.Cellular);
        blade.SetFrequency(0.10f);

        // Fine grain for close-up detail (Gothic 3/Two Worlds style)
        var fineGrain = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 600);
        fineGrain.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        fineGrain.SetFrequency(0.25f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n = noise.GetNoise(x, y) * 0.5f + 0.5f;
            float d = detail.GetNoise(x, y) * 0.5f + 0.5f;
            float bl = blade.GetNoise(x, y) * 0.5f + 0.5f;
            float fg = fineGrain.GetNoise(x, y) * 0.5f + 0.5f;
            float combined = n * 0.35f + d * 0.30f + bl * 0.20f + fg * 0.15f;

            // Richer, more varied greens with earthy undertones
            int i = (y * size + x) * 4;
            float warmth = n * 0.15f; // Slight warm patches
            pixels[i + 0] = (byte)Math.Clamp(30 + combined * 50 + warmth * 40, 0, 255);
            pixels[i + 1] = (byte)Math.Clamp(50 + combined * 115, 0, 255);
            pixels[i + 2] = (byte)Math.Clamp(10 + combined * 22, 0, 255);
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    public static byte[] GenerateStonePixels(int size = DefaultSize, int seed = 123)
    {
        var noise = new CongCraft.Engine.Terrain.FastNoiseLite(seed);
        noise.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.Cellular);
        noise.SetFrequency(0.03f);

        var crack = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 50);
        crack.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.Cellular);
        crack.SetFrequency(0.05f);

        var roughness = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 300);
        roughness.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        roughness.SetFractalType(CongCraft.Engine.Terrain.FastNoiseLite.FractalType.FBm);
        roughness.SetFractalOctaves(6);
        roughness.SetFrequency(0.08f);

        // Vein/mineral detail layer
        var veins = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 700);
        veins.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        veins.SetFrequency(0.15f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n = noise.GetNoise(x, y) * 0.5f + 0.5f;
            float cr = crack.GetNoise(x, y) * 0.5f + 0.5f;
            float rough = roughness.GetNoise(x, y) * 0.5f + 0.5f;
            float vn = veins.GetNoise(x, y) * 0.5f + 0.5f;
            float combined = n * 0.35f + cr * 0.30f + rough * 0.25f + vn * 0.10f;

            // Warmer stone with slight color variation (like Gothic 3 mountain stone)
            int i = (y * size + x) * 4;
            float warmShift = vn * 8f;
            byte rVal = (byte)Math.Clamp(88 + combined * 92 + warmShift, 0, 255);
            byte gVal = (byte)Math.Clamp(84 + combined * 88, 0, 255);
            byte bVal = (byte)Math.Clamp(78 + combined * 82, 0, 255);
            pixels[i + 0] = rVal;
            pixels[i + 1] = gVal;
            pixels[i + 2] = bVal;
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    public static byte[] GenerateDirtPixels(int size = DefaultSize, int seed = 777)
    {
        var noise = new CongCraft.Engine.Terrain.FastNoiseLite(seed);
        noise.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFractalType(CongCraft.Engine.Terrain.FastNoiseLite.FractalType.FBm);
        noise.SetFractalOctaves(5);
        noise.SetFrequency(0.05f);

        var pebble = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 400);
        pebble.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.Cellular);
        pebble.SetFrequency(0.06f);

        var grain = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 500);
        grain.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        grain.SetFrequency(0.2f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n = noise.GetNoise(x, y) * 0.5f + 0.5f;
            float p = pebble.GetNoise(x, y) * 0.5f + 0.5f;
            float g = grain.GetNoise(x, y) * 0.5f + 0.5f;
            float combined = n * 0.45f + p * 0.3f + g * 0.25f;

            int i = (y * size + x) * 4;
            pixels[i + 0] = (byte)(80 + combined * 70);
            pixels[i + 1] = (byte)(55 + combined * 55);
            pixels[i + 2] = (byte)(28 + combined * 35);
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    /// <summary>
    /// Generates a snow/ice texture for high-altitude terrain.
    /// Sparkly crystals with blue-tinted shadows.
    /// </summary>
    public static byte[] GenerateSnowPixels(int size = DefaultSize, int seed = 1100)
    {
        var noise = new CongCraft.Engine.Terrain.FastNoiseLite(seed);
        noise.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFrequency(0.03f);

        var sparkle = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 100);
        sparkle.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.Cellular);
        sparkle.SetFrequency(0.2f);

        var drift = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 200);
        drift.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        drift.SetFractalType(CongCraft.Engine.Terrain.FastNoiseLite.FractalType.FBm);
        drift.SetFractalOctaves(4);
        drift.SetFrequency(0.06f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n = noise.GetNoise(x, y) * 0.5f + 0.5f;
            float sp = sparkle.GetNoise(x, y) * 0.5f + 0.5f;
            float dr = drift.GetNoise(x, y) * 0.5f + 0.5f;
            float combined = n * 0.4f + sp * 0.25f + dr * 0.35f;

            int i = (y * size + x) * 4;
            // Bright white-blue snow with subtle blue tint in crevices
            float blueTint = (1.0f - combined) * 0.08f;
            pixels[i + 0] = (byte)Math.Clamp(210 + combined * 40, 0, 255);
            pixels[i + 1] = (byte)Math.Clamp(215 + combined * 38, 0, 255);
            pixels[i + 2] = (byte)Math.Clamp(225 + combined * 28 + blueTint * 20, 0, 255);
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    /// <summary>
    /// Generates a path/road texture for worn terrain paths.
    /// Packed earth with pebbles and wear marks.
    /// </summary>
    public static byte[] GeneratePathPixels(int size = DefaultSize, int seed = 1200)
    {
        var noise = new CongCraft.Engine.Terrain.FastNoiseLite(seed);
        noise.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFractalType(CongCraft.Engine.Terrain.FastNoiseLite.FractalType.FBm);
        noise.SetFractalOctaves(4);
        noise.SetFrequency(0.06f);

        var pebbles = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 100);
        pebbles.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.Cellular);
        pebbles.SetFrequency(0.1f);

        var wear = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 200);
        wear.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        wear.SetFrequency(0.04f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n = noise.GetNoise(x, y) * 0.5f + 0.5f;
            float pb = pebbles.GetNoise(x, y) * 0.5f + 0.5f;
            float wr = wear.GetNoise(x, y) * 0.5f + 0.5f;
            float combined = n * 0.4f + pb * 0.3f + wr * 0.3f;

            int i = (y * size + x) * 4;
            // Sandy-brown packed earth
            pixels[i + 0] = (byte)Math.Clamp(120 + combined * 55, 0, 255);
            pixels[i + 1] = (byte)Math.Clamp(100 + combined * 45, 0, 255);
            pixels[i + 2] = (byte)Math.Clamp(65 + combined * 35, 0, 255);
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    /// <summary>
    /// Generates a metallic surface texture (armor, weapons, metal fittings).
    /// Scratched, slightly worn metal with specular variation.
    /// </summary>
    public static byte[] GenerateMetalPixels(int size = DefaultSize, int seed = 500)
    {
        var noise = new CongCraft.Engine.Terrain.FastNoiseLite(seed);
        noise.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFrequency(0.06f);

        var scratches = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 100);
        scratches.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.Cellular);

        scratches.SetFrequency(0.08f);

        var grain = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 200);
        grain.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        grain.SetFractalType(CongCraft.Engine.Terrain.FastNoiseLite.FractalType.FBm);
        grain.SetFractalOctaves(6);
        grain.SetFrequency(0.2f);

        var pitting = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 300);
        pitting.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.Cellular);
        pitting.SetFrequency(0.12f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n = noise.GetNoise(x, y) * 0.5f + 0.5f;
            float sc = scratches.GetNoise(x, y) * 0.5f + 0.5f;
            float gr = grain.GetNoise(x, y) * 0.5f + 0.5f;
            float pt = pitting.GetNoise(x, y) * 0.5f + 0.5f;
            float combined = n * 0.3f + sc * 0.25f + gr * 0.25f + pt * 0.2f;

            int i = (y * size + x) * 4;
            // Silver-gray metal with slight warm tint from rust/patina
            float warmPatch = pt > 0.6f ? (pt - 0.6f) * 0.5f : 0f;
            pixels[i + 0] = (byte)Math.Clamp(140 + combined * 80 + warmPatch * 30, 0, 255);
            pixels[i + 1] = (byte)Math.Clamp(135 + combined * 75, 0, 255);
            pixels[i + 2] = (byte)Math.Clamp(130 + combined * 70, 0, 255);
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    /// <summary>
    /// Generates a leather/hide texture (armor straps, boots, belts).
    /// Worn leather with grain and creases.
    /// </summary>
    public static byte[] GenerateLeatherPixels(int size = DefaultSize, int seed = 600)
    {
        var noise = new CongCraft.Engine.Terrain.FastNoiseLite(seed);
        noise.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFractalType(CongCraft.Engine.Terrain.FastNoiseLite.FractalType.FBm);
        noise.SetFractalOctaves(5);
        noise.SetFrequency(0.05f);

        var grain = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 100);
        grain.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.Cellular);

        grain.SetFrequency(0.15f);

        var crease = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 200);
        crease.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        crease.SetFrequency(0.08f);

        var detail = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 300);
        detail.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        detail.SetFrequency(0.25f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n = noise.GetNoise(x, y) * 0.5f + 0.5f;
            float gr = grain.GetNoise(x, y) * 0.5f + 0.5f;
            float cr = crease.GetNoise(x, y) * 0.5f + 0.5f;
            float dt = detail.GetNoise(x, y) * 0.5f + 0.5f;
            float combined = n * 0.35f + gr * 0.25f + cr * 0.2f + dt * 0.2f;

            int i = (y * size + x) * 4;
            // Warm brown leather tones
            pixels[i + 0] = (byte)Math.Clamp(95 + combined * 65, 0, 255);
            pixels[i + 1] = (byte)Math.Clamp(60 + combined * 50, 0, 255);
            pixels[i + 2] = (byte)Math.Clamp(30 + combined * 30, 0, 255);
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    /// <summary>
    /// Generates a skin/organic texture (faces, hands, exposed skin).
    /// Subtle pore detail and color variation.
    /// </summary>
    public static byte[] GenerateSkinPixels(int size = DefaultSize, int seed = 700)
    {
        var noise = new CongCraft.Engine.Terrain.FastNoiseLite(seed);
        noise.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFrequency(0.03f);

        var pores = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 100);
        pores.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.Cellular);
        pores.SetFrequency(0.3f);

        var blush = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 200);
        blush.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        blush.SetFrequency(0.015f);

        var micro = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 300);
        micro.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        micro.SetFractalType(CongCraft.Engine.Terrain.FastNoiseLite.FractalType.FBm);
        micro.SetFractalOctaves(4);
        micro.SetFrequency(0.15f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n = noise.GetNoise(x, y) * 0.5f + 0.5f;
            float pr = pores.GetNoise(x, y) * 0.5f + 0.5f;
            float bl = blush.GetNoise(x, y) * 0.5f + 0.5f;
            float mc = micro.GetNoise(x, y) * 0.5f + 0.5f;
            float combined = n * 0.3f + pr * 0.2f + bl * 0.2f + mc * 0.3f;

            int i = (y * size + x) * 4;
            // Warm peach-brown skin tones with subtle red blush patches
            float blushTint = bl > 0.55f ? (bl - 0.55f) * 0.4f : 0f;
            pixels[i + 0] = (byte)Math.Clamp(170 + combined * 45 + blushTint * 40, 0, 255);
            pixels[i + 1] = (byte)Math.Clamp(130 + combined * 35, 0, 255);
            pixels[i + 2] = (byte)Math.Clamp(100 + combined * 30, 0, 255);
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    /// <summary>
    /// Generates a wood/bark texture (trees, wooden structures, staffs).
    /// Grain lines with knot patterns.
    /// </summary>
    public static byte[] GenerateWoodPixels(int size = DefaultSize, int seed = 800)
    {
        // Use stretched noise for wood grain direction
        var grain = new CongCraft.Engine.Terrain.FastNoiseLite(seed);
        grain.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        grain.SetFrequency(0.04f);

        var rings = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 100);
        rings.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        rings.SetFrequency(0.02f);

        var bark = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 200);
        bark.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.Cellular);

        bark.SetFrequency(0.06f);

        var detail = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 300);
        detail.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        detail.SetFractalType(CongCraft.Engine.Terrain.FastNoiseLite.FractalType.FBm);
        detail.SetFractalOctaves(5);
        detail.SetFrequency(0.12f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            // Stretch Y for wood grain effect
            float gr = grain.GetNoise(x, y * 3) * 0.5f + 0.5f;
            float rn = rings.GetNoise(x, y * 3) * 0.5f + 0.5f;
            float bk = bark.GetNoise(x, y) * 0.5f + 0.5f;
            float dt = detail.GetNoise(x, y) * 0.5f + 0.5f;

            // Create ring pattern
            float ringPattern = MathF.Sin(rn * MathF.PI * 8 + gr * 4) * 0.5f + 0.5f;
            float combined = ringPattern * 0.35f + bk * 0.25f + gr * 0.2f + dt * 0.2f;

            int i = (y * size + x) * 4;
            // Warm brown wood tones
            pixels[i + 0] = (byte)Math.Clamp(75 + combined * 70, 0, 255);
            pixels[i + 1] = (byte)Math.Clamp(50 + combined * 50, 0, 255);
            pixels[i + 2] = (byte)Math.Clamp(25 + combined * 30, 0, 255);
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    /// <summary>
    /// Generates a fabric/cloth texture (cloaks, robes, tunics).
    /// Woven pattern with subtle folds.
    /// </summary>
    public static byte[] GenerateFabricPixels(int size = DefaultSize, int seed = 900)
    {
        var weave = new CongCraft.Engine.Terrain.FastNoiseLite(seed);
        weave.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        weave.SetFrequency(0.15f);

        var folds = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 100);
        folds.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        folds.SetFrequency(0.03f);

        var thread = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 200);
        thread.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.Cellular);

        thread.SetFrequency(0.25f);

        var dirt = new CongCraft.Engine.Terrain.FastNoiseLite(seed + 300);
        dirt.SetNoiseType(CongCraft.Engine.Terrain.FastNoiseLite.NoiseType.OpenSimplex2);
        dirt.SetFractalType(CongCraft.Engine.Terrain.FastNoiseLite.FractalType.FBm);
        dirt.SetFractalOctaves(3);
        dirt.SetFrequency(0.02f);

        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float wv = weave.GetNoise(x, y) * 0.5f + 0.5f;
            float fd = folds.GetNoise(x, y) * 0.5f + 0.5f;
            float th = thread.GetNoise(x, y) * 0.5f + 0.5f;
            float dr = dirt.GetNoise(x, y) * 0.5f + 0.5f;

            // Cross-hatch weave pattern
            float weaveX = MathF.Sin(x * 0.5f) * 0.5f + 0.5f;
            float weaveY = MathF.Sin(y * 0.5f) * 0.5f + 0.5f;
            float weavePattern = (weaveX * weaveY) * 0.3f + 0.7f;

            float combined = wv * 0.25f + fd * 0.25f + th * 0.2f + weavePattern * 0.3f;
            float dirtStain = dr < 0.35f ? (0.35f - dr) * 0.3f : 0f;

            int i = (y * size + x) * 4;
            // Neutral gray-brown fabric (tinted by vertex color in shader)
            pixels[i + 0] = (byte)Math.Clamp(140 + combined * 60 - dirtStain * 40, 0, 255);
            pixels[i + 1] = (byte)Math.Clamp(135 + combined * 55 - dirtStain * 35, 0, 255);
            pixels[i + 2] = (byte)Math.Clamp(125 + combined * 50 - dirtStain * 30, 0, 255);
            pixels[i + 3] = 255;
        }
        return pixels;
    }

    /// <summary>
    /// Upload a texture with mipmaps for higher quality rendering at distance.
    /// </summary>
    public static unsafe uint UploadTexture(GL gl, byte[] pixels, int width, int height)
    {
        uint texture = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, texture);

        // Set pixel alignment before upload (required for macOS compatibility)
        gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

        // Set filtering params before mipmap generation for correct allocation
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);

        fixed (byte* p = pixels)
        {
            // Use Rgba8 (explicitly sized) — macOS Core Profile requires sized internal formats
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8,
                (uint)width, (uint)height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, p);
        }

        // Generate mipmaps after upload and filter setup
        gl.GenerateMipmap(TextureTarget.Texture2D);

        // Anisotropic filtering (best quality for terrain viewed at angles)
        gl.GetFloat(GLEnum.MaxTextureMaxAnisotropy, out float maxAniso);
        if (maxAniso > 1f)
        {
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxAnisotropy,
                MathF.Min(maxAniso, 16f));
        }

        gl.BindTexture(TextureTarget.Texture2D, 0);

        return texture;
    }
}
