using CongCraft.Engine.Procedural;
using static CongCraft.Engine.Procedural.MeshLofter;

namespace CongCraft.Engine.Tests.Procedural;

public class MeshLofterTests
{
    [Fact]
    public void Loft_TwoRings_ProducesConnectedMesh()
    {
        var verts = new List<float>();
        var inds = new List<uint>();
        var rings = new Ring[]
        {
            R(0f, 1f, 0.5f, 0.5f, 0.5f),
            R(1f, 1f, 0.5f, 0.5f, 0.5f),
        };

        Loft(verts, inds, rings, 8);

        Assert.True(verts.Count > 0);
        Assert.True(inds.Count > 0);
        Assert.Equal(0, verts.Count % 9);
    }

    [Fact]
    public void Loft_IndicesWithinBounds()
    {
        var verts = new List<float>();
        var inds = new List<uint>();
        var rings = new Ring[]
        {
            R(0f, 0.5f, 0.8f, 0.2f, 0.1f),
            R(0.5f, 0.8f, 0.8f, 0.2f, 0.1f),
            R(1f, 0.6f, 0.8f, 0.2f, 0.1f),
            R(1.5f, 0.3f, 0.8f, 0.2f, 0.1f),
        };

        Loft(verts, inds, rings, 10);

        uint maxVertex = (uint)(verts.Count / 9);
        foreach (var idx in inds)
            Assert.True(idx < maxVertex, $"Index {idx} out of bounds (max {maxVertex})");
    }

    [Fact]
    public void Loft_MoreSegments_MoreVertices()
    {
        var rings = new Ring[]
        {
            R(0f, 1f, 0.5f, 0.5f, 0.5f),
            R(1f, 0.8f, 0.5f, 0.5f, 0.5f),
            R(2f, 0.5f, 0.5f, 0.5f, 0.5f),
        };

        var v4 = new List<float>();
        var i4 = new List<uint>();
        Loft(v4, i4, rings, 4);

        var v12 = new List<float>();
        var i12 = new List<uint>();
        Loft(v12, i12, rings, 12);

        Assert.True(v12.Count > v4.Count);
    }

    [Fact]
    public void Loft_SingleRing_ProducesNothing()
    {
        var verts = new List<float>();
        var inds = new List<uint>();
        Loft(verts, inds, new[] { R(0f, 1f, 0.5f, 0.5f, 0.5f) }, 8);

        Assert.Empty(verts);
        Assert.Empty(inds);
    }

    [Fact]
    public void RE_CreatesEllipticalRing()
    {
        var ring = RE(1f, 0.5f, 1.0f, 0.3f, 0.4f, 0.5f);
        Assert.Equal(0.5f, ring.RadiusX);
        Assert.Equal(1.0f, ring.RadiusZ);
        Assert.Equal(1f, ring.Y);
    }

    [Fact]
    public void LoftLimb_ProducesConnectedMesh()
    {
        var verts = new List<float>();
        var inds = new List<uint>();
        var rings = new Ring[]
        {
            R(0f, 0.2f, 0.7f, 0.5f, 0.3f),
            R(0.3f, 0.18f, 0.7f, 0.5f, 0.3f),
            R(0.6f, 0.15f, 0.7f, 0.5f, 0.3f),
            R(0.9f, 0.10f, 0.7f, 0.5f, 0.3f),
        };

        LoftLimb(verts, inds, 0.5f, 1.0f, 0f, rings, 8, angleX: 0.5f, angleZ: 0.3f);

        Assert.True(verts.Count > 0);
        Assert.Equal(0, verts.Count % 9);
        uint maxVertex = (uint)(verts.Count / 9);
        foreach (var idx in inds)
            Assert.True(idx < maxVertex, $"Limb index {idx} out of bounds (max {maxVertex})");
    }

    [Fact]
    public void LoftLimb_AngleAffectsPosition()
    {
        var rings = new Ring[]
        {
            R(0f, 0.1f, 0.5f, 0.5f, 0.5f),
            R(1f, 0.1f, 0.5f, 0.5f, 0.5f),
        };

        var v1 = new List<float>();
        var i1 = new List<uint>();
        LoftLimb(v1, i1, 0f, 0f, 0f, rings, 6, angleX: 0f, angleZ: 0f);

        var v2 = new List<float>();
        var i2 = new List<uint>();
        LoftLimb(v2, i2, 0f, 0f, 0f, rings, 6, angleX: 1.0f, angleZ: 0f);

        // Different angles should produce different vertex positions
        Assert.NotEqual(v1.ToArray(), v2.ToArray());
    }

    [Fact]
    public void AddSphere_ProducesValidMesh()
    {
        var verts = new List<float>();
        var inds = new List<uint>();

        AddSphere(verts, inds, 0f, 0f, 0f, 0.5f, 8, 0.5f, 0.5f, 0.5f);

        Assert.True(verts.Count > 0);
        Assert.Equal(0, verts.Count % 9);
        uint maxVertex = (uint)(verts.Count / 9);
        foreach (var idx in inds)
            Assert.True(idx < maxVertex);
    }

    [Fact]
    public void Loft_NormalsAreNormalized()
    {
        var verts = new List<float>();
        var inds = new List<uint>();
        var rings = new Ring[]
        {
            R(0f, 1f, 0.5f, 0.5f, 0.5f),
            R(0.5f, 1.2f, 0.5f, 0.5f, 0.5f),
            R(1f, 0.8f, 0.5f, 0.5f, 0.5f),
        };

        Loft(verts, inds, rings, 8, capBottom: false, capTop: false);

        int vertexCount = verts.Count / 9;
        for (int i = 0; i < vertexCount; i++)
        {
            float nx = verts[i * 9 + 3];
            float ny = verts[i * 9 + 4];
            float nz = verts[i * 9 + 5];
            float len = MathF.Sqrt(nx * nx + ny * ny + nz * nz);
            Assert.InRange(len, 0.95f, 1.05f); // approximately unit length
        }
    }
}
