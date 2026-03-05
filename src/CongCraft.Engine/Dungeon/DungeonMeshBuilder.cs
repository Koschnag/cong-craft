using System.Numerics;
using CongCraft.Engine.Procedural;
using CongCraft.Engine.Rendering;
using Silk.NET.OpenGL;

namespace CongCraft.Engine.Dungeon;

/// <summary>
/// Builds 3D mesh from a 2D dungeon layout. Creates floor, walls, and ceiling geometry.
/// Scale: 1 tile = 3 world units.
/// </summary>
public static class DungeonMeshBuilder
{
    public const float TileSize = 3f;
    public const float WallHeight = 4f;

    public static MeshData GenerateFloor(DungeonLayout layout)
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        for (int x = 0; x < layout.Width; x++)
            for (int y = 0; y < layout.Height; y++)
            {
                if (layout.Grid[x, y] == TileType.Wall) continue;

                float wx = x * TileSize;
                float wz = y * TileSize;

                // Choose color based on tile type
                var (r, g, b) = layout.Grid[x, y] switch
                {
                    TileType.Entrance => (0.3f, 0.5f, 0.3f),
                    TileType.Exit => (0.5f, 0.3f, 0.3f),
                    TileType.Corridor => (0.22f, 0.2f, 0.18f),
                    _ => (0.25f, 0.23f, 0.2f) // Floor
                };

                AddQuad(verts, inds,
                    new Vector3(wx, 0, wz),
                    new Vector3(wx + TileSize, 0, wz),
                    new Vector3(wx + TileSize, 0, wz + TileSize),
                    new Vector3(wx, 0, wz + TileSize),
                    Vector3.UnitY, r, g, b);
            }

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    public static MeshData GenerateWalls(DungeonLayout layout)
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        for (int x = 0; x < layout.Width; x++)
            for (int y = 0; y < layout.Height; y++)
            {
                if (layout.Grid[x, y] == TileType.Wall) continue;

                float wx = x * TileSize;
                float wz = y * TileSize;

                // Check each neighbor: if wall, add wall face
                if (x > 0 && layout.Grid[x - 1, y] == TileType.Wall)
                    AddWallFace(verts, inds, wx, wz, wx, wz + TileSize, Vector3.UnitX);

                if (x < layout.Width - 1 && layout.Grid[x + 1, y] == TileType.Wall)
                    AddWallFace(verts, inds, wx + TileSize, wz + TileSize, wx + TileSize, wz, -Vector3.UnitX);

                if (y > 0 && layout.Grid[x, y - 1] == TileType.Wall)
                    AddWallFace(verts, inds, wx + TileSize, wz, wx, wz, Vector3.UnitZ);

                if (y < layout.Height - 1 && layout.Grid[x, y + 1] == TileType.Wall)
                    AddWallFace(verts, inds, wx, wz + TileSize, wx + TileSize, wz + TileSize, -Vector3.UnitZ);
            }

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    public static MeshData GenerateCeiling(DungeonLayout layout)
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        for (int x = 0; x < layout.Width; x++)
            for (int y = 0; y < layout.Height; y++)
            {
                if (layout.Grid[x, y] == TileType.Wall) continue;

                float wx = x * TileSize;
                float wz = y * TileSize;

                AddQuad(verts, inds,
                    new Vector3(wx, WallHeight, wz + TileSize),
                    new Vector3(wx + TileSize, WallHeight, wz + TileSize),
                    new Vector3(wx + TileSize, WallHeight, wz),
                    new Vector3(wx, WallHeight, wz),
                    -Vector3.UnitY, 0.15f, 0.13f, 0.12f);
            }

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    public static Mesh CreateFloor(GL gl, DungeonLayout layout)
    {
        var data = GenerateFloor(layout);
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
    }

    public static Mesh CreateWalls(GL gl, DungeonLayout layout)
    {
        var data = GenerateWalls(layout);
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
    }

    public static Mesh CreateCeiling(GL gl, DungeonLayout layout)
    {
        var data = GenerateCeiling(layout);
        return new Mesh(gl, data.Vertices, data.Indices, VertexLayout.PositionNormalColor);
    }

    private static void AddWallFace(List<float> verts, List<uint> inds,
        float x1, float z1, float x2, float z2, Vector3 normal)
    {
        float r = 0.35f, g = 0.3f, b = 0.25f;
        AddQuad(verts, inds,
            new Vector3(x1, 0, z1),
            new Vector3(x2, 0, z2),
            new Vector3(x2, WallHeight, z2),
            new Vector3(x1, WallHeight, z1),
            normal, r, g, b);
    }

    private static void AddQuad(List<float> verts, List<uint> inds,
        Vector3 a, Vector3 b, Vector3 c, Vector3 d,
        Vector3 normal, float r, float g, float bl)
    {
        uint baseIdx = (uint)(verts.Count / 9);
        foreach (var v in new[] { a, b, c, d })
        {
            verts.Add(v.X); verts.Add(v.Y); verts.Add(v.Z);
            verts.Add(normal.X); verts.Add(normal.Y); verts.Add(normal.Z);
            verts.Add(r); verts.Add(g); verts.Add(bl);
        }
        inds.AddRange(new[] { baseIdx, baseIdx + 1, baseIdx + 2, baseIdx, baseIdx + 2, baseIdx + 3 });
    }
}
