using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Crafting;
using CongCraft.Engine.Dialogue;
using CongCraft.Engine.Dungeon;
using CongCraft.Engine.Inventory;
using CongCraft.Engine.Leveling;
using CongCraft.Engine.Magic;
using CongCraft.Engine.Quest;
using CongCraft.Engine.SaveLoad;
using CongCraft.Engine.Boss;
using CongCraft.Engine.Level;
using CongCraft.Engine.Weather;
using CongCraft.Engine.Procedural;
using CongCraft.Engine.Rendering;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Shader = CongCraft.Engine.Rendering.Shader;

namespace CongCraft.Engine.UI;

/// <summary>
/// Renders 2D HUD overlay in illuminated manuscript style.
/// Features: parchment backgrounds, gold vine borders, corner ornaments, bitmap font text.
/// </summary>
public sealed class HudSystem : ISystem
{
    public int Priority => 110;

    private GL _gl = null!;
    private World _world = null!;
    private Shader _hudShader = null!;
    private Shader _texShader = null!;
    private Mesh _quad = null!;
    private IWindow _window = null!;
    private LevelTerrainGenerator? _terrainGen;
    private BitmapFont _font = null!;
    private TextRenderer _textRenderer = null!;
    private UITextureAtlas _textures = null!;

    // Illuminated manuscript color palette
    // SpellForce 1-style color palette: warm amber/gold on dark carved stone
    private static readonly Vector4 GoldColor = new(0.88f, 0.65f, 0.18f, 0.95f);
    private static readonly Vector4 DarkGold = new(0.58f, 0.40f, 0.10f, 0.90f);
    private static readonly Vector4 ParchmentTint = new(0.30f, 0.26f, 0.22f, 0.92f);
    private static readonly Vector4 DeepRed = new(0.60f, 0.12f, 0.10f, 0.90f);
    private static readonly Vector4 DeepBlue = new(0.15f, 0.22f, 0.55f, 0.85f);
    private static readonly Vector4 CreamText = new(0.93f, 0.88f, 0.72f, 1.0f);
    private static readonly Vector4 DarkInk = new(0.15f, 0.10f, 0.08f, 0.95f);

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
        services.TryGet(out _terrainGen);
    }

    public void Update(GameTime time) { }

    public void Render(GameTime time)
    {
        // Skip HUD during menu (MenuSystem handles that)
        if (_world.TryGetSingleton<GameStateManager>(out var gs) && gs!.CurrentMode == GameMode.MainMenu)
            return;

        _gl.Disable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        int w = _window.Size.X, h = _window.Size.Y;
        var ortho = Matrix4x4.CreateOrthographicOffCenter(0, w, 0, h, -1, 1);

        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);

        foreach (var (entity, player) in _world.Query<PlayerComponent>())
        {
            float health, maxHealth;
            if (_world.HasComponent<HealthComponent>(entity))
            {
                var hc = _world.GetComponent<HealthComponent>(entity);
                health = hc.Current;
                maxHealth = hc.Max;

                if (hc.DamageFlashTimer > 0)
                {
                    DrawRect(new HudElement(new Vector2(0, 0), new Vector2(w, h),
                        new Vector4(0.5f, 0f, 0f, hc.DamageFlashTimer * 0.3f)));
                }
            }
            else
            {
                health = player.Health;
                maxHealth = player.MaxHealth;
            }

            // === Ornate Health/Mana panel (bottom-left) ===
            DrawOrnatePanel(10, 5, 240, 70, ortho);

            // Health bar with ornate frame
            DrawOrnateBar(20, 35, 200, 18, health / maxHealth, DeepRed, "HP", ortho);
            _textRenderer.DrawText($"{(int)health}/{(int)maxHealth}", 225, 37, 1.5f, CreamText, ortho);

            // Mana bar
            if (_world.HasComponent<ManaComponent>(entity))
            {
                var mana = _world.GetComponent<ManaComponent>(entity);
                DrawOrnateBar(20, 12, 200, 14, mana.Percentage, DeepBlue, "MP", ortho);
                _textRenderer.DrawText($"{(int)mana.Current}/{(int)mana.Max}", 225, 14, 1.5f, CreamText, ortho);

                if (_world.HasComponent<SpellState>(entity))
                {
                    var spellState = _world.GetComponent<SpellState>(entity);
                    if (spellState.HasActiveShield)
                    {
                        DrawRect(new HudElement(new Vector2(10, 5), new Vector2(240, 70),
                            new Vector4(0.3f, 0.5f, 1f, 0.08f)));
                    }
                }
            }

            // === Spell hotbar (bottom center) ===
            if (_world.HasComponent<SpellState>(entity) && _world.HasComponent<ManaComponent>(entity))
            {
                DrawSpellHotbar(_world.GetComponent<SpellState>(entity),
                    _world.GetComponent<ManaComponent>(entity), w, h, ortho);
            }

            // Combat indicators
            if (_world.HasComponent<CombatComponent>(entity))
            {
                var combat = _world.GetComponent<CombatComponent>(entity);
                if (combat.IsBlocking)
                {
                    _hudShader.Use();
                    _hudShader.SetUniform("uProjection", ortho);
                    DrawRect(new HudElement(new Vector2(20, 58), new Vector2(200, 4),
                        new Vector4(0.3f, 0.5f, 0.8f, 0.8f)));
                }
                if (combat.DodgeCooldownTimer > 0)
                {
                    float dodgeFill = 200f * (1f - combat.DodgeCooldownTimer / combat.DodgeCooldown);
                    _hudShader.Use();
                    _hudShader.SetUniform("uProjection", ortho);
                    DrawRect(new HudElement(new Vector2(20, 63), new Vector2(200, 3),
                        new Vector4(0.2f, 0.2f, 0.2f, 0.4f)));
                    DrawRect(new HudElement(new Vector2(20, 63), new Vector2(dodgeFill, 3),
                        new Vector4(0.8f, 0.75f, 0.3f, 0.7f)));
                }
            }

            // === Equipment slots (ornate) ===
            if (_world.HasComponent<EquipmentComponent>(entity))
            {
                var equip = _world.GetComponent<EquipmentComponent>(entity);
                float eqY = 82f;
                DrawEquipSlot(15, eqY, "W", equip.MainHand, ortho);
                DrawEquipSlot(52, eqY, "H", equip.Head, ortho);
                DrawEquipSlot(89, eqY, "C", equip.Chest, ortho);
                DrawEquipSlot(126, eqY, "L", equip.Legs, ortho);
            }

            // === Inventory panel ===
            if (_world.HasComponent<InventoryComponent>(entity))
            {
                var inventory = _world.GetComponent<InventoryComponent>(entity);
                if (inventory.IsOpen)
                    DrawInventoryPanel(inventory, w, h, ortho);
                else
                {
                    _hudShader.Use();
                    _hudShader.SetUniform("uProjection", ortho);
                    float countWidth = MathF.Min(60f, inventory.Items.Count * 3f);
                    DrawRect(new HudElement(new Vector2(20, 118), new Vector2(60, 10),
                        new Vector4(0.15f, 0.12f, 0.08f, 0.5f)));
                    DrawRect(new HudElement(new Vector2(20, 118), new Vector2(countWidth, 10),
                        new Vector4(0.5f, 0.40f, 0.18f, 0.6f)));
                }
            }

            // === XP bar & Level ===
            if (_world.HasComponent<LevelComponent>(entity))
            {
                var level = _world.GetComponent<LevelComponent>(entity);
                DrawXpBar(w, level, ortho);
                if (level.IsSkillMenuOpen && _world.HasComponent<SkillTree>(entity))
                    DrawSkillMenu(level, _world.GetComponent<SkillTree>(entity), w, h, ortho);
            }
        }

        // === Dialogue panel ===
        if (_world.TryGetSingleton<DialogueState>(out var dialogueState) && dialogueState!.IsActive)
            DrawDialoguePanel(dialogueState, w, h, ortho);
        else
            DrawNpcProximityHint(w, h, ortho);

        // === Crafting panel ===
        if (_world.TryGetSingleton<CraftingState>(out var craftState) && craftState!.IsOpen)
            DrawCraftingPanel(craftState, w, h, ortho);
        else
            DrawCraftingStationHint(w, h, ortho);

        DrawBossHealthBar(w, h, ortho);
        DrawQuestTracker(w, h, ortho);
        DrawDungeonHints(w, h, ortho);
        DrawSaveNotification(w, h, ortho);
        DrawWeatherIndicator(w, h, ortho);
        DrawMinimap(w, h, ortho);

        _gl.Enable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.Blend);
    }

    // === Ornate Panel Drawing ===

    private void DrawOrnatePanel(float x, float y, float pw, float ph, Matrix4x4 ortho)
    {
        // Parchment background
        DrawTexturedRect(x, y, pw, ph, _textures.Parchment, ParchmentTint, ortho);

        // Gold border (top, bottom, left, right)
        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);
        DrawRect(new HudElement(new Vector2(x, y + ph - 3), new Vector2(pw, 3), GoldColor));
        DrawRect(new HudElement(new Vector2(x, y), new Vector2(pw, 3), GoldColor));
        DrawRect(new HudElement(new Vector2(x, y), new Vector2(3, ph), GoldColor));
        DrawRect(new HudElement(new Vector2(x + pw - 3, y), new Vector2(3, ph), GoldColor));

        // Corner ornaments (small gold squares with flourish)
        float cs = 8f;
        DrawRect(new HudElement(new Vector2(x - 1, y + ph - cs + 1), new Vector2(cs, cs), DarkGold));
        DrawRect(new HudElement(new Vector2(x + pw - cs + 1, y + ph - cs + 1), new Vector2(cs, cs), DarkGold));
        DrawRect(new HudElement(new Vector2(x - 1, y - 1), new Vector2(cs, cs), DarkGold));
        DrawRect(new HudElement(new Vector2(x + pw - cs + 1, y - 1), new Vector2(cs, cs), DarkGold));

        // Inner corner highlights (smaller, brighter gold)
        float ics = 4f;
        DrawRect(new HudElement(new Vector2(x + 1, y + ph - ics - 1), new Vector2(ics, ics), GoldColor));
        DrawRect(new HudElement(new Vector2(x + pw - ics - 1, y + ph - ics - 1), new Vector2(ics, ics), GoldColor));
        DrawRect(new HudElement(new Vector2(x + 1, y + 1), new Vector2(ics, ics), GoldColor));
        DrawRect(new HudElement(new Vector2(x + pw - ics - 1, y + 1), new Vector2(ics, ics), GoldColor));
    }

    private void DrawOrnateBar(float x, float y, float bw, float bh, float fill,
        Vector4 fillColor, string label, Matrix4x4 ortho)
    {
        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);

        // Bar background (dark with parchment tint)
        DrawRect(new HudElement(new Vector2(x, y), new Vector2(bw, bh),
            new Vector4(0.12f, 0.10f, 0.07f, 0.85f)));

        // Fill
        float fillWidth = bw * Math.Clamp(fill, 0f, 1f);
        DrawRect(new HudElement(new Vector2(x, y), new Vector2(fillWidth, bh), fillColor));

        // Gold frame around bar
        DrawRect(new HudElement(new Vector2(x - 1, y + bh), new Vector2(bw + 2, 2), DarkGold));
        DrawRect(new HudElement(new Vector2(x - 1, y - 1), new Vector2(bw + 2, 2), DarkGold));
        DrawRect(new HudElement(new Vector2(x - 1, y), new Vector2(2, bh), DarkGold));
        DrawRect(new HudElement(new Vector2(x + bw - 1, y), new Vector2(2, bh), DarkGold));

        // Label
        _textRenderer.DrawText(label, x + 3, y + (bh - 8) / 2f, 1.0f, CreamText, ortho);
    }

    private void DrawXpBar(int screenW, LevelComponent level, Matrix4x4 ortho)
    {
        // Full-width XP bar at very bottom
        float barW = screenW - 40f;
        float barX = 20f;
        float barY = 0f;

        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);
        DrawRect(new HudElement(new Vector2(barX, barY), new Vector2(barW, 6),
            new Vector4(0.10f, 0.06f, 0.15f, 0.5f)));
        float xpWidth = barW * level.LevelProgress;
        DrawRect(new HudElement(new Vector2(barX, barY), new Vector2(xpWidth, 6),
            new Vector4(0.55f, 0.30f, 0.75f, 0.8f)));
        // Gold end-caps
        DrawRect(new HudElement(new Vector2(barX - 2, barY), new Vector2(4, 6), DarkGold));
        DrawRect(new HudElement(new Vector2(barX + barW - 2, barY), new Vector2(4, 6), DarkGold));

        // Level text
        _textRenderer.DrawText($"Lv.{level.Level}", barX + barW / 2f - 16, barY + 7, 1.5f, GoldColor, ortho);

        if (level.SkillPoints > 0)
        {
            _textRenderer.DrawText($"+{level.SkillPoints} SP", barX + barW / 2f + 20, barY + 7, 1.5f,
                new Vector4(0.95f, 0.85f, 0.25f, 1f), ortho);
        }
    }

    private void DrawEquipSlot(float x, float y, string label, ItemStack? item, Matrix4x4 ortho)
    {
        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);

        // Slot background (leather-like)
        DrawRect(new HudElement(new Vector2(x, y), new Vector2(32, 32),
            new Vector4(0.18f, 0.14f, 0.08f, 0.8f)));

        // Gold border
        DrawRect(new HudElement(new Vector2(x, y + 30), new Vector2(32, 2), DarkGold));
        DrawRect(new HudElement(new Vector2(x, y), new Vector2(32, 2), DarkGold));

        if (item != null)
        {
            DrawRect(new HudElement(new Vector2(x + 4, y + 4), new Vector2(24, 24),
                new Vector4(item.Item.IconR, item.Item.IconG, item.Item.IconB, 0.9f)));
        }

        // Label below slot
        _textRenderer.DrawText(label, x + 12, y - 10, 1.0f, DarkGold, ortho);
    }

    private void DrawInventoryPanel(InventoryComponent inventory, int screenW, int screenH, Matrix4x4 ortho)
    {
        float panelW = 240f;
        float panelH = 420f;
        float panelX = screenW - panelW - 20;
        float panelY = (screenH - panelH) / 2f;

        DrawOrnatePanel(panelX, panelY, panelW, panelH, ortho);

        // Title
        _textRenderer.DrawText("INVENTORY", panelX + 70, panelY + panelH - 20, 2.0f, GoldColor, ortho);

        // Vine separator under title
        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);
        DrawRect(new HudElement(new Vector2(panelX + 10, panelY + panelH - 26), new Vector2(panelW - 20, 2), DarkGold));

        float itemY = panelY + panelH - 45;
        for (int i = 0; i < inventory.Items.Count && i < 14; i++)
        {
            var stack = inventory.Items[i];
            bool selected = i == inventory.SelectedSlot;

            _hudShader.Use();
            _hudShader.SetUniform("uProjection", ortho);

            if (selected)
            {
                DrawRect(new HudElement(new Vector2(panelX + 6, itemY - 1), new Vector2(panelW - 12, 24),
                    new Vector4(0.50f, 0.40f, 0.18f, 0.4f)));
                // Selection arrow
                DrawRect(new HudElement(new Vector2(panelX + 8, itemY + 5), new Vector2(6, 10), GoldColor));
            }

            // Item icon
            DrawRect(new HudElement(new Vector2(panelX + 18, itemY + 2), new Vector2(18, 18),
                new Vector4(stack.Item.IconR, stack.Item.IconG, stack.Item.IconB, 0.9f)));

            // Item name
            _textRenderer.DrawText(stack.Item.Name, panelX + 40, itemY + 5, 1.5f,
                selected ? CreamText : new Vector4(0.7f, 0.65f, 0.55f, 0.9f), ortho);

            if (stack.Quantity > 1)
            {
                _textRenderer.DrawText($"x{stack.Quantity}", panelX + panelW - 40, itemY + 5, 1.5f,
                    new Vector4(0.8f, 0.75f, 0.6f, 0.8f), ortho);
            }

            // Type indicator strip
            _hudShader.Use();
            _hudShader.SetUniform("uProjection", ortho);
            var typeColor = stack.Item.Type switch
            {
                ItemType.Weapon => new Vector4(0.7f, 0.25f, 0.15f, 0.7f),
                ItemType.Armor => new Vector4(0.20f, 0.35f, 0.65f, 0.7f),
                ItemType.Consumable => new Vector4(0.25f, 0.60f, 0.25f, 0.7f),
                _ => new Vector4(0.4f, 0.4f, 0.4f, 0.5f)
            };
            DrawRect(new HudElement(new Vector2(panelX + panelW - 10, itemY + 2), new Vector2(4, 18), typeColor));

            itemY -= 26;
        }

        // Key hints at bottom
        _textRenderer.DrawText("1-5 Equip  H Heal  Esc Close", panelX + 12, panelY + 8, 1.0f,
            new Vector4(0.6f, 0.55f, 0.4f, 0.7f), ortho);
    }

    private void DrawDialoguePanel(DialogueState state, int screenW, int screenH, Matrix4x4 ortho)
    {
        var node = state.CurrentNode;
        if (node == null) return;

        float panelW = MathF.Min(620f, screenW - 40f);
        float panelH = 200f;
        float panelX = (screenW - panelW) / 2f;
        float panelY = 20f;

        DrawOrnatePanel(panelX, panelY, panelW, panelH, ortho);

        // Speaker name
        _textRenderer.DrawText(node.SpeakerName ?? "???", panelX + 15, panelY + panelH - 22, 2.0f, GoldColor, ortho);

        // Divider
        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);
        DrawRect(new HudElement(new Vector2(panelX + 10, panelY + panelH - 28), new Vector2(panelW - 20, 2), DarkGold));

        // Dialogue text (word-wrapped to fit panel)
        float textScale = 1.5f;
        float textAreaW = panelW - 30f;
        string wrappedText = TextRenderer.WrapText(node.Text ?? "", textAreaW, textScale);
        _textRenderer.DrawText(wrappedText, panelX + 15, panelY + panelH - 50, textScale, DarkInk, ortho);

        // Count text lines to position choices below the text
        int textLines = 1;
        foreach (char c in wrappedText) if (c == '\n') textLines++;
        float textBottomY = panelY + panelH - 50 - textLines * (BitmapFont.LogicalHeight * textScale + 2 * textScale);

        // Choices
        if (node.Choices.Count > 0)
        {
            float choiceY = textBottomY - 8;
            for (int i = 0; i < node.Choices.Count; i++)
            {
                bool selected = i == state.SelectedChoice;

                _hudShader.Use();
                _hudShader.SetUniform("uProjection", ortho);

                if (selected)
                {
                    DrawRect(new HudElement(new Vector2(panelX + 20, choiceY - i * 28), new Vector2(panelW - 40, 24),
                        new Vector4(0.50f, 0.40f, 0.18f, 0.35f)));
                    DrawRect(new HudElement(new Vector2(panelX + 14, choiceY - i * 28 + 7), new Vector2(5, 10), GoldColor));
                }

                string wrappedChoice = TextRenderer.WrapText(node.Choices[i].Text, textAreaW - 30f, textScale);
                _textRenderer.DrawText(wrappedChoice, panelX + 30, choiceY - i * 28 + 5, textScale,
                    selected ? CreamText : new Vector4(0.5f, 0.45f, 0.35f, 0.8f), ortho);
            }
        }
        else
        {
            // Continue indicator
            _textRenderer.DrawText("[Enter]", panelX + panelW - 80, panelY + 10, 1.5f, GoldColor, ortho);
        }
    }

    private void DrawNpcProximityHint(int screenW, int screenH, Matrix4x4 ortho)
    {
        Vector3 playerPos = Vector3.Zero;
        foreach (var (entity, player, transform) in _world.Query<PlayerComponent, TransformComponent>())
        {
            playerPos = transform.Position;
            break;
        }

        foreach (var (entity, npc, transform) in _world.Query<NpcComponent, TransformComponent>())
        {
            float dist = Vector3.Distance(playerPos, transform.Position);
            if (dist < npc.InteractionRadius)
            {
                float hintX = (screenW - 160) / 2f;
                _hudShader.Use();
                _hudShader.SetUniform("uProjection", ortho);
                DrawRect(new HudElement(new Vector2(hintX, 55), new Vector2(160, 24),
                    new Vector4(0.12f, 0.10f, 0.06f, 0.75f)));
                DrawRect(new HudElement(new Vector2(hintX, 55 + 22), new Vector2(160, 2), DarkGold));
                DrawRect(new HudElement(new Vector2(hintX, 55), new Vector2(160, 2), DarkGold));
                _textRenderer.DrawText("Press T to talk", hintX + 20, 60, 1.5f, CreamText, ortho);
                break;
            }
        }
    }

    private void DrawDungeonHints(int screenW, int screenH, Matrix4x4 ortho)
    {
        Vector3 playerPos = Vector3.Zero;
        foreach (var (entity, player, transform) in _world.Query<PlayerComponent, TransformComponent>())
        {
            playerPos = transform.Position;
            break;
        }

        float distToEntrance = Vector2.Distance(
            new Vector2(playerPos.X, playerPos.Z),
            new Vector2(25f, 25f));

        if (distToEntrance < 5f)
        {
            float hintX = (screenW - 200) / 2f;
            _hudShader.Use();
            _hudShader.SetUniform("uProjection", ortho);
            DrawRect(new HudElement(new Vector2(hintX, 85), new Vector2(200, 24),
                new Vector4(0.15f, 0.08f, 0.06f, 0.8f)));
            DrawRect(new HudElement(new Vector2(hintX, 85 + 22), new Vector2(200, 2), DarkGold));
            DrawRect(new HudElement(new Vector2(hintX, 85), new Vector2(200, 2), DarkGold));
            _textRenderer.DrawText("Press G - Dungeon", hintX + 24, 90, 1.5f, CreamText, ortho);
        }
    }

    private void DrawQuestTracker(int screenW, int screenH, Matrix4x4 ortho)
    {
        foreach (var (entity, player) in _world.Query<PlayerComponent>())
        {
            if (!_world.HasComponent<QuestJournal>(entity)) continue;
            var journal = _world.GetComponent<QuestJournal>(entity);
            if (journal.ActiveQuests.Count == 0) continue;

            float trackerX = 20f;
            float trackerY = screenH - 30f;
            var quest = journal.ActiveQuests[0];

            // Quest title background
            _hudShader.Use();
            _hudShader.SetUniform("uProjection", ortho);
            DrawRect(new HudElement(new Vector2(trackerX - 2, trackerY - 2), new Vector2(210, 22),
                new Vector4(0.12f, 0.10f, 0.06f, 0.7f)));
            DrawRect(new HudElement(new Vector2(trackerX - 2, trackerY + 18), new Vector2(210, 2), DarkGold));

            _textRenderer.DrawText(quest.Quest.Title ?? "Quest", trackerX + 4, trackerY + 2, 1.5f, GoldColor, ortho);

            float objY = trackerY - 22;
            foreach (var obj in quest.Objectives)
            {
                _hudShader.Use();
                _hudShader.SetUniform("uProjection", ortho);

                DrawRect(new HudElement(new Vector2(trackerX + 8, objY), new Vector2(185, 16),
                    new Vector4(0.08f, 0.07f, 0.05f, 0.5f)));

                float progress = obj.RequiredCount > 0
                    ? (float)obj.CurrentCount / obj.RequiredCount
                    : 0;
                float barWidth = 120f * progress;

                var barColor = obj.IsComplete
                    ? new Vector4(0.25f, 0.55f, 0.20f, 0.7f)
                    : new Vector4(0.55f, 0.45f, 0.18f, 0.6f);
                DrawRect(new HudElement(new Vector2(trackerX + 12, objY + 2), new Vector2(barWidth, 12), barColor));

                if (obj.IsComplete)
                {
                    _textRenderer.DrawText("*", trackerX + 175, objY + 2, 1.5f,
                        new Vector4(0.3f, 0.8f, 0.3f, 0.9f), ortho);
                }

                _textRenderer.DrawText(obj.Description ?? "...", trackerX + 14, objY + 3, 1.0f,
                    new Vector4(0.7f, 0.65f, 0.55f, 0.8f), ortho);

                objY -= 20;
            }

            if (journal.ActiveQuests.Count > 1)
            {
                _textRenderer.DrawText($"+{journal.ActiveQuests.Count - 1} more", trackerX + 8, objY + 2, 1.0f,
                    new Vector4(0.5f, 0.45f, 0.3f, 0.5f), ortho);
            }

            break;
        }
    }

    private void DrawCraftingPanel(CraftingState state, int screenW, int screenH, Matrix4x4 ortho)
    {
        float panelW = 300f;
        float panelH = 340f;
        float panelX = (screenW - panelW) / 2f;
        float panelY = (screenH - panelH) / 2f;

        DrawOrnatePanel(panelX, panelY, panelW, panelH, ortho);

        // Station type title
        string stationName = state.StationType switch
        {
            CraftingStationType.Anvil => "ANVIL",
            CraftingStationType.Alchemy => "ALCHEMY",
            _ => "CRAFTING"
        };
        _textRenderer.DrawText(stationName, panelX + panelW / 2f - TextRenderer.MeasureWidth(stationName, 2f) / 2f,
            panelY + panelH - 22, 2.0f, GoldColor, ortho);

        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);
        DrawRect(new HudElement(new Vector2(panelX + 10, panelY + panelH - 28), new Vector2(panelW - 20, 2), DarkGold));

        InventoryComponent? playerInv = null;
        foreach (var (entity, player) in _world.Query<PlayerComponent>())
        {
            if (_world.HasComponent<InventoryComponent>(entity))
                playerInv = _world.GetComponent<InventoryComponent>(entity);
            break;
        }

        float recipeY = panelY + panelH - 50;
        for (int i = 0; i < state.AvailableRecipes.Count && i < 9; i++)
        {
            var recipe = state.AvailableRecipes[i];
            bool selected = i == state.SelectedRecipe;
            bool canCraft = playerInv != null && CraftingSystem.CanCraft(recipe, playerInv);

            _hudShader.Use();
            _hudShader.SetUniform("uProjection", ortho);

            if (selected)
            {
                DrawRect(new HudElement(new Vector2(panelX + 6, recipeY - 2), new Vector2(panelW - 12, 30),
                    new Vector4(0.45f, 0.35f, 0.15f, 0.4f)));
            }

            var outputItem = ItemDatabase.Get(recipe.OutputItemId);
            if (outputItem != null)
            {
                DrawRect(new HudElement(new Vector2(panelX + 10, recipeY + 4), new Vector2(20, 20),
                    new Vector4(outputItem.IconR, outputItem.IconG, outputItem.IconB, 0.9f)));

                var nameColor = canCraft
                    ? new Vector4(0.3f, 0.65f, 0.3f, 0.9f)
                    : new Vector4(0.55f, 0.35f, 0.25f, 0.7f);
                _textRenderer.DrawText(outputItem.Name, panelX + 36, recipeY + 8, 1.5f, nameColor, ortho);
            }

            // Ingredient dots
            _hudShader.Use();
            _hudShader.SetUniform("uProjection", ortho);
            float dotX = panelX + 10;
            for (int j = 0; j < recipe.Ingredients.Count; j++)
            {
                var ingredient = recipe.Ingredients[j];
                var ingredientItem = ItemDatabase.Get(ingredient.ItemId);
                bool hasEnough = playerInv != null && playerInv.CountOf(ingredient.ItemId) >= ingredient.Count;
                if (ingredientItem != null)
                {
                    float alpha = hasEnough ? 0.9f : 0.25f;
                    DrawRect(new HudElement(new Vector2(dotX, recipeY - 4), new Vector2(10, 10),
                        new Vector4(ingredientItem.IconR, ingredientItem.IconG, ingredientItem.IconB, alpha)));
                }
                dotX += 14;
            }

            recipeY -= 34;
        }

        _textRenderer.DrawText("Enter:Craft  Esc:Close", panelX + 20, panelY + 10, 1.5f,
            new Vector4(0.6f, 0.5f, 0.35f, 0.7f), ortho);
    }

    private void DrawSpellHotbar(SpellState state, ManaComponent mana, int screenW, int screenH, Matrix4x4 ortho)
    {
        var spells = SpellDatabase.GetSpellBar();
        float slotSize = 40f;
        float gap = 5f;
        float totalWidth = spells.Length * slotSize + (spells.Length - 1) * gap;
        float startX = (screenW - totalWidth) / 2f;
        float y = 8f;

        for (int i = 0; i < spells.Length; i++)
        {
            var spell = spells[i];
            float x = startX + i * (slotSize + gap);
            bool selected = i == state.SelectedSpell;
            bool onCooldown = state.Cooldowns[i] > 0;
            bool canAfford = mana.CanSpend(spell.ManaCost);

            _hudShader.Use();
            _hudShader.SetUniform("uProjection", ortho);

            // Slot background (leather-like)
            DrawRect(new HudElement(new Vector2(x, y), new Vector2(slotSize, slotSize),
                new Vector4(0.15f, 0.12f, 0.08f, 0.85f)));

            // Spell icon
            float iconAlpha = (onCooldown || !canAfford) ? 0.25f : 0.9f;
            DrawRect(new HudElement(new Vector2(x + 4, y + 4), new Vector2(slotSize - 8, slotSize - 8),
                new Vector4(spell.VfxR, spell.VfxG, spell.VfxB, iconAlpha)));

            // Cooldown overlay
            if (onCooldown)
            {
                float cdFill = state.Cooldowns[i] / spell.Cooldown;
                float cdHeight = (slotSize - 8) * cdFill;
                DrawRect(new HudElement(new Vector2(x + 4, y + 4), new Vector2(slotSize - 8, cdHeight),
                    new Vector4(0f, 0f, 0f, 0.5f)));
            }

            // Gold border (selected = bright gold, normal = dark gold)
            var borderColor = selected ? GoldColor : new Vector4(0.4f, 0.3f, 0.12f, 0.6f);
            DrawRect(new HudElement(new Vector2(x, y + slotSize - 2), new Vector2(slotSize, 2), borderColor));
            DrawRect(new HudElement(new Vector2(x, y), new Vector2(slotSize, 2), borderColor));
            DrawRect(new HudElement(new Vector2(x, y), new Vector2(2, slotSize), borderColor));
            DrawRect(new HudElement(new Vector2(x + slotSize - 2, y), new Vector2(2, slotSize), borderColor));

            // Key number
            _textRenderer.DrawText($"{i + 1}", x + 2, y + slotSize - 12, 1.0f,
                new Vector4(0.7f, 0.6f, 0.4f, 0.7f), ortho);
        }
    }

    private void DrawSkillMenu(LevelComponent level, SkillTree skills, int screenW, int screenH, Matrix4x4 ortho)
    {
        float panelW = 220f;
        float panelH = 220f;
        float panelX = 20f;
        float panelY = screenH - panelH - 60f;

        DrawOrnatePanel(panelX, panelY, panelW, panelH, ortho);

        _textRenderer.DrawText("SKILLS", panelX + 75, panelY + panelH - 20, 2.0f, GoldColor, ortho);

        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);
        DrawRect(new HudElement(new Vector2(panelX + 10, panelY + panelH - 26), new Vector2(panelW - 20, 2), DarkGold));

        // Skill points
        _textRenderer.DrawText($"Points: {level.SkillPoints}", panelX + 12, panelY + panelH - 42, 1.5f,
            level.SkillPoints > 0 ? GoldColor : new Vector4(0.5f, 0.45f, 0.35f, 0.7f), ortho);

        // Skill bars
        float barY = panelY + panelH - 70;
        DrawSkillBar(panelX + 10, barY, panelW - 20, "1:Strength", skills.Strength,
            new Vector4(0.65f, 0.18f, 0.15f, 0.8f), ortho);
        DrawSkillBar(panelX + 10, barY - 44, panelW - 20, "2:Endurance", skills.Endurance,
            new Vector4(0.18f, 0.55f, 0.18f, 0.8f), ortho);
        DrawSkillBar(panelX + 10, barY - 88, panelW - 20, "3:Agility", skills.Agility,
            new Vector4(0.18f, 0.30f, 0.65f, 0.8f), ortho);
    }

    private void DrawSkillBar(float x, float y, float w, string label, int value, Vector4 color, Matrix4x4 ortho)
    {
        _textRenderer.DrawText(label, x, y + 18, 1.5f, CreamText, ortho);

        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);
        DrawRect(new HudElement(new Vector2(x, y), new Vector2(w, 14),
            new Vector4(0.10f, 0.08f, 0.06f, 0.6f)));
        float fill = w * (value / (float)SkillTree.MaxSkillLevel);
        DrawRect(new HudElement(new Vector2(x, y), new Vector2(fill, 14), color));
        // Frame
        DrawRect(new HudElement(new Vector2(x, y + 14), new Vector2(w, 1), DarkGold));
        DrawRect(new HudElement(new Vector2(x, y - 1), new Vector2(w, 1), DarkGold));

        _textRenderer.DrawText($"{value}/{SkillTree.MaxSkillLevel}", x + w - 40, y + 2, 1.0f,
            CreamText, ortho);
    }

    private void DrawWeatherIndicator(int screenW, int screenH, Matrix4x4 ortho)
    {
        if (!_world.TryGetSingleton<WeatherState>(out var weather) || weather == null) return;

        float x = screenW - 170;
        float y = screenH - 15;

        string weatherName = weather.Current switch
        {
            WeatherType.Clear => "Clear",
            WeatherType.Cloudy => "Cloudy",
            WeatherType.Rain => "Rain",
            WeatherType.HeavyRain => "Storm",
            WeatherType.Fog => "Fog",
            WeatherType.Storm => "Thunder",
            _ => "..."
        };

        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);
        DrawRect(new HudElement(new Vector2(x, y), new Vector2(150, 10),
            new Vector4(0.08f, 0.07f, 0.05f, 0.4f)));

        var weatherColor = weather.Current switch
        {
            WeatherType.Clear => new Vector4(0.4f, 0.6f, 0.85f, 0.6f),
            WeatherType.Cloudy => new Vector4(0.45f, 0.45f, 0.50f, 0.6f),
            WeatherType.Rain => new Vector4(0.25f, 0.35f, 0.65f, 0.7f),
            WeatherType.HeavyRain => new Vector4(0.20f, 0.28f, 0.55f, 0.8f),
            WeatherType.Fog => new Vector4(0.55f, 0.55f, 0.60f, 0.7f),
            WeatherType.Storm => new Vector4(0.20f, 0.15f, 0.40f, 0.8f),
            _ => new Vector4(0.5f, 0.5f, 0.5f, 0.5f)
        };

        float fillWidth = 150f * weather.Intensity;
        DrawRect(new HudElement(new Vector2(x, y), new Vector2(fillWidth, 10), weatherColor));

        _textRenderer.DrawText(weatherName, x + 2, y + 12, 1.0f,
            new Vector4(0.7f, 0.65f, 0.55f, 0.6f), ortho);
    }

    private void DrawSaveNotification(int screenW, int screenH, Matrix4x4 ortho)
    {
        if (_world.TryGetSingleton<SaveNotification>(out var notif) && notif!.Timer > 0)
        {
            float alpha = MathF.Min(1f, notif.Timer);
            float cx = (screenW - 180) / 2f;
            _hudShader.Use();
            _hudShader.SetUniform("uProjection", ortho);
            DrawRect(new HudElement(new Vector2(cx, screenH - 40), new Vector2(180, 26),
                new Vector4(0.12f, 0.25f, 0.12f, 0.75f * alpha)));
            DrawRect(new HudElement(new Vector2(cx, screenH - 40 + 24), new Vector2(180, 2),
                new Vector4(0.3f, 0.6f, 0.3f, 0.6f * alpha)));
            _textRenderer.DrawText("Game Saved", cx + 40, screenH - 36, 2.0f,
                new Vector4(0.6f, 0.9f, 0.6f, alpha), ortho);
        }
    }

    private void DrawCraftingStationHint(int screenW, int screenH, Matrix4x4 ortho)
    {
        Vector3 playerPos = Vector3.Zero;
        foreach (var (entity, player, transform) in _world.Query<PlayerComponent, TransformComponent>())
        {
            playerPos = transform.Position;
            break;
        }

        foreach (var (entity, station, transform) in _world.Query<CraftingComponent, TransformComponent>())
        {
            float dist = Vector3.Distance(playerPos, transform.Position);
            if (dist < station.InteractionRadius)
            {
                float hintX = (screenW - 170) / 2f;
                _hudShader.Use();
                _hudShader.SetUniform("uProjection", ortho);
                DrawRect(new HudElement(new Vector2(hintX, 115), new Vector2(170, 24),
                    new Vector4(0.15f, 0.12f, 0.06f, 0.75f)));
                DrawRect(new HudElement(new Vector2(hintX, 115 + 22), new Vector2(170, 2), DarkGold));
                DrawRect(new HudElement(new Vector2(hintX, 115), new Vector2(170, 2), DarkGold));
                _textRenderer.DrawText("Press C to craft", hintX + 16, 120, 1.5f, CreamText, ortho);
                break;
            }
        }
    }

    private void DrawMinimap(int screenW, int screenH, Matrix4x4 ortho)
    {
        const float mapSize = MinimapData.MapSize;
        const int res = MinimapData.Resolution;
        const float range = MinimapData.WorldRange;
        float mapX = screenW - mapSize - 20;
        float mapY = screenH - mapSize - 20;
        float cellSize = mapSize / res;

        Vector3 playerPos = Vector3.Zero;
        foreach (var (entity, player, transform) in _world.Query<PlayerComponent, TransformComponent>())
        {
            playerPos = transform.Position;
            break;
        }

        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);

        // Background with ornate border
        DrawRect(new HudElement(new Vector2(mapX - 4, mapY - 4), new Vector2(mapSize + 8, mapSize + 8),
            new Vector4(0.05f, 0.04f, 0.03f, 0.75f)));

        // Terrain
        if (_terrainGen != null)
        {
            for (int z = 0; z < res; z++)
            for (int x = 0; x < res; x++)
            {
                float worldX = playerPos.X + (x - res / 2f) * (range * 2f / res);
                float worldZ = playerPos.Z + (z - res / 2f) * (range * 2f / res);
                float height = _terrainGen.GetHeightAt(worldX, worldZ);
                var color = HeightToColor(height);
                float px = mapX + x * cellSize;
                float py = mapY + (res - 1 - z) * cellSize;
                DrawRect(new HudElement(new Vector2(px, py), new Vector2(cellSize + 0.5f, cellSize + 0.5f), color));
            }
        }

        // Player marker
        DrawRect(new HudElement(new Vector2(mapX + mapSize / 2f - 3, mapY + mapSize / 2f - 3),
            new Vector2(6, 6), new Vector4(1f, 0.95f, 0.8f, 0.95f)));

        // NPC markers (green)
        foreach (var (entity, npc, transform) in _world.Query<NpcComponent, TransformComponent>())
        {
            var markerPos = WorldToMinimap(transform.Position, playerPos, mapX, mapY, mapSize, range);
            if (markerPos.HasValue)
                DrawRect(new HudElement(new Vector2(markerPos.Value.X - 2, markerPos.Value.Y - 2),
                    new Vector2(5, 5), new Vector4(0.25f, 0.7f, 0.25f, 0.9f)));
        }

        // Enemy markers (red)
        foreach (var (entity, enemy, health, transform) in QueryEnemiesForMinimap())
        {
            if (!health.IsAlive) continue;
            var markerPos = WorldToMinimap(transform.Position, playerPos, mapX, mapY, mapSize, range);
            if (markerPos.HasValue)
                DrawRect(new HudElement(new Vector2(markerPos.Value.X - 2, markerPos.Value.Y - 2),
                    new Vector2(4, 4), new Vector4(0.8f, 0.2f, 0.15f, 0.8f)));
        }

        // Crafting markers (orange)
        foreach (var (entity, station, transform) in _world.Query<CraftingComponent, TransformComponent>())
        {
            var markerPos = WorldToMinimap(transform.Position, playerPos, mapX, mapY, mapSize, range);
            if (markerPos.HasValue)
                DrawRect(new HudElement(new Vector2(markerPos.Value.X - 2, markerPos.Value.Y - 2),
                    new Vector2(5, 5), new Vector4(0.85f, 0.55f, 0.15f, 0.8f)));
        }

        // Boss markers (magenta)
        foreach (var (entity, boss, health) in _world.Query<BossComponent, HealthComponent>())
        {
            if (!health.IsAlive) continue;
            if (!_world.HasComponent<TransformComponent>(entity)) continue;
            var bossTransform = _world.GetComponent<TransformComponent>(entity);
            var markerPos = WorldToMinimap(bossTransform.Position, playerPos, mapX, mapY, mapSize, range);
            if (markerPos.HasValue)
                DrawRect(new HudElement(new Vector2(markerPos.Value.X - 3, markerPos.Value.Y - 3),
                    new Vector2(7, 7), new Vector4(0.75f, 0.2f, 0.75f, 0.9f)));
        }

        // Gold border frame
        DrawRect(new HudElement(new Vector2(mapX - 4, mapY - 4), new Vector2(mapSize + 8, 3), GoldColor));
        DrawRect(new HudElement(new Vector2(mapX - 4, mapY + mapSize + 1), new Vector2(mapSize + 8, 3), GoldColor));
        DrawRect(new HudElement(new Vector2(mapX - 4, mapY - 4), new Vector2(3, mapSize + 8), GoldColor));
        DrawRect(new HudElement(new Vector2(mapX + mapSize + 1, mapY - 4), new Vector2(3, mapSize + 8), GoldColor));

        // Corner ornaments
        float cs = 6f;
        DrawRect(new HudElement(new Vector2(mapX - 5, mapY + mapSize), new Vector2(cs, cs), DarkGold));
        DrawRect(new HudElement(new Vector2(mapX + mapSize - 1, mapY + mapSize), new Vector2(cs, cs), DarkGold));
        DrawRect(new HudElement(new Vector2(mapX - 5, mapY - 5), new Vector2(cs, cs), DarkGold));
        DrawRect(new HudElement(new Vector2(mapX + mapSize - 1, mapY - 5), new Vector2(cs, cs), DarkGold));
    }

    private void DrawBossHealthBar(int screenW, int screenH, Matrix4x4 ortho)
    {
        foreach (var (entity, boss, health) in _world.Query<BossComponent, HealthComponent>())
        {
            if (!boss.IsActivated || !health.IsAlive) continue;

            var bossData = BossDatabase.Get(boss.BossId);
            if (bossData == null) continue;

            float barW = 420f;
            float barH = 22f;
            float barX = (screenW - barW) / 2f;
            float barY = screenH - 45f;

            // Name
            _textRenderer.DrawText(bossData.Name, barX + barW / 2f - TextRenderer.MeasureWidth(bossData.Name, 2f) / 2f,
                barY + barH + 6, 2.0f, GoldColor, ortho);

            // Health bar with ornate frame
            _hudShader.Use();
            _hudShader.SetUniform("uProjection", ortho);
            DrawRect(new HudElement(new Vector2(barX, barY), new Vector2(barW, barH),
                new Vector4(0.08f, 0.04f, 0.04f, 0.85f)));

            float healthFill = barW * health.Percentage;
            var fillColor = boss.CurrentPhase switch
            {
                >= 2 => new Vector4(0.80f, 0.18f, 0.10f, 0.9f),
                1 => new Vector4(0.80f, 0.45f, 0.10f, 0.9f),
                _ => new Vector4(0.70f, 0.12f, 0.12f, 0.85f)
            };
            DrawRect(new HudElement(new Vector2(barX, barY), new Vector2(healthFill, barH), fillColor));

            // Gold frame
            DrawRect(new HudElement(new Vector2(barX - 2, barY + barH), new Vector2(barW + 4, 3), GoldColor));
            DrawRect(new HudElement(new Vector2(barX - 2, barY - 2), new Vector2(barW + 4, 3), GoldColor));
            DrawRect(new HudElement(new Vector2(barX - 2, barY), new Vector2(3, barH), GoldColor));
            DrawRect(new HudElement(new Vector2(barX + barW - 1, barY), new Vector2(3, barH), GoldColor));

            // Phase dots
            for (int i = 0; i < boss.MaxPhases; i++)
            {
                float dotX = barX + barW + 8 + i * 16;
                var dotColor = i < boss.CurrentPhase
                    ? new Vector4(0.9f, 0.3f, 0.1f, 0.9f)
                    : new Vector4(0.3f, 0.25f, 0.2f, 0.5f);
                DrawRect(new HudElement(new Vector2(dotX, barY + 4), new Vector2(12, 14), dotColor));
            }

            if (boss.State == BossState.Enraged)
            {
                _textRenderer.DrawText("ENRAGED", barX + barW / 2f - 28, barY + 4, 1.5f,
                    new Vector4(1f, 0.3f, 0.1f, 0.9f), ortho);
            }

            break;
        }
    }

    // === Utility ===

    private void DrawTexturedRect(float x, float y, float w, float h, uint textureId, Vector4 tint, Matrix4x4 ortho)
    {
        _texShader.Use();
        _texShader.SetUniform("uProjection", ortho);
        _texShader.SetUniform("uRect", new Vector4(x, y, w, h));
        _texShader.SetUniform("uColor", tint);
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, textureId);
        _texShader.SetUniform("uTexture", 0);
        _quad.Draw();
    }

    private void DrawRect(HudElement element)
    {
        // Ensure HUD shader is active (text/texture rendering may have switched programs)
        _hudShader.Use();
        _hudShader.SetUniform("uRect", new Vector4(
            element.Position.X, element.Position.Y,
            element.Size.X, element.Size.Y));
        _hudShader.SetUniform("uColor", element.Color);
        _quad.Draw();
    }

    private static Vector4 HeightToColor(float height)
    {
        if (height < 1f)
            return new Vector4(0.12f, 0.20f, 0.40f, 0.8f);
        if (height < 3f)
            return new Vector4(0.45f, 0.40f, 0.28f, 0.7f);
        if (height < 8f)
            return new Vector4(0.18f, 0.35f, 0.12f, 0.7f);
        if (height < 15f)
            return new Vector4(0.12f, 0.25f, 0.08f, 0.7f);
        return new Vector4(0.32f, 0.30f, 0.28f, 0.7f);
    }

    private static Vector2? WorldToMinimap(Vector3 worldPos, Vector3 playerPos,
        float mapX, float mapY, float mapSize, float range)
    {
        float dx = worldPos.X - playerPos.X;
        float dz = worldPos.Z - playerPos.Z;
        if (MathF.Abs(dx) > range || MathF.Abs(dz) > range) return null;
        float nx = (dx / range + 1f) * 0.5f;
        float nz = (dz / range + 1f) * 0.5f;
        return new Vector2(mapX + nx * mapSize, mapY + (1f - nz) * mapSize);
    }

    private IEnumerable<(Entity, EnemyComponent, HealthComponent, TransformComponent)> QueryEnemiesForMinimap()
    {
        foreach (var (entity, enemy, health) in _world.Query<EnemyComponent, HealthComponent>())
        {
            if (_world.HasComponent<TransformComponent>(entity))
                yield return (entity, enemy, health, _world.GetComponent<TransformComponent>(entity));
        }
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
