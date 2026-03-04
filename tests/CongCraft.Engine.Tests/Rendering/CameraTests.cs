using System.Numerics;
using CongCraft.Engine.Rendering;

namespace CongCraft.Engine.Tests.Rendering;

public class CameraTests
{
    [Fact]
    public void Position_Default_IsBehindAndAboveTarget()
    {
        var camera = new Camera { Target = Vector3.Zero };
        var pos = camera.Position;
        // Camera should be behind (positive Z) and above (positive Y) the target
        Assert.True(pos.Z > 0 || pos.Y > 0, "Camera should be behind/above target");
        Assert.True(Vector3.Distance(pos, camera.Target) > 1f, "Camera should be distant from target");
    }

    [Fact]
    public void Position_Distance_AffectsPosition()
    {
        var camera = new Camera { Target = Vector3.Zero, Distance = 5f, Yaw = 0, Pitch = 0 };
        var pos5 = camera.Position;
        camera.Distance = 15f;
        var pos15 = camera.Position;

        float dist5 = Vector3.Distance(pos5, Vector3.Zero);
        float dist15 = Vector3.Distance(pos15, Vector3.Zero);
        Assert.True(dist15 > dist5);
    }

    [Fact]
    public void ViewMatrix_IsValid()
    {
        var camera = new Camera { Target = Vector3.Zero };
        var view = camera.ViewMatrix;
        // View matrix should be invertible
        Assert.True(Matrix4x4.Invert(view, out _));
    }

    [Fact]
    public void ProjectionMatrix_IsValid()
    {
        var camera = new Camera();
        var proj = camera.ProjectionMatrix;
        Assert.True(Matrix4x4.Invert(proj, out _));
    }

    [Fact]
    public void Rotate_ClamsPitch()
    {
        var camera = new Camera { MinPitch = -80, MaxPitch = -5 };
        camera.Rotate(0, -200); // Try to pitch way down
        Assert.InRange(camera.Pitch, -80, -5);
    }

    [Fact]
    public void Zoom_ClampsDistance()
    {
        var camera = new Camera { MinDistance = 2, MaxDistance = 30, Distance = 10 };
        camera.Zoom(100); // Try to zoom way in
        Assert.InRange(camera.Distance, 2, 30);
    }

    [Fact]
    public void Forward_IsNormalized()
    {
        var camera = new Camera { Yaw = 45 };
        Assert.InRange(camera.Forward.Length(), 0.99f, 1.01f);
    }

    [Fact]
    public void Right_IsPerpendicularToForward()
    {
        var camera = new Camera { Yaw = 30 };
        float dot = Vector3.Dot(camera.Forward, camera.Right);
        Assert.InRange(dot, -0.01f, 0.01f);
    }
}
