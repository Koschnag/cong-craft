using Silk.NET.OpenGL;
using CongCraft.Engine.Rendering;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Manages loading and caching of external 3D model assets (OBJ files).
/// Assets are stored in the "assets/models" directory relative to the executable.
/// Falls back to procedural generation when external assets are unavailable.
///
/// Workflow: On first run, procedural meshes are exported as OBJ files.
/// These OBJs can then be replaced with AI-generated models (e.g. from Meshy, Luma, Rodin)
/// while keeping the same vertex color material hints for the shader system.
/// </summary>
public sealed class AssetManager
{
    private readonly GL _gl;
    private readonly Dictionary<string, MeshData> _meshDataCache = new();
    private readonly Dictionary<string, Mesh> _meshCache = new();
    private readonly string _assetsDir;

    public AssetManager(GL gl)
    {
        _gl = gl;
        _assetsDir = Path.Combine(AppContext.BaseDirectory, "assets", "models");
        Directory.CreateDirectory(_assetsDir);
    }

    /// <summary>
    /// Load a mesh from an OBJ file in the assets directory.
    /// Returns null if the file doesn't exist (caller should fall back to procedural).
    /// </summary>
    public Mesh? LoadMesh(string name, float defaultR = 0.5f, float defaultG = 0.5f, float defaultB = 0.5f)
    {
        if (_meshCache.TryGetValue(name, out var cached))
            return cached;

        var data = LoadMeshData(name, defaultR, defaultG, defaultB);
        if (data == null) return null;

        var mesh = new Mesh(_gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
        _meshCache[name] = mesh;
        return mesh;
    }

    /// <summary>
    /// Load mesh data from an OBJ file (without creating GPU resources).
    /// </summary>
    public MeshData? LoadMeshData(string name, float defaultR = 0.5f, float defaultG = 0.5f, float defaultB = 0.5f)
    {
        if (_meshDataCache.TryGetValue(name, out var cached))
            return cached;

        string path = Path.Combine(_assetsDir, name + ".obj");
        if (!File.Exists(path)) return null;

        var data = ObjLoader.Load(path, defaultR, defaultG, defaultB);
        _meshDataCache[name] = data;
        return data;
    }

    /// <summary>
    /// Load a mesh from OBJ if available, otherwise use procedural fallback and export OBJ.
    /// </summary>
    public Mesh LoadOrGenerate(string name, Func<MeshData> proceduralFallback,
        float defaultR = 0.5f, float defaultG = 0.5f, float defaultB = 0.5f)
    {
        var mesh = LoadMesh(name, defaultR, defaultG, defaultB);
        if (mesh != null) return mesh;

        var data = proceduralFallback();
        SaveAsObj(name, data);
        mesh = new Mesh(_gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
        _meshCache[name] = mesh;
        _meshDataCache[name] = data;
        return mesh;
    }

    /// <summary>
    /// Save mesh data as an OBJ file for external editing or AI regeneration.
    /// </summary>
    public void SaveAsObj(string name, MeshData data)
    {
        string path = Path.Combine(_assetsDir, name + ".obj");
        ObjExporter.Export(path, data, name);
    }

    /// <summary>
    /// Export all procedural meshes as OBJ files on first run.
    /// These serve as templates that can be replaced with AI-generated versions.
    /// </summary>
    public void GenerateAllAssets()
    {
        // Player
        GenerateIfMissing("player_warrior", HighResPlayerMeshBuilder.GenerateData);

        // Trees
        GenerateIfMissing("tree_default", () => TreeMeshBuilder.GenerateData());
        GenerateIfMissing("tree_tall", () => TreeMeshBuilder.GenerateData(
            new TreeMeshBuilder.TreeParams(TrunkHeight: 3.5f, CrownRadius: 2.0f)));
        GenerateIfMissing("tree_small", () => TreeMeshBuilder.GenerateData(
            new TreeMeshBuilder.TreeParams(TrunkHeight: 1.5f, CrownRadius: 1.0f, TrunkRadius: 0.10f)));

        // Rocks
        GenerateIfMissing("rock_default", () => RockMeshBuilder.GenerateData());
        GenerateIfMissing("rock_small", () => RockMeshBuilder.GenerateData(
            new RockMeshBuilder.RockParams(BaseRadius: 0.4f, Deformation: 0.15f)));
        GenerateIfMissing("rock_boulder", () => RockMeshBuilder.GenerateData(
            new RockMeshBuilder.RockParams(BaseRadius: 1.5f, Deformation: 0.5f, Subdivisions: 4)));

        // Ruins
        GenerateIfMissing("ruin_pillar", () =>
        {
            var (v, i) = RuinMeshBuilder.GenerateData(RuinMeshBuilder.RuinType.Pillar);
            return new MeshData(v, i);
        });
        GenerateIfMissing("ruin_broken_pillar", () =>
        {
            var (v, i) = RuinMeshBuilder.GenerateData(RuinMeshBuilder.RuinType.BrokenPillar);
            return new MeshData(v, i);
        });
        GenerateIfMissing("ruin_wall", () =>
        {
            var (v, i) = RuinMeshBuilder.GenerateData(RuinMeshBuilder.RuinType.WallSegment);
            return new MeshData(v, i);
        });
        GenerateIfMissing("ruin_arch", () =>
        {
            var (v, i) = RuinMeshBuilder.GenerateData(RuinMeshBuilder.RuinType.ArchFragment);
            return new MeshData(v, i);
        });

        // Enemies
        GenerateIfMissing("enemy_bandit", HighResEnemyMeshBuilder.GenerateData);
        GenerateIfMissing("enemy_skeleton", HighResEnemyMeshBuilder.GenerateSkeletonData);
        GenerateIfMissing("enemy_wolf", HighResEnemyMeshBuilder.GenerateWolfData);
        GenerateIfMissing("enemy_troll", HighResEnemyMeshBuilder.GenerateTrollData);

        // Bushes
        GenerateIfMissing("bush_scrub", () => BushMeshBuilder.GenerateScrub(101));
        GenerateIfMissing("bush_berry", () => BushMeshBuilder.GenerateBerry(202));
        GenerateIfMissing("bush_grass", () => BushMeshBuilder.GenerateGrassTuft(303));

        // Sword
        GenerateIfMissing("weapon_sword", SwordMeshBuilder.GenerateData);
    }

    private void GenerateIfMissing(string name, Func<MeshData> generator)
    {
        string path = Path.Combine(_assetsDir, name + ".obj");
        if (File.Exists(path)) return;

        var data = generator();
        ObjExporter.Export(path, data, name);
        _meshDataCache[name] = data;
    }

    public string AssetsDirectory => _assetsDir;
}
