using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Core;

/// <summary>
/// Global game state (Menu, Playing, Paused). Drives which systems update/render.
/// </summary>
public sealed class GameStateManager : IComponent
{
    public GameMode CurrentMode { get; private set; } = GameMode.MainMenu;
    public GameMode PreviousMode { get; private set; } = GameMode.MainMenu;
    public float TransitionTimer { get; set; }
    public float TransitionDuration { get; set; } = 1.0f;
    public bool IsTransitioning => TransitionTimer > 0;

    public void SetMode(GameMode mode)
    {
        PreviousMode = CurrentMode;
        CurrentMode = mode;
        TransitionTimer = TransitionDuration;
    }

    public void Update(float dt)
    {
        if (TransitionTimer > 0)
            TransitionTimer = MathF.Max(0, TransitionTimer - dt);
    }

    public float TransitionProgress => TransitionTimer > 0
        ? 1f - TransitionTimer / TransitionDuration
        : 1f;
}

public enum GameMode
{
    MainMenu,
    Playing,
    Paused
}
