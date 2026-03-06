using System.Globalization;

namespace CongCraft.Engine.Procedural;

/// <summary>
/// Parses Wavefront OBJ files into vertex/index data compatible with the engine's
/// PositionNormalColor vertex layout (9 floats per vertex).
/// Supports: v (positions), vn (normals), f (faces with v//vn format),
/// and maps vertex colors from MTL-style comments or defaults to material hinting colors.
/// </summary>
public static class ObjLoader
{
    /// <summary>
    /// Load an OBJ file from disk and return mesh data.
    /// defaultColor sets the vertex color (R,G,B) used when no per-vertex color is available.
    /// </summary>
    public static MeshData Load(string path, float defaultR = 0.5f, float defaultG = 0.5f, float defaultB = 0.5f)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"OBJ file not found: {path}");

        string text = File.ReadAllText(path);
        return Parse(text, defaultR, defaultG, defaultB);
    }

    /// <summary>
    /// Parse OBJ text content into engine-compatible mesh data.
    /// Handles v, vn, vt, f directives. Faces can be:
    ///   f v1 v2 v3                 (position only)
    ///   f v1/vt1 v2/vt2 v3/vt3    (position + texcoord)
    ///   f v1//vn1 v2//vn2 v3//vn3 (position + normal)
    ///   f v1/vt1/vn1 ...          (position + texcoord + normal)
    /// Also supports # color R G B comments for per-group vertex coloring.
    /// </summary>
    public static MeshData Parse(string objText, float defaultR = 0.5f, float defaultG = 0.5f, float defaultB = 0.5f)
    {
        var positions = new List<float[]>();
        var normals = new List<float[]>();
        var vertexMap = new Dictionary<string, uint>();
        var verts = new List<float>();
        var inds = new List<uint>();

        float curR = defaultR, curG = defaultG, curB = defaultB;

        // Reusable buffer to avoid per-face allocation
        var faceVerts = new List<uint>(8);

        var lines = objText.Split('\n', StringSplitOptions.TrimEntries);

        foreach (var line in lines)
        {
            if (line.Length == 0) continue;

            // Per-group color hint: # color R G B
            if (line.StartsWith("# color ", StringComparison.Ordinal))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 5)
                {
                    float.TryParse(parts[2], CultureInfo.InvariantCulture, out curR);
                    float.TryParse(parts[3], CultureInfo.InvariantCulture, out curG);
                    float.TryParse(parts[4], CultureInfo.InvariantCulture, out curB);
                }
                continue;
            }

            if (line[0] == '#') continue; // skip other comments

            if (line.StartsWith("v ", StringComparison.Ordinal))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4)
                {
                    positions.Add(new[]
                    {
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture),
                        float.Parse(parts[3], CultureInfo.InvariantCulture)
                    });
                }
            }
            else if (line.StartsWith("vn ", StringComparison.Ordinal))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4)
                {
                    normals.Add(new[]
                    {
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture),
                        float.Parse(parts[3], CultureInfo.InvariantCulture)
                    });
                }
            }
            else if (line.StartsWith("f ", StringComparison.Ordinal))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 4) continue;

                // Triangulate face (fan from first vertex for quads/polygons)
                faceVerts.Clear();
                string colorSuffix = string.Concat("|", curR.ToString("F3"), curG.ToString("F3"), curB.ToString("F3"));
                for (int i = 1; i < parts.Length; i++)
                {
                    string key = string.Concat(parts[i], colorSuffix);
                    if (!vertexMap.TryGetValue(key, out uint idx))
                    {
                        idx = (uint)(verts.Count / 9);
                        vertexMap[key] = idx;

                        ParseFaceVertex(parts[i], positions, normals, out var pos, out var nrm);
                        verts.Add(pos[0]); verts.Add(pos[1]); verts.Add(pos[2]);
                        verts.Add(nrm[0]); verts.Add(nrm[1]); verts.Add(nrm[2]);
                        verts.Add(curR); verts.Add(curG); verts.Add(curB);
                    }
                    faceVerts.Add(idx);
                }

                // Fan triangulation
                for (int i = 1; i < faceVerts.Count - 1; i++)
                {
                    inds.Add(faceVerts[0]);
                    inds.Add(faceVerts[i]);
                    inds.Add(faceVerts[i + 1]);
                }
            }
        }

        // If no normals were provided in the file, compute per-face normals
        if (normals.Count == 0)
        {
            RecalculateNormals(verts, inds);
        }

        return new MeshData(verts.ToArray(), inds.ToArray());
    }

    private static void ParseFaceVertex(string token, List<float[]> positions, List<float[]> normals,
        out float[] pos, out float[] nrm)
    {
        var parts = token.Split('/');
        int vi = int.Parse(parts[0], CultureInfo.InvariantCulture);
        // OBJ indices are 1-based, negative means relative
        vi = vi > 0 ? vi - 1 : positions.Count + vi;
        pos = positions[vi];

        nrm = new float[] { 0, 1, 0 }; // default up
        if (parts.Length >= 3 && parts[2].Length > 0)
        {
            int ni = int.Parse(parts[2], CultureInfo.InvariantCulture);
            ni = ni > 0 ? ni - 1 : normals.Count + ni;
            if (ni >= 0 && ni < normals.Count)
                nrm = normals[ni];
        }
    }

    /// <summary>
    /// Recompute flat normals from triangle faces when OBJ has no vn data.
    /// </summary>
    private static void RecalculateNormals(List<float> verts, List<uint> inds)
    {
        // Accumulate face normals per vertex
        var normalAccum = new float[verts.Count / 9 * 3];

        for (int i = 0; i < inds.Count; i += 3)
        {
            uint i0 = inds[i], i1 = inds[i + 1], i2 = inds[i + 2];
            int o0 = (int)i0 * 9, o1 = (int)i1 * 9, o2 = (int)i2 * 9;

            float ax = verts[o1] - verts[o0], ay = verts[o1 + 1] - verts[o0 + 1], az = verts[o1 + 2] - verts[o0 + 2];
            float bx = verts[o2] - verts[o0], by = verts[o2 + 1] - verts[o0 + 1], bz = verts[o2 + 2] - verts[o0 + 2];

            float nx = ay * bz - az * by;
            float ny = az * bx - ax * bz;
            float nz = ax * by - ay * bx;

            int no0 = (int)i0 * 3, no1 = (int)i1 * 3, no2 = (int)i2 * 3;
            normalAccum[no0] += nx; normalAccum[no0 + 1] += ny; normalAccum[no0 + 2] += nz;
            normalAccum[no1] += nx; normalAccum[no1 + 1] += ny; normalAccum[no1 + 2] += nz;
            normalAccum[no2] += nx; normalAccum[no2 + 1] += ny; normalAccum[no2 + 2] += nz;
        }

        // Normalize and write back into vertex data
        for (int v = 0; v < verts.Count / 9; v++)
        {
            int no = v * 3;
            float nx = normalAccum[no], ny = normalAccum[no + 1], nz = normalAccum[no + 2];
            float len = MathF.Sqrt(nx * nx + ny * ny + nz * nz);
            if (len > 0.0001f) { nx /= len; ny /= len; nz /= len; }
            else { nx = 0; ny = 1; nz = 0; }

            int vo = v * 9;
            verts[vo + 3] = nx;
            verts[vo + 4] = ny;
            verts[vo + 5] = nz;
        }
    }
}
