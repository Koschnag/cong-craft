using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using Silk.NET.OpenAL;

namespace CongCraft.Engine.Audio;

/// <summary>
/// Manages OpenAL audio context, background music, and sound effects.
/// Switches between menu/exploration/combat themes based on GameMode.
/// Provides SFX playback for UI interactions and gameplay.
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

    // SFX (one-shot sources, pooled)
    private uint _sfxClickBuffer;
    private uint _sfxHoverBuffer;
    private uint _sfxSelectBuffer;
    private readonly uint[] _sfxSources = new uint[8];
    private int _nextSfxSource;

    private GameMode _currentPlaying = (GameMode)(-1);
    private const float MusicVolume = 0.4f;
    private const float SfxVolume = 0.6f;

    // Singleton accessor for other systems to trigger SFX
    private static AudioSystem? _instance;
    public static AudioSystem? Instance => _instance;

    public void Initialize(ServiceLocator services)
    {
        _world = services.Get<World>();
        _instance = this;

        try
        {
            _alc = ALContext.GetApi();
            _al = AL.GetApi();
            DevLog.Info("OpenAL API acquired");

            unsafe
            {
                _device = _alc.OpenDevice(null);
                if (_device == null)
                {
                    DevLog.Warn("OpenAL: Failed to open audio device — audio disabled");
                    return;
                }

                _context = _alc.CreateContext(_device, null);
                _alc.MakeContextCurrent(_context);
            }

            DevLog.Info("OpenAL device and context created");

            // Generate all music themes
            LoadTrack(ProceduralMusic.GenerateMenuTheme(40), out _menuBuffer, out _menuSource);
            LoadTrack(ProceduralMusic.GenerateExplorationTheme(45), out _explorationBuffer, out _explorationSource);
            LoadTrack(ProceduralMusic.GenerateCombatTheme(30), out _combatBuffer, out _combatSource);

            // Generate SFX
            LoadSfxBuffer(ProceduralMusic.GenerateClickSfx(), out _sfxClickBuffer);
            LoadSfxBuffer(ProceduralMusic.GenerateHoverSfx(), out _sfxHoverBuffer);
            LoadSfxBuffer(ProceduralMusic.GenerateSelectSfx(), out _sfxSelectBuffer);

            // Create SFX source pool
            for (int i = 0; i < _sfxSources.Length; i++)
                _sfxSources[i] = _al.GenSource();

            DevLog.Info("Audio tracks and SFX loaded");

            // Start with menu music
            _al.SetSourceProperty(_menuSource, SourceFloat.Gain, MusicVolume);
            _al.SourcePlay(_menuSource);
            _currentPlaying = GameMode.MainMenu;

            _initialized = true;
            DevLog.Info("AudioSystem initialized successfully");
        }
        catch (Exception ex)
        {
            DevLog.Warn($"AudioSystem init failed: {ex.Message} — audio disabled");
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

    private unsafe void LoadSfxBuffer(short[] data, out uint buffer)
    {
        buffer = _al!.GenBuffer();
        fixed (short* p = data)
        {
            _al.BufferData(buffer, BufferFormat.Mono16, p,
                data.Length * sizeof(short), 44100);
        }
    }

    /// <summary>Play a one-shot SFX using a round-robin source pool.</summary>
    public void PlaySfx(SfxType type)
    {
        if (!_initialized || _al == null) return;

        uint buffer = type switch
        {
            SfxType.Click => _sfxClickBuffer,
            SfxType.Hover => _sfxHoverBuffer,
            SfxType.Select => _sfxSelectBuffer,
            _ => 0
        };
        if (buffer == 0) return;

        uint src = _sfxSources[_nextSfxSource];
        _nextSfxSource = (_nextSfxSource + 1) % _sfxSources.Length;

        _al.SourceStop(src);
        _al.SetSourceProperty(src, SourceInteger.Buffer, (int)buffer);
        _al.SetSourceProperty(src, SourceBoolean.Looping, false);
        _al.SetSourceProperty(src, SourceFloat.Gain, SfxVolume);
        _al.SourcePlay(src);
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
        _instance = null;
        if (!_initialized || _al == null || _alc == null) return;

        void Cleanup(uint source, uint buffer)
        {
            if (source != 0) { _al.SourceStop(source); _al.DeleteSource(source); }
            if (buffer != 0) _al.DeleteBuffer(buffer);
        }

        Cleanup(_menuSource, _menuBuffer);
        Cleanup(_explorationSource, _explorationBuffer);
        Cleanup(_combatSource, _combatBuffer);

        foreach (var src in _sfxSources)
            if (src != 0) { _al.SourceStop(src); _al.DeleteSource(src); }

        if (_sfxClickBuffer != 0) _al.DeleteBuffer(_sfxClickBuffer);
        if (_sfxHoverBuffer != 0) _al.DeleteBuffer(_sfxHoverBuffer);
        if (_sfxSelectBuffer != 0) _al.DeleteBuffer(_sfxSelectBuffer);

        unsafe
        {
            _alc.DestroyContext(_context);
            _alc.CloseDevice(_device);
        }

        _al.Dispose();
        _alc.Dispose();
    }
}

/// <summary>Types of sound effects available.</summary>
public enum SfxType
{
    Click,
    Hover,
    Select
}
