using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Procedural;
using CongCraft.Engine.Rendering;
using CongCraft.Engine.Terrain;
using CongCraft.Engine.VFX;
using Silk.NET.OpenGL;
using Shader = CongCraft.Engine.Rendering.Shader;

namespace CongCraft.Engine.Boss;

/// <summary>
/// Manages boss AI: activation, phase transitions, and attack patterns.
/// Spawns boss entities and renders them with larger models.
/// </summary>
public sealed class BossAISystem : ISystem
{
    public int Priority => 21; // After enemy AI

    private World _world = null!;
    private GL _gl = null!;
    private TerrainGenerator _terrainGen = null!;
    private Mesh _bossMesh = null!;
    private Shader _basicShader = null!;
    private MaterialTextures? _materialTextures;
    private ParticleEmitter _bossVfx = null!;
    private bool _spawned;
    private Random _rng = new(888);

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _world = services.Get<World>();
        _terrainGen = services.Get<TerrainGenerator>();
        _basicShader = new Shader(_gl, ShaderSources.BasicVertex, ShaderSources.BasicFragment);
        _materialTextures = services.Get<MaterialTextures>();

        // Boss mesh: larger humanoid from boxes
        _bossMesh = PrimitiveMeshBuilder.CreateCube(_gl, 1f, 0.4f, 0.2f, 0.3f);

