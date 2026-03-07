using Silk.NET.OpenGL;
using CongCraft.Engine.Core;
using CongCraft.Engine.Rendering;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Manages loading and caching of external 3D model assets (OBJ files).
/// Searches for assets in congcraft_assets/models/ first, then assets/models/.
/// Falls back to procedural generation when external assets are unavailable.
///
/// Workflow: Assets from congcraft_assets/ are loaded when available.
/// If not found, procedural meshes are generated and exported as OBJ files.
/// These OBJs can then be replaced with AI-generated models (e.g. from Meshy, Luma, Rodin).
/// </summary>
public sealed class AssetManager
{
    private readonly GL _gl;
    private readonly Dictionary<string, MeshData> _meshDataCache = new();
    private readonly Dictionary<string, Mesh> _meshCache = new();
    private readonly string _assetsDir;

    // Mapping from code asset names to congcraft_assets paths
    private static readonly Dictionary<string, string[]> _assetNameMap = new()
    {
        // Player
        ["player_warrior"] = new[] { "characters/player/player_lod0.obj", "characters/player/player_idle.obj" },

        // Enemies
        ["enemy_bandit"] = new[] { "characters/bandit/bandit_lod0.obj" },
        ["enemy_skeleton"] = new[] { "characters/skeleton/skeleton_lod0.obj" },
        ["enemy_wolf"] = new[] { "characters/wolf/wolf_lod0.obj" },
        ["enemy_troll"] = new[] { "characters/boss/boss_lod0.obj" },

        // Trees
        ["tree_default"] = new[] { "environment/trees/pine_a.obj" },
        ["tree_tall"] = new[] { "environment/trees/oak_a.obj" },
        ["tree_small"] = new[] { "environment/trees/dead_tree_a.obj" },

        // Rocks
        ["rock_default"] = new[] { "environment/rocks/rock_medium_a.obj" },
        ["rock_small"] = new[] { "environment/rocks/rock_large_a.obj" },
        ["rock_boulder"] = new[] { "environment/rocks/cliff_fragment_a.obj" },

        // Ruins
        ["ruin_pillar"] = new[] { "environment/ruins/pillar_broken_a.obj" },
        ["ruin_broken_pillar"] = new[] { "environment/ruins/pillar_broken_a.obj" },
        ["ruin_wall"] = new[] { "environment/ruins/wall_ruin_a.obj" },
        ["ruin_arch"] = new[] { "environment/ruins/arch_ruin_a.obj" },

        // Props
        ["weapon_sword"] = new[] { "weapons/sword_iron_a.obj" },

        // Stations
        ["station_anvil"] = new[] { "stations/anvil_a.obj" },
        ["station_alchemy"] = new[] { "stations/alchemy_table_a.obj" },
        ["station_workbench"] = new[] { "stations/workbench_a.obj" },

        // Buildings
        ["building_hut"] = new[] { "environment/buildings/hut_a.obj" },
        ["building_smithy"] = new[] { "environment/buildings/smithy_a.obj" },
        ["building_watchtower"] = new[] { "environment/buildings/watchtower_a.obj" },
        ["building_gate"] = new[] { "environment/buildings/wooden_gate_a.obj" },

        // Props
        ["prop_crate"] = new[] { "props/crates/crate_a.obj" },
        ["prop_barrel"] = new[] { "props/barrels/barrel_a.obj" },
        ["prop_torch"] = new[] { "props/torches/torch_a.obj" },
        ["prop_campfire"] = new[] { "props/camp/campfire_a.obj" },
        ["prop_chest"] = new[] { "props/loot/chest_wood_a.obj" },
    };

    public AssetManager(GL gl)
    {
        _gl = gl;
        _assetsDir = ResolveAssetsDir();
        Directory.CreateDirectory(_assetsDir);
    }

    private static string ResolveAssetsDir()
    {
        // Standard location next to executable
        var baseDir = AppContext.BaseDirectory;
        var standard = Path.Combine(baseDir, "assets", "models");
        if (Directory.Exists(standard)) return standard;

        // macOS .app bundle: assets live in Contents/Resources/
        var resourcesDir = Path.Combine(baseDir, "..", "Resources", "assets", "models");
        if (Directory.Exists(resourcesDir)) return Path.GetFullPath(resourcesDir);

        return standard;
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
    /// Searches in congcraft_assets/models/ first, then assets/models/.
    /// </summary>
    public MeshData? LoadMeshData(string name, float defaultR = 0.5f, float defaultG = 0.5f, float defaultB = 0.5f)
    {
        if (_meshDataCache.TryGetValue(name, out var cached))
            return cached;

        // Try congcraft_assets first (name-mapped paths)
        if (_assetNameMap.TryGetValue(name, out var candidates))
        {
            foreach (var candidate in candidates)
            {
                var artPath = AssetPaths.ModelFile(candidate);
                if (File.Exists(artPath))
                {
                    var data = ObjLoader.Load(artPath, defaultR, defaultG, defaultB);
                    _meshDataCache[name] = data;
                    DevLog.Info($"Loaded model: {candidate} (as '{name}')");
                    return data;
                }
            }
        }

        // Try congcraft_assets with the code name directly
        var directArtPath = AssetPaths.ModelFile(name + ".obj");
        if (File.Exists(directArtPath))
        {
            var data = ObjLoader.Load(directArtPath, defaultR, defaultG, defaultB);
            _meshDataCache[name] = data;
            DevLog.Info($"Loaded model: {name}.obj");
            return data;
        }

        // Fall back to assets/models/ (exported procedural OBJs)
        string path = Path.Combine(_assetsDir, name + ".obj");
        if (!File.Exists(path)) return null;

        var fallbackData = ObjLoader.Load(path, defaultR, defaultG, defaultB);
        _meshDataCache[name] = fallbackData;
        return fallbackData;
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
