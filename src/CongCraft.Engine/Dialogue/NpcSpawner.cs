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

namespace CongCraft.Engine.Dialogue;

/// <summary>
/// Spawns NPC entities at fixed positions defined by the level data.
/// </summary>
public sealed class NpcSpawner : ISystem
{
    public int Priority => 47; // After enemies

    private GL _gl = null!;
    private World _world = null!;
    private LevelTerrainGenerator _levelGen = null!;
    private LevelData? _levelData;
    private readonly Dictionary<string, Mesh> _npcMeshes = new();
    private Shader _basicShader = null!;
    private MaterialTextures? _materialTextures;
    private bool _spawned;

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _world = services.Get<World>();
        _levelGen = services.Get<LevelTerrainGenerator>();
        services.TryGet(out _levelData);
        _basicShader = new Shader(_gl, ShaderSources.BasicVertex, ShaderSources.BasicFragment);
        _materialTextures = services.Get<MaterialTextures>();
    }

    public void Update(GameTime time)
    {
        if (_spawned) return;
        _spawned = true;

        if (_levelData?.Npcs != null)
        {
            foreach (var npc in _levelData.Npcs)
                SpawnNpc(npc.NpcType, npc.Name, npc.X, npc.Z, npc.FacingAngle);
        }
        else
        {
            SpawnNpc("blacksmith", "Blacksmith Aldric", 8f, 12f);
            SpawnNpc("elder", "Elder Maren", -5f, 15f);
            SpawnNpc("merchant", "Merchant Gregor", 12f, 5f);
        }
    }

    private void SpawnNpc(string npcType, string name, float x, float z, float facingAngle = 0f)
    {
        float height = _levelGen.GetHeightAt(x, z);
        if (height < 2f) height = 2f;

        if (!_npcMeshes.ContainsKey(npcType))
            _npcMeshes[npcType] = HighResNpcMeshBuilder.Create(_gl, npcType);

        var entity = _world.CreateEntity();
        _world.AddComponent(entity, new TransformComponent
        {
            Position = new Vector3(x, height + 0.5f, z),
            Scale = Vector3.One,
            Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, facingAngle)
        });
        _world.AddComponent(entity, new NpcComponent
        {
            DialogueTreeId = npcType,
            Name = name
        });
        _world.AddComponent(entity, new MeshRendererComponent
        {
            Mesh = _npcMeshes[npcType],
            Shader = _basicShader
        });
    }

    public void Render(GameTime time) { }

    public void Dispose()
    {
        foreach (var mesh in _npcMeshes.Values)
            mesh.Dispose();
        _basicShader.Dispose();
    }
}
