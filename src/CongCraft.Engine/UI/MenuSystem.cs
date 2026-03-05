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
/// SpellForce 1-inspired main menu and pause menu system.
/// Features: atmospheric background, ornate gold frame, particle embers, medieval font.
/// </summary>
public sealed class MenuSystem : ISystem
{
    public int Priority => 109; // Before HudSystem

    private GL _gl = null!;
    private World _world = null!;
    private IWindow _window = null!;
    private Shader _hudShader = null!;
    private Shader _texShader = null!;
    private Mesh _quad = null!;
    private BitmapFont _font = null!;
    private TextRenderer _textRenderer = null!;
    private UITextureAtlas _textures = null!;
    private GameStateManager _gameState = null!;
    private InputState? _inputState;

    private int _selectedItem;
    private float _animTimer;
    private bool _escWasPressed;

    // Menu ember particles (simple inline implementation)
    private readonly MenuParticle[] _embers = new MenuParticle[40];
    private readonly Random _rng = new(42);

    // Color palette (matching HudSystem)
    private static readonly Vector4 GoldColor = new(0.85f, 0.68f, 0.22f, 0.95f);
    private static readonly Vector4 DarkGold = new(0.60f, 0.45f, 0.15f, 0.90f);
    private static readonly Vector4 ParchmentTint = new(0.92f, 0.85f, 0.70f, 0.92f);
    private static readonly Vector4 CreamText = new(0.95f, 0.90f, 0.75f, 1.0f);
    private static readonly Vector4 DimText = new(0.65f, 0.58f, 0.45f, 0.8f);

    private struct MenuParticle
    {
        public float X, Y, Vx, Vy;
        public float Life, MaxLife;
        public float Size;
        public float Brightness;
    }

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

        // Initialize game state
        _gameState = new GameStateManager();
        _world.SetSingleton(_gameState);

