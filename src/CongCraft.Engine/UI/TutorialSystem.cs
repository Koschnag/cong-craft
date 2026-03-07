using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Input;
using CongCraft.Engine.Procedural;
using CongCraft.Engine.Rendering;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Shader = CongCraft.Engine.Rendering.Shader;

namespace CongCraft.Engine.UI;

/// <summary>
/// Step-by-step tutorial overlay that guides the player through controls and game mechanics.
/// SpellForce 1-style parchment panels with gold trim.
/// Progresses automatically when the player performs the required action.
/// </summary>
public sealed class TutorialSystem : ISystem
{
    public int Priority => 108; // Before MenuSystem (109) and HudSystem (110)

    private GL _gl = null!;
    private World _world = null!;
    private IWindow _window = null!;
    private Shader _hudShader = null!;
    private Shader _texShader = null!;
    private Mesh _quad = null!;
    private BitmapFont _font = null!;
    private TextRenderer _textRenderer = null!;
    private UITextureAtlas _textures = null!;

    private TutorialStep _currentStep = TutorialStep.Welcome;
    private float _stepTimer;
    private float _totalPlayTime;
    private bool _tutorialComplete;
    private bool _wasPlaying;

    // Track player actions for progression
    private bool _hasMoved;
    private bool _hasLookedAround;
    private bool _hasRun;
    private bool _hasAttacked;
    private bool _hasBlocked;
    private bool _hasDodged;
    private bool _hasOpenedInventory;
    private bool _hasTalkedToNpc;
    private bool _hasCastSpell;
    private float _moveDistance;
    private float _lookAngle;
    private Vector3 _lastPlayerPos;

