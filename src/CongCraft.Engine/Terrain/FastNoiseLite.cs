// FastNoiseLite - Simplified version for CongCraft terrain generation
// Based on FastNoiseLite by Jordan Peck (MIT License)
// https://github.com/Auburn/FastNoiseLite

using System.Runtime.CompilerServices;

namespace CongCraft.Engine.Terrain;

public sealed class FastNoiseLite
{
    public enum NoiseType { OpenSimplex2, Perlin, Value, Cellular }
    public enum FractalType { None, FBm, Ridged }

    private int _seed = 1337;
    private float _frequency = 0.01f;
    private NoiseType _noiseType = NoiseType.OpenSimplex2;
    private FractalType _fractalType = FractalType.None;
    private int _octaves = 3;
    private float _lacunarity = 2.0f;
    private float _gain = 0.5f;

    public FastNoiseLite(int seed = 1337) => _seed = seed;

    public void SetSeed(int seed) => _seed = seed;
    public void SetFrequency(float frequency) => _frequency = frequency;
    public void SetNoiseType(NoiseType type) => _noiseType = type;
    public void SetFractalType(FractalType type) => _fractalType = type;
    public void SetFractalOctaves(int octaves) => _octaves = octaves;
    public void SetFractalLacunarity(float lacunarity) => _lacunarity = lacunarity;
    public void SetFractalGain(float gain) => _gain = gain;

    public float GetNoise(float x, float y)
    {
        x *= _frequency;
        y *= _frequency;

        return _fractalType switch
        {
            FractalType.FBm => FractalFBm2D(x, y),
            FractalType.Ridged => FractalRidged2D(x, y),
            _ => SingleNoise2D(_seed, x, y)
        };
    }

    public float GetNoise(float x, float y, float z)
    {
        x *= _frequency;
        y *= _frequency;
        z *= _frequency;

        return _fractalType switch
        {
            FractalType.FBm => FractalFBm3D(x, y, z),
            FractalType.Ridged => FractalRidged3D(x, y, z),
            _ => SingleNoise3D(_seed, x, y, z)
        };
    }

    private float SingleNoise2D(int seed, float x, float y)
    {
        return _noiseType switch
        {
            NoiseType.OpenSimplex2 => OpenSimplex2_2D(seed, x, y),
            NoiseType.Perlin => Perlin2D(seed, x, y),
            NoiseType.Value => Value2D(seed, x, y),
            NoiseType.Cellular => Cellular2D(seed, x, y),
            _ => 0
        };
    }

    private float SingleNoise3D(int seed, float x, float y, float z)
    {
        return _noiseType switch
        {
            NoiseType.OpenSimplex2 => OpenSimplex2_3D(seed, x, y, z),
            NoiseType.Perlin => Perlin3D(seed, x, y, z),
            _ => 0
        };
    }

    private float FractalFBm2D(float x, float y)
    {
        int seed = _seed;
        float sum = 0, amp = 1f, freq = 1f;
        for (int i = 0; i < _octaves; i++)
        {
            sum += SingleNoise2D(seed++, x * freq, y * freq) * amp;
            amp *= _gain;
            freq *= _lacunarity;
        }
        return sum;
    }

    private float FractalRidged2D(float x, float y)
    {
        int seed = _seed;
        float sum = 0, amp = 1f, freq = 1f;
        for (int i = 0; i < _octaves; i++)
        {
            float n = MathF.Abs(SingleNoise2D(seed++, x * freq, y * freq));
            sum += (1f - n) * amp;
            amp *= _gain;
            freq *= _lacunarity;
        }
        return sum * 2f - 1f;
    }

    private float FractalFBm3D(float x, float y, float z)
    {
        int seed = _seed;
        float sum = 0, amp = 1f, freq = 1f;
        for (int i = 0; i < _octaves; i++)
        {
            sum += SingleNoise3D(seed++, x * freq, y * freq, z * freq) * amp;
            amp *= _gain;
            freq *= _lacunarity;
        }
        return sum;
    }

    private float FractalRidged3D(float x, float y, float z)
    {
        int seed = _seed;
        float sum = 0, amp = 1f, freq = 1f;
        for (int i = 0; i < _octaves; i++)
        {
            float n = MathF.Abs(SingleNoise3D(seed++, x * freq, y * freq, z * freq));
            sum += (1f - n) * amp;
            amp *= _gain;
            freq *= _lacunarity;
        }
        return sum * 2f - 1f;
    }

    // --- OpenSimplex2 ---