        // Initialize ember particles
        for (int i = 0; i < _embers.Length; i++)
            RespawnEmber(ref _embers[i], true);
    }

    public void Update(GameTime time)
    {
        _gameState.Update(time.DeltaTimeF);
        _animTimer += time.DeltaTimeF;

        // Get input
        if (_world.TryGetSingleton<InputState>(out _inputState) && _inputState != null)
        {
            bool escNow = _inputState.KeysPressed.Contains(Key.Escape);

            if (escNow && !_escWasPressed)
            {
                switch (_gameState.CurrentMode)
                {
                    case GameMode.MainMenu:
                        // Esc in main menu does nothing (or could quit)
                        break;
                    case GameMode.Playing:
                        _gameState.SetMode(GameMode.Paused);
                        _selectedItem = 0;
                        _inputState.IsMouseCaptured = false;
                        break;
                    case GameMode.Paused:
                        _gameState.SetMode(GameMode.Playing);
                        _inputState.IsMouseCaptured = true;
                        break;
                }
            }
            _escWasPressed = escNow;

            // Menu navigation
            if (_gameState.CurrentMode == GameMode.MainMenu || _gameState.CurrentMode == GameMode.Paused)
            {
                int itemCount = _gameState.CurrentMode == GameMode.MainMenu ? 2 : 3;

                if (_inputState.KeysPressed.Contains(Key.Up) || _inputState.KeysPressed.Contains(Key.W))
                    _selectedItem = (_selectedItem - 1 + itemCount) % itemCount;
                if (_inputState.KeysPressed.Contains(Key.Down) || _inputState.KeysPressed.Contains(Key.S))
                    _selectedItem = (_selectedItem + 1) % itemCount;

                if (_inputState.KeysPressed.Contains(Key.Enter) || _inputState.KeysPressed.Contains(Key.Space))
                {
                    HandleMenuSelect();
                }
            }
        }

        // Update embers
        if (_gameState.CurrentMode == GameMode.MainMenu || _gameState.CurrentMode == GameMode.Paused)
        {
            for (int i = 0; i < _embers.Length; i++)
            {
                ref var e = ref _embers[i];
                e.Life -= time.DeltaTimeF;
                if (e.Life <= 0)
                {
                    RespawnEmber(ref e, false);
                    continue;
                }
                e.X += e.Vx * time.DeltaTimeF;
                e.Y += e.Vy * time.DeltaTimeF;
                e.Vx += (float)(_rng.NextDouble() - 0.5) * 20f * time.DeltaTimeF;
            }
        }
    }

    private void HandleMenuSelect()
    {
        if (_gameState.CurrentMode == GameMode.MainMenu)
        {
            switch (_selectedItem)
            {
                case 0: // New Game / Continue
                    _gameState.SetMode(GameMode.Playing);
                    if (_inputState != null)
                        _inputState.IsMouseCaptured = true;
                    _selectedItem = 0;
                    break;
                case 1: // Quit
                    _window.Close();
                    break;
            }
        }
        else if (_gameState.CurrentMode == GameMode.Paused)
        {
            switch (_selectedItem)
            {
                case 0: // Resume
                    _gameState.SetMode(GameMode.Playing);
                    if (_inputState != null)
                        _inputState.IsMouseCaptured = true;
                    break;
                case 1: // Save
                    // Trigger save through singleton
                    break;
                case 2: // Quit to Menu
                    _gameState.SetMode(GameMode.MainMenu);
                    _selectedItem = 0;
                    break;
            }
        }
    }

    public void Render(GameTime time)
    {
        if (_gameState.CurrentMode == GameMode.Playing) return;

        _gl.Disable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        int w = _window.Size.X, h = _window.Size.Y;
        var ortho = Matrix4x4.CreateOrthographicOffCenter(0, w, 0, h, -1, 1);

        if (_gameState.CurrentMode == GameMode.MainMenu)
            RenderMainMenu(w, h, ortho);
        else if (_gameState.CurrentMode == GameMode.Paused)
            RenderPauseMenu(w, h, ortho);

        _gl.Enable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.Blend);
    }

    private void RenderMainMenu(int w, int h, Matrix4x4 ortho)
    {
        // Full-screen atmospheric background
        DrawTexturedRect(0, 0, w, h, _textures.MenuBackground, new Vector4(1, 1, 1, 1), ortho);

        // Dark overlay for depth
        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);
        DrawRect(new HudElement(new Vector2(0, 0), new Vector2(w, h),
            new Vector4(0, 0, 0, 0.3f)));

        // Ember particles
        RenderEmbers(w, h, ortho);

        // Ornate frame (centered)
        float frameW = MathF.Min(500, w - 80);
        float frameH = MathF.Min(400, h - 80);
        float frameX = (w - frameW) / 2f;
        float frameY = (h - frameH) / 2f;

        // Frame parchment background
        DrawTexturedRect(frameX, frameY, frameW, frameH, _textures.Parchment,
            new Vector4(0.75f, 0.68f, 0.55f, 0.90f), ortho);

        // Dark inner area
        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);
        DrawRect(new HudElement(new Vector2(frameX + 15, frameY + 15),
            new Vector2(frameW - 30, frameH - 30),
            new Vector4(0.06f, 0.05f, 0.04f, 0.75f)));

        // Gold border (thick ornate)
        DrawGoldFrame(frameX, frameY, frameW, frameH, ortho);

        // Vine border decoration (top)
        DrawTexturedRect(frameX + 20, frameY + frameH - 45, frameW - 40, 25,
            _textures.VineBorder, new Vector4(1, 1, 1, 0.7f), ortho);

        // Vine border (bottom)
        DrawTexturedRect(frameX + 20, frameY + 15, frameW - 40, 25,
            _textures.VineBorder, new Vector4(1, 1, 1, 0.5f), ortho);

        // Title: CongCraft
        string title = "CongCraft";
        float titleScale = 5.0f;
        float titleW = TextRenderer.MeasureWidth(title, titleScale);
        float titleX = (w - titleW) / 2f;
        float titleY = frameY + frameH - 85;

        // Title shadow
        _textRenderer.DrawText(title, titleX + 3, titleY - 3, titleScale,
            new Vector4(0.1f, 0.05f, 0.02f, 0.6f), ortho);
        // Title (gold)
        _textRenderer.DrawText(title, titleX, titleY, titleScale, GoldColor, ortho);

        // Subtitle
        string subtitle = "Medieval Action RPG";
        float subW = TextRenderer.MeasureWidth(subtitle, 2.0f);
        _textRenderer.DrawText(subtitle, (w - subW) / 2f, titleY - 30, 2.0f, DimText, ortho);

        // Divider
        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);
        DrawRect(new HudElement(new Vector2(frameX + 40, titleY - 45), new Vector2(frameW - 80, 2), DarkGold));

        // Menu items
        string[] items = { "New Game", "Quit" };
        float itemStartY = titleY - 80;
        float itemSpacing = 45f;

        for (int i = 0; i < items.Length; i++)
        {
            float iy = itemStartY - i * itemSpacing;
            bool selected = i == _selectedItem;
            float itemW = TextRenderer.MeasureWidth(items[i], 3.0f);
            float itemX = (w - itemW) / 2f;

            if (selected)
            {
                // Selection background
                _hudShader.Use();
                _hudShader.SetUniform("uProjection", ortho);
                DrawRect(new HudElement(new Vector2(frameX + 30, iy - 5), new Vector2(frameW - 60, 32),
                    new Vector4(0.45f, 0.35f, 0.15f, 0.35f)));

                // Selection border
                DrawRect(new HudElement(new Vector2(frameX + 30, iy + 25), new Vector2(frameW - 60, 2), GoldColor));
                DrawRect(new HudElement(new Vector2(frameX + 30, iy - 5), new Vector2(frameW - 60, 2), GoldColor));

                // Arrow indicator
                float arrowPulse = MathF.Sin(_animTimer * 3f) * 3f;
                _textRenderer.DrawText(">", itemX - 20 - arrowPulse, iy + 2, 3.0f, GoldColor, ortho);

                _textRenderer.DrawText(items[i], itemX, iy + 2, 3.0f, CreamText, ortho);
            }
            else
            {
                _textRenderer.DrawText(items[i], itemX, iy + 2, 3.0f, DimText, ortho);
            }
        }

        // Bottom hint
        string hint = "Arrow Keys + Enter";
        float hintW = TextRenderer.MeasureWidth(hint, 1.5f);
        _textRenderer.DrawText(hint, (w - hintW) / 2f, frameY + 25, 1.5f,
            new Vector4(0.5f, 0.45f, 0.35f, 0.5f), ortho);
    }

    private void RenderPauseMenu(int w, int h, Matrix4x4 ortho)
    {
        // Semi-transparent dark overlay over the game
        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);
        DrawRect(new HudElement(new Vector2(0, 0), new Vector2(w, h),
            new Vector4(0, 0, 0, 0.55f)));

        // Embers
        RenderEmbers(w, h, ortho);

        // Pause panel (smaller than main menu)
        float panelW = MathF.Min(380, w - 60);
        float panelH = MathF.Min(320, h - 60);
        float panelX = (w - panelW) / 2f;
        float panelY = (h - panelH) / 2f;

        // Parchment background
        DrawTexturedRect(panelX, panelY, panelW, panelH, _textures.Parchment,
            new Vector4(0.70f, 0.63f, 0.50f, 0.92f), ortho);

        // Dark inner
        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);
        DrawRect(new HudElement(new Vector2(panelX + 12, panelY + 12),
            new Vector2(panelW - 24, panelH - 24),
            new Vector4(0.06f, 0.05f, 0.04f, 0.70f)));

        // Gold frame
        DrawGoldFrame(panelX, panelY, panelW, panelH, ortho);

        // Title
        string title = "PAUSED";
        float titleScale = 4.0f;
        float titleW = TextRenderer.MeasureWidth(title, titleScale);
        _textRenderer.DrawText(title, (w - titleW) / 2f, panelY + panelH - 55, titleScale, GoldColor, ortho);

        // Divider
        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);
        DrawRect(new HudElement(new Vector2(panelX + 30, panelY + panelH - 70),
            new Vector2(panelW - 60, 2), DarkGold));

        // Menu items
        string[] items = { "Resume", "Save Game", "Quit to Menu" };
        float itemStartY = panelY + panelH - 110;
        float itemSpacing = 40f;

        for (int i = 0; i < items.Length; i++)
        {
            float iy = itemStartY - i * itemSpacing;
            bool selected = i == _selectedItem;
            float itemTxtW = TextRenderer.MeasureWidth(items[i], 2.5f);
            float itemX = (w - itemTxtW) / 2f;

            if (selected)
            {
                _hudShader.Use();
                _hudShader.SetUniform("uProjection", ortho);
                DrawRect(new HudElement(new Vector2(panelX + 25, iy - 4), new Vector2(panelW - 50, 28),
                    new Vector4(0.45f, 0.35f, 0.15f, 0.30f)));
                DrawRect(new HudElement(new Vector2(panelX + 25, iy + 22), new Vector2(panelW - 50, 2), GoldColor));
                DrawRect(new HudElement(new Vector2(panelX + 25, iy - 4), new Vector2(panelW - 50, 2), GoldColor));

                float arrowPulse = MathF.Sin(_animTimer * 3f) * 2f;
                _textRenderer.DrawText(">", itemX - 16 - arrowPulse, iy + 2, 2.5f, GoldColor, ortho);
                _textRenderer.DrawText(items[i], itemX, iy + 2, 2.5f, CreamText, ortho);
            }
            else
            {
                _textRenderer.DrawText(items[i], itemX, iy + 2, 2.5f, DimText, ortho);
            }
        }

        // Hint
        string hint = "Esc to Resume";
        float hintW = TextRenderer.MeasureWidth(hint, 1.5f);
        _textRenderer.DrawText(hint, (w - hintW) / 2f, panelY + 22, 1.5f,
            new Vector4(0.5f, 0.45f, 0.35f, 0.4f), ortho);
    }

    private void RenderEmbers(int w, int h, Matrix4x4 ortho)
    {
        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);

        for (int i = 0; i < _embers.Length; i++)
        {
            ref var e = ref _embers[i];
            if (e.Life <= 0) continue;

            float lifeRatio = e.Life / e.MaxLife;
            float alpha = MathF.Min(1f, lifeRatio * 2f) * e.Brightness;
            float size = e.Size * (0.5f + lifeRatio * 0.5f);

            // Warm ember color (orange-gold)
            var color = new Vector4(
                0.95f * e.Brightness,
                0.55f * e.Brightness,
                0.15f * e.Brightness,
                alpha * 0.6f);

            DrawRect(new HudElement(new Vector2(e.X, e.Y), new Vector2(size, size), color));
        }
    }

    private void RespawnEmber(ref MenuParticle e, bool randomLife)
    {
        int w = _window?.Size.X ?? 1280;
        int h = _window?.Size.Y ?? 720;

        e.X = (float)_rng.NextDouble() * w;
        e.Y = (float)_rng.NextDouble() * h * 0.3f; // Start from bottom
        e.Vx = ((float)_rng.NextDouble() - 0.5f) * 30f;
        e.Vy = 20f + (float)_rng.NextDouble() * 40f;
        e.MaxLife = 3f + (float)_rng.NextDouble() * 5f;
        e.Life = randomLife ? (float)_rng.NextDouble() * e.MaxLife : e.MaxLife;
        e.Size = 2f + (float)_rng.NextDouble() * 4f;
        e.Brightness = 0.4f + (float)_rng.NextDouble() * 0.6f;
    }

    private void DrawGoldFrame(float x, float y, float fw, float fh, Matrix4x4 ortho)
    {
        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);

        float t = 4f; // Border thickness

        // Outer border
        DrawRect(new HudElement(new Vector2(x, y + fh - t), new Vector2(fw, t), GoldColor));
        DrawRect(new HudElement(new Vector2(x, y), new Vector2(fw, t), GoldColor));
        DrawRect(new HudElement(new Vector2(x, y), new Vector2(t, fh), GoldColor));
        DrawRect(new HudElement(new Vector2(x + fw - t, y), new Vector2(t, fh), GoldColor));

        // Inner border (thinner, darker)
        float it = 2f;
        float o = t + 2;
        DrawRect(new HudElement(new Vector2(x + o, y + fh - o - it), new Vector2(fw - o * 2, it), DarkGold));
        DrawRect(new HudElement(new Vector2(x + o, y + o), new Vector2(fw - o * 2, it), DarkGold));
        DrawRect(new HudElement(new Vector2(x + o, y + o), new Vector2(it, fh - o * 2), DarkGold));
        DrawRect(new HudElement(new Vector2(x + fw - o - it, y + o), new Vector2(it, fh - o * 2), DarkGold));

        // Corner ornaments (larger, decorative)
        float cs = 14f;
        // Outer corner squares
        DrawRect(new HudElement(new Vector2(x - 2, y + fh - cs + 2), new Vector2(cs, cs), DarkGold));
        DrawRect(new HudElement(new Vector2(x + fw - cs + 2, y + fh - cs + 2), new Vector2(cs, cs), DarkGold));
        DrawRect(new HudElement(new Vector2(x - 2, y - 2), new Vector2(cs, cs), DarkGold));
        DrawRect(new HudElement(new Vector2(x + fw - cs + 2, y - 2), new Vector2(cs, cs), DarkGold));

        // Inner corner highlights
        float ics = 8f;
        DrawRect(new HudElement(new Vector2(x + 1, y + fh - ics - 1), new Vector2(ics, ics), GoldColor));
        DrawRect(new HudElement(new Vector2(x + fw - ics - 1, y + fh - ics - 1), new Vector2(ics, ics), GoldColor));
        DrawRect(new HudElement(new Vector2(x + 1, y + 1), new Vector2(ics, ics), GoldColor));
        DrawRect(new HudElement(new Vector2(x + fw - ics - 1, y + 1), new Vector2(ics, ics), GoldColor));

        // Tiny diamond center in each corner
        float ds = 3f;
        DrawRect(new HudElement(new Vector2(x + 3, y + fh - 5), new Vector2(ds, ds), CreamText));
        DrawRect(new HudElement(new Vector2(x + fw - 6, y + fh - 5), new Vector2(ds, ds), CreamText));
        DrawRect(new HudElement(new Vector2(x + 3, y + 2), new Vector2(ds, ds), CreamText));
        DrawRect(new HudElement(new Vector2(x + fw - 6, y + 2), new Vector2(ds, ds), CreamText));
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
