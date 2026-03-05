using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Procedural;
using CongCraft.Engine.Rendering;
using Silk.NET.OpenGL;
using Shader = CongCraft.Engine.Rendering.Shader;

namespace CongCraft.Engine.Terrain;

/// <summary>
/// Manages terrain chunk loading around the player position.
/// Now with triplanar texturing and shadow support.
/// </summary>
public sealed class TerrainSystem : ISystem, IShadowCaster
{
    public int Priority => 40;

    private GL _gl = null!;
    private World _world = null!;
    private TerrainGenerator _generator = null!;
    private Shader _terrainShader = null!;
    private Camera _camera = null!;
    private LightingData _lighting = null!;
    private ShadowMap? _shadowMap;

    private uint _grassTex, _stoneTex, _dirtTex;

    private readonly int _viewDistance;
    private readonly Dictionary<(int, int), Entity> _loadedChunks = new();

    public TerrainGenerator Generator => _generator;

    public TerrainSystem(int viewDistance = 2)
    {
        _viewDistance = viewDistance;
    }

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _world = services.Get<World>();
        _camera = services.Get<Camera>();
        _lighting = services.Get<LightingData>();
        _generator = new TerrainGenerator();

        _terrainShader = new Shader(_gl, ShaderSources.TerrainVertex, ShaderSources.TerrainFragment);
        _shadowMap = services.Get<ShadowMap>();

        // Generate and upload terrain textures (512x512 with mipmaps)
        var grassPixels = TextureGenerator.GenerateGrassPixels();
        _grassTex = TextureGenerator.UploadTexture(_gl, grassPixels, 512, 512);

        var stonePixels = TextureGenerator.GenerateStonePixels();
        _stoneTex = TextureGenerator.UploadTexture(_gl, stonePixels, 512, 512);

        var dirtPixels = TextureGenerator.GenerateDirtPixels();
        _dirtTex = TextureGenerator.UploadTexture(_gl, dirtPixels, 512, 512);

        // Load initial chunks around origin
        LoadChunksAround(0, 0);
    }

    public void Update(GameTime time)
    {
        var camPos = _camera.Target;
        int cx = (int)MathF.Floor(camPos.X / 64f);
        int cz = (int)MathF.Floor(camPos.Z / 64f);
        LoadChunksAround(cx, cz);
    }

    public void RenderShadowPass(ShadowMap shadowMap)
    {
        var shader = shadowMap.GetTerrainShader();
        foreach (var (entity, chunk) in _world.Query<TerrainChunkComponent>())
        {
            chunk.Mesh.Draw();
        }
    }

    public void Render(GameTime time)
    {
        _terrainShader.Use();
        _terrainShader.SetUniform("uView", _camera.ViewMatrix);
        _terrainShader.SetUniform("uProjection", _camera.ProjectionMatrix);
        _terrainShader.SetUniform("uModel", Matrix4x4.Identity);
        _terrainShader.SetUniform("uCameraPos", _camera.Position);
        _lighting.ApplyToShader(_terrainShader);

        // Bind terrain textures
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _grassTex);
        _terrainShader.SetUniform("uGrassTex", 0);

        _gl.ActiveTexture(TextureUnit.Texture1);
        _gl.BindTexture(TextureTarget.Texture2D, _stoneTex);
        _terrainShader.SetUniform("uStoneTex", 1);

        _gl.ActiveTexture(TextureUnit.Texture2);
        _gl.BindTexture(TextureTarget.Texture2D, _dirtTex);
        _terrainShader.SetUniform("uDirtTex", 2);

        // Bind shadow map
        _shadowMap?.BindToShader(_terrainShader, 3);

        foreach (var (entity, chunk) in _world.Query<TerrainChunkComponent>())
        {
            chunk.Mesh.Draw();
        }
    }

    private void LoadChunksAround(int centerX, int centerZ)
    {
        var neededChunks = new HashSet<(int, int)>();

        for (int z = centerZ - _viewDistance; z <= centerZ + _viewDistance; z++)
        for (int x = centerX - _viewDistance; x <= centerX + _viewDistance; x++)
        {
            neededChunks.Add((x, z));
            if (!_loadedChunks.ContainsKey((x, z)))
                CreateChunk(x, z);
        }

        // Unload far chunks
        var toRemove = _loadedChunks.Keys.Where(k => !neededChunks.Contains(k)).ToList();
        foreach (var key in toRemove)
        {
            if (_loadedChunks.TryGetValue(key, out var entity))
            {
                var chunk = _world.GetComponent<TerrainChunkComponent>(entity);
                chunk.Mesh.Dispose();
                _world.DestroyEntity(entity);
                _loadedChunks.Remove(key);
            }
        }
    }

    private void CreateChunk(int chunkX, int chunkZ)
    {
        var heightmap = _generator.GenerateChunk(chunkX, chunkZ);
        var vertices = heightmap.BuildVertices();
        var indices = heightmap.BuildIndices();
        var mesh = new Mesh(_gl, vertices, indices, VertexLayout.PositionNormalTexCoord);

        var entity = _world.CreateEntity();
        _world.AddComponent(entity, new TerrainChunkComponent
        {
            ChunkX = chunkX,
            ChunkZ = chunkZ,
            Mesh = mesh,
            HeightmapData = heightmap
        });

        _loadedChunks[(chunkX, chunkZ)] = entity;
    }

    public void Dispose()
    {
        foreach (var (_, entity) in _loadedChunks)
        {
            if (_world.HasComponent<TerrainChunkComponent>(entity))
            {
                var chunk = _world.GetComponent<TerrainChunkComponent>(entity);
                chunk.Mesh.Dispose();
            }
        }
        _terrainShader.Dispose();
        _gl.DeleteTexture(_grassTex);
        _gl.DeleteTexture(_stoneTex);
        _gl.DeleteTexture(_dirtTex);
    }
}
