using CongCraft.Engine.Core;
using CongCraft.Engine.ECS.Systems;
using Silk.NET.OpenAL;

namespace CongCraft.Engine.Audio;

/// <summary>
/// Manages OpenAL audio context and plays background music/ambient sounds.
/// </summary>
public sealed class AudioSystem : ISystem
{
    public int Priority => 120;

    private AL? _al;
    private ALContext? _alc;
    private unsafe Device* _device;
    private unsafe Context* _context;
    private uint _musicSource = 0;
    private uint _musicBuffer = 0;
    private uint _ambientSource;
    private uint _ambientBuffer;
    private bool _initialized;

    public void Initialize(ServiceLocator services)
    {
        try
        {
            _alc = ALContext.GetApi();
            _al = AL.GetApi();

            unsafe
            {
                _device = _alc.OpenDevice(null);
                if (_device == null) return;

                _context = _alc.CreateContext(_device, null);
                _alc.MakeContextCurrent(_context);
            }

            // Generate procedural ambient drone
            var ambientData = ProceduralMusic.GenerateAmbientDrone(sampleRate: 44100, durationSeconds: 30);
            _ambientBuffer = _al.GenBuffer();
            _ambientSource = _al.GenSource();

            unsafe
            {
                fixed (short* data = ambientData)
                {
                    _al.BufferData(_ambientBuffer, BufferFormat.Mono16, data,
                        ambientData.Length * sizeof(short), 44100);
                }
            }

            _al.SetSourceProperty(_ambientSource, SourceInteger.Buffer, (int)_ambientBuffer);
            _al.SetSourceProperty(_ambientSource, SourceBoolean.Looping, true);
            _al.SetSourceProperty(_ambientSource, SourceFloat.Gain, 0.3f);
            _al.SourcePlay(_ambientSource);

            _initialized = true;
        }
        catch
        {
            // Audio is optional — game works without it
            _initialized = false;
        }
    }

    public void Update(GameTime time) { }
    public void Render(GameTime time) { }

    public void Dispose()
    {
        if (!_initialized || _al == null || _alc == null) return;

        _al.SourceStop(_ambientSource);
        _al.DeleteSource(_ambientSource);
        _al.DeleteBuffer(_ambientBuffer);

        if (_musicSource != 0)
        {
            _al.SourceStop(_musicSource);
            _al.DeleteSource(_musicSource);
            _al.DeleteBuffer(_musicBuffer);
        }

        unsafe
        {
            _alc.DestroyContext(_context);
            _alc.CloseDevice(_device);
        }

        _al.Dispose();
        _alc.Dispose();
    }
}
