using System.Numerics;
using System.Runtime.InteropServices;
using CongCraft.Engine.Combat;
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
    private readonly Dictionary<SfxType, uint> _sfxBuffers = new();
    private readonly uint[] _sfxSources = new uint[12];
    private int _nextSfxSource;

    // Ambient loop
    private uint _ambientWindSource, _ambientWindBuffer;

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
            if (!TryInitOpenAL())
            {
                DevLog.Warn("OpenAL: Could not initialize audio — audio disabled");
                return;
            }

            DevLog.Info("OpenAL device and context created");

            // Generate all music themes
            LoadTrack(ProceduralMusic.GenerateMenuTheme(40), out _menuBuffer, out _menuSource);
            LoadTrack(ProceduralMusic.GenerateExplorationTheme(45), out _explorationBuffer, out _explorationSource);
            LoadTrack(ProceduralMusic.GenerateCombatTheme(30), out _combatBuffer, out _combatSource);

            CheckAlError("after loading music tracks");

            // Generate SFX
            LoadSfxType(SfxType.Click, ProceduralMusic.GenerateClickSfx());
            LoadSfxType(SfxType.Hover, ProceduralMusic.GenerateHoverSfx());
            LoadSfxType(SfxType.Select, ProceduralMusic.GenerateSelectSfx());
            LoadSfxType(SfxType.SwordSwing, ProceduralMusic.GenerateSwordSwingSfx());
            LoadSfxType(SfxType.SwordHit, ProceduralMusic.GenerateSwordHitSfx());
            LoadSfxType(SfxType.FootstepGrass, ProceduralMusic.GenerateFootstepGrassSfx());
            LoadSfxType(SfxType.FootstepStone, ProceduralMusic.GenerateFootstepStoneSfx());
            LoadSfxType(SfxType.EnemyHit, ProceduralMusic.GenerateEnemyHitSfx());
            LoadSfxType(SfxType.EnemyDeath, ProceduralMusic.GenerateEnemyDeathSfx());
            LoadSfxType(SfxType.PlayerHurt, ProceduralMusic.GeneratePlayerHurtSfx());
            LoadSfxType(SfxType.ItemPickup, ProceduralMusic.GenerateItemPickupSfx());
            LoadSfxType(SfxType.SpellCast, ProceduralMusic.GenerateSpellCastSfx());
            LoadSfxType(SfxType.DodgeWhoosh, ProceduralMusic.GenerateDodgeWhooshSfx());

            // Ambient wind loop
            LoadTrack(ProceduralMusic.GenerateAmbientWind(20), out _ambientWindBuffer, out _ambientWindSource);

            CheckAlError("after loading SFX");

            // Create SFX source pool
            for (int i = 0; i < _sfxSources.Length; i++)
                _sfxSources[i] = _al!.GenSource();

            DevLog.Info("Audio tracks and SFX loaded");

            // Start with menu music
            _al!.SetSourceProperty(_menuSource, SourceFloat.Gain, MusicVolume);
            _al.SourcePlay(_menuSource);
            _currentPlaying = GameMode.MainMenu;

            CheckAlError("after starting menu music");

            _initialized = true;
            DevLog.Info("AudioSystem initialized successfully");
        }
        catch (Exception ex)
        {
            DevLog.Warn($"AudioSystem init failed: {ex.Message} — audio disabled");
            _initialized = false;
        }
    }

    /// <summary>
    /// Try multiple approaches to open OpenAL device — macOS M1 may need
    /// specific device enumeration rather than null (default) device.
    /// </summary>
    private unsafe bool TryInitOpenAL()
    {
        _alc = ALContext.GetApi();
        _al = AL.GetApi();
        DevLog.Info($"OpenAL API acquired (OS: {RuntimeInformation.RuntimeIdentifier}, Arch: {RuntimeInformation.OSArchitecture})");

        // Try default device first
        _device = _alc.OpenDevice(null);

        // On macOS, null device can fail — try the default device specifier
        if (_device == null)
        {
            DevLog.Warn("OpenAL: Default device (null) failed, trying named device...");
            try
            {
                // Query the default device name and try opening it explicitly
                var defaultName = _alc.GetContextProperty(null, GetContextString.DeviceSpecifier);
                if (!string.IsNullOrEmpty(defaultName))
                {
                    DevLog.Info($"OpenAL: Trying device '{defaultName}'");
                    _device = _alc.OpenDevice(defaultName);
                }
            }
            catch (Exception ex)
            {
                DevLog.Warn($"OpenAL: Named device query failed: {ex.Message}");
            }
        }

        if (_device == null)
        {
            DevLog.Warn("OpenAL: No audio device available — audio disabled");
            return false;
        }

        var deviceName = _alc.GetContextProperty(_device, GetContextString.DeviceSpecifier);
        DevLog.Info($"OpenAL: Opened device '{deviceName}'");

        _context = _alc.CreateContext(_device, null);
        if (_context == null)
        {
            DevLog.Warn("OpenAL: Failed to create audio context — audio disabled");
            _alc.CloseDevice(_device);
            _device = null;
            return false;
        }

        if (!_alc.MakeContextCurrent(_context))
        {
            DevLog.Warn("OpenAL: Failed to make context current — audio disabled");
            _alc.DestroyContext(_context);
            _alc.CloseDevice(_device);
            _device = null;
            _context = null;
            return false;
        }

        // Log OpenAL info for debugging
        var vendor = _al.GetStateProperty(StateString.Vendor);
        var renderer = _al.GetStateProperty(StateString.Renderer);
        var version = _al.GetStateProperty(StateString.Version);
        DevLog.Info($"OpenAL: {vendor} / {renderer} / {version}");

        return true;
    }

    private void CheckAlError(string context)
    {
        if (_al == null) return;
        var err = _al.GetError();
        if (err != AudioError.NoError)
            DevLog.Warn($"OpenAL error {context}: {err}");
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

    private void LoadSfxType(SfxType type, short[] data)
    {
        LoadSfxBuffer(data, out uint buffer);
        _sfxBuffers[type] = buffer;
    }

    /// <summary>Play a one-shot SFX using a round-robin source pool.</summary>
    public void PlaySfx(SfxType type)
    {
        if (!_initialized || _al == null) return;

        if (!_sfxBuffers.TryGetValue(type, out uint buffer) || buffer == 0) return;

        uint src = _sfxSources[_nextSfxSource];
        _nextSfxSource = (_nextSfxSource + 1) % _sfxSources.Length;

        _al.SourceStop(src);
        _al.SetSourceProperty(src, SourceInteger.Buffer, (int)buffer);
        _al.SetSourceProperty(src, SourceBoolean.Looping, false);
        _al.SetSourceProperty(src, SourceFloat.Gain, SfxVolume);
        _al.SourcePlay(src);
    }

    private bool _ambientPlaying;

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

            // Switch to combat music when enemies are nearby
            if (targetMode == GameMode.Playing && IsPlayerInCombat())
                targetMode = GameMode.Combat;
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

            // Start ambient wind when playing
            bool shouldAmbient = gs2.CurrentMode == GameMode.Playing;
            if (shouldAmbient && !_ambientPlaying)
            {
                _al.SetSourceProperty(_ambientWindSource, SourceFloat.Gain, 0.18f);
                _al.SourcePlay(_ambientWindSource);
                _ambientPlaying = true;
            }
            else if (!shouldAmbient && _ambientPlaying)
            {
                _al.SetSourceProperty(_ambientWindSource, SourceFloat.Gain, 0f);
                _al.SourceStop(_ambientWindSource);
                _ambientPlaying = false;
            }
        }
    }

    private uint GetSourceForMode(GameMode mode) => mode switch
    {
        GameMode.MainMenu => _menuSource,
        GameMode.Playing => _explorationSource,
        GameMode.Combat => _combatSource,
        _ => _explorationSource
    };

    private const float CombatDetectionRange = 25f;

    private bool IsPlayerInCombat()
    {
        if (_world == null) return false;

        Vector3 playerPos = default;
        bool foundPlayer = false;

        foreach (var (entity, _, transform) in _world.Query<PlayerComponent, TransformComponent>())
        {
            playerPos = transform.Position;
            foundPlayer = true;
            break;
        }

        if (!foundPlayer) return false;

        foreach (var (entity, enemy, health, transform) in QueryAliveEnemies())
        {
            float dist = Vector3.Distance(playerPos, transform.Position);
            if (dist < CombatDetectionRange)
                return true;
        }

        return false;
    }

    private IEnumerable<(Entity, EnemyComponent, HealthComponent, TransformComponent)> QueryAliveEnemies()
    {
        foreach (var (entity, enemy, health) in _world!.Query<EnemyComponent, HealthComponent>())
        {
            if (!health.IsAlive) continue;
            if (_world.HasComponent<TransformComponent>(entity))
                yield return (entity, enemy, health, _world.GetComponent<TransformComponent>(entity));
        }
    }

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
        Cleanup(_ambientWindSource, _ambientWindBuffer);

        foreach (var src in _sfxSources)
            if (src != 0) { _al.SourceStop(src); _al.DeleteSource(src); }

        foreach (var buf in _sfxBuffers.Values)
            if (buf != 0) _al.DeleteBuffer(buf);

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
    Select,
    SwordSwing,
    SwordHit,
    FootstepGrass,
    FootstepStone,
    EnemyHit,
    EnemyDeath,
    PlayerHurt,
    ItemPickup,
    SpellCast,
    DodgeWhoosh
}
