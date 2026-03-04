using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Input;
using CongCraft.Engine.Rendering;
using CongCraft.Engine.VFX;
using Silk.NET.Input;

namespace CongCraft.Engine.Magic;

/// <summary>
/// Handles spell casting, mana regeneration, cooldowns, and spell effects.
/// Spells: F1=Fireball, F2=Heal, F3=Shield, F4=Ice Nova.
/// </summary>
public sealed class MagicSystem : ISystem
{
    public int Priority => 16; // After combat, before enemy AI

    private World _world = null!;
    private Camera _camera = null!;
    private SpellData[] _spellBar = null!;
    private ParticleEmitter _spellEmitter = null!;

    public void Initialize(ServiceLocator services)
    {
        _world = services.Get<World>();
        _camera = services.Get<Camera>();
        _spellBar = SpellDatabase.GetSpellBar();

        var vfxConfig = new ParticleEmitterConfig
        {
            MaxParticles = 40,
            MinLife = 0.4f,
            MaxLife = 1.0f,
            MinSpeed = 2f,
            MaxSpeed = 5f,
            MinSize = 0.08f,
            MaxSize = 0.25f,
            Gravity = new Vector3(0, -1f, 0),
            SpreadAngle = MathF.PI
        };
        _spellEmitter = new ParticleEmitter(vfxConfig, 777);
        ParticleRenderSystem.RegisterEmitter(_spellEmitter);
    }

    public void Update(GameTime time)
    {
        var input = _world.GetSingleton<InputState>();
        float dt = time.DeltaTimeF;

        foreach (var (entity, player, mana) in _world.Query<PlayerComponent, ManaComponent>())
        {
            if (!_world.HasComponent<SpellState>(entity)) continue;
            if (!_world.HasComponent<TransformComponent>(entity)) continue;

            var spellState = _world.GetComponent<SpellState>(entity);
            var transform = _world.GetComponent<TransformComponent>(entity);

            // Mana regeneration
            mana.Regenerate(dt);

            // Update cooldowns
            for (int i = 0; i < spellState.Cooldowns.Length; i++)
                spellState.Cooldowns[i] = MathF.Max(0, spellState.Cooldowns[i] - dt);

            // Update shield timer
            if (spellState.ShieldTimer > 0)
                spellState.ShieldTimer -= dt;

            // Spell selection and casting
            if (input.IsKeyPressed(Key.F1)) TryCastSpell(0, entity, transform, mana, spellState);
            if (input.IsKeyPressed(Key.F2)) TryCastSpell(1, entity, transform, mana, spellState);
            if (input.IsKeyPressed(Key.F3)) TryCastSpell(2, entity, transform, mana, spellState);
            if (input.IsKeyPressed(Key.F4)) TryCastSpell(3, entity, transform, mana, spellState);

            // Apply shield damage reduction to combat
            if (_world.HasComponent<CombatComponent>(entity))
            {
                var combat = _world.GetComponent<CombatComponent>(entity);
                combat.MagicShieldActive = spellState.HasActiveShield;
            }
        }
    }

    private void TryCastSpell(int index, Entity caster, TransformComponent transform,
        ManaComponent mana, SpellState state)
    {
        if (index < 0 || index >= _spellBar.Length) return;
        var spell = _spellBar[index];

        // Check cooldown
        if (state.Cooldowns[index] > 0) return;

        // Check mana
        if (!mana.TrySpend(spell.ManaCost)) return;

        // Start cooldown
        state.Cooldowns[index] = spell.Cooldown;
        state.SelectedSpell = index;

        // Apply spell effect
        switch (spell.Type)
        {
            case SpellType.Projectile:
                CastProjectile(transform.Position, spell);
                break;
            case SpellType.SelfHeal:
                CastHeal(caster, spell);
                break;
            case SpellType.Shield:
                CastShield(state, spell);
                break;
            case SpellType.AreaEffect:
                CastAreaEffect(transform.Position, spell);
                break;
        }

        // Spawn VFX
        SpawnSpellVfx(transform.Position, spell);
    }

    private void CastProjectile(Vector3 origin, SpellData spell)
    {
        var direction = _camera.Forward;
        var targetPos = origin + direction * spell.Range;

        // Damage enemies in path (simplified: check range from target point)
        foreach (var (entity, enemy, health, transform) in QueryEnemies())
        {
            if (!health.IsAlive) continue;
            float dist = Vector3.Distance(transform.Position, targetPos);
            if (dist > 3f) continue; // Explosion radius

            float distFromOrigin = Vector3.Distance(transform.Position, origin);
            if (distFromOrigin > spell.Range) continue;

            health.TakeDamage(spell.Damage);
            if (!health.IsAlive)
            {
                enemy.State = EnemyState.Dead;
                enemy.IsDead = true;
                enemy.DeathTimer = 3f;
            }
        }
    }

    private void CastHeal(Entity caster, SpellData spell)
    {
        if (!_world.HasComponent<HealthComponent>(caster)) return;
        var health = _world.GetComponent<HealthComponent>(caster);
        health.Heal(spell.HealAmount);
    }

    private void CastShield(SpellState state, SpellData spell)
    {
        state.ShieldTimer = spell.Duration;
    }

    private void CastAreaEffect(Vector3 origin, SpellData spell)
    {
        foreach (var (entity, enemy, health, transform) in QueryEnemies())
        {
            if (!health.IsAlive) continue;
            float dist = Vector3.Distance(transform.Position, origin);
            if (dist > spell.Range) continue;

            health.TakeDamage(spell.Damage);
            if (!health.IsAlive)
            {
                enemy.State = EnemyState.Dead;
                enemy.IsDead = true;
                enemy.DeathTimer = 3f;
            }
        }
    }

    private void SpawnSpellVfx(Vector3 position, SpellData spell)
    {
        // Temporarily set colors for this spell's particles
        _spellEmitter.Config.StartColor = new Vector4(spell.VfxR, spell.VfxG, spell.VfxB, 1f);
        _spellEmitter.Config.EndColor = new Vector4(spell.VfxR * 0.5f, spell.VfxG * 0.5f, spell.VfxB * 0.5f, 0f);
        _spellEmitter.Emit(15, position + Vector3.UnitY * 1.2f);
    }

    public ParticleEmitter SpellEmitter => _spellEmitter;

    private IEnumerable<(Entity, EnemyComponent, HealthComponent, TransformComponent)> QueryEnemies()
    {
        foreach (var (entity, enemy, health) in _world.Query<EnemyComponent, HealthComponent>())
        {
            if (_world.HasComponent<TransformComponent>(entity))
                yield return (entity, enemy, health, _world.GetComponent<TransformComponent>(entity));
        }
    }

    public void Render(GameTime time) { }
    public void Dispose() { }
}
