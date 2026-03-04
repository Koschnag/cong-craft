using CongCraft.Engine.Procedural;

namespace CongCraft.Engine.Tests.Procedural;

public class TreeMeshBuilderTests
{
    [Fact]
    public void GenerateData_ProducesVertices()
    {
        var data = TreeMeshBuilder.GenerateData();
        Assert.True(data.Vertices.Length > 0);
        Assert.True(data.Indices.Length > 0);
    }

    [Fact]
    public void GenerateData_VertexCount_MultiplesOfNine()
    {
        var data = TreeMeshBuilder.GenerateData();
        Assert.Equal(0, data.Vertices.Length % 9); // 9 floats per vertex
    }

    [Fact]
    public void GenerateData_CustomParams_AffectsOutput()
    {
        var small = TreeMeshBuilder.GenerateData(new TreeMeshBuilder.TreeParams(Segments: 4));
        var large = TreeMeshBuilder.GenerateData(new TreeMeshBuilder.TreeParams(Segments: 12));
        Assert.True(large.VertexCount > small.VertexCount);
    }

    [Fact]
    public void GenerateData_IndicesAreValid()
    {
        var data = TreeMeshBuilder.GenerateData();
        uint maxIndex = (uint)(data.VertexCount - 1);
        foreach (var idx in data.Indices)
        {
            Assert.InRange(idx, 0u, maxIndex);
        }
    }
}
