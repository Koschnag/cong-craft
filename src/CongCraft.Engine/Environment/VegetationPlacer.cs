using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Level;
using CongCraft.Engine.Procedural;
using CongCraft.Engine.Rendering;
using CongCraft.Engine.Terrain;
using Silk.NET.OpenGL;
using Shader = CongCraft.Engine.Rendering.Shader;

namespace CongCraft.Engine.Environment;

/// <summary>
/// Places trees, rocks, bushes, and ruins at fixed positions from the level data.
/// No random placement — every object is intentionally positioned.
/// </summary>
public sealed class VegetationPlacer : ISystem
{
    public int Priority => 45; // After terrain system

    private GL _gl = null!;
    private World _world = null!;
    private Camera _camera = null!;
    private LightingData _lighting = null!;
    private LevelTerrainGenerator _levelGen = null!;
    private LevelData? _levelData;
    private Shader _basicShader = null!;
    private ShadowMap? _shadowMap;
    private MaterialTextures? _materialTextures;
    private Mesh _treeMesh = null!;
    private Mesh _rockMesh = null!;
    private Mesh _ruinPillarMesh = null!;
    private Mesh _ruinBrokenPillarMesh = null!;
    private Mesh _ruinWallMesh = null!;
    private Mesh _ruinArchMesh = null!;
    private Mesh _scrubMesh = null!;
    private Mesh _berryBushMesh = null!;
    private Mesh _grassTuftMesh = null!;
    private bool _placed;

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _world = services.Get<World>();
        _camera = services.Get<Camera>();
        _lighting = services.Get<LightingData>();
        _levelGen = services.Get<LevelTerrainGenerator>();
        services.TryGet(out _levelData);
        _basicShader = new Shader(_gl, ShaderSources.BasicVertex, ShaderSources.BasicFragment);
        _shadowMap = services.Get<ShadowMap>();
        _materialTextures = services.Get<MaterialTextures>();
        _treeMesh = TreeMeshBuilder.Create(_gl);
        _rockMesh = RockMeshBuilder.Create(_gl);
        _ruinPillarMesh = RuinMeshBuilder.Create(_gl, RuinMeshBuilder.RuinType.Pillar, 11);
        _ruinBrokenPillarMesh = RuinMeshBuilder.Create(_gl, RuinMeshBuilder.RuinType.BrokenPillar, 22);
        _ruinWallMesh = RuinMeshBuilder.Create(_gl, RuinMeshBuilder.RuinType.WallSegment, 33);
        _ruinArchMesh = RuinMeshBuilder.Create(_gl, RuinMeshBuilder.RuinType.ArchFragment, 44);

        var scrubData = BushMeshBuilder.GenerateScrub(101);
        _scrubMesh = new Mesh(_gl, scrubData.Vertices, scrubData.Indices, VertexLayout.PositionNormalColor);
        var berryData = BushMeshBuilder.GenerateBerry(202);
        _berryBushMesh = new Mesh(_gl, berryData.Vertices, berryData.Indices, VertexLayout.PositionNormalColor);
        var grassData = BushMeshBuilder.GenerateGrassTuft(303);
        _grassTuftMesh = new Mesh(_gl, grassData.Vertices, grassData.Indices, VertexLayout.PositionNormalColor);
    }

    public void Update(GameTime time)
    {
        if (_placed) return;
        _placed = true;

        if (_levelData != null)
            PlaceFromLevelData();
        else
            PlaceFallback();
    }

    private void PlaceFromLevelData()
    {
        foreach (var tree in _levelData!.Trees)
        {
            float height = _levelGen.GetHeightAt(tree.X, tree.Z);
            if (height < _levelData.WaterLevel) continue;
            PlaceObject(tree.X, height, tree.Z, tree.Scale, tree.RotationY, _treeMesh);
        }

        foreach (var rock in _levelData.Rocks)
        {
            float height = _levelGen.GetHeightAt(rock.X, rock.Z);
            if (height < _levelData.WaterLevel - 0.5f) continue;
            PlaceObject(rock.X, height, rock.Z, rock.Scale, rock.RotationY, _rockMesh);
        }

        foreach (var bush in _levelData.Bushes)
        {
            float height = _levelGen.GetHeightAt(bush.X, bush.Z);
            if (height < _levelData.WaterLevel) continue;
            var mesh = bush.Variant switch
            {
                "berry" => _berryBushMesh,
                "grass" => _grassTuftMesh,
                _ => _scrubMesh
            };
            PlaceObject(bush.X, height, bush.Z, bush.Scale, bush.RotationY, mesh);
        }

        foreach (var ruin in _levelData.Ruins)
        {
            float height = _levelGen.GetHeightAt(ruin.X, ruin.Z);
            if (height < _levelData.WaterLevel) continue;
            var mesh = ruin.Type switch
            {
                "pillar" => _ruinPillarMesh,
                "broken_pillar" => _ruinBrokenPillarMesh,
                "wall" => _ruinWallMesh,
                "arch" => _ruinArchMesh,
                _ => _ruinPillarMesh
            };
            PlaceObject(ruin.X, height, ruin.Z, ruin.Scale, ruin.RotationY, mesh);
        }
    }

    private void PlaceObject(float x, float height, float z, float scale, float rotY, Mesh mesh)
    {
        var entity = _world.CreateEntity();
        _world.AddComponent(entity, new TransformComponent
        {
            Position = new Vector3(x, height, z),
            Scale = Vector3.One * scale,
            Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, rotY)
        });
        _world.AddComponent(entity, new MeshRendererComponent
        {
            Mesh = mesh,
            Shader = _basicShader
        });
        _world.AddComponent(entity, new VegetationTag());
    }

    private void PlaceFallback()
    {
        var rng = new Random(42);
        float areaSize = 200f, halfArea = areaSize / 2f;

        for (int i = 0; i < 150; i++)
        {
            float x = (float)(rng.NextDouble() * areaSize - halfArea);
            float z = (float)(rng.NextDouble() * areaSize - halfArea);
            float height = _levelGen.GetHeightAt(x, z);
            if (height > 2f && height < 15f)
                PlaceObject(x, height, z, 0.8f + (float)rng.NextDouble() * 0.6f, (float)rng.NextDouble() * MathF.Tau, _treeMesh);
        }

        for (int i = 0; i < 80; i++)
        {
            float x = (float)(rng.NextDouble() * areaSize - halfArea);
            float z = (float)(rng.NextDouble() * areaSize - halfArea);
            float height = _levelGen.GetHeightAt(x, z);
            if (height > 1.5f)
                PlaceObject(x, height, z, 0.5f + (float)rng.NextDouble() * 1f, (float)rng.NextDouble() * MathF.Tau, _rockMesh);
        }
    }

    public void Render(GameTime time)
    {
        _basicShader.Use();
        _basicShader.SetUniform("uView", _camera.ViewMatrix);
        _basicShader.SetUniform("uProjection", _camera.ProjectionMatrix);
        _basicShader.SetUniform("uCameraPos", _camera.Position);
        _lighting.ApplyToShader(_basicShader);
        _shadowMap?.BindToShader(_basicShader, 0);
        _materialTextures?.BindToShader(_basicShader);

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
        _scrubMesh.Dispose();
        _berryBushMesh.Dispose();
        _grassTuftMesh.Dispose();
    }
}

public sealed class VegetationTag : IComponent { }
