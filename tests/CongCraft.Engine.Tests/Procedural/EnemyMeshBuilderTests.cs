using CongCraft.Engine.Procedural;

namespace CongCraft.Engine.Tests.Procedural;

public class EnemyMeshBuilderTests
{
    [Fact]
    public void GenerateData_HasVertices()
    {
        var data = EnemyMeshBuilder.GenerateData();
        Assert.True(data.Vertices.Length > 0, "Enemy mesh should have vertices");
    }

    [Fact]
    public void GenerateData_HasIndices()
    {
        var data = EnemyMeshBuilder.GenerateData();
        Assert.True(data.Indices.Length > 0, "Enemy mesh should have indices");
    }

    [Fact]
    public void GenerateData_VertexCountDivisibleByStride()
    {
        var data = EnemyMeshBuilder.GenerateData();
        Assert.Equal(0, data.Vertices.Length % 9);
    }

    [Fact]
    public void GenerateData_HasSubstantialGeometry()
    {
        var data = EnemyMeshBuilder.GenerateData();
        // Smooth mesh with capsules, spheres, and cylinders has significantly
        // more vertices than the old box-based approach
        Assert.True(data.VertexCount > 100, "Smooth enemy mesh should have substantial geometry");
    }

    [Fact]
    public void GenerateData_IndicesWithinBounds()
    {
        var data = EnemyMeshBuilder.GenerateData();
        uint maxVertex = (uint)(data.Vertices.Length / 9);
        foreach (var idx in data.Indices)
        {
            Assert.True(idx < maxVertex, $"Index {idx} out of bounds (max {maxVertex})");
        }
    }

    [Fact]
    public void GenerateSkeletonData_HasSubstantialGeometry()
    {
        var data = EnemyMeshBuilder.GenerateSkeletonData();
        Assert.True(data.VertexCount > 100, "Skeleton mesh should have substantial geometry");
    }

    [Fact]
    public void GenerateSkeletonData_VertexCountDivisibleByStride()
    {
        var data = EnemyMeshBuilder.GenerateSkeletonData();
        Assert.Equal(0, data.Vertices.Length % 9);
    }

    [Fact]
    public void GenerateSkeletonData_IndicesWithinBounds()
    {
        var data = EnemyMeshBuilder.GenerateSkeletonData();
        uint maxVertex = (uint)(data.Vertices.Length / 9);
        foreach (var idx in data.Indices)
        {
            Assert.True(idx < maxVertex, $"Skeleton index {idx} out of bounds (max {maxVertex})");
        }
    }

    [Fact]
    public void GenerateSkeletonData_BoneWhiteColors()
    {
        var data = EnemyMeshBuilder.GenerateSkeletonData();
        // Average color should be bright (bone-white), not dark (armor)
        float totalR = 0, totalG = 0, totalB = 0;
        int vertexCount = data.Vertices.Length / 9;
        for (int i = 0; i < vertexCount; i++)
        {
            totalR += data.Vertices[i * 9 + 6];
            totalG += data.Vertices[i * 9 + 7];
            totalB += data.Vertices[i * 9 + 8];
        }
        float avgR = totalR / vertexCount;
        float avgG = totalG / vertexCount;
        // Skeleton should be brighter overall than the dark armor enemy
        Assert.True(avgR > 0.4f, $"Skeleton should be bright (avg R={avgR:F2})");
        Assert.True(avgG > 0.35f, $"Skeleton should be bright (avg G={avgG:F2})");
    }
}
