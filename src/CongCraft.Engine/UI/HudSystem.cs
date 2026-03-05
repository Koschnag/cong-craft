using System.Numerics;
using CongCraft.Engine.Boss;
using CongCraft.Engine.Core;
using CongCraft.Engine.Crafting;
using CongCraft.Engine.Dialogue;
using CongCraft.Engine.Dungeon;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Inventory;
using CongCraft.Engine.Leveling;
using CongCraft.Engine.Magic;
using CongCraft.Engine.Procedural;
using CongCraft.Engine.Quest;
using CongCraft.Engine.Rendering;
using CongCraft.Engine.SaveLoad;
using CongCraft.Engine.Terrain;
using CongCraft.Engine.Weather;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Shader = CongCraft.Engine.Rendering.Shader;

namespace CongCraft.Engine.UI;

/// <summary>
/// Renders 2D HUD overlay using orthographic projection.
/// Includes health bar, combat indicators, and inventory panel.
/// </summary>
public sealed class HudSystem : ISystem
{
    public int Priority => 110;

    private GL _gl = null!;
    private World _world = null!;
    private Shader _hudShader = null!;
    private Mesh _quad = null!;
    private IWindow _window = null!;
    private TerrainGenerator? _terrainGen;

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _world = services.Get<World>();
        _window = services.Get<IWindow>();
        _hudShader = new Shader(_gl, ShaderSources.HudVertex, ShaderSources.HudFragment);
        _quad = PrimitiveMeshBuilder.CreateUnitQuad(_gl);
        services.TryGet(out _terrainGen);
    }

    public void Update(GameTime time) { }

    public void Render(GameTime time)
    {
        _gl.Disable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        int w = _window.Size.X, h = _window.Size.Y;
        var ortho = Matrix4x4.CreateOrthographicOffCenter(0, w, 0, h, -1, 1);

        _hudShader.Use();
        _hudShader.SetUniform("uProjection", ortho);

        // Render health bar for player (using HealthComponent if available, fallback to PlayerComponent)
        foreach (var (entity, player) in _world.Query<PlayerComponent>())
        {
            float health, maxHealth;
            if (_world.HasComponent<HealthComponent>(entity))
            {
                var hc = _world.GetComponent<HealthComponent>(entity);
                health = hc.Current;
                maxHealth = hc.Max;

                // Damage flash: red border when hit
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

            var (bg, fill) = HealthBar.Calculate(health, maxHealth,
                new Vector2(20, 20), new Vector2(200, 20));

            DrawRect(bg);
            DrawRect(fill);

            // Mana bar (below health bar)
            if (_world.HasComponent<ManaComponent>(entity))
            {
                var mana = _world.GetComponent<ManaComponent>(entity);
                // Mana bar background
                DrawRect(new HudElement(new Vector2(20, 44), new Vector2(200, 12),
                    new Vector4(0.1f, 0.1f, 0.15f, 0.7f)));
                // Mana bar fill (blue)
                float manaWidth = 200f * mana.Percentage;
                DrawRect(new HudElement(new Vector2(20, 44), new Vector2(manaWidth, 12),
                    new Vector4(0.2f, 0.3f, 0.9f, 0.8f)));

                // Shield active indicator (cyan glow)
                if (_world.HasComponent<SpellState>(entity))
                {
                    var spellState = _world.GetComponent<SpellState>(entity);
                    if (spellState.HasActiveShield)
                    {
                        DrawRect(new HudElement(new Vector2(16, 16), new Vector2(208, 44),
                            new Vector4(0.3f, 0.5f, 1f, 0.15f)));
                    }
                }
            }

            // Spell hotbar (bottom center)
            if (_world.HasComponent<SpellState>(entity) && _world.HasComponent<ManaComponent>(entity))
            {
                DrawSpellHotbar(_world.GetComponent<SpellState>(entity),
                    _world.GetComponent<ManaComponent>(entity), w, h);
            }

            // Combat HUD hints
            if (_world.HasComponent<CombatComponent>(entity))
            {
                var combat = _world.GetComponent<CombatComponent>(entity);

                // Block indicator (blue bar when blocking)
                if (combat.IsBlocking)
                {
                    DrawRect(new HudElement(new Vector2(20, 45), new Vector2(200, 5),
                        new Vector4(0.2f, 0.4f, 0.8f, 0.8f)));
                }

                // Dodge cooldown indicator
                if (combat.DodgeCooldownTimer > 0)
                {
                    float dodgeFill = 200f * (1f - combat.DodgeCooldownTimer / combat.DodgeCooldown);
                    DrawRect(new HudElement(new Vector2(20, 55), new Vector2(200, 4),
                        new Vector4(0.3f, 0.3f, 0.3f, 0.5f)));
                    DrawRect(new HudElement(new Vector2(20, 55), new Vector2(dodgeFill, 4),
                        new Vector4(0.8f, 0.8f, 0.2f, 0.7f)));
                }
            }

            // Equipment indicator (bottom-left, below health)
            if (_world.HasComponent<EquipmentComponent>(entity))
            {
                var equip = _world.GetComponent<EquipmentComponent>(entity);
                float eqY = 70f;

                // Weapon slot
                DrawEquipSlot(20, eqY, "W", equip.MainHand);
                DrawEquipSlot(55, eqY, "H", equip.Head);
                DrawEquipSlot(90, eqY, "C", equip.Chest);
                DrawEquipSlot(125, eqY, "L", equip.Legs);
            }

            // Inventory panel (center-right when open)
            if (_world.HasComponent<InventoryComponent>(entity))
            {
                var inventory = _world.GetComponent<InventoryComponent>(entity);
                if (inventory.IsOpen)
                {
                    DrawInventoryPanel(inventory, w, h);
                }
                else
                {
                    // Small item count indicator
                    float countWidth = MathF.Min(60f, inventory.Items.Count * 3f);
                    DrawRect(new HudElement(new Vector2(20, 100), new Vector2(60, 12),
                        new Vector4(0.15f, 0.15f, 0.15f, 0.6f)));
                    DrawRect(new HudElement(new Vector2(20, 100), new Vector2(countWidth, 12),
                        new Vector4(0.4f, 0.35f, 0.2f, 0.7f)));
                }
            }

            // XP bar and level indicator
            if (_world.HasComponent<LevelComponent>(entity))
            {
                var level = _world.GetComponent<LevelComponent>(entity);

                // XP bar background (bottom, above health)
                DrawRect(new HudElement(new Vector2(20, 8), new Vector2(200, 8),
                    new Vector4(0.15f, 0.1f, 0.2f, 0.6f)));

                // XP bar fill (purple)
                float xpWidth = 200f * level.LevelProgress;
                DrawRect(new HudElement(new Vector2(20, 8), new Vector2(xpWidth, 8),
                    new Vector4(0.5f, 0.2f, 0.8f, 0.8f)));

                // Level number indicator (small square, wider = higher level)
                float levelWidth = MathF.Min(40f, level.Level * 4f);
                DrawRect(new HudElement(new Vector2(225, 8), new Vector2(levelWidth, 8),
                    new Vector4(0.7f, 0.5f, 0.9f, 0.8f)));

                // Skill point available indicator (blinking gold dot)
                if (level.SkillPoints > 0)
                {
                    float pointWidth = MathF.Min(30f, level.SkillPoints * 6f);
                    DrawRect(new HudElement(new Vector2(225, 20), new Vector2(pointWidth, 10),
                        new Vector4(0.9f, 0.8f, 0.2f, 0.8f)));
                }

                // Skill menu (left side when open)
                if (level.IsSkillMenuOpen && _world.HasComponent<SkillTree>(entity))
                {
                    DrawSkillMenu(level, _world.GetComponent<SkillTree>(entity), w, h);
                }
            }
        }

        // Dialogue panel (bottom-center when active)
        if (_world.TryGetSingleton<DialogueState>(out var dialogueState) && dialogueState!.IsActive)
        {
            DrawDialoguePanel(dialogueState, w, h);
        }
        else
        {
            // NPC proximity hint
            DrawNpcProximityHint(w, h);
        }

        // Crafting panel (center when open)
        if (_world.TryGetSingleton<CraftingState>(out var craftState) && craftState!.IsOpen)
        {
            DrawCraftingPanel(craftState, w, h);
        }
        else
        {
            // Crafting station proximity hint
            DrawCraftingStationHint(w, h);
        }

        // Boss health bar (top-center when boss is active)
        DrawBossHealthBar(w, h);

        // Quest tracker (top-left)
        DrawQuestTracker(w, h);

        // Dungeon indicators
        DrawDungeonHints(w, h);

        // Save/Load notification (top-center)
        DrawSaveNotification(w, h);

        // Weather indicator (top-right, above minimap)
        DrawWeatherIndicator(w, h);

        // Minimap (top-right)
        DrawMinimap(w, h);

        _gl.Enable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.Blend);
    }

    private void DrawEquipSlot(float x, float y, string label, ItemStack? item)
    {
        // Slot background
        DrawRect(new HudElement(new Vector2(x, y), new Vector2(30, 30),
            new Vector4(0.15f, 0.15f, 0.15f, 0.7f)));

        // Item color if equipped
        if (item != null)
        {
            DrawRect(new HudElement(new Vector2(x + 3, y + 3), new Vector2(24, 24),
                new Vector4(item.Item.IconR, item.Item.IconG, item.Item.IconB, 0.9f)));
        }
        else
        {
            // Empty slot indicator
            DrawRect(new HudElement(new Vector2(x + 10, y + 10), new Vector2(10, 10),
                new Vector4(0.3f, 0.3f, 0.3f, 0.4f)));
        }
    }

    private void DrawInventoryPanel(InventoryComponent inventory, int screenW, int screenH)
    {
        float panelW = 220f;
        float panelH = 400f;
        float panelX = screenW - panelW - 20;
        float panelY = (screenH - panelH) / 2f;

        // Panel background
        DrawRect(new HudElement(new Vector2(panelX, panelY), new Vector2(panelW, panelH),
            new Vector4(0.08f, 0.08f, 0.1f, 0.85f)));

        // Panel border
        DrawRect(new HudElement(new Vector2(panelX, panelY + panelH - 2), new Vector2(panelW, 2),
            new Vector4(0.5f, 0.4f, 0.2f, 0.8f)));
        DrawRect(new HudElement(new Vector2(panelX, panelY), new Vector2(panelW, 2),
            new Vector4(0.5f, 0.4f, 0.2f, 0.8f)));

        // Item list
        float itemY = panelY + panelH - 30;
        for (int i = 0; i < inventory.Items.Count && i < 15; i++)
        {
            var stack = inventory.Items[i];
            bool selected = i == inventory.SelectedSlot;

            // Selection highlight
            if (selected)
            {
                DrawRect(new HudElement(new Vector2(panelX + 4, itemY - 2), new Vector2(panelW - 8, 22),
                    new Vector4(0.4f, 0.35f, 0.2f, 0.5f)));
            }

            // Item color icon
            DrawRect(new HudElement(new Vector2(panelX + 8, itemY), new Vector2(18, 18),
                new Vector4(stack.Item.IconR, stack.Item.IconG, stack.Item.IconB, 0.9f)));

            // Quantity indicator (wider bar = more items)
            if (stack.Quantity > 1)
            {
                float qtyWidth = MathF.Min(40f, stack.Quantity * 4f);
                DrawRect(new HudElement(new Vector2(panelX + 30, itemY + 12), new Vector2(qtyWidth, 4),
                    new Vector4(0.7f, 0.7f, 0.7f, 0.6f)));
            }

            // Type indicator (color strip on right)
            var typeColor = stack.Item.Type switch
            {
                ItemType.Weapon => new Vector4(0.8f, 0.3f, 0.2f, 0.7f),
                ItemType.Armor => new Vector4(0.3f, 0.5f, 0.8f, 0.7f),
                ItemType.Consumable => new Vector4(0.3f, 0.8f, 0.3f, 0.7f),
                _ => new Vector4(0.5f, 0.5f, 0.5f, 0.5f)
            };
            DrawRect(new HudElement(new Vector2(panelX + panelW - 12, itemY), new Vector2(4, 18), typeColor));

            itemY -= 24;
        }

        // Key hints at bottom of panel
        float hintY = panelY + 8;
        // Small colored rectangles as key hints
        DrawRect(new HudElement(new Vector2(panelX + 8, hintY), new Vector2(50, 10),
            new Vector4(0.6f, 0.5f, 0.2f, 0.6f))); // "1-5 Equip"
        DrawRect(new HudElement(new Vector2(panelX + 65, hintY), new Vector2(40, 10),
            new Vector4(0.2f, 0.6f, 0.2f, 0.6f))); // "H Heal"
    }

    private void DrawDialoguePanel(DialogueState state, int screenW, int screenH)
    {
        var node = state.CurrentNode;
        if (node == null) return;

        float panelW = MathF.Min(600f, screenW - 40f);
        float panelH = 180f;
        float panelX = (screenW - panelW) / 2f;
        float panelY = 20f;

        // Dark panel background
        DrawRect(new HudElement(new Vector2(panelX, panelY), new Vector2(panelW, panelH),
            new Vector4(0.05f, 0.05f, 0.08f, 0.9f)));

        // Gold border top & bottom
        DrawRect(new HudElement(new Vector2(panelX, panelY + panelH - 2), new Vector2(panelW, 2),
            new Vector4(0.7f, 0.55f, 0.2f, 0.9f)));
        DrawRect(new HudElement(new Vector2(panelX, panelY), new Vector2(panelW, 2),
            new Vector4(0.7f, 0.55f, 0.2f, 0.9f)));

        // Speaker name indicator (colored bar at top)
        DrawRect(new HudElement(new Vector2(panelX + 10, panelY + panelH - 20), new Vector2(150, 14),
            new Vector4(0.5f, 0.4f, 0.2f, 0.8f)));

        // Text area indicator (light area showing text exists)
        DrawRect(new HudElement(new Vector2(panelX + 10, panelY + panelH - 60), new Vector2(panelW - 20, 30),
            new Vector4(0.15f, 0.15f, 0.18f, 0.5f)));

        // Choices or continue indicator
        if (node.Choices.Count > 0)
        {
            float choiceY = panelY + panelH - 80;
            for (int i = 0; i < node.Choices.Count; i++)
            {
                bool selected = i == state.SelectedChoice;
                float choiceAlpha = selected ? 0.7f : 0.3f;

                // Choice background
                DrawRect(new HudElement(new Vector2(panelX + 20, choiceY - i * 26), new Vector2(panelW - 40, 22),
                    new Vector4(0.2f, 0.18f, 0.12f, choiceAlpha)));

                // Selection arrow
                if (selected)
                {
                    DrawRect(new HudElement(new Vector2(panelX + 12, choiceY - i * 26 + 6), new Vector2(6, 10),
                        new Vector4(0.9f, 0.7f, 0.2f, 0.9f)));
                }
            }
        }
        else
        {
            // Continue indicator (blinking dot)
            DrawRect(new HudElement(new Vector2(panelX + panelW - 30, panelY + 10), new Vector2(12, 12),
                new Vector4(0.7f, 0.55f, 0.2f, 0.7f)));
        }
    }

    private void DrawNpcProximityHint(int screenW, int screenH)
    {
        // Check if player is near any NPC
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
                // Show "Press T to talk" indicator
                DrawRect(new HudElement(
                    new Vector2((screenW - 120) / 2f, 60),
                    new Vector2(120, 20),
                    new Vector4(0.3f, 0.3f, 0.15f, 0.7f)));
                break;
            }
        }
    }

    private void DrawDungeonHints(int screenW, int screenH)
    {
        // Check if player is near dungeon entrance (at 25, ?, 25)
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
            // "Press G to enter dungeon" hint
            DrawRect(new HudElement(
                new Vector2((screenW - 160) / 2f, 90),
                new Vector2(160, 22),
                new Vector4(0.3f, 0.15f, 0.1f, 0.8f)));
        }
    }

    private void DrawQuestTracker(int screenW, int screenH)
    {
        foreach (var (entity, player) in _world.Query<PlayerComponent>())
        {
            if (!_world.HasComponent<QuestJournal>(entity)) continue;
            var journal = _world.GetComponent<QuestJournal>(entity);
            if (journal.ActiveQuests.Count == 0) continue;

            float trackerX = 20f;
            float trackerY = screenH - 30f;

            // Show first active quest
            var quest = journal.ActiveQuests[0];

            // Quest title bar
            DrawRect(new HudElement(new Vector2(trackerX, trackerY), new Vector2(200, 18),
                new Vector4(0.3f, 0.25f, 0.1f, 0.7f)));

            // Objectives
            float objY = trackerY - 22;
            foreach (var obj in quest.Objectives)
            {
                // Objective background
                DrawRect(new HudElement(new Vector2(trackerX + 8, objY), new Vector2(185, 16),
                    new Vector4(0.1f, 0.1f, 0.12f, 0.6f)));

                // Progress bar
                float progress = obj.RequiredCount > 0
                    ? (float)obj.CurrentCount / obj.RequiredCount
                    : 0;
                float barWidth = 120f * progress;

                var barColor = obj.IsComplete
                    ? new Vector4(0.2f, 0.7f, 0.2f, 0.7f)
                    : new Vector4(0.6f, 0.5f, 0.2f, 0.6f);

                DrawRect(new HudElement(new Vector2(trackerX + 12, objY + 2), new Vector2(barWidth, 12),
                    barColor));

                // Completion checkmark (small green square)
                if (obj.IsComplete)
                {
                    DrawRect(new HudElement(new Vector2(trackerX + 170, objY + 3), new Vector2(10, 10),
                        new Vector4(0.2f, 0.8f, 0.2f, 0.9f)));
                }

                objY -= 20;
            }

            // Quest count indicator
            if (journal.ActiveQuests.Count > 1)
            {
                DrawRect(new HudElement(new Vector2(trackerX, objY), new Vector2(60, 12),
                    new Vector4(0.4f, 0.3f, 0.15f, 0.5f)));
            }

            break; // Only show first player's quests
        }
    }

    private void DrawCraftingPanel(CraftingState state, int screenW, int screenH)
    {
        float panelW = 280f;
        float panelH = 300f;
        float panelX = (screenW - panelW) / 2f;
        float panelY = (screenH - panelH) / 2f;

        // Panel background
        DrawRect(new HudElement(new Vector2(panelX, panelY), new Vector2(panelW, panelH),
            new Vector4(0.08f, 0.06f, 0.04f, 0.9f)));

        // Border
        DrawRect(new HudElement(new Vector2(panelX, panelY + panelH - 2), new Vector2(panelW, 2),
            new Vector4(0.6f, 0.45f, 0.2f, 0.9f)));
        DrawRect(new HudElement(new Vector2(panelX, panelY), new Vector2(panelW, 2),
            new Vector4(0.6f, 0.45f, 0.2f, 0.9f)));

        // Station type indicator (colored bar at top)
        var stationColor = state.StationType switch
        {
            CraftingStationType.Anvil => new Vector4(0.6f, 0.6f, 0.7f, 0.8f),
            CraftingStationType.Alchemy => new Vector4(0.3f, 0.7f, 0.4f, 0.8f),
            _ => new Vector4(0.6f, 0.45f, 0.25f, 0.8f)
        };
        DrawRect(new HudElement(new Vector2(panelX + 8, panelY + panelH - 24), new Vector2(160, 16), stationColor));

        // Get player inventory for ingredient checking
        InventoryComponent? playerInv = null;
        foreach (var (entity, player) in _world.Query<PlayerComponent>())
        {
            if (_world.HasComponent<InventoryComponent>(entity))
                playerInv = _world.GetComponent<InventoryComponent>(entity);
            break;
        }

        // Recipe list
        float recipeY = panelY + panelH - 50;
        for (int i = 0; i < state.AvailableRecipes.Count && i < 10; i++)
        {
            var recipe = state.AvailableRecipes[i];
            bool selected = i == state.SelectedRecipe;
            bool canCraft = playerInv != null && CraftingSystem.CanCraft(recipe, playerInv);

            // Selection highlight
            if (selected)
            {
                DrawRect(new HudElement(new Vector2(panelX + 4, recipeY - 2), new Vector2(panelW - 8, 28),
                    new Vector4(0.3f, 0.25f, 0.15f, 0.6f)));
            }

            // Recipe name bar (green if craftable, red if not)
            var nameColor = canCraft
                ? new Vector4(0.2f, 0.6f, 0.2f, 0.8f)
                : new Vector4(0.5f, 0.2f, 0.2f, 0.6f);
            DrawRect(new HudElement(new Vector2(panelX + 8, recipeY + 10), new Vector2(120, 14), nameColor));

            // Output item icon
            var outputItem = ItemDatabase.Get(recipe.OutputItemId);
            if (outputItem != null)
            {
                DrawRect(new HudElement(new Vector2(panelX + panelW - 30, recipeY + 6), new Vector2(18, 18),
                    new Vector4(outputItem.IconR, outputItem.IconG, outputItem.IconB, 0.9f)));
            }

            // Ingredient dots (small squares showing required materials)
            float dotX = panelX + 8;
            for (int j = 0; j < recipe.Ingredients.Count; j++)
            {
                var ingredient = recipe.Ingredients[j];
                var ingredientItem = ItemDatabase.Get(ingredient.ItemId);
                bool hasEnough = playerInv != null && playerInv.CountOf(ingredient.ItemId) >= ingredient.Count;

                if (ingredientItem != null)
                {
                    float alpha = hasEnough ? 0.9f : 0.3f;
                    DrawRect(new HudElement(new Vector2(dotX, recipeY - 2), new Vector2(10, 10),
                        new Vector4(ingredientItem.IconR, ingredientItem.IconG, ingredientItem.IconB, alpha)));
                }
                dotX += 14;
            }

            recipeY -= 34;
        }

        // Craft hint at bottom
        DrawRect(new HudElement(new Vector2(panelX + 8, panelY + 8), new Vector2(80, 14),
            new Vector4(0.5f, 0.4f, 0.2f, 0.7f))); // "Enter to Craft"
        DrawRect(new HudElement(new Vector2(panelX + 100, panelY + 8), new Vector2(60, 14),
            new Vector4(0.4f, 0.2f, 0.2f, 0.7f))); // "Esc to Close"
    }

    private void DrawSpellHotbar(SpellState state, ManaComponent mana, int screenW, int screenH)
    {
        var spells = SpellDatabase.GetSpellBar();
        float slotSize = 36f;
        float gap = 6f;
        float totalWidth = spells.Length * slotSize + (spells.Length - 1) * gap;
        float startX = (screenW - totalWidth) / 2f;
        float y = 60f;

        for (int i = 0; i < spells.Length; i++)
        {
            var spell = spells[i];
            float x = startX + i * (slotSize + gap);
            bool selected = i == state.SelectedSpell;
            bool onCooldown = state.Cooldowns[i] > 0;
            bool canAfford = mana.CanSpend(spell.ManaCost);

            // Slot background
            var bgColor = selected
                ? new Vector4(0.25f, 0.2f, 0.3f, 0.8f)
                : new Vector4(0.1f, 0.1f, 0.15f, 0.7f);
            DrawRect(new HudElement(new Vector2(x, y), new Vector2(slotSize, slotSize), bgColor));

            // Spell color icon
            float iconAlpha = (onCooldown || !canAfford) ? 0.3f : 0.9f;
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

            // Selection border
            if (selected)
            {
                DrawRect(new HudElement(new Vector2(x, y + slotSize - 2), new Vector2(slotSize, 2),
                    new Vector4(0.8f, 0.7f, 0.3f, 0.9f)));
            }
        }
    }

    private void DrawSkillMenu(LevelComponent level, SkillTree skills, int screenW, int screenH)
    {
        float panelW = 200f;
        float panelH = 180f;
        float panelX = 20f;
        float panelY = screenH - panelH - 60f;

        // Panel background
        DrawRect(new HudElement(new Vector2(panelX, panelY), new Vector2(panelW, panelH),
            new Vector4(0.06f, 0.04f, 0.1f, 0.85f)));

        // Border
        DrawRect(new HudElement(new Vector2(panelX, panelY + panelH - 2), new Vector2(panelW, 2),
            new Vector4(0.5f, 0.3f, 0.7f, 0.8f)));
        DrawRect(new HudElement(new Vector2(panelX, panelY), new Vector2(panelW, 2),
            new Vector4(0.5f, 0.3f, 0.7f, 0.8f)));

        // Skill points available indicator
        float spWidth = MathF.Min(80f, level.SkillPoints * 16f);
        DrawRect(new HudElement(new Vector2(panelX + 8, panelY + panelH - 22), new Vector2(spWidth, 14),
            new Vector4(0.9f, 0.8f, 0.2f, 0.8f)));

        // Strength bar (red) - Key 1
        float strY = panelY + panelH - 50;
        DrawRect(new HudElement(new Vector2(panelX + 8, strY), new Vector2(panelW - 16, 24),
            new Vector4(0.15f, 0.08f, 0.08f, 0.6f)));
        float strFill = (panelW - 16) * (skills.Strength / (float)SkillTree.MaxSkillLevel);
        DrawRect(new HudElement(new Vector2(panelX + 8, strY), new Vector2(strFill, 24),
            new Vector4(0.8f, 0.2f, 0.2f, 0.7f)));

        // Endurance bar (green) - Key 2
        float endY = strY - 34;
        DrawRect(new HudElement(new Vector2(panelX + 8, endY), new Vector2(panelW - 16, 24),
            new Vector4(0.08f, 0.15f, 0.08f, 0.6f)));
        float endFill = (panelW - 16) * (skills.Endurance / (float)SkillTree.MaxSkillLevel);
        DrawRect(new HudElement(new Vector2(panelX + 8, endY), new Vector2(endFill, 24),
            new Vector4(0.2f, 0.8f, 0.2f, 0.7f)));

        // Agility bar (blue) - Key 3
        float agiY = endY - 34;
        DrawRect(new HudElement(new Vector2(panelX + 8, agiY), new Vector2(panelW - 16, 24),
            new Vector4(0.08f, 0.08f, 0.15f, 0.6f)));
        float agiFill = (panelW - 16) * (skills.Agility / (float)SkillTree.MaxSkillLevel);
        DrawRect(new HudElement(new Vector2(panelX + 8, agiY), new Vector2(agiFill, 24),
            new Vector4(0.2f, 0.4f, 0.8f, 0.7f)));

        // Key hints
        DrawRect(new HudElement(new Vector2(panelX + 8, panelY + 8), new Vector2(50, 12),
            new Vector4(0.4f, 0.3f, 0.5f, 0.6f))); // "1/2/3"
    }

    private void DrawWeatherIndicator(int screenW, int screenH)
    {
        if (!_world.TryGetSingleton<WeatherState>(out var weather) || weather == null) return;

        float x = screenW - 170;
        float y = screenH - 15;

        // Weather type color indicator
        var weatherColor = weather.Current switch
        {
            WeatherType.Clear => new Vector4(0.4f, 0.6f, 0.9f, 0.6f),
            WeatherType.Cloudy => new Vector4(0.5f, 0.5f, 0.55f, 0.6f),
            WeatherType.Rain => new Vector4(0.3f, 0.4f, 0.7f, 0.7f),
            WeatherType.HeavyRain => new Vector4(0.2f, 0.3f, 0.6f, 0.8f),
            WeatherType.Fog => new Vector4(0.6f, 0.6f, 0.65f, 0.7f),
            WeatherType.Storm => new Vector4(0.2f, 0.15f, 0.4f, 0.8f),
            _ => new Vector4(0.5f, 0.5f, 0.5f, 0.5f)
        };

        // Background
        DrawRect(new HudElement(new Vector2(x, y), new Vector2(150, 10), new Vector4(0.1f, 0.1f, 0.1f, 0.4f)));

        // Intensity fill
        float fillWidth = 150f * weather.Intensity;
        DrawRect(new HudElement(new Vector2(x, y), new Vector2(fillWidth, 10), weatherColor));
    }

    private void DrawSaveNotification(int screenW, int screenH)
    {
        // Find SaveLoadSystem via world singleton pattern - check all systems
        // Use a simple approach: look for notification state on entities
        // Actually, we reference the system directly through a registered service
        if (_world.TryGetSingleton<SaveNotification>(out var notif) && notif!.Timer > 0)
        {
            float alpha = MathF.Min(1f, notif.Timer);
            DrawRect(new HudElement(
                new Vector2((screenW - 200) / 2f, screenH - 40),
                new Vector2(200, 24),
                new Vector4(0.15f, 0.4f, 0.15f, 0.8f * alpha)));
        }
    }

    private void DrawCraftingStationHint(int screenW, int screenH)
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
                // "Press C to craft" hint
                DrawRect(new HudElement(
                    new Vector2((screenW - 140) / 2f, 120),
                    new Vector2(140, 20),
                    new Vector4(0.35f, 0.25f, 0.1f, 0.7f)));
                break;
            }
        }
    }

    private void DrawMinimap(int screenW, int screenH)
    {
        const float mapSize = MinimapData.MapSize;
        const int res = MinimapData.Resolution;
        const float range = MinimapData.WorldRange;
        float mapX = screenW - mapSize - 20;
        float mapY = screenH - mapSize - 20;
        float cellSize = mapSize / res;

        // Get player position for centering
        Vector3 playerPos = Vector3.Zero;
        foreach (var (entity, player, transform) in _world.Query<PlayerComponent, TransformComponent>())
        {
            playerPos = transform.Position;
            break;
        }

        // Background
        DrawRect(new HudElement(new Vector2(mapX - 2, mapY - 2), new Vector2(mapSize + 4, mapSize + 4),
            new Vector4(0.05f, 0.05f, 0.05f, 0.7f)));

        // Terrain grid
        if (_terrainGen != null)
        {
            for (int z = 0; z < res; z++)
            {
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
        }

        // Player marker (white dot, center)
        float playerMarkerX = mapX + mapSize / 2f - 3;
        float playerMarkerY = mapY + mapSize / 2f - 3;
        DrawRect(new HudElement(new Vector2(playerMarkerX, playerMarkerY), new Vector2(6, 6),
            new Vector4(1f, 1f, 1f, 0.9f)));

        // NPC markers (green)
        foreach (var (entity, npc, transform) in _world.Query<NpcComponent, TransformComponent>())
        {
            var markerPos = WorldToMinimap(transform.Position, playerPos, mapX, mapY, mapSize, range);
            if (markerPos.HasValue)
            {
                DrawRect(new HudElement(new Vector2(markerPos.Value.X - 2, markerPos.Value.Y - 2),
                    new Vector2(5, 5), new Vector4(0.2f, 0.8f, 0.2f, 0.9f)));
            }
        }

        // Enemy markers (red)
        foreach (var (entity, enemy, health, transform) in QueryEnemiesForMinimap())
        {
            if (!health.IsAlive) continue;
            var markerPos = WorldToMinimap(transform.Position, playerPos, mapX, mapY, mapSize, range);
            if (markerPos.HasValue)
            {
                DrawRect(new HudElement(new Vector2(markerPos.Value.X - 2, markerPos.Value.Y - 2),
                    new Vector2(4, 4), new Vector4(0.9f, 0.2f, 0.2f, 0.8f)));
            }
        }

        // Crafting station markers (orange)
        foreach (var (entity, station, transform) in _world.Query<CraftingComponent, TransformComponent>())
        {
            var markerPos = WorldToMinimap(transform.Position, playerPos, mapX, mapY, mapSize, range);
            if (markerPos.HasValue)
            {
                DrawRect(new HudElement(new Vector2(markerPos.Value.X - 2, markerPos.Value.Y - 2),
                    new Vector2(5, 5), new Vector4(0.9f, 0.6f, 0.1f, 0.8f)));
            }
        }

        // Boss markers (magenta)
        foreach (var (entity, boss, health) in _world.Query<BossComponent, HealthComponent>())
        {
            if (!health.IsAlive) continue;
            if (!_world.HasComponent<TransformComponent>(entity)) continue;
            var bossTransform = _world.GetComponent<TransformComponent>(entity);
            var markerPos = WorldToMinimap(bossTransform.Position, playerPos, mapX, mapY, mapSize, range);
            if (markerPos.HasValue)
            {
                DrawRect(new HudElement(new Vector2(markerPos.Value.X - 3, markerPos.Value.Y - 3),
                    new Vector2(7, 7), new Vector4(0.8f, 0.2f, 0.8f, 0.9f)));
            }
        }

        // Border
        DrawRect(new HudElement(new Vector2(mapX - 2, mapY - 2), new Vector2(mapSize + 4, 2),
            new Vector4(0.4f, 0.35f, 0.2f, 0.7f)));
        DrawRect(new HudElement(new Vector2(mapX - 2, mapY + mapSize), new Vector2(mapSize + 4, 2),
            new Vector4(0.4f, 0.35f, 0.2f, 0.7f)));
    }

    private static Vector4 HeightToColor(float height)
    {
        if (height < 1f) // Water
            return new Vector4(0.15f, 0.25f, 0.5f, 0.8f);
        if (height < 3f) // Beach/sand
            return new Vector4(0.5f, 0.45f, 0.3f, 0.7f);
        if (height < 8f) // Grass
            return new Vector4(0.2f, 0.4f, 0.15f, 0.7f);
        if (height < 15f) // Forest/hill
            return new Vector4(0.15f, 0.3f, 0.1f, 0.7f);
        // Mountain
        return new Vector4(0.35f, 0.35f, 0.3f, 0.7f);
    }

    private static Vector2? WorldToMinimap(Vector3 worldPos, Vector3 playerPos,
        float mapX, float mapY, float mapSize, float range)
    {
        float dx = worldPos.X - playerPos.X;
        float dz = worldPos.Z - playerPos.Z;

        if (MathF.Abs(dx) > range || MathF.Abs(dz) > range) return null;

        float nx = (dx / range + 1f) * 0.5f; // 0..1
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

    private void DrawBossHealthBar(int screenW, int screenH)
    {
        foreach (var (entity, boss, health) in _world.Query<BossComponent, HealthComponent>())
        {
            if (!boss.IsActivated || !health.IsAlive) continue;

            var bossData = BossDatabase.Get(boss.BossId);
            if (bossData == null) continue;

            float barW = 400f;
            float barH = 20f;
            float barX = (screenW - barW) / 2f;
            float barY = screenH - 40f;

            // Boss name bar (colored by boss)
            DrawRect(new HudElement(new Vector2(barX, barY + barH + 4), new Vector2(160, 14),
                new Vector4(bossData.ColorR, bossData.ColorG, bossData.ColorB, 0.8f)));

            // Health bar background
            DrawRect(new HudElement(new Vector2(barX, barY), new Vector2(barW, barH),
                new Vector4(0.1f, 0.05f, 0.05f, 0.8f)));

            // Health bar fill (red to orange based on phase)
            float healthFill = barW * health.Percentage;
            var fillColor = boss.CurrentPhase switch
            {
                >= 2 => new Vector4(0.9f, 0.2f, 0.1f, 0.9f),
                1 => new Vector4(0.9f, 0.5f, 0.1f, 0.9f),
                _ => new Vector4(0.8f, 0.15f, 0.15f, 0.85f)
            };
            DrawRect(new HudElement(new Vector2(barX, barY), new Vector2(healthFill, barH), fillColor));

            // Phase indicator dots
            for (int i = 0; i < boss.MaxPhases; i++)
            {
                float dotX = barX + barW + 8 + i * 14;
                var dotColor = i < boss.CurrentPhase
                    ? new Vector4(1f, 0.3f, 0.1f, 0.9f)
                    : new Vector4(0.3f, 0.3f, 0.3f, 0.6f);
                DrawRect(new HudElement(new Vector2(dotX, barY + 4), new Vector2(10, 12), dotColor));
            }

            // Enraged indicator
            if (boss.State == BossState.Enraged)
            {
                DrawRect(new HudElement(new Vector2(barX, barY - 4), new Vector2(barW, 2),
                    new Vector4(1f, 0.3f, 0f, 0.9f)));
            }

            break; // Only show first active boss
        }
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
        _quad.Dispose();
    }
}
