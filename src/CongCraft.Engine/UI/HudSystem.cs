using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Inventory;
using CongCraft.Engine.Procedural;
using CongCraft.Engine.Rendering;
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

    public void Initialize(ServiceLocator services)
    {
        _gl = services.Get<GL>();
        _world = services.Get<World>();
        _window = services.Get<IWindow>();
        _hudShader = new Shader(_gl, ShaderSources.HudVertex, ShaderSources.HudFragment);
        _quad = PrimitiveMeshBuilder.CreateUnitQuad(_gl);
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
        }

        // Minimap placeholder (top-right)
        DrawRect(new HudElement(
            new Vector2(w - 170, h - 170),
            new Vector2(150, 150),
            new Vector4(0.1f, 0.1f, 0.1f, 0.5f)));

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
