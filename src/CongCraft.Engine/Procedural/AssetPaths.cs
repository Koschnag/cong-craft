using CongCraft.Engine.Core;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Resolves file paths within the congcraft_assets directory.
/// Handles different deployment scenarios (dev, release, macOS bundle).
/// </summary>
public static class AssetPaths
{
    private static string? _resolvedRoot;

    /// <summary>
    /// Root path to the congcraft_assets directory.
    /// </summary>
    public static string Root
    {
        get
        {
            _resolvedRoot ??= ResolveRoot();
            return _resolvedRoot;
        }
    }

    public static string Audio => Path.Combine(Root, "audio");
    public static string Models => Path.Combine(Root, "models");
    public static string Textures => Path.Combine(Root, "textures");
    public static string UI => Path.Combine(Root, "ui");
    public static string Icons => Path.Combine(Textures, "icons");
    public static string Portraits => Path.Combine(Textures, "portraits");
    public static string Particles => Path.Combine(Textures, "particles");

    /// <summary>Get path to a specific audio file.</summary>
    public static string AudioFile(string relativePath) => Path.Combine(Audio, relativePath);

    /// <summary>Get path to a specific model file.</summary>
    public static string ModelFile(string relativePath) => Path.Combine(Models, relativePath);

    /// <summary>Get path to a specific texture file.</summary>
    public static string TextureFile(string relativePath) => Path.Combine(Textures, relativePath);

    /// <summary>Get path to a specific UI asset file.</summary>
    public static string UIFile(string relativePath) => Path.Combine(UI, relativePath);

    /// <summary>Get path to a specific icon file.</summary>
    public static string IconFile(string relativePath) => Path.Combine(Icons, relativePath);

    /// <summary>Get path to a specific portrait file.</summary>
    public static string PortraitFile(string relativePath) => Path.Combine(Portraits, relativePath);

    /// <summary>Get path to a specific particle texture file.</summary>
    public static string ParticleFile(string relativePath) => Path.Combine(Particles, relativePath);

    /// <summary>Check if a specific asset file exists.</summary>
    public static bool Exists(string fullPath) => File.Exists(fullPath);

    private static string ResolveRoot()
    {
        var baseDir = AppContext.BaseDirectory;

        // 1. Output directory (copied by MSBuild)
        var outputDir = Path.Combine(baseDir, "congcraft_assets");
        if (Directory.Exists(outputDir))
        {
            DevLog.Info($"Assets found at: {outputDir}");
            return outputDir;
        }

        // 2. macOS .app bundle: Contents/Resources/congcraft_assets
        var macDir = Path.GetFullPath(Path.Combine(baseDir, "..", "Resources", "congcraft_assets"));
        if (Directory.Exists(macDir))
        {
            DevLog.Info($"Assets found at: {macDir} (macOS bundle)");
            return macDir;
        }

        // 3. Development: relative from bin to project root
        var devDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", "congcraft_assets"));
        if (Directory.Exists(devDir))
        {
            DevLog.Info($"Assets found at: {devDir} (dev)");
            return devDir;
        }

        // 4. Workspace root (running from repo root)
        var workspaceDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "congcraft_assets"));
        if (Directory.Exists(workspaceDir))
        {
            DevLog.Info($"Assets found at: {workspaceDir} (workspace)");
            return workspaceDir;
        }

        DevLog.Warn($"congcraft_assets not found — procedural fallbacks will be used");
        return outputDir; // Return expected path even if not found
    }
}
