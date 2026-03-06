using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Procedural;
using CongCraft.Engine.Rendering;
using CongCraft.Engine.Terrain;
using Silk.NET.OpenGL;
using Shader = CongCraft.Engine.Rendering.Shader;

namespace CongCraft.Engine.Environment;

/// <summary>
/// Places trees and rocks on the terrain using noise-based distribution.
/// </summary>
public sealed class VegetationPlacer : ISystem
{
    public int Priority => 45; // After terrain system

    private GL _gl = null!;
    private World _world = null!;
    private Camera _camera = null!;
    private LightingData _lighting = null!;
    private TerrainGenerator _terrainGen = null!;
    private Shader _basicShader = null!;
    private Mesh _treeMesh = null!;
    private Mesh _rockMesh = null!;
    private Mesh _ruinPillarMesh = null!;
    private Mesh _ruinBrokenPillarMesh = null!;
    private Mesh _ruinWallMesh = null!;
    private Mesh _ruinArchMesh = null!;
    private bool _placed;

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _world = services.Get<World>();
        _camera = services.Get<Camera>();
        _lighting = services.Get<LightingData>();
        _terrainGen = services.Get<TerrainGenerator>();
        _basicShader = new Shader(_gl, ShaderSources.BasicVertex, ShaderSources.BasicFragment);
        _treeMesh = TreeMeshBuilder.Create(_gl);
        _rockMesh = RockMeshBuilder.Create(_gl);
        _ruinPillarMesh = RuinMeshBuilder.Create(_gl, RuinMeshBuilder.RuinType.Pillar, 11);
        _ruinBrokenPillarMesh = RuinMeshBuilder.Create(_gl, RuinMeshBuilder.RuinType.BrokenPillar, 22);
        _ruinWallMesh = RuinMeshBuilder.Create(_gl, RuinMeshBuilder.RuinType.WallSegment, 33);
        _ruinArchMesh = RuinMeshBuilder.Create(_gl, RuinMeshBuilder.RuinType.ArchFragment, 44);
    }

    public void Update(GameTime time)
    {
        if (_placed) return;
        _placed = true;

        // Use a separate noise for placement
        var placementNoise = new FastNoiseLite(9999);
        placementNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        placementNoise.SetFrequency(0.02f);

        var rng = new Random(42);
        float areaSize = 200f;
        float halfArea = areaSize / 2f;

        // Place trees
        for (int i = 0; i < 150; i++)
        {
            float x = (float)(rng.NextDouble() * areaSize - halfArea);
            float z = (float)(rng.NextDouble() * areaSize - halfArea);
            float height = _terrainGen.GetHeightAt(x, z);
            float noise = placementNoise.GetNoise(x, z);

            // Only place on suitable terrain (above water, not too steep/high)
            if (height > 2f && height < 15f && noise > 0.0f)
            {
                var entity = _world.CreateEntity();
                var transform = new TransformComponent
                {
                    Position = new Vector3(x, height, z),
                    Scale = Vector3.One * (0.8f + (float)rng.NextDouble() * 0.6f)
                };
                _world.AddComponent(entity, transform);
                _world.AddComponent(entity, new MeshRendererComponent
                {
                    Mesh = _treeMesh,
                    Shader = _basicShader
                });
                _world.AddComponent(entity, new VegetationTag());
            }
        }

        // Place rocks
        for (int i = 0; i < 80; i++)
        {
            float x = (float)(rng.NextDouble() * areaSize - halfArea);
            float z = (float)(rng.NextDouble() * areaSize - halfArea);
            float height = _terrainGen.GetHeightAt(x, z);

            if (height > 1.5f)
            {
                var entity = _world.CreateEntity();
                var transform = new TransformComponent
                {
                    Position = new Vector3(x, height, z),
                    Scale = Vector3.One * (0.5f + (float)rng.NextDouble() * 1f),
                    Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY,
                        (float)(rng.NextDouble() * MathF.Tau))
                };
                _world.AddComponent(entity, transform);
                _world.AddComponent(entity, new MeshRendererComponent
                {
                    Mesh = _rockMesh,
                    Shader = _basicShader
                });
                _world.AddComponent(entity, new VegetationTag());
            }
        }

        // Place ruins (~28 ruin pieces scattered across the world)
        var ruinMeshes = new[] { _ruinPillarMesh, _ruinBrokenPillarMesh, _ruinWallMesh, _ruinArchMesh };
        var ruinNoise = new FastNoiseLite(7777);
        ruinNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        ruinNoise.SetFrequency(0.015f);

        int ruinsPlaced = 0;
        int attempts = 0;
        while (ruinsPlaced < 28 && attempts < 500)
        {
            attempts++;
            float x = (float)(rng.NextDouble() * areaSize - halfArea);
            float z = (float)(rng.NextDouble() * areaSize - halfArea);
            float height = _terrainGen.GetHeightAt(x, z);

            // Place ruins on flat-ish terrain above water, away from steep slopes
            if (height < 2.5f || height > 12f) continue;
            float nv = ruinNoise.GetNoise(x, z);
            if (nv < -0.2f) continue; // Cluster ruins in noisier regions

            var mesh = ruinMeshes[rng.Next(ruinMeshes.Length)];
            var entity = _world.CreateEntity();
            var transform = new TransformComponent
            {
                Position = new Vector3(x, height, z),
                Scale = Vector3.One * (0.9f + (float)rng.NextDouble() * 0.5f),
                Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY,
                    (float)(rng.NextDouble() * MathF.Tau))
            };
            _world.AddComponent(entity, transform);
            _world.AddComponent(entity, new MeshRendererComponent
            {
                Mesh = mesh,
                Shader = _basicShader
            });
            _world.AddComponent(entity, new VegetationTag());
            ruinsPlaced++;
        }
    }

    public void Render(GameTime time)
    {
        _basicShader.Use();
        _basicShader.SetUniform("uView", _camera.ViewMatrix);
        _basicShader.SetUniform("uProjection", _camera.ProjectionMatrix);
        _basicShader.SetUniform("uCameraPos", _camera.Position);
        _lighting.ApplyToShader(_basicShader);

        foreach (var (entity, meshComp) in _world.Query<MeshRendererComponent>())
        {
            if (!_world.HasComponent<VegetationTag>(entity)) continue;
            var transform = _world.GetComponent<TransformComponent>(entity);
            _basicShader.SetUniform("uModel", transform.ModelMatrix);
            meshComp.Mesh.Draw();
        }
    }

    public void Dispose()
    {
        _basicShader.Dispose();
        _treeMesh.Dispose();
        _rockMesh.Dispose();
        _ruinPillarMesh.Dispose();
        _ruinBrokenPillarMesh.Dispose();
        _ruinWallMesh.Dispose();
        _ruinArchMesh.Dispose();
    }
}

public sealed class VegetationTag : IComponent { }
