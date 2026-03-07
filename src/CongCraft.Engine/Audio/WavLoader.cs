namespace CongCraft.Engine.Audio;

/// <summary>
/// Minimal WAV file loader for PCM 16-bit audio data.
/// Supports mono and stereo, 44100 Hz. AOT-safe (no reflection).
/// </summary>
public static class WavLoader
{
    public readonly record struct WavData(short[] Samples, int SampleRate, int Channels, int BitsPerSample);

    /// <summary>
    /// Load a WAV file and return its PCM samples.
    /// Returns null if the file doesn't exist or can't be parsed.
    /// </summary>
    public static WavData? Load(string path)
    {
        if (!File.Exists(path)) return null;

        try
        {
            using var stream = File.OpenRead(path);
            using var reader = new BinaryReader(stream);

            // RIFF header
            var riff = new string(reader.ReadChars(4));
            if (riff != "RIFF") return null;

            reader.ReadInt32(); // file size

            var wave = new string(reader.ReadChars(4));
            if (wave != "WAVE") return null;

            // Find fmt chunk
            int channels = 0, sampleRate = 0, bitsPerSample = 0;
            short[] samples = Array.Empty<short>();

            while (stream.Position < stream.Length)
            {
                var chunkId = new string(reader.ReadChars(4));
                int chunkSize = reader.ReadInt32();

                if (chunkId == "fmt ")
                {
                    int audioFormat = reader.ReadInt16();
                    if (audioFormat != 1) // Only PCM supported
                        return null;

                    channels = reader.ReadInt16();
                    sampleRate = reader.ReadInt32();
                    reader.ReadInt32(); // byte rate
                    reader.ReadInt16(); // block align
                    bitsPerSample = reader.ReadInt16();

                    // Skip any extra format bytes
                    int extraBytes = chunkSize - 16;
                    if (extraBytes > 0) reader.ReadBytes(extraBytes);
                }
                else if (chunkId == "data")
                {
                    if (bitsPerSample == 16)
                    {
                        int sampleCount = chunkSize / 2;
                        samples = new short[sampleCount];
                        for (int i = 0; i < sampleCount; i++)
                            samples[i] = reader.ReadInt16();
                    }
                    else if (bitsPerSample == 8)
                    {
                        // Convert 8-bit unsigned to 16-bit signed
                        int sampleCount = chunkSize;
                        samples = new short[sampleCount];
                        for (int i = 0; i < sampleCount; i++)
                            samples[i] = (short)((reader.ReadByte() - 128) << 8);
                    }
                    else
                    {
                        return null; // Unsupported bit depth
                    }
                }
                else
                {
                    // Skip unknown chunks
                    if (chunkSize > 0 && stream.Position + chunkSize <= stream.Length)
                        reader.ReadBytes(chunkSize);
                    else
                        break;
                }
            }

            if (samples.Length == 0 || sampleRate == 0) return null;

            // Convert stereo to mono if needed (OpenAL Mono16 expected)
            if (channels == 2)
            {
                var mono = new short[samples.Length / 2];
                for (int i = 0; i < mono.Length; i++)
                    mono[i] = (short)((samples[i * 2] + samples[i * 2 + 1]) / 2);
                samples = mono;
                channels = 1;
            }

            return new WavData(samples, sampleRate, channels, 16);
        }
        catch
        {
            return null;
        }
    }
}
