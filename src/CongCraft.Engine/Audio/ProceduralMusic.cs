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

    // ─── Gameplay Sound Effects ─────────────────────────────────────────

    /// <summary>Sword swing whoosh — fast rising noise burst.</summary>
    public static short[] GenerateSwordSwingSfx()
    {
        int samples = SampleRate / 5; // 200ms
        var data = new short[samples];
        var rng = new Random(101);
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float env = MathF.Exp(-t * 15f) * MathF.Min(1f, t * 200f);
            float freq = 200f + t * 800f; // rising whoosh
            float noise = (float)(rng.NextDouble() * 2 - 1);
            float sample = noise * 0.3f * env;
            sample += MathF.Sin(t * freq * MathF.Tau) * 0.15f * env;
            sample += MathF.Sin(t * freq * 1.5f * MathF.Tau) * 0.08f * env;
            data[i] = (short)(Math.Clamp(sample, -0.95f, 0.95f) * short.MaxValue);
        }
        return data;
    }

    /// <summary>Sword hit impact — metallic clang + thud.</summary>
    public static short[] GenerateSwordHitSfx()
    {
        int samples = SampleRate / 4; // 250ms
        var data = new short[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float decay = MathF.Exp(-t * 25f);
            // Metallic ring
            float metal = MathF.Sin(t * 1800f * MathF.Tau) * 0.25f * decay;
            metal += MathF.Sin(t * 2400f * MathF.Tau) * 0.12f * MathF.Exp(-t * 35f);
            metal += MathF.Sin(t * 3200f * MathF.Tau) * 0.06f * MathF.Exp(-t * 50f);
            // Impact thud
            float thud = MathF.Sin(t * 80f * MathF.Tau) * 0.3f * MathF.Exp(-t * 40f);
            float sample = metal + thud;
            data[i] = (short)(Math.Clamp(sample, -0.95f, 0.95f) * short.MaxValue);
        }
        return data;
    }

    /// <summary>Soft footstep on grass — muffled thump + rustle.</summary>
    public static short[] GenerateFootstepGrassSfx()
    {
        int samples = SampleRate / 10; // 100ms
        var data = new short[samples];
        var rng = new Random(201);
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float decay = MathF.Exp(-t * 50f);
            float thump = MathF.Sin(t * 60f * MathF.Tau) * 0.2f * decay;
            float rustle = (float)(rng.NextDouble() * 2 - 1) * 0.12f * MathF.Exp(-t * 30f);
            data[i] = (short)(Math.Clamp(thump + rustle, -0.95f, 0.95f) * short.MaxValue);
        }
        return data;
    }

    /// <summary>Footstep on stone — harder, sharper impact.</summary>
    public static short[] GenerateFootstepStoneSfx()
    {
        int samples = SampleRate / 10; // 100ms
        var data = new short[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float decay = MathF.Exp(-t * 60f);
            float click = MathF.Sin(t * 400f * MathF.Tau) * 0.2f * decay;
            click += MathF.Sin(t * 800f * MathF.Tau) * 0.1f * MathF.Exp(-t * 80f);
            float thud = MathF.Sin(t * 120f * MathF.Tau) * 0.15f * MathF.Exp(-t * 50f);
            data[i] = (short)(Math.Clamp(click + thud, -0.95f, 0.95f) * short.MaxValue);
        }
        return data;
    }

    /// <summary>Enemy taking a hit — fleshy impact + grunt.</summary>
    public static short[] GenerateEnemyHitSfx()
    {
        int samples = SampleRate / 5; // 200ms
        var data = new short[samples];
        var rng = new Random(301);
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float impact = MathF.Sin(t * 100f * MathF.Tau) * 0.35f * MathF.Exp(-t * 30f);
            // Vocal grunt (low formant)
            float grunt = MathF.Sin(t * 150f * MathF.Tau) * 0.15f * MathF.Exp(-t * 12f);
            grunt += MathF.Sin(t * 250f * MathF.Tau) * 0.08f * MathF.Exp(-t * 15f);
            float noise = (float)(rng.NextDouble() * 2 - 1) * 0.05f * MathF.Exp(-t * 20f);
            data[i] = (short)(Math.Clamp(impact + grunt + noise, -0.95f, 0.95f) * short.MaxValue);
        }
        return data;
    }

    /// <summary>Enemy death — descending groan + collapse thud.</summary>
    public static short[] GenerateEnemyDeathSfx()
    {
        int samples = SampleRate / 2; // 500ms
        var data = new short[samples];
        var rng = new Random(302);
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            // Descending groan
            float freq = 180f - t * 120f;
            float groan = MathF.Sin(t * freq * MathF.Tau) * 0.2f * MathF.Exp(-t * 5f);
            groan += MathF.Sin(t * freq * 1.5f * MathF.Tau) * 0.08f * MathF.Exp(-t * 6f);
            // Collapse thud
            float thud = 0f;
            if (t > 0.3f)
            {
                float tt = t - 0.3f;
                thud = MathF.Sin(tt * 50f * MathF.Tau) * 0.25f * MathF.Exp(-tt * 30f);
            }
            float noise = (float)(rng.NextDouble() * 2 - 1) * 0.03f * MathF.Exp(-t * 8f);
            data[i] = (short)(Math.Clamp(groan + thud + noise, -0.95f, 0.95f) * short.MaxValue);
        }
        return data;
    }

    /// <summary>Player taking damage — sharp yelp + impact.</summary>
    public static short[] GeneratePlayerHurtSfx()
    {
        int samples = SampleRate / 4; // 250ms
        var data = new short[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            // Sharp vocal yelp (higher pitch)
            float yelp = MathF.Sin(t * 320f * MathF.Tau) * 0.2f * MathF.Exp(-t * 10f);
            yelp += MathF.Sin(t * 480f * MathF.Tau) * 0.12f * MathF.Exp(-t * 12f);
            // Impact
            float hit = MathF.Sin(t * 90f * MathF.Tau) * 0.25f * MathF.Exp(-t * 35f);
            data[i] = (short)(Math.Clamp(yelp + hit, -0.95f, 0.95f) * short.MaxValue);
        }
        return data;
    }

    /// <summary>Item pickup — bright ascending chime.</summary>
    public static short[] GenerateItemPickupSfx()
    {
        int samples = SampleRate / 5; // 200ms
        var data = new short[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float decay = MathF.Exp(-t * 10f);
            float freq = 600f + t * 1200f; // quick ascending sparkle
            float sample = MathF.Sin(t * freq * MathF.Tau) * 0.25f * decay;
            sample += MathF.Sin(t * freq * 2f * MathF.Tau) * 0.12f * MathF.Exp(-t * 15f);
            sample += MathF.Sin(t * 880f * MathF.Tau) * 0.08f * decay; // bell overtone
            data[i] = (short)(Math.Clamp(sample, -0.95f, 0.95f) * short.MaxValue);
        }
        return data;
    }

    /// <summary>Spell casting — ethereal whoosh + crystalline shimmer.</summary>
    public static short[] GenerateSpellCastSfx()
    {
        int samples = SampleRate * 2 / 5; // 400ms
        var data = new short[samples];
        var rng = new Random(401);
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float env = MathF.Min(1f, t * 15f) * MathF.Exp(-t * 6f);
            // Ethereal sweep
            float sweep = 300f + MathF.Sin(t * 3f * MathF.Tau) * 200f;
            float crystal = MathF.Sin(t * sweep * MathF.Tau) * 0.18f * env;
            crystal += MathF.Sin(t * sweep * 1.5f * MathF.Tau) * 0.1f * env;
            // Shimmer noise
            float shimmer = (float)(rng.NextDouble() * 2 - 1) * 0.06f * env;
            // Deep undertone
            float bass = MathF.Sin(t * 80f * MathF.Tau) * 0.1f * env;
            data[i] = (short)(Math.Clamp(crystal + shimmer + bass, -0.95f, 0.95f) * short.MaxValue);
        }
        return data;
    }

    /// <summary>Dodge roll whoosh — quick air displacement.</summary>
    public static short[] GenerateDodgeWhooshSfx()
    {
        int samples = SampleRate / 6; // ~167ms
        var data = new short[samples];
        var rng = new Random(501);
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float env = MathF.Sin(t * 6f * MathF.PI) * MathF.Exp(-t * 12f);
            if (env < 0) env = 0;
            float noise = (float)(rng.NextDouble() * 2 - 1) * 0.3f * env;
            float whoosh = MathF.Sin(t * (150f + t * 400f) * MathF.Tau) * 0.15f * env;
            data[i] = (short)(Math.Clamp(noise + whoosh, -0.95f, 0.95f) * short.MaxValue);
        }
        return data;
    }

    // ─── Voice Synthesis (AI-style procedural speech) ──────────────────

    /// <summary>
    /// Generates a procedural voice line for NPC dialogue.
    /// Uses formant synthesis to produce speech-like sounds.
    /// pitch: 0.5 = deep male, 1.0 = normal, 1.5 = high female
    /// textLength: approximate number of characters (affects duration)
    /// </summary>
    public static short[] GenerateVoiceLine(float pitch = 1.0f, int textLength = 40, int seed = 0)
    {
        float wordsPerSec = 3.0f;
        float durationSec = Math.Max(0.5f, textLength / 5f / wordsPerSec);
        int totalSamples = (int)(SampleRate * durationSec);
        var data = new short[totalSamples];
        var rng = new Random(seed);

        // Formant frequencies for vowel-like sounds (A, E, I, O, U)
        float[][] formants = {
            new[] { 800f, 1200f, 2500f },  // A
            new[] { 500f, 1800f, 2500f },  // E
            new[] { 300f, 2300f, 3000f },  // I
            new[] { 500f, 900f, 2500f },   // O
            new[] { 350f, 600f, 2500f },   // U
        };

        float glottalFreq = 120f * pitch; // fundamental voice frequency
        float syllableDuration = 0.12f + (float)rng.NextDouble() * 0.08f;

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / SampleRate;

            // Syllable index and phase
            int syllableIdx = (int)(t / syllableDuration);
            float syllablePhase = (t % syllableDuration) / syllableDuration;

            // Choose formant set based on syllable
            int formantIdx = (syllableIdx + seed) % formants.Length;
            var f = formants[formantIdx];

            // Glottal pulse (voice source — buzzy waveform)
            float glottal = 0f;
            float gPhase = (t * glottalFreq) % 1f;
            // Rosenberg glottal pulse approximation
            if (gPhase < 0.4f)
                glottal = 0.5f * (1f - MathF.Cos(gPhase / 0.4f * MathF.PI));
            else if (gPhase < 0.6f)
                glottal = 0.5f * (1f + MathF.Cos((gPhase - 0.4f) / 0.2f * MathF.PI));

            // Apply pitch variation (natural speech prosody)
            float pitchVar = 1.0f + 0.05f * MathF.Sin(t * 3f * MathF.Tau)
                                  + 0.03f * MathF.Sin(t * 7f * MathF.Tau);
            glottal *= pitchVar;

            // Formant resonances
            float voiced = 0f;
            for (int fi = 0; fi < 3; fi++)
            {
                float freq = f[fi] * pitch;
                float bw = freq * 0.12f; // bandwidth
                float amp = 1.0f / (1f + fi * 0.5f);
                voiced += MathF.Sin(t * freq * MathF.Tau) * amp * glottal;
            }

            // Consonant-like noise bursts at syllable boundaries
            float consonant = 0f;
            if (syllablePhase < 0.15f || syllablePhase > 0.85f)
            {
                float noise = (float)(rng.NextDouble() * 2 - 1);
                float consEnv = syllablePhase < 0.15f
                    ? MathF.Sin(syllablePhase / 0.15f * MathF.PI * 0.5f)
                    : MathF.Sin((syllablePhase - 0.85f) / 0.15f * MathF.PI * 0.5f);
                consonant = noise * 0.15f * consEnv;

                // Sibilant (S/SH) on some syllables
                if (syllableIdx % 3 == 0)
                    consonant += MathF.Sin(t * 5000f * MathF.Tau) * noise * 0.05f * consEnv;
            }

            // Syllable amplitude envelope
            float env = MathF.Sin(syllablePhase * MathF.PI);
            // Add micro-pauses between words (every ~3-5 syllables)
            int wordBoundary = 3 + (seed % 3);
            if (syllableIdx % wordBoundary == wordBoundary - 1 && syllablePhase > 0.7f)
                env *= MathF.Max(0f, 1f - (syllablePhase - 0.7f) * 5f);

            float sample = (voiced * 0.4f + consonant) * env;

            // Overall fade in/out
            float fadeSamples = SampleRate * 0.1f;
            float fadeIn = Math.Min(1f, i / fadeSamples);
            float fadeOut = Math.Min(1f, (totalSamples - i) / fadeSamples);
            sample *= fadeIn * fadeOut;

            data[i] = (short)(Math.Clamp(sample, -0.95f, 0.95f) * short.MaxValue);
        }
        return data;
    }

    /// <summary>NPC greeting voice — warm, medium pitch.</summary>
    public static short[] GenerateNpcGreetingSfx() => GenerateVoiceLine(1.0f, 30, 701);

    /// <summary>NPC farewell voice — slightly lower pitch.</summary>
    public static short[] GenerateNpcFarewellSfx() => GenerateVoiceLine(0.9f, 20, 702);

    /// <summary>Female NPC voice — higher pitch.</summary>
    public static short[] GenerateNpcFemaleVoiceSfx() => GenerateVoiceLine(1.3f, 35, 703);

    /// <summary>Deep male voice (blacksmith, warrior).</summary>
    public static short[] GenerateNpcDeepVoiceSfx() => GenerateVoiceLine(0.7f, 40, 704);

    /// <summary>Merchant haggling voice.</summary>
    public static short[] GenerateNpcMerchantVoiceSfx() => GenerateVoiceLine(1.1f, 25, 705);

    // ─── Ambient Loops ──────────────────────────────────────────────────

    /// <summary>Procedural wind ambience — layered filtered noise with gusts.</summary>
    public static short[] GenerateAmbientWind(int durationSeconds = 20)
    {
        int totalSamples = SampleRate * durationSeconds;
        var samples = new short[totalSamples];
        var rng = new Random(600);

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / SampleRate;

            // Base wind (filtered noise)
            float noise = (float)(rng.NextDouble() * 2 - 1);
            float windMod = 0.3f + 0.2f * MathF.Sin(t * 0.15f * MathF.Tau)
                                 + 0.15f * MathF.Sin(t * 0.07f * MathF.Tau + 1.2f)
                                 + 0.1f * MathF.Sin(t * 0.03f * MathF.Tau + 2.5f);

            float sample = noise * 0.12f * windMod;

            // Gentle tree rustle
            float rustle = MathF.Sin(t * 1200f * MathF.Tau) * MathF.Sin(t * 7f * MathF.Tau);
            sample += rustle * 0.02f * windMod;

            // Occasional distant bird chirp (every ~8 seconds)
            float birdPhase = (t * 0.125f) % 1f;
            if (birdPhase < 0.02f)
            {
                float birdT = birdPhase / 0.02f;
                float birdEnv = MathF.Sin(birdT * MathF.PI);
                float birdFreq = 2200f + MathF.Sin(birdT * 12f * MathF.PI) * 600f;
                sample += MathF.Sin(t * birdFreq * MathF.Tau) * 0.04f * birdEnv;
            }

            // Loop fade
            float fadeSamples = SampleRate * 3f;
            float fadeIn = Math.Min(1f, i / fadeSamples);
            float fadeOut = Math.Min(1f, (totalSamples - i) / fadeSamples);
            sample *= fadeIn * fadeOut;

            samples[i] = (short)(Math.Clamp(sample, -0.95f, 0.95f) * short.MaxValue);
        }
        return samples;
    }
}
