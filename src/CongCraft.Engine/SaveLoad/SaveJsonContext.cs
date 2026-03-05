using System.Text.Json.Serialization;

namespace CongCraft.Engine.SaveLoad;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(SaveData))]
internal partial class SaveJsonContext : JsonSerializerContext
{
}
