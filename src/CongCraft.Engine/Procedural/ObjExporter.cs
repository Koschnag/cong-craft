using System.Globalization;
using System.Text;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Exports MeshData (PositionNormalColor, 9 floats/vertex) to Wavefront OBJ format.
/// Includes vertex colors as # color comments for the ObjLoader to read back.
/// Groups vertices by similar color for material organization.
/// </summary>
public static class ObjExporter
{
    public static void Export(string path, MeshData data, string objectName = "mesh")
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# CongCraft AI-Ready Asset: {objectName}");
        sb.AppendLine($"# Vertices: {data.VertexCount}, Triangles: {data.Indices.Length / 3}");
        sb.AppendLine($"# Format: position + normal + vertex color (9 floats/vertex)");
        sb.AppendLine($"# Replace this file with AI-generated OBJ to upgrade model quality.");
        sb.AppendLine($"# Use '# color R G B' before face groups to set vertex colors.");
        sb.AppendLine($"o {objectName}");
        sb.AppendLine();

        int vertCount = data.VertexCount;

        // Write positions
        for (int i = 0; i < vertCount; i++)
        {
            int o = i * 9;
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "v {0:F6} {1:F6} {2:F6}",
                data.Vertices[o], data.Vertices[o + 1], data.Vertices[o + 2]));
        }
        sb.AppendLine();

        // Write normals
        for (int i = 0; i < vertCount; i++)
        {
            int o = i * 9;
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "vn {0:F6} {1:F6} {2:F6}",
                data.Vertices[o + 3], data.Vertices[o + 4], data.Vertices[o + 5]));
        }
        sb.AppendLine();

        // Group faces by vertex color for material hints
        var faceGroups = new Dictionary<string, List<(uint a, uint b, uint c)>>();
        for (int i = 0; i < data.Indices.Length; i += 3)
        {
            uint a = data.Indices[i], b = data.Indices[i + 1], c = data.Indices[i + 2];
            // Use first vertex color as group key
            int o = (int)a * 9;
            string colorKey = $"{data.Vertices[o + 6]:F2}_{data.Vertices[o + 7]:F2}_{data.Vertices[o + 8]:F2}";
            if (!faceGroups.ContainsKey(colorKey))
                faceGroups[colorKey] = new();
            faceGroups[colorKey].Add((a, b, c));
        }

        int groupIdx = 0;
        foreach (var (colorKey, faces) in faceGroups)
        {
            var parts = colorKey.Split('_');
            sb.AppendLine($"# color {parts[0]} {parts[1]} {parts[2]}");
            sb.AppendLine($"g group_{groupIdx++}");
            foreach (var (a, b, c) in faces)
            {
                // OBJ is 1-based
                sb.AppendLine($"f {a + 1}//{a + 1} {b + 1}//{b + 1} {c + 1}//{c + 1}");
            }
            sb.AppendLine();
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, sb.ToString());
    }
}
