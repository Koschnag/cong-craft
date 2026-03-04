using System.Numerics;
using CongCraft.Engine.ECS;

namespace CongCraft.Engine.UI;

/// <summary>
/// Stores pre-computed minimap terrain colors and marker positions.
/// Updated each frame by HudSystem.
/// </summary>
public sealed class MinimapData : IComponent
{
    public const int Resolution = 32; // 32x32 grid of terrain cells
    public const float WorldRange = 60f; // How much world space the minimap covers
    public const float MapSize = 150f; // Pixel size on screen

    // Pre-computed terrain colors (row-major, y-down)
    public Vector4[] TerrainColors { get; } = new Vector4[Resolution * Resolution];

    // Dynamic markers
    public List<MinimapMarker> Markers { get; } = new();

    public float CellSize => MapSize / Resolution;
    public float WorldCellSize => WorldRange * 2f / Resolution;
}

public struct MinimapMarker
{
    public Vector2 ScreenPos; // Position on minimap in screen coords
    public Vector4 Color;
    public float Size;
}