        // VFX for boss attacks
        var vfxConfig = new ParticleEmitterConfig
        {
            MaxParticles = 50,
            MinLife = 0.3f,
            MaxLife = 0.8f,
            MinSpeed = 3f,
            MaxSpeed = 8f,
            MinSize = 0.1f,
            MaxSize = 0.3f,
            Gravity = new Vector3(0, -5f, 0),
            SpreadAngle = MathF.PI
        };
        _bossVfx = new ParticleEmitter(vfxConfig, 888);
        ParticleRenderSystem.RegisterEmitter(_bossVfx);
    }

    public void Update(GameTime time)
    {
        if (!_spawned)
        {
            _spawned = true;
            SpawnBosses();
        }

        float dt = time.DeltaTimeF;
        Vector3 playerPos = GetPlayerPosition();

        foreach (var (entity, boss, health, transform) in QueryBosses())
        {
            if (!health.IsAlive)
            {
                boss.State = BossState.Dead;
                continue;
            }

            var bossData = BossDatabase.Get(boss.BossId);
            if (bossData == null) continue;

            // Activation check
            if (!boss.IsActivated)
            {
                float dist = Vector3.Distance(playerPos, transform.Position);
                if (dist < boss.ActivationRadius)
                {
                    boss.IsActivated = true;
                    boss.State = BossState.Activated;
                    // Emit activation VFX
                    _bossVfx.Config.StartColor = new Vector4(bossData.ColorR, bossData.ColorG, bossData.ColorB, 1f);
                    _bossVfx.Config.EndColor = new Vector4(1f, 0.5f, 0f, 0f);
                    _bossVfx.Emit(20, transform.Position + Vector3.UnitY * 2f);
                }
                continue;
            }

            // Phase transition check
            CheckPhaseTransition(boss, health, bossData);

            // Update timers
            boss.AttackTimer = MathF.Max(0, boss.AttackTimer - dt);
            boss.SpecialAttackTimer = MathF.Max(0, boss.SpecialAttackTimer - dt);

            // Process current attack
            if (boss.State == BossState.Attacking || boss.State == BossState.SpecialAttack)
            {
                boss.AttackProgress += dt * 2f;
                if (boss.AttackProgress >= 1f)
                {
                    ExecuteAttack(boss, bossData, transform, playerPos);
                    boss.State = BossState.Activated;
                    boss.AttackProgress = 0;
                    boss.CurrentAttack = BossAttackType.None;
                }
                continue;
            }

            // Choose next action
            float distToPlayer = Vector3.Distance(playerPos, transform.Position);

            if (boss.SpecialAttackTimer <= 0 && _rng.NextDouble() < 0.3)
            {
                // Special attack
                boss.CurrentAttack = ChooseSpecialAttack(boss);
                boss.State = BossState.SpecialAttack;
                boss.AttackProgress = 0;
                boss.SpecialAttackTimer = bossData.SpecialCooldown;
            }
            else if (distToPlayer < bossData.MeleeRange && boss.AttackTimer <= 0)
            {
                // Melee attack
                boss.CurrentAttack = BossAttackType.Melee;
                boss.State = BossState.Attacking;
                boss.AttackProgress = 0;
                boss.AttackTimer = bossData.MeleeCooldown;
            }
            else
            {
                // Move toward player
                var dir = Vector3.Normalize(playerPos - transform.Position);
                float speed = boss.State == BossState.Enraged ? 5f : 3f;
                var newPos = transform.Position + dir * speed * dt;
                newPos.Y = _terrainGen.GetHeightAt(newPos.X, newPos.Z) + bossData.Scale * 0.5f;
                transform.Position = newPos;

                // Face player
                float yaw = MathF.Atan2(dir.X, dir.Z);
                transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw);
            }
        }
    }

    private BossAttackType ChooseSpecialAttack(BossComponent boss)
    {
        // More dangerous attacks in later phases
        if (boss.CurrentPhase >= 2)
            return _rng.NextDouble() < 0.5 ? BossAttackType.Slam : BossAttackType.Charge;
        return BossAttackType.Slam;
    }

    private void CheckPhaseTransition(BossComponent boss, HealthComponent health, BossData data)
    {
        float healthPct = health.Current / health.Max;
        int newPhase = 0;
        for (int i = 0; i < data.PhaseThresholds.Length; i++)
        {
            if (healthPct <= data.PhaseThresholds[i])
                newPhase = i + 1;
        }

        if (newPhase > boss.CurrentPhase)
        {
            boss.CurrentPhase = newPhase;
            boss.State = BossState.Enraged;

            // Phase transition VFX burst
            _bossVfx.Config.StartColor = new Vector4(1f, 0.3f, 0.1f, 1f);
            _bossVfx.Config.EndColor = new Vector4(1f, 0f, 0f, 0f);
        }
    }

    private void ExecuteAttack(BossComponent boss, BossData data, TransformComponent transform, Vector3 playerPos)
    {
        float dist = Vector3.Distance(playerPos, transform.Position);

        switch (boss.CurrentAttack)
        {
            case BossAttackType.Melee:
                if (dist < data.MeleeRange)
                    DamagePlayer(data.MeleeDamage * (1f + boss.CurrentPhase * 0.2f));
                break;

            case BossAttackType.Slam:
                if (dist < data.SlamRadius)
                    DamagePlayer(data.SlamDamage * (1f + boss.CurrentPhase * 0.15f));
                // VFX
                _bossVfx.Config.StartColor = new Vector4(0.5f, 0.3f, 0.1f, 1f);
                _bossVfx.Config.EndColor = new Vector4(0.3f, 0.15f, 0.05f, 0f);
                _bossVfx.Emit(25, transform.Position);
                break;

            case BossAttackType.Charge:
                // Move boss toward player rapidly
                var dir = Vector3.Normalize(playerPos - transform.Position);
                transform.Position += dir * data.ChargeSpeed * 0.5f;
                transform.Position = new Vector3(transform.Position.X,
                    _terrainGen.GetHeightAt(transform.Position.X, transform.Position.Z) + data.Scale * 0.5f,
                    transform.Position.Z);
                if (dist < data.MeleeRange * 1.5f)
                    DamagePlayer(data.ChargeDamage);
                break;
        }
    }

    private void DamagePlayer(float damage)
    {
        foreach (var (entity, player, health) in _world.Query<PlayerComponent, HealthComponent>())
        {
            // Check for block/dodge
            if (_world.HasComponent<CombatComponent>(entity))
            {
                var combat = _world.GetComponent<CombatComponent>(entity);
                if (combat.IsDodging) return;
                if (combat.IsBlocking) damage *= (1f - combat.BlockDamageReduction);
                if (combat.MagicShieldActive) damage *= (1f - combat.MagicShieldReduction);
            }
            if (damage > 0) health.TakeDamage(damage);
        }
    }

    private void SpawnBosses()
    {
        foreach (var bossData in BossDatabase.All.Values)
        {
            float height = _terrainGen.GetHeightAt(bossData.SpawnPosition.X, bossData.SpawnPosition.Z);
            if (height < 2f) height = 2f;

            var entity = _world.CreateEntity();
            _world.AddComponent(entity, new TransformComponent
            {
                Position = new Vector3(bossData.SpawnPosition.X, height + bossData.Scale * 0.5f, bossData.SpawnPosition.Z),
                Scale = Vector3.One * bossData.Scale
            });
            _world.AddComponent(entity, new BossComponent
            {
                BossId = bossData.Id,
                Name = bossData.Name,
                MaxPhases = bossData.MaxPhases,
                PhaseThresholds = bossData.PhaseThresholds
            });
            _world.AddComponent(entity, new HealthComponent
            {
                Current = bossData.Health,
                Max = bossData.Health
            });
            _world.AddComponent(entity, new MeshRendererComponent
            {
                Mesh = _bossMesh,
                Shader = _basicShader
            });
        }
    }

    private Vector3 GetPlayerPosition()
    {
        foreach (var (entity, player, transform) in _world.Query<PlayerComponent, TransformComponent>())
            return transform.Position;
        return Vector3.Zero;
    }

    private IEnumerable<(Entity, BossComponent, HealthComponent, TransformComponent)> QueryBosses()
    {
        foreach (var (entity, boss, health) in _world.Query<BossComponent, HealthComponent>())
        {
            if (_world.HasComponent<TransformComponent>(entity))
                yield return (entity, boss, health, _world.GetComponent<TransformComponent>(entity));
        }
    }

    public void Render(GameTime time) { }

    public void Dispose()
    {
        _bossMesh.Dispose();
        _basicShader.Dispose();
    }
}
