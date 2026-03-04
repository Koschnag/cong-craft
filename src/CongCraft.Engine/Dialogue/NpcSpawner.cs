using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Procedural;
using CongCraft.Engine.Rendering;
using CongCraft.Engine.Terrain;
using Silk.NET.OpenGL;
using Shader = CongCraft.Engine.Rendering.Shader;

namespace CongCraft.Engine.Dialogue;

/// <summary>
/// Spawns NPC entities on the terrain at startup.
/// </summary>
public sealed class NpcSpawner : ISystem
{
    public int Priority => 47; // After enemies

    private GL _gl = null!;
    private World _world = null!;
    private TerrainGenerator _terrainGen = null!;
    private readonly Dictionary<string, Mesh> _npcMeshes = new();
    private Shader _basicShader = null!;
    private bool _spawned;

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _world = services.Get<World>();
        _terrainGen = services.Get<TerrainGenerator>();
        _basicShader = new Shader(_gl, ShaderSources.BasicVertex, ShaderSources.BasicFragment);
    }

    public void Update(GameTime time)
    {
        if (_spawned) return;
        _spawned = true;

        SpawnNpc("blacksmith", "Blacksmith Aldric", 8f, 12f);
        SpawnNpc("elder", "Elder Maren", -5f, 15f);
        SpawnNpc("merchant", "Merchant Gregor", 12f, 5f);
    }

    private void SpawnNpc(string npcType, string name, float x, float z)
    {
        float height = _terrainGen.GetHeightAt(x, z);
        if (height < 2f) height = 2f; // Ensure above water

        if (!_npcMeshes.ContainsKey(npcType))
            _npcMeshes[npcType] = NpcMeshBuilder.Create(_gl, npcType);

        var entity = _world.CreateEntity();
        _world.AddComponent(entity, new TransformComponent
        {
            Position = new Vector3(x, height + 0.5f, z),
            Scale = Vector3.One
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
