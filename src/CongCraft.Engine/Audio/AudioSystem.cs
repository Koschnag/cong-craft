using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using Silk.NET.OpenAL;

namespace CongCraft.Engine.Audio;

/// <summary>
/// Manages OpenAL audio context and plays background music.
/// Switches between menu/exploration/combat themes based on GameMode.
/// </summary>
public sealed class AudioSystem : ISystem
{
    public int Priority => 120;

    private AL? _al;
    private ALContext? _alc;
    private unsafe Device* _device;
    private unsafe Context* _context;
    private bool _initialized;
    private World? _world;

    // Music tracks
    private uint _menuSource, _menuBuffer;
    private uint _explorationSource, _explorationBuffer;
    private uint _combatSource, _combatBuffer;

    private GameMode _currentPlaying = (GameMode)(-1);
    private float _fadeTarget = 0.4f;
    private const float MusicVolume = 0.4f;

    public void Initialize(ServiceLocator services)
    {
        _world = services.Get<World>();

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

            // Generate all music themes
            LoadTrack(ProceduralMusic.GenerateMenuTheme(40), out _menuBuffer, out _menuSource);
            LoadTrack(ProceduralMusic.GenerateExplorationTheme(45), out _explorationBuffer, out _explorationSource);
            LoadTrack(ProceduralMusic.GenerateCombatTheme(30), out _combatBuffer, out _combatSource);

            // Start with menu music
            _al.SetSourceProperty(_menuSource, SourceFloat.Gain, MusicVolume);
            _al.SourcePlay(_menuSource);
            _currentPlaying = GameMode.MainMenu;

            _initialized = true;
        }
        catch
        {
            _initialized = false;
        }
    }

    private unsafe void LoadTrack(short[] data, out uint buffer, out uint source)
    {
        buffer = _al!.GenBuffer();
        source = _al.GenSource();

        fixed (short* p = data)
        {
            _al.BufferData(buffer, BufferFormat.Mono16, p,
                data.Length * sizeof(short), 44100);
        }

        _al.SetSourceProperty(source, SourceInteger.Buffer, (int)buffer);
        _al.SetSourceProperty(source, SourceBoolean.Looping, true);
        _al.SetSourceProperty(source, SourceFloat.Gain, 0f);
    }

    public void Update(GameTime time)
    {
        if (!_initialized || _al == null || _world == null) return;

        // Determine target music based on game mode
        GameMode targetMode = GameMode.MainMenu;
        if (_world.TryGetSingleton<GameStateManager>(out var gs) && gs != null)
        {
            targetMode = gs.CurrentMode;
            // Paused keeps the current gameplay music (just quieter)
            if (targetMode == GameMode.Paused)
                targetMode = GameMode.Playing;
        }

        if (targetMode != _currentPlaying)
        {
            // Crossfade: stop old, start new
            StopSource(GetSourceForMode(_currentPlaying));
            var newSource = GetSourceForMode(targetMode);
            _al.SetSourceProperty(newSource, SourceFloat.Gain, MusicVolume);
            _al.SourcePlay(newSource);
            _currentPlaying = targetMode;
        }

        // Lower volume during pause
        if (_world.TryGetSingleton<GameStateManager>(out var gs2) && gs2 != null)
        {
            float vol = gs2.CurrentMode == GameMode.Paused ? MusicVolume * 0.4f : MusicVolume;
            var activeSource = GetSourceForMode(_currentPlaying);
            _al.SetSourceProperty(activeSource, SourceFloat.Gain, vol);
        }
    }

    private uint GetSourceForMode(GameMode mode) => mode switch
    {
        GameMode.MainMenu => _menuSource,
        GameMode.Playing => _explorationSource,
        _ => _explorationSource
    };

    private void StopSource(uint source)
    {
        if (source == 0) return;
        _al?.SetSourceProperty(source, SourceFloat.Gain, 0f);
        _al?.SourceStop(source);
    }

    public void Render(GameTime time) { }

    public void Dispose()
    {
        if (!_initialized || _al == null || _alc == null) return;

        void Cleanup(uint source, uint buffer)
        {
            if (source != 0) { _al.SourceStop(source); _al.DeleteSource(source); }
            if (buffer != 0) _al.DeleteBuffer(buffer);
        }

        Cleanup(_menuSource, _menuBuffer);
        Cleanup(_explorationSource, _explorationBuffer);
        Cleanup(_combatSource, _combatBuffer);

        unsafe
        {
            _alc.DestroyContext(_context);
            _alc.CloseDevice(_device);
        }

        _al.Dispose();
        _alc.Dispose();
    }
}