    private static float OpenSimplex2_2D(int seed, float x, float y)
    {
        // Skew to simplex space
        float s = (x + y) * 0.366025403784f; // (sqrt(3) - 1) / 2
        float xs = x + s, ys = y + s;
        int i = FastFloor(xs), j = FastFloor(ys);
        float g = (i + j) * 0.211324865405f; // (3 - sqrt(3)) / 6
        float x0 = x - (i - g), y0 = y - (j - g);

        int i1, j1;
        if (x0 > y0) { i1 = 1; j1 = 0; }
        else { i1 = 0; j1 = 1; }

        float x1 = x0 - i1 + 0.211324865405f;
        float y1 = y0 - j1 + 0.211324865405f;
        float x2 = x0 - 1f + 2f * 0.211324865405f;
        float y2 = y0 - 1f + 2f * 0.211324865405f;

        float n = 0;
        float t0 = 0.5f - x0 * x0 - y0 * y0;
        if (t0 > 0) { t0 *= t0; n += t0 * t0 * GradCoord2D(seed, i, j, x0, y0); }

        float t1 = 0.5f - x1 * x1 - y1 * y1;
        if (t1 > 0) { t1 *= t1; n += t1 * t1 * GradCoord2D(seed, i + i1, j + j1, x1, y1); }

        float t2 = 0.5f - x2 * x2 - y2 * y2;
        if (t2 > 0) { t2 *= t2; n += t2 * t2 * GradCoord2D(seed, i + 1, j + 1, x2, y2); }

        return n * 99.83685446303647f;
    }

    private static float OpenSimplex2_3D(int seed, float x, float y, float z)
    {
        // Classic simplex noise 3D
        float s = (x + y + z) / 3f;
        float xs = x + s, ys = y + s, zs = z + s;
        int i = FastFloor(xs), j = FastFloor(ys), k = FastFloor(zs);

        float g = (i + j + k) / 6f;
        float x0 = x - (i - g), y0 = y - (j - g), z0 = z - (k - g);

        int i1, j1, k1, i2, j2, k2;
        if (x0 >= y0)
        {
            if (y0 >= z0) { i1=1; j1=0; k1=0; i2=1; j2=1; k2=0; }
            else if (x0 >= z0) { i1=1; j1=0; k1=0; i2=1; j2=0; k2=1; }
            else { i1=0; j1=0; k1=1; i2=1; j2=0; k2=1; }
        }
        else
        {
            if (y0 < z0) { i1=0; j1=0; k1=1; i2=0; j2=1; k2=1; }
            else if (x0 < z0) { i1=0; j1=1; k1=0; i2=0; j2=1; k2=1; }
            else { i1=0; j1=1; k1=0; i2=1; j2=1; k2=0; }
        }

        float x1 = x0 - i1 + 1/6f, y1 = y0 - j1 + 1/6f, z1 = z0 - k1 + 1/6f;
        float x2 = x0 - i2 + 1/3f, y2 = y0 - j2 + 1/3f, z2 = z0 - k2 + 1/3f;
        float x3 = x0 - 0.5f, y3 = y0 - 0.5f, z3 = z0 - 0.5f;

        float n = 0;
        float t0 = 0.6f - x0*x0 - y0*y0 - z0*z0;
        if (t0 > 0) { t0 *= t0; n += t0 * t0 * GradCoord3D(seed, i, j, k, x0, y0, z0); }

        float t1 = 0.6f - x1*x1 - y1*y1 - z1*z1;
        if (t1 > 0) { t1 *= t1; n += t1 * t1 * GradCoord3D(seed, i+i1, j+j1, k+k1, x1, y1, z1); }

        float t2 = 0.6f - x2*x2 - y2*y2 - z2*z2;
        if (t2 > 0) { t2 *= t2; n += t2 * t2 * GradCoord3D(seed, i+i2, j+j2, k+k2, x2, y2, z2); }

        float t3 = 0.6f - x3*x3 - y3*y3 - z3*z3;
        if (t3 > 0) { t3 *= t3; n += t3 * t3 * GradCoord3D(seed, i+1, j+1, k+1, x3, y3, z3); }

        return n * 32f;
    }

    // --- Perlin ---

    private static float Perlin2D(int seed, float x, float y)
    {
        int x0 = FastFloor(x), y0 = FastFloor(y);
        float xd0 = x - x0, yd0 = y - y0;
        float xd1 = xd0 - 1, yd1 = yd0 - 1;
        float xs = InterpQuintic(xd0), ys = InterpQuintic(yd0);

        float xf0 = Lerp(GradCoord2D(seed, x0, y0, xd0, yd0), GradCoord2D(seed, x0 + 1, y0, xd1, yd0), xs);
        float xf1 = Lerp(GradCoord2D(seed, x0, y0 + 1, xd0, yd1), GradCoord2D(seed, x0 + 1, y0 + 1, xd1, yd1), xs);
        return Lerp(xf0, xf1, ys);
    }

