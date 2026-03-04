using System.Numerics;

namespace CongCraft.Engine.Rendering;

/// <summary>
/// Third-person RPG camera that orbits around a target (the player).
/// </summary>
public sealed class Camera
{
    private const float Deg2Rad = MathF.PI / 180f;

    public Vector3 Target { get; set; }
    public float Distance { get; set; } = 8f;
    public float Yaw { get; set; }
    public float Pitch { get; set; } = -25f;
    public float Fov { get; set; } = 60f;
    public float AspectRatio { get; set; } = 16f / 9f;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 500f;

    public float MinDistance { get; set; } = 2f;
    public float MaxDistance { get; set; } = 30f;
    public float MinPitch { get; set; } = -80f;
    public float MaxPitch { get; set; } = -5f;

    public Vector3 Position
    {
        get
        {
            float pitchRad = Pitch * Deg2Rad;
            float yawRad = Yaw * Deg2Rad;
            return Target + new Vector3(
                Distance * MathF.Cos(pitchRad) * MathF.Sin(yawRad),
                -Distance * MathF.Sin(pitchRad),
                Distance * MathF.Cos(pitchRad) * MathF.Cos(yawRad)
            );
        }
    }

    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Position, Target, Vector3.UnitY);

    public Matrix4x4 ProjectionMatrix =>
        Matrix4x4.CreatePerspectiveFieldOfView(Fov * Deg2Rad, AspectRatio, NearPlane, FarPlane);

    /// <summary>
    /// Forward direction on the XZ plane (for player movement relative to camera).
    /// </summary>
    public Vector3 Forward
    {
        get
        {
            float yawRad = Yaw * Deg2Rad;
            return Vector3.Normalize(new Vector3(-MathF.Sin(yawRad), 0, -MathF.Cos(yawRad)));
        }
    }

    public Vector3 Right => Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));

    public void Rotate(float deltaYaw, float deltaPitch)
    {
        Yaw += deltaYaw;
        Pitch = Math.Clamp(Pitch + deltaPitch, MinPitch, MaxPitch);
    }

    public void Zoom(float delta)
    {
        Distance = Math.Clamp(Distance - delta, MinDistance, MaxDistance);
    }
}
