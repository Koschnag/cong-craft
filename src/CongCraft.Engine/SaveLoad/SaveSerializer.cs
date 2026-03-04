using System.Text.Json;

namespace CongCraft.Engine.SaveLoad;

/// <summary>
/// Handles serialization/deserialization of SaveData to/from JSON.
/// </summary>
public static class SaveSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static string SaveDirectory => Path.Combine(
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
        ".congcraft", "saves");

    private static string SaveFilePath(int slot = 0) =>
        Path.Combine(SaveDirectory, $"save_{slot}.json");

    public static void Save(SaveData data, int slot = 0)
    {
        Directory.CreateDirectory(SaveDirectory);
        data.SavedAt = DateTime.UtcNow;
        string json = JsonSerializer.Serialize(data, Options);
        File.WriteAllText(SaveFilePath(slot), json);
    }

    public static SaveData? Load(int slot = 0)
    {
        string path = SaveFilePath(slot);
        if (!File.Exists(path)) return null;

        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<SaveData>(json, Options);
    }

    public static bool SaveExists(int slot = 0) =>
        File.Exists(SaveFilePath(slot));

    /// <summary>
    /// Serializes to JSON string (for testing without file I/O).
    /// </summary>
    public static string SerializeToString(SaveData data) =>
        JsonSerializer.Serialize(data, Options);

    /// <summary>
    /// Deserializes from JSON string (for testing without file I/O).
    /// </summary>
    public static SaveData? DeserializeFromString(string json) =>
        JsonSerializer.Deserialize<SaveData>(json, Options);
}
