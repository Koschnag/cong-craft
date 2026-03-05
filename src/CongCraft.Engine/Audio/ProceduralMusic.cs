namespace CongCraft.Engine.Audio;

/// <summary>
/// Generates procedural music themes using additive synthesis.
/// Three themes: Menu (majestic), Exploration (peaceful), Combat (driving).
/// All audio computed from sine/triangle/sawtooth waves — no samples or files.
/// </summary>
public static class ProceduralMusic
{
    private const int SampleRate = 44100;

    /// <summary>
    /// Dark ambient drone (original, kept for compatibility).
    /// </summary>
    public static short[] GenerateAmbientDrone(int sampleRate = 44100, int durationSeconds = 30)
    {
        int totalSamples = sampleRate * durationSeconds;
        var samples = new short[totalSamples];

        float root = 73.42f;
        float fifth = 110.0f;
        float octave = 146.83f;
        float minorThird = 87.31f;

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;
            float mod1 = 0.5f + 0.5f * MathF.Sin(t * 0.15f * MathF.Tau);
            float mod2 = 0.5f + 0.5f * MathF.Sin(t * 0.08f * MathF.Tau + 1f);
            float mod3 = 0.5f + 0.5f * MathF.Sin(t * 0.05f * MathF.Tau + 2f);

            float sample = 0;
            sample += MathF.Sin(t * root * MathF.Tau) * 0.3f * mod1;
            sample += MathF.Sin(t * fifth * MathF.Tau) * 0.15f * mod2;
            sample += MathF.Sin(t * octave * MathF.Tau) * 0.1f * mod3;
            sample += MathF.Sin(t * minorThird * MathF.Tau) * 0.12f * mod1 * mod2;
            sample += MathF.Sin(t * 36.71f * MathF.Tau) * 0.15f;

            float texture = MathF.Sin(t * 440f * MathF.Tau) * MathF.Sin(t * 0.3f * MathF.Tau) * 0.02f;
            sample += texture;

            float fadeSamples = sampleRate * 2f;
            float fadeIn = Math.Min(1f, i / fadeSamples);
            float fadeOut = Math.Min(1f, (totalSamples - i) / fadeSamples);
            sample *= fadeIn * fadeOut;

            sample = Math.Clamp(sample, -0.95f, 0.95f);
            samples[i] = (short)(sample * short.MaxValue);
        }
        return samples;
    }

    /// <summary>
    /// Majestic menu theme — strings + brass, D major chord progression.
    /// </summary>
    public static short[] GenerateMenuTheme(int durationSeconds = 40)
    {
        int totalSamples = SampleRate * durationSeconds;
        var samples = new short[totalSamples];

        // D major progression: D - A - Bm - G
        float[][] chords = {
            new[] { 146.83f, 185.00f, 220.00f, 293.66f },
            new[] { 110.00f, 138.59f, 164.81f, 220.00f },
            new[] { 123.47f, 146.83f, 185.00f, 246.94f },
            new[] { 98.00f, 123.47f, 146.83f, 196.00f },
        };

        float chordDuration = durationSeconds / (float)chords.Length;

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / SampleRate;
            int chordIdx = Math.Min((int)(t / chordDuration), chords.Length - 1);
            float chordT = (t % chordDuration) / chordDuration;
            var chord = chords[chordIdx];

            float sample = 0;

            // Strings (sine + overtones)
            for (int n = 0; n < chord.Length; n++)
            {
                float freq = chord[n];
                float vol = 0.12f - n * 0.015f;
                float env = MathF.Min(1f, chordT * 4f) * MathF.Min(1f, (1f - chordT) * 3f);
                sample += MathF.Sin(t * freq * MathF.Tau) * vol * env;
                sample += MathF.Sin(t * freq * 2f * MathF.Tau) * vol * 0.3f * env;
                sample += MathF.Sin(t * freq * 3f * MathF.Tau) * vol * 0.1f * env;
            }

            // Brass drone
            float bassFreq = chord[0] * 0.5f;
            float brassMod = 0.5f + 0.5f * MathF.Sin(t * 0.2f * MathF.Tau);
            sample += Sawtooth(t * bassFreq) * 0.08f * brassMod;
            sample += Sawtooth(t * bassFreq * 2f) * 0.03f * brassMod;

            // Slow melody
            float melodyFreq = GetMenuMelodyNote(t);
            if (melodyFreq > 0)
            {
                float melodyEnv = MathF.Sin(((t * 0.5f) % 1f) * MathF.PI);
                sample += MathF.Sin(t * melodyFreq * MathF.Tau) * 0.06f * melodyEnv;
                sample += MathF.Sin(t * melodyFreq * 2f * MathF.Tau) * 0.02f * melodyEnv;
            }

            sample += MathF.Sin(t * 36.71f * MathF.Tau) * 0.06f;

            float fadeSamples = SampleRate * 3f;
            float fadeIn = Math.Min(1f, i / fadeSamples);
            float fadeOut = Math.Min(1f, (totalSamples - i) / fadeSamples);
            sample *= fadeIn * fadeOut * 0.85f;

            sample = Math.Clamp(sample, -0.95f, 0.95f);
            samples[i] = (short)(sample * short.MaxValue);
        }
        return samples;
    }

    /// <summary>
    /// Peaceful exploration theme — gentle strings, harp arpeggios.
    /// </summary>
    public static short[] GenerateExplorationTheme(int durationSeconds = 45)
    {
        int totalSamples = SampleRate * durationSeconds;
        var samples = new short[totalSamples];

        float[][] chords = {
            new[] { 110.00f, 130.81f, 164.81f },
            new[] { 87.31f, 110.00f, 130.81f },
            new[] { 130.81f, 164.81f, 196.00f },
            new[] { 98.00f, 123.47f, 146.83f },
        };

        float chordDuration = durationSeconds / (float)chords.Length;

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / SampleRate;
            int chordIdx = Math.Min((int)(t / chordDuration), chords.Length - 1);
            float chordT = (t % chordDuration) / chordDuration;
            var chord = chords[chordIdx];

            float sample = 0;

            // Soft pad
            for (int n = 0; n < chord.Length; n++)
            {
                float freq = chord[n];
                float env = MathF.Min(1f, chordT * 3f) * MathF.Min(1f, (1f - chordT) * 4f);
                sample += MathF.Sin(t * freq * MathF.Tau) * 0.08f * env;
                sample += MathF.Sin(t * freq * 2f * MathF.Tau) * 0.025f * env;
            }

            // Harp arpeggio
            float arpRate = 2.5f;
            float arpT = (t * arpRate) % 1f;
            int arpNote = (int)(t * arpRate) % chord.Length;
            float arpFreq = chord[arpNote] * 2f;
            float arpDecay = MathF.Exp(-arpT * 8f);
            sample += MathF.Sin(t * arpFreq * MathF.Tau) * 0.05f * arpDecay;
            sample += MathF.Sin(t * arpFreq * 3f * MathF.Tau) * 0.015f * arpDecay;

            // Flute melody
            float fluteFreq = GetExplorationMelodyNote(t);
            if (fluteFreq > 0)
            {
                float fluteEnv = MathF.Sin(((t * 0.33f) % 1f) * MathF.PI);
                sample += Triangle(t * fluteFreq) * 0.04f * fluteEnv;
            }

            float fadeSamples = SampleRate * 3f;
            float fadeIn = Math.Min(1f, i / fadeSamples);
            float fadeOut = Math.Min(1f, (totalSamples - i) / fadeSamples);
            sample *= fadeIn * fadeOut * 0.8f;

            sample = Math.Clamp(sample, -0.95f, 0.95f);
            samples[i] = (short)(sample * short.MaxValue);
        }
        return samples;
    }

    /// <summary>
    /// Driving combat theme — aggressive brass, percussion, minor key.
    /// </summary>
    public static short[] GenerateCombatTheme(int durationSeconds = 30)
    {
        int totalSamples = SampleRate * durationSeconds;
        var samples = new short[totalSamples];

        float[][] chords = {
            new[] { 73.42f, 87.31f, 110.00f },
            new[] { 58.27f, 73.42f, 87.31f },
            new[] { 49.00f, 58.27f, 73.42f },
            new[] { 55.00f, 69.30f, 82.41f },
        };

        float chordDuration = durationSeconds / (float)chords.Length;

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / SampleRate;
            int chordIdx = Math.Min((int)(t / chordDuration), chords.Length - 1);
            float chordT = (t % chordDuration) / chordDuration;
            var chord = chords[chordIdx];

            float sample = 0;

            // Power chords (sawtooth brass)
            for (int n = 0; n < chord.Length; n++)
            {
                float freq = chord[n] * 2f;
                float env = MathF.Min(1f, chordT * 6f) * MathF.Min(1f, (1f - chordT) * 2f);
                sample += Sawtooth(t * freq) * 0.06f * env;
                sample += MathF.Sin(t * freq * MathF.Tau) * 0.04f * env;
            }

            // Kick drum
            float bpm = 140f;
            float beatT = (t * bpm / 60f) % 1f;
            float kick = MathF.Exp(-beatT * 20f) * MathF.Sin(beatT * 80f * MathF.Tau);
            sample += kick * 0.15f;

            // Snare on off-beats
            float halfBeatT = (t * bpm / 60f + 0.5f) % 1f;
            if (halfBeatT < 0.05f)
            {
                float noise = MathF.Sin(t * 3000f) * MathF.Sin(t * 7000f) * MathF.Sin(t * 11000f);
                float snareDecay = MathF.Exp(-halfBeatT * 40f);
                sample += noise * 0.06f * snareDecay;
            }

            // War horn
            float hornFreq = GetCombatMelodyNote(t);
            if (hornFreq > 0)
            {
                float hornEnv = MathF.Min(1f, ((t * 2f) % 1f) * 5f) * MathF.Min(1f, (1f - (t * 2f) % 1f) * 3f);
                sample += Sawtooth(t * hornFreq) * 0.04f * hornEnv;
                sample += MathF.Sin(t * hornFreq * MathF.Tau) * 0.03f * hornEnv;
            }

            // Sub-bass throb
            float throb = MathF.Sin(t * 36f * MathF.Tau) * (0.06f + 0.04f * MathF.Sin(t * bpm / 60f * MathF.Tau));
            sample += throb;

            float fadeSamples = SampleRate * 2f;
            float fadeIn = Math.Min(1f, i / fadeSamples);
            float fadeOut = Math.Min(1f, (totalSamples - i) / fadeSamples);
            sample *= fadeIn * fadeOut * 0.75f;

            sample = Math.Clamp(sample, -0.95f, 0.95f);
            samples[i] = (short)(sample * short.MaxValue);
        }
        return samples;
    }

    private static float Sawtooth(float phase)
    {
        float p = phase % 1f;
        if (p < 0) p += 1f;
        return 2f * p - 1f;
    }

    private static float Triangle(float phase)
    {
        float p = phase % 1f;
        if (p < 0) p += 1f;
        return MathF.Abs(4f * p - 2f) - 1f;
    }

    private static float GetMenuMelodyNote(float t)
    {
        float[] notes = { 293.66f, 329.63f, 369.99f, 440.00f, 493.88f, 587.33f };
        int noteIdx = (int)(t * 0.4f) % notes.Length;
        float noteT = (t * 0.4f) % 1f;
        return noteT > 0.7f ? 0 : notes[noteIdx];
    }

    private static float GetExplorationMelodyNote(float t)
    {
        float[] notes = { 220.00f, 261.63f, 293.66f, 329.63f, 392.00f };
        int noteIdx = (int)(t * 0.5f) % notes.Length;
        float noteT = (t * 0.5f) % 1f;
        return noteT > 0.6f ? 0 : notes[noteIdx];
    }

    private static float GetCombatMelodyNote(float t)
    {
        float[] notes = { 293.66f, 349.23f, 293.66f, 440.00f, 392.00f, 349.23f };
        int noteIdx = (int)(t * 2.0f) % notes.Length;
        float noteT = (t * 2.0f) % 1f;
        return noteT > 0.8f ? 0 : notes[noteIdx];
    }

    // ─── Sound Effects ──────────────────────────────────────────────────

    /// <summary>Short metallic click for menu button press.</summary>
    public static short[] GenerateClickSfx()
    {
        int samples = SampleRate / 10; // 100ms
        var data = new short[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float decay = MathF.Exp(-t * 60f);
            float sample = MathF.Sin(t * 800f * MathF.Tau) * 0.4f * decay;
            sample += MathF.Sin(t * 1200f * MathF.Tau) * 0.2f * decay;
            sample += MathF.Sin(t * 2400f * MathF.Tau) * 0.1f * MathF.Exp(-t * 100f);
            sample = Math.Clamp(sample, -0.95f, 0.95f);
            data[i] = (short)(sample * short.MaxValue);
        }
        return data;
    }

    /// <summary>Soft tonal hover sound for menu item highlight.</summary>
    public static short[] GenerateHoverSfx()
    {
        int samples = SampleRate / 15; // ~67ms
        var data = new short[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float env = MathF.Sin(t * samples / (float)SampleRate * MathF.PI);
            float sample = MathF.Sin(t * 520f * MathF.Tau) * 0.15f * env;
            sample += MathF.Sin(t * 780f * MathF.Tau) * 0.08f * env;
            sample = Math.Clamp(sample, -0.95f, 0.95f);
            data[i] = (short)(sample * short.MaxValue);
        }
        return data;
    }

    /// <summary>Bright confirmation chime for menu selection.</summary>
    public static short[] GenerateSelectSfx()
    {
        int samples = SampleRate / 4; // 250ms
        var data = new short[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float decay = MathF.Exp(-t * 12f);
            float sweep = 440f + t * 600f; // rising pitch
            float sample = MathF.Sin(t * sweep * MathF.Tau) * 0.3f * decay;
            sample += MathF.Sin(t * sweep * 1.5f * MathF.Tau) * 0.15f * decay;
            sample += MathF.Sin(t * sweep * 2f * MathF.Tau) * 0.08f * MathF.Exp(-t * 20f);
            float fadeIn = Math.Min(1f, i / (SampleRate * 0.005f));
            sample *= fadeIn;
            sample = Math.Clamp(sample, -0.95f, 0.95f);
            data[i] = (short)(sample * short.MaxValue);
        }
        return data;
    }
}
