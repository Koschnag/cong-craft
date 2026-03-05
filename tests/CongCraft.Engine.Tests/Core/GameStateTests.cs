using CongCraft.Engine.Core;

namespace CongCraft.Engine.Tests.Core;

public class GameStateTests
{
    [Fact]
    public void GameStateManager_DefaultMode_IsMainMenu()
    {
        var gs = new GameStateManager();
        Assert.Equal(GameMode.MainMenu, gs.CurrentMode);
    }

    [Fact]
    public void SetMode_ChangesCurrentMode()
    {
        var gs = new GameStateManager();
        gs.SetMode(GameMode.Playing);
        Assert.Equal(GameMode.Playing, gs.CurrentMode);
    }

    [Fact]
    public void SetMode_TracksPreviousMode()
    {
        var gs = new GameStateManager();
        gs.SetMode(GameMode.Playing);
        Assert.Equal(GameMode.MainMenu, gs.PreviousMode);
    }

    [Fact]
    public void SetMode_StartsTransition()
    {
        var gs = new GameStateManager();
        gs.SetMode(GameMode.Playing);
        Assert.True(gs.IsTransitioning);
        Assert.True(gs.TransitionTimer > 0);
    }

    [Fact]
    public void Update_ReducesTransitionTimer()
    {
        var gs = new GameStateManager();
        gs.SetMode(GameMode.Playing);
        float initial = gs.TransitionTimer;
        gs.Update(0.5f);
        Assert.True(gs.TransitionTimer < initial);
    }

    [Fact]
    public void Update_TransitionCompletes()
    {
        var gs = new GameStateManager();
        gs.TransitionDuration = 1.0f;
        gs.SetMode(GameMode.Playing);
        gs.Update(2.0f);
        Assert.False(gs.IsTransitioning);
        Assert.Equal(1f, gs.TransitionProgress);
    }

    [Fact]
    public void SetMode_MultipleTransitions()
    {
        var gs = new GameStateManager();
        gs.SetMode(GameMode.Playing);
        gs.Update(2f);
        gs.SetMode(GameMode.Paused);
        Assert.Equal(GameMode.Paused, gs.CurrentMode);
        Assert.Equal(GameMode.Playing, gs.PreviousMode);
    }
}
