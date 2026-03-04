namespace CongCraft.Engine.Audio;

/// <summary>
/// Generates procedural ambient audio — dark medieval drone atmosphere.
/// Uses layered sine waves at harmonic intervals with slow modulation.
/// </summary>
public static class ProceduralMusic
{
    public static short[] GenerateAmbientDrone(int sampleRate = 44100, int durationSeconds = 30)
    {
        int totalSamples = sampleRate * durationSeconds;
        var samples = new short[totalSamples];

        // Root frequencies for a dark, medieval drone (D minor-ish)
        float root = 73.42f;      // D2
        float fifth = 110.0f;     // A2
        float octave = 146.83f;   // D3
        float minorThird = 87.31f; // F2

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;

            // Slow amplitude modulation for breathing effect
            float mod1 = 0.5f + 0.5f * MathF.Sin(t * 0.15f * MathF.Tau);
            float mod2 = 0.5f + 0.5f * MathF.Sin(t * 0.08f * MathF.Tau + 1f);
            float mod3 = 0.5f + 0.5f * MathF.Sin(t * 0.05f * MathF.Tau + 2f);

            // Layer sine waves
            float sample = 0;
            sample += MathF.Sin(t * root * MathF.Tau) * 0.3f * mod1;
            sample += MathF.Sin(t * fifth * MathF.Tau) * 0.15f * mod2;
            sample += MathF.Sin(t * octave * MathF.Tau) * 0.1f * mod3;
            sample += MathF.Sin(t * minorThird * MathF.Tau) * 0.12f * mod1 * mod2;

            // Sub-bass rumble
            sample += MathF.Sin(t * 36.71f * MathF.Tau) * 0.15f;

            // Subtle noise/texture via high-frequency modulation
            float texture = MathF.Sin(t * 440f * MathF.Tau) * MathF.Sin(t * 0.3f * MathF.Tau) * 0.02f;
            sample += texture;

            // Smooth fade in/out at boundaries
            float fadeSamples = sampleRate * 2f; // 2 second fade
            float fadeIn = Math.Min(1f, i / fadeSamples);
            float fadeOut = Math.Min(1f, (totalSamples - i) / fadeSamples);
            sample *= fadeIn * fadeOut;

            // Clamp and convert to 16-bit
            sample = Math.Clamp(sample, -0.95f, 0.95f);
            samples[i] = (short)(sample * short.MaxValue);
        }

        return samples;
    }
}
