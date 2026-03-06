using CongCraft.Engine.Procedural;

namespace CongCraft.Engine.Tests.Procedural;

public class ObjLoaderTests
{
    [Fact]
    public void Parse_SimpleTriangle_ProducesCorrectVertexCount()
    {
        string obj = @"
v 0.0 0.0 0.0
v 1.0 0.0 0.0
v 0.0 1.0 0.0
vn 0.0 0.0 1.0
f 1//1 2//1 3//1
";
        var data = ObjLoader.Parse(obj);
        Assert.Equal(3, data.VertexCount);
        Assert.Equal(3, data.Indices.Length);
        Assert.Equal(0, data.Vertices.Length % 9);
    }

    [Fact]
    public void Parse_Quad_TriangulatedToTwoTriangles()
    {
        string obj = @"
v 0.0 0.0 0.0
v 1.0 0.0 0.0
v 1.0 1.0 0.0
v 0.0 1.0 0.0
vn 0.0 0.0 1.0
f 1//1 2//1 3//1 4//1
";
        var data = ObjLoader.Parse(obj);
        Assert.Equal(6, data.Indices.Length); // 2 triangles = 6 indices
    }

    [Fact]
    public void Parse_IndicesWithinBounds()
    {
        string obj = @"
v -1.0 0.0 0.0
v 1.0 0.0 0.0
v 0.0 1.5 0.0
v 0.0 0.0 1.0
vn 0.0 1.0 0.0
vn 0.0 0.0 1.0
f 1//1 2//1 3//1
f 1//2 2//2 4//2
";
        var data = ObjLoader.Parse(obj);
        int vertCount = data.VertexCount;
        foreach (uint idx in data.Indices)
        {
            Assert.True(idx < (uint)vertCount, $"Index {idx} out of bounds (vertCount={vertCount})");
        }
    }

    [Fact]
    public void Parse_WithColorComment_AppliesVertexColor()
    {
        string obj = @"
v 0.0 0.0 0.0
v 1.0 0.0 0.0
v 0.0 1.0 0.0
vn 0.0 0.0 1.0
# color 0.80 0.20 0.10
f 1//1 2//1 3//1
";
        var data = ObjLoader.Parse(obj);
        // First vertex color at offset 6,7,8
        Assert.Equal(0.80f, data.Vertices[6], 2);
        Assert.Equal(0.20f, data.Vertices[7], 2);
        Assert.Equal(0.10f, data.Vertices[8], 2);
    }

    [Fact]
    public void Parse_NoNormals_RecalculatesNormals()
    {
        string obj = @"
v 0.0 0.0 0.0
v 1.0 0.0 0.0
v 0.0 0.0 1.0
f 1 2 3
";
        var data = ObjLoader.Parse(obj);
        Assert.Equal(3, data.VertexCount);
        // Normal should point up (Y-axis) for XZ triangle
        float ny = data.Vertices[4]; // second float of normal
        Assert.True(MathF.Abs(ny) > 0.9f, $"Expected Y-normal near ±1, got {ny}");
    }

    [Fact]
    public void RoundTrip_ExportThenReimport_PreservesVertexCount()
    {
        // Generate a simple mesh
        var original = RockMeshBuilder.GenerateData(new RockMeshBuilder.RockParams(Subdivisions: 1));

        // Export to OBJ string
        string tempDir = Path.Combine(Path.GetTempPath(), "congcraft_test_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);
        string path = Path.Combine(tempDir, "test_rock.obj");

        try
        {
            ObjExporter.Export(path, original, "test_rock");
            Assert.True(File.Exists(path));

            var reimported = ObjLoader.Load(path);
            Assert.True(reimported.VertexCount > 0);
            Assert.True(reimported.Indices.Length > 0);
            Assert.Equal(0, reimported.Vertices.Length % 9);

            // All indices within bounds
            foreach (uint idx in reimported.Indices)
            {
                Assert.True(idx < (uint)reimported.VertexCount);
            }
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
