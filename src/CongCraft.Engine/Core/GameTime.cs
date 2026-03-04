namespace CongCraft.Engine.Core;

/// <summary>
/// Immutable snapshot of timing information for the current frame.
/// </summary>
public readonly record struct GameTime(double DeltaTime, double TotalTime, long FrameCount)
{
    public float DeltaTimeF => (float)DeltaTime;
    public float TotalTimeF => (float)TotalTime;
}
