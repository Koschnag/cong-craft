using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Procedural;
using CongCraft.Engine.Rendering;
using CongCraft.Engine.Terrain;
using Silk.NET.OpenGL;
using Shader = CongCraft.Engine.Rendering.Shader;

namespace CongCraft.Engine.VFX;

/// <summary>
/// Places torches in the world near NPCs and at set locations.
/// Manages torch fire particles and point light flickering.
/// </summary>
public sealed class TorchSystem : ISystem
{
    public int Priority => 48; // After NPC spawner

    private GL _gl = null!;
    private World _world = null!;
    private TerrainGenerator _terrainGen = null!;
    private PointLightData _pointLightData = null!;
    private Mesh _torchMesh = null!;
    private Shader _basicShader = null!;
    private MaterialTextures? _materialTextures;
    private bool _spawned;

    private readonly List<ParticleEmitter> _fireEmitters = new();
    private readonly List<ParticleEmitter> _emberEmitters = new();
    private readonly Random _rng = new(77777);
    private float _flickerTimer;

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _world = services.Get<World>();
        _terrainGen = services.Get<TerrainGenerator>();
        _torchMesh = CreateTorchMesh();
        _basicShader = new Shader(_gl, ShaderSources.BasicVertex, ShaderSources.BasicFragment);
        _materialTextures = services.Get<MaterialTextures>();

