using CongCraft.Engine.Procedural;

namespace CongCraft.Engine.Tests.Procedural;

public class HighResPlayerMeshBuilderTests
{
    [Fact]
    public void GenerateData_HasSubstantialGeometry()
    {
        var data = HighResPlayerMeshBuilder.GenerateData();
        // High-res should have significantly more geometry than low-res (>100 verts)
        Assert.True(data.VertexCount > 500, $"High-res player mesh should have substantial geometry, got {data.VertexCount}");
    }

    [Fact]
    public void GenerateData_HasMoreDetailThanLowRes()
    {
        var hiRes = HighResPlayerMeshBuilder.GenerateData();
        var loRes = PlayerMeshBuilder.GenerateData();
        Assert.True(hiRes.VertexCount > loRes.VertexCount,
            $"High-res ({hiRes.VertexCount}) should have more vertices than low-res ({loRes.VertexCount})");
    }

    [Fact]
    public void GenerateData_VertexCountDivisibleByStride()
    {
        var data = HighResPlayerMeshBuilder.GenerateData();
        Assert.Equal(0, data.Vertices.Length % 9);
    }

    [Fact]
    public void GenerateData_IndicesWithinBounds()
    {
        var data = HighResPlayerMeshBuilder.GenerateData();
        uint maxVertex = (uint)(data.Vertices.Length / 9);
        foreach (var idx in data.Indices)
        {
            Assert.True(idx < maxVertex, $"Index {idx} out of bounds (max {maxVertex})");
        }
    }

    [Fact]
    public void GenerateData_HasValidNormals()
    {
        var data = HighResPlayerMeshBuilder.GenerateData();
        int vertexCount = data.Vertices.Length / 9;
        for (int i = 0; i < vertexCount; i++)
        {
            float nx = data.Vertices[i * 9 + 3];
            float ny = data.Vertices[i * 9 + 4];
            float nz = data.Vertices[i * 9 + 5];
            float lenSq = nx * nx + ny * ny + nz * nz;
            // Normals should be roughly unit length (allow some tolerance)
            Assert.True(lenSq > 0.5f && lenSq < 2.0f,
                $"Normal at vertex {i} has invalid length squared: {lenSq:F3}");
        }
    }

    [Fact]
    public void GenerateData_ColorsInValidRange()
    {
        var data = HighResPlayerMeshBuilder.GenerateData();
        int vertexCount = data.Vertices.Length / 9;
        for (int i = 0; i < vertexCount; i++)
        {
            float r = data.Vertices[i * 9 + 6];
            float g = data.Vertices[i * 9 + 7];
            float b = data.Vertices[i * 9 + 8];
            Assert.True(r >= 0f && r <= 1f, $"Red channel out of range at vertex {i}: {r}");
            Assert.True(g >= 0f && g <= 1f, $"Green channel out of range at vertex {i}: {g}");
            Assert.True(b >= 0f && b <= 1f, $"Blue channel out of range at vertex {i}: {b}");
        }
    }

    [Fact]
    public void GenerateData_TriangleCountDivisibleByThree()
    {
        var data = HighResPlayerMeshBuilder.GenerateData();
        Assert.Equal(0, data.Indices.Length % 3);
    }
}