    private static float Perlin3D(int seed, float x, float y, float z)
    {
        int x0 = FastFloor(x), y0 = FastFloor(y), z0 = FastFloor(z);
        float xd0 = x - x0, yd0 = y - y0, zd0 = z - z0;
        float xd1 = xd0 - 1, yd1 = yd0 - 1, zd1 = zd0 - 1;
        float xs = InterpQuintic(xd0), ys = InterpQuintic(yd0), zs = InterpQuintic(zd0);

        float xf00 = Lerp(GradCoord3D(seed, x0, y0, z0, xd0, yd0, zd0), GradCoord3D(seed, x0+1, y0, z0, xd1, yd0, zd0), xs);
        float xf10 = Lerp(GradCoord3D(seed, x0, y0+1, z0, xd0, yd1, zd0), GradCoord3D(seed, x0+1, y0+1, z0, xd1, yd1, zd0), xs);
        float xf01 = Lerp(GradCoord3D(seed, x0, y0, z0+1, xd0, yd0, zd1), GradCoord3D(seed, x0+1, y0, z0+1, xd1, yd0, zd1), xs);
        float xf11 = Lerp(GradCoord3D(seed, x0, y0+1, z0+1, xd0, yd1, zd1), GradCoord3D(seed, x0+1, y0+1, z0+1, xd1, yd1, zd1), xs);

        float yf0 = Lerp(xf00, xf10, ys);
        float yf1 = Lerp(xf01, xf11, ys);
        return Lerp(yf0, yf1, zs);
    }

    // --- Value ---

    private static float Value2D(int seed, float x, float y)
    {
        int x0 = FastFloor(x), y0 = FastFloor(y);
        float xs = InterpQuintic(x - x0), ys = InterpQuintic(y - y0);

        float xf0 = Lerp(ValCoord2D(seed, x0, y0), ValCoord2D(seed, x0 + 1, y0), xs);
        float xf1 = Lerp(ValCoord2D(seed, x0, y0 + 1), ValCoord2D(seed, x0 + 1, y0 + 1), xs);
        return Lerp(xf0, xf1, ys);
    }

    // --- Cellular ---

    private static float Cellular2D(int seed, float x, float y)
    {
        int xr = FastRound(x), yr = FastRound(y);
        float minDist = float.MaxValue;

        for (int xi = xr - 1; xi <= xr + 1; xi++)
        for (int yi = yr - 1; yi <= yr + 1; yi++)
        {
            int hash = Hash2D(seed, xi, yi);
            float vx = xi + (hash & 0xFF) / 255f - 0.5f;
            float vy = yi + ((hash >> 8) & 0xFF) / 255f - 0.5f;
            float dx = x - vx, dy = y - vy;
            float dist = dx * dx + dy * dy;
            minDist = MathF.Min(minDist, dist);
        }

        return MathF.Sqrt(minDist) - 1f;
    }

    // --- Utility ---

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FastFloor(float f) => f >= 0 ? (int)f : (int)f - 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FastRound(float f) => f >= 0 ? (int)(f + 0.5f) : (int)(f - 0.5f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Lerp(float a, float b, float t) => a + t * (b - a);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float InterpQuintic(float t) => t * t * t * (t * (t * 6 - 15) + 10);

    private static readonly int[] Gradients2D =
    {
         1, 0, -1, 0, 0, 1, 0,-1,
         1, 1, -1, 1, 1,-1,-1,-1,
    };

    private static readonly int[] Gradients3D =
    {
         1, 1, 0, -1, 1, 0,  1,-1, 0, -1,-1, 0,
         1, 0, 1, -1, 0, 1,  1, 0,-1, -1, 0,-1,
         0, 1, 1,  0,-1, 1,  0, 1,-1,  0,-1,-1,
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Hash2D(int seed, int x, int y)
    {
        int h = seed ^ (x * 1619) ^ (y * 31337);
        h = h * h * h * 60493;
        return (h >> 13) ^ h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Hash3D(int seed, int x, int y, int z)
    {
        int h = seed ^ (x * 1619) ^ (y * 31337) ^ (z * 6971);
        h = h * h * h * 60493;
        return (h >> 13) ^ h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float GradCoord2D(int seed, int x, int y, float xd, float yd)
    {
        int hash = Hash2D(seed, x, y) & 15;
        int idx = (hash & 7) * 2;
        return xd * Gradients2D[idx] + yd * Gradients2D[idx + 1];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float GradCoord3D(int seed, int x, int y, int z, float xd, float yd, float zd)
    {
        int hash = Hash3D(seed, x, y, z);
        int idx = (hash & 11) * 3;
        idx = Math.Abs(idx) % Gradients3D.Length;
        // Ensure we don't go out of bounds (need 3 consecutive values)
        if (idx + 2 >= Gradients3D.Length) idx = 0;
        return xd * Gradients3D[idx] + yd * Gradients3D[idx + 1] + zd * Gradients3D[idx + 2];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float ValCoord2D(int seed, int x, int y)
    {
        int n = Hash2D(seed, x, y);
        return n / (float)int.MaxValue;
    }
}
