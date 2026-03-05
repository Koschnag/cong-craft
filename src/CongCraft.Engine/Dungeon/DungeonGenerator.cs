using System.Numerics;

namespace CongCraft.Engine.Dungeon;

/// <summary>
/// Procedurally generates dungeon layouts using room placement and corridor connection.
/// Uses a grid-based approach with simple BSP-like room generation.
/// </summary>
public sealed class DungeonGenerator
{
    private readonly int _gridWidth;
    private readonly int _gridHeight;
    private readonly Random _rng;

    public DungeonGenerator(int gridWidth = 40, int gridHeight = 40, int seed = 42)
    {
        _gridWidth = gridWidth;
        _gridHeight = gridHeight;
        _rng = new Random(seed);
    }

    public DungeonLayout Generate(int roomCount = 6)
    {
        var layout = new DungeonLayout(_gridWidth, _gridHeight);

        // Generate rooms
        var rooms = new List<DungeonRoom>();
        int attempts = 0;

        while (rooms.Count < roomCount && attempts < roomCount * 20)
        {
            attempts++;
            var room = TryPlaceRoom(layout, rooms);
            if (room != null)
                rooms.Add(room);
        }

        layout.Rooms = rooms;

        // Connect rooms with corridors
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            ConnectRooms(layout, rooms[i], rooms[i + 1]);
        }

        // Place entrance in first room and exit in last
        if (rooms.Count >= 2)
        {
            layout.Entrance = rooms[0].Center;
            layout.Exit = rooms[^1].Center;
            layout.Grid[rooms[0].Center.X, rooms[0].Center.Y] = TileType.Entrance;
            layout.Grid[rooms[^1].Center.X, rooms[^1].Center.Y] = TileType.Exit;
        }

        return layout;
    }

    private DungeonRoom? TryPlaceRoom(DungeonLayout layout, List<DungeonRoom> existing)
    {
        int w = _rng.Next(4, 9);
        int h = _rng.Next(4, 9);
        int x = _rng.Next(1, _gridWidth - w - 1);
        int y = _rng.Next(1, _gridHeight - h - 1);

        var room = new DungeonRoom(x, y, w, h);

        // Check overlap with existing rooms (with padding)
        foreach (var other in existing)
        {
            if (room.Overlaps(other, padding: 2))
                return null;
        }

        // Carve room
        for (int rx = room.X; rx < room.X + room.Width; rx++)
            for (int ry = room.Y; ry < room.Y + room.Height; ry++)
            {
                layout.Grid[rx, ry] = TileType.Floor;
            }

        return room;
    }

    private void ConnectRooms(DungeonLayout layout, DungeonRoom a, DungeonRoom b)
    {
        var start = a.Center;
        var end = b.Center;

        // L-shaped corridor: horizontal then vertical (or vice versa)
        if (_rng.NextDouble() < 0.5)
        {
            CarveHorizontal(layout, start.X, end.X, start.Y);
            CarveVertical(layout, start.Y, end.Y, end.X);
        }
        else
        {
            CarveVertical(layout, start.Y, end.Y, start.X);
            CarveHorizontal(layout, start.X, end.X, end.Y);
        }
    }

    private void CarveHorizontal(DungeonLayout layout, int x1, int x2, int y)
    {
        int minX = Math.Min(x1, x2);
        int maxX = Math.Max(x1, x2);
        for (int x = minX; x <= maxX; x++)
        {
            if (layout.Grid[x, y] == TileType.Wall)
                layout.Grid[x, y] = TileType.Corridor;
        }
    }

    private void CarveVertical(DungeonLayout layout, int y1, int y2, int x)
    {
        int minY = Math.Min(y1, y2);
        int maxY = Math.Max(y1, y2);
        for (int y = minY; y <= maxY; y++)
        {
            if (layout.Grid[x, y] == TileType.Wall)
                layout.Grid[x, y] = TileType.Corridor;
        }
    }
}

/// <summary>
/// The generated dungeon layout with a 2D tile grid.
/// </summary>
public sealed class DungeonLayout
{
    public TileType[,] Grid { get; }
    public int Width { get; }
    public int Height { get; }
    public List<DungeonRoom> Rooms { get; set; } = new();
    public (int X, int Y) Entrance { get; set; }
    public (int X, int Y) Exit { get; set; }

    public DungeonLayout(int width, int height)
    {
        Width = width;
        Height = height;
        Grid = new TileType[width, height];
        // Default: all walls
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                Grid[x, y] = TileType.Wall;
    }

    public bool IsWalkable(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return false;
        return Grid[x, y] != TileType.Wall;
    }

    public int FloorTileCount
    {
        get
        {
            int count = 0;
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (Grid[x, y] != TileType.Wall) count++;
            return count;
        }
    }
}

/// <summary>
/// A rectangular room in the dungeon.
/// </summary>
public sealed class DungeonRoom
{
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }
    public (int X, int Y) Center => (X + Width / 2, Y + Height / 2);

    public DungeonRoom(int x, int y, int width, int height)
    {
        X = x; Y = y; Width = width; Height = height;
    }

    public bool Overlaps(DungeonRoom other, int padding = 0)
    {
        return X - padding < other.X + other.Width + padding &&
               X + Width + padding > other.X - padding &&
               Y - padding < other.Y + other.Height + padding &&
               Y + Height + padding > other.Y - padding;
    }
}

public enum TileType
{
    Wall,
    Floor,
    Corridor,
    Entrance,
    Exit
}