    // SpellForce 1-style color palette
    private static readonly Vector4 GoldColor = new(0.88f, 0.65f, 0.18f, 0.95f);
    private static readonly Vector4 DarkGold = new(0.58f, 0.40f, 0.10f, 0.90f);
    private static readonly Vector4 ParchmentBg = new(0.30f, 0.26f, 0.22f, 0.92f);
    private static readonly Vector4 CreamText = new(0.93f, 0.88f, 0.72f, 1.0f);
    private static readonly Vector4 DimText = new(0.65f, 0.58f, 0.44f, 0.8f);
    private static readonly Vector4 HighlightText = new(1.0f, 0.90f, 0.50f, 1.0f);

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _world = services.Get<World>();
        _window = services.Get<IWindow>();
        _hudShader = new Shader(_gl, ShaderSources.HudVertex, ShaderSources.HudFragment);
        _texShader = new Shader(_gl, ShaderSources.HudTexturedVertex, ShaderSources.HudTexturedFragment);
        _quad = PrimitiveMeshBuilder.CreateUnitQuad(_gl);
        _font = new BitmapFont(_gl);
        _textRenderer = new TextRenderer(_gl, _font);
        _textures = new UITextureAtlas(_gl);
    }

    public void Update(GameTime time)
    {
        if (_tutorialComplete) return;

        // Only active during gameplay
        if (!_world.TryGetSingleton<GameStateManager>(out var gs) || gs == null)
            return;

        if (gs.CurrentMode != GameMode.Playing)
        {
            _wasPlaying = false;
            return;
        }

        // Detect transition from menu to playing (game just started)
        if (!_wasPlaying)
        {
            _wasPlaying = true;
            _stepTimer = 0f;
            _currentStep = TutorialStep.Welcome;
        }

        float dt = time.DeltaTimeF;
        _stepTimer += dt;
        _totalPlayTime += dt;

        var input = _world.GetSingleton<InputState>();
        TrackPlayerActions(input, dt);
        AdvanceTutorial();
    }

    private void TrackPlayerActions(InputState input, float dt)
    {
        // Track movement
        bool isMoving = input.IsKeyDown(Key.W) || input.IsKeyDown(Key.A) ||
                        input.IsKeyDown(Key.S) || input.IsKeyDown(Key.D);

        foreach (var (entity, player, transform) in _world.Query<PlayerComponent, TransformComponent>())
        {
            if (_lastPlayerPos != Vector3.Zero)
            {
                float dist = Vector3.Distance(transform.Position, _lastPlayerPos);
                if (dist > 0.01f && dist < 10f) // Sanity check
                    _moveDistance += dist;
            }
            _lastPlayerPos = transform.Position;
        }

        if (_moveDistance > 3f) _hasMoved = true;

        // Track camera look
        if (MathF.Abs(input.MouseDelta.X) > 0.5f || MathF.Abs(input.MouseDelta.Y) > 0.5f)
            _lookAngle += MathF.Abs(input.MouseDelta.X) + MathF.Abs(input.MouseDelta.Y);
        if (_lookAngle > 100f) _hasLookedAround = true;

        // Track running
        if (isMoving && input.IsKeyDown(Key.ShiftLeft))
            _hasRun = true;

        // Track combat
        if (input.IsMouseButtonPressed(MouseButton.Left))
            _hasAttacked = true;
        if (input.IsMouseButtonDown(MouseButton.Right))
            _hasBlocked = true;
        if (input.IsKeyPressed(Key.Space))
            _hasDodged = true;

        // Track spell casting
        if (input.IsKeyPressed(Key.Q))
            _hasCastSpell = true;

        // Track inventory
        if (input.IsKeyPressed(Key.I) || input.IsKeyPressed(Key.Tab))
            _hasOpenedInventory = true;

        // Track NPC talking
        if (input.IsKeyPressed(Key.T))
            _hasTalkedToNpc = true;
    }

    private void AdvanceTutorial()
    {
        switch (_currentStep)
        {
            case TutorialStep.Welcome:
                if (_stepTimer > 4f) NextStep();
                break;
            case TutorialStep.Movement:
                if (_hasMoved) NextStep();
                else if (_stepTimer > 20f) NextStep(); // Auto skip after timeout
                break;
            case TutorialStep.Camera:
                if (_hasLookedAround) NextStep();
                else if (_stepTimer > 15f) NextStep();
                break;
            case TutorialStep.Running:
                if (_hasRun) NextStep();
                else if (_stepTimer > 12f) NextStep();
                break;
            case TutorialStep.Combat:
                if (_hasAttacked) NextStep();
                else if (_stepTimer > 15f) NextStep();
                break;
            case TutorialStep.Blocking:
                if (_hasBlocked) NextStep();
                else if (_stepTimer > 12f) NextStep();
                break;
            case TutorialStep.Dodging:
                if (_hasDodged) NextStep();
                else if (_stepTimer > 12f) NextStep();
                break;
            case TutorialStep.Spells:
                if (_hasCastSpell) NextStep();
                else if (_stepTimer > 12f) NextStep();
                break;
            case TutorialStep.Inventory:
                if (_hasOpenedInventory) NextStep();
                else if (_stepTimer > 12f) NextStep();
                break;
            case TutorialStep.TalkToNpc:
                if (_hasTalkedToNpc) NextStep();
                else if (_stepTimer > 12f) NextStep();
                break;
            case TutorialStep.QuestHint:
                if (_stepTimer > 6f) NextStep();
                break;
            case TutorialStep.Complete:
                if (_stepTimer > 4f)
                    _tutorialComplete = true;
                break;
        }
    }

    private void NextStep()
    {
        _currentStep++;
        _stepTimer = 0f;
    }

    public void Render(GameTime time)
    {
        if (_tutorialComplete) return;

        if (!_world.TryGetSingleton<GameStateManager>(out var gs) || gs == null)
            return;
        if (gs.CurrentMode != GameMode.Playing) return;

        _gl.Disable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        int w = _window.Size.X, h = _window.Size.Y;
        var ortho = Matrix4x4.CreateOrthographicOffCenter(0, w, 0, h, -1, 1);

        // Fade-in/out effect for step transitions
        float fadeIn = MathF.Min(1f, _stepTimer * 2f);
        float alpha = fadeIn;

        // Get tutorial text
        var (title, hint, keyHint) = GetStepContent();

        if (_currentStep == TutorialStep.Welcome)
            RenderWelcomeOverlay(w, h, ortho, alpha);
        else
            RenderTutorialHint(w, h, ortho, title, hint, keyHint, alpha);

        _gl.Enable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.Blend);
    }

    private void RenderWelcomeOverlay(int w, int h, Matrix4x4 ortho, float alpha)
    {
        // Dark vignette
        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);
        float vigAlpha = 0.4f * alpha;
        DrawRect(new HudElement(new Vector2(0, 0), new Vector2(w, h),
            new Vector4(0, 0, 0, vigAlpha)));

        // Center parchment panel
        float panelW = MathF.Min(520, w - 60);
        float panelH = 200f;
        float panelX = (w - panelW) / 2f;
        float panelY = (h - panelH) / 2f + 50f;

        DrawOrnatePanel(panelX, panelY, panelW, panelH, ortho, alpha);

        // Title
        string title = "Welcome to Ashvale";
        float titleScale = 3.5f;
        float titleW = TextRenderer.MeasureWidth(title, titleScale);
        _textRenderer.DrawText(title, (w - titleW) / 2f, panelY + panelH - 50, titleScale,
            GoldColor with { W = alpha }, ortho);

        // Divider
        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);
        DrawRect(new HudElement(new Vector2(panelX + 30, panelY + panelH - 60),
            new Vector2(panelW - 60, 2), DarkGold with { W = alpha }));

        // Description
        string desc = "A frontier settlement at the edge\nof dark wilderness.";
        _textRenderer.DrawText(desc, panelX + 40, panelY + panelH - 95, 2.0f,
            CreamText with { W = alpha * 0.9f }, ortho);

        string hint = "Your adventure begins now...";
        float hintW = TextRenderer.MeasureWidth(hint, 1.5f);
        _textRenderer.DrawText(hint, (w - hintW) / 2f, panelY + 20, 1.5f,
            DimText with { W = alpha * 0.7f }, ortho);
    }

    private void RenderTutorialHint(int w, int h, Matrix4x4 ortho,
        string title, string hint, string keyHint, float alpha)
    {
        // Top-center tutorial bar
        float panelW = MathF.Min(480, w - 80);
        float panelH = 80f;
        float panelX = (w - panelW) / 2f;
        float panelY = h - panelH - 15;

        DrawOrnatePanel(panelX, panelY, panelW, panelH, ortho, alpha);

        // Title (gold, left-aligned)
        _textRenderer.DrawText(title, panelX + 15, panelY + panelH - 25, 2.0f,
            GoldColor with { W = alpha }, ortho);

        // Hint text (cream)
        _textRenderer.DrawText(hint, panelX + 15, panelY + panelH - 50, 1.5f,
            CreamText with { W = alpha * 0.9f }, ortho);

        // Key hint (highlighted, right-aligned)
        float keyW = TextRenderer.MeasureWidth(keyHint, 2.0f);
        float pulse = 0.7f + MathF.Sin(_stepTimer * 3f) * 0.3f;
        _textRenderer.DrawText(keyHint, panelX + panelW - keyW - 15, panelY + 10, 2.0f,
            HighlightText with { W = alpha * pulse }, ortho);

        // Progress dots (bottom of panel)
        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);
        int totalSteps = (int)TutorialStep.Complete + 1;
        int currentIdx = (int)_currentStep;
        float dotSize = 6f;
        float dotGap = 12f;
        float dotsW = totalSteps * dotSize + (totalSteps - 1) * (dotGap - dotSize);
        float dotStartX = panelX + (panelW - dotsW) / 2f;

        for (int i = 0; i < totalSteps; i++)
        {
            float dx = dotStartX + i * dotGap;
            var dotColor = i <= currentIdx
                ? GoldColor with { W = alpha * 0.9f }
                : new Vector4(0.3f, 0.25f, 0.18f, alpha * 0.4f);
            DrawRect(new HudElement(new Vector2(dx, panelY + 5), new Vector2(dotSize, dotSize), dotColor));
        }
    }

    private (string title, string hint, string keyHint) GetStepContent()
    {
        return _currentStep switch
        {
            TutorialStep.Welcome => ("", "", ""),
            TutorialStep.Movement => ("Movement", "Walk around to explore.", "W A S D"),
            TutorialStep.Camera => ("Camera", "Look around with the mouse.", "Mouse"),
            TutorialStep.Running => ("Sprint", "Hold Shift while moving to run.", "Shift"),
            TutorialStep.Combat => ("Attack", "Left-click to swing your weapon.", "LMB"),
            TutorialStep.Blocking => ("Block", "Hold right-click to raise your guard.", "RMB"),
            TutorialStep.Dodging => ("Dodge", "Press Space to dodge and evade.", "Space"),
            TutorialStep.Spells => ("Magic", "Press Q to cast a spell.", "Q"),
            TutorialStep.Inventory => ("Inventory", "Open your bag to see your gear.", "I / Tab"),
            TutorialStep.TalkToNpc => ("Speak", "Approach a villager and talk.", "T"),
            TutorialStep.QuestHint => ("Quests", "Follow the quest tracker (top-left).", "J = Journal"),
            TutorialStep.Complete => ("Ready!", "You are prepared. Go forth!", ""),
            _ => ("", "", "")
        };
    }

    private void DrawOrnatePanel(float x, float y, float pw, float ph, Matrix4x4 ortho, float alpha)
    {
        // Parchment background
        DrawTexturedRect(x, y, pw, ph, _textures.Parchment,
            ParchmentBg with { W = ParchmentBg.W * alpha }, ortho);

        // Gold border
        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);
        var gold = GoldColor with { W = GoldColor.W * alpha };
        var darkG = DarkGold with { W = DarkGold.W * alpha };

        DrawRect(new HudElement(new Vector2(x, y + ph - 3), new Vector2(pw, 3), gold));
        DrawRect(new HudElement(new Vector2(x, y), new Vector2(pw, 3), gold));
        DrawRect(new HudElement(new Vector2(x, y), new Vector2(3, ph), gold));
        DrawRect(new HudElement(new Vector2(x + pw - 3, y), new Vector2(3, ph), gold));

        // Corner ornaments
        float cs = 8f;
        DrawRect(new HudElement(new Vector2(x - 1, y + ph - cs + 1), new Vector2(cs, cs), darkG));
        DrawRect(new HudElement(new Vector2(x + pw - cs + 1, y + ph - cs + 1), new Vector2(cs, cs), darkG));
        DrawRect(new HudElement(new Vector2(x - 1, y - 1), new Vector2(cs, cs), darkG));
        DrawRect(new HudElement(new Vector2(x + pw - cs + 1, y - 1), new Vector2(cs, cs), darkG));

        float ics = 4f;
        DrawRect(new HudElement(new Vector2(x + 1, y + ph - ics - 1), new Vector2(ics, ics), gold));
        DrawRect(new HudElement(new Vector2(x + pw - ics - 1, y + ph - ics - 1), new Vector2(ics, ics), gold));
        DrawRect(new HudElement(new Vector2(x + 1, y + 1), new Vector2(ics, ics), gold));
        DrawRect(new HudElement(new Vector2(x + pw - ics - 1, y + 1), new Vector2(ics, ics), gold));
    }

    private void DrawTexturedRect(float x, float y, float rw, float rh, uint texId, Vector4 tint, Matrix4x4 ortho)
    {
        _texShader.Use();
        _texShader.SetUniform("uProjection", ortho);
        _texShader.SetUniform("uRect", new Vector4(x, y, rw, rh));
        _texShader.SetUniform("uColor", tint);
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, texId);
        _texShader.SetUniform("uTexture", 0);
        _quad.Draw();
    }

    private void DrawRect(HudElement element)
    {
        _hudShader.SetUniform("uRect", new Vector4(
            element.Position.X, element.Position.Y,
            element.Size.X, element.Size.Y));
        _hudShader.SetUniform("uColor", element.Color);
        _quad.Draw();
    }

    public void Dispose()
    {
        _hudShader.Dispose();
        _texShader.Dispose();
        _quad.Dispose();
        _font.Dispose();
        _textRenderer.Dispose();
        _textures.Dispose();
    }
}

public enum TutorialStep
{
    Welcome,
    Movement,
    Camera,
    Running,
    Combat,
    Blocking,
    Dodging,
    Spells,
    Inventory,
    TalkToNpc,
    QuestHint,
    Complete
}