        _pointLightData = new PointLightData();
        services.Register(_pointLightData);
    }

    private Mesh CreateTorchMesh()
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        // Post (dark brown)
        AddBox(verts, inds, 0, 0.5f, 0, 0.03f, 0.5f, 0.03f, 0.35f, 0.2f, 0.1f);
        // Top (orange/red ember holder)
        AddBox(verts, inds, 0, 1.05f, 0, 0.05f, 0.06f, 0.05f, 0.6f, 0.3f, 0.1f);

        return new Mesh(_gl, verts.ToArray(), inds.ToArray(), VertexLayout.PositionNormalColor);
    }

    public void Update(GameTime time)
    {
        if (!_spawned)
        {
            _spawned = true;
            SpawnTorches();
        }

        float dt = time.DeltaTimeF;
        _flickerTimer += dt;

        // Emit fire particles from each torch
        foreach (var emitter in _fireEmitters)
            emitter.Emit(1);
        foreach (var emitter in _emberEmitters)
        {
            if (_rng.NextDouble() < 0.3)
                emitter.Emit(1);
        }

        // Update point light data from torch entities
        UpdatePointLights();
    }

    private void SpawnTorches()
    {
        // Torch positions: near NPCs and along paths
        var positions = new (float x, float z)[]
        {
            (6f, 12f), (10f, 12f),   // Near blacksmith
            (-3f, 15f), (-7f, 15f),  // Near elder
            (10f, 3f), (14f, 5f),    // Near merchant
            (0f, 5f), (4f, 0f),      // Village center
        };

        foreach (var (x, z) in positions)
        {
            float height = _terrainGen.GetHeightAt(x, z);
            if (height < 2f) height = 2f;
            var pos = new Vector3(x, height, z);

            var entity = _world.CreateEntity();
            _world.AddComponent(entity, new TransformComponent
            {
                Position = pos,
                Scale = Vector3.One
            });
            _world.AddComponent(entity, new PointLightComponent
            {
                Color = new Vector3(1f, 0.6f, 0.2f),
                Intensity = 1.5f,
                Radius = 8f,
                FlickerSpeed = 4f + (float)_rng.NextDouble() * 3f,
                FlickerAmount = 0.15f + (float)_rng.NextDouble() * 0.1f
            });
            _world.AddComponent(entity, new MeshRendererComponent
            {
                Mesh = _torchMesh,
                Shader = _basicShader
            });

            // Fire emitter at torch top
            var fireConfig = VfxPresets.TorchFire;
            fireConfig.Position = pos + new Vector3(0, 1.1f, 0);
            var fireEmitter = new ParticleEmitter(fireConfig, _rng.Next());
            _fireEmitters.Add(fireEmitter);
            ParticleRenderSystem.RegisterEmitter(fireEmitter);

            // Ember emitter
            var emberConfig = VfxPresets.TorchEmbers;
            emberConfig.Position = pos + new Vector3(0, 1.2f, 0);
            var emberEmitter = new ParticleEmitter(emberConfig, _rng.Next());
            _emberEmitters.Add(emberEmitter);
            ParticleRenderSystem.RegisterEmitter(emberEmitter);
        }
    }

    private void UpdatePointLights()
    {
        // Find player position for distance sorting
        Vector3 playerPos = Vector3.Zero;
        foreach (var (entity, player, transform) in _world.Query<PlayerComponent, TransformComponent>())
        {
            playerPos = transform.Position;
            break;
        }

        // Collect all point lights, sorted by distance to player
        var lights = new List<(Vector3 pos, PointLightComponent light, float dist)>();
        foreach (var (entity, light, transform) in _world.Query<PointLightComponent, TransformComponent>())
        {
            float dist = Vector3.Distance(transform.Position, playerPos);
            lights.Add((transform.Position, light, dist));
        }
        lights.Sort((a, b) => a.dist.CompareTo(b.dist));

        // Upload closest 4 to point light data
        _pointLightData.Count = Math.Min(lights.Count, PointLightData.MaxLights);
        for (int i = 0; i < _pointLightData.Count; i++)
        {
            var (pos, light, _) = lights[i];
            float flicker = 1f + MathF.Sin(_flickerTimer * light.FlickerSpeed) * light.FlickerAmount;
            _pointLightData.Positions[i] = pos + new Vector3(0, 1.1f, 0);
            _pointLightData.Colors[i] = light.Color;
            _pointLightData.Intensities[i] = light.Intensity * flicker;
            _pointLightData.Radii[i] = light.Radius;
        }
    }

    private static void AddBox(List<float> verts, List<uint> inds,
        float cx, float cy, float cz, float hw, float hh, float hd,
        float r, float g, float b)
    {
        float[][] corners =
        {
            new[] { cx-hw, cy-hh, cz-hd }, new[] { cx+hw, cy-hh, cz-hd },
            new[] { cx+hw, cy+hh, cz-hd }, new[] { cx-hw, cy+hh, cz-hd },
            new[] { cx-hw, cy-hh, cz+hd }, new[] { cx+hw, cy-hh, cz+hd },
            new[] { cx+hw, cy+hh, cz+hd }, new[] { cx-hw, cy+hh, cz+hd },
        };

        (int[] face, float[] normal)[] faces =
        {
            (new[]{0,1,2,3}, new[]{0f,0f,-1f}), (new[]{5,4,7,6}, new[]{0f,0f,1f}),
            (new[]{4,0,3,7}, new[]{-1f,0f,0f}), (new[]{1,5,6,2}, new[]{1f,0f,0f}),
            (new[]{3,2,6,7}, new[]{0f,1f,0f}), (new[]{4,5,1,0}, new[]{0f,-1f,0f}),
        };

        foreach (var (face, normal) in faces)
        {
            uint fi = (uint)(verts.Count / 9);
            foreach (int idx in face)
            {
                verts.AddRange(corners[idx]);
                verts.AddRange(normal);
                verts.AddRange(new[] { r, g, b });
            }
            inds.AddRange(new[] { fi, fi+1, fi+2, fi, fi+2, fi+3 });
        }
    }

    public void Render(GameTime time) { }

    public void Dispose()
    {
        foreach (var emitter in _fireEmitters)
            ParticleRenderSystem.UnregisterEmitter(emitter);
        foreach (var emitter in _emberEmitters)
            ParticleRenderSystem.UnregisterEmitter(emitter);
        _torchMesh.Dispose();
        _basicShader.Dispose();
    }
}
