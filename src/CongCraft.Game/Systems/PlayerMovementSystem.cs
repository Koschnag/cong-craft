using System.Numerics;
using CongCraft.Engine.Core;
using CongCraft.Engine.ECS;
using CongCraft.Engine.ECS.Systems;
using CongCraft.Engine.Input;
using CongCraft.Engine.Rendering;
using CongCraft.Engine.Terrain;
using Silk.NET.Input;

namespace CongCraft.Game.Systems;

/// <summary>
/// Reads input and moves the player entity. Handles camera rotation and ground clamping.
/// </summary>
public sealed class PlayerMovementSystem : ISystem
{
    public int Priority => 10;

    private World _world = null!;
    private Camera _camera = null!;
    private TerrainGenerator _terrainGen = null!;
    private float _mouseSensitivity = 0.15f;

    public void Initialize(ServiceLocator services)
    {
        _world = services.Get<World>();
        _camera = services.Get<Camera>();
        _terrainGen = services.Get<TerrainGenerator>();
    }

    public void Update(GameTime time)
    {
        var input = _world.GetSingleton<InputState>();
        float dt = time.DeltaTimeF;

        // Camera rotation from mouse
        if (input.IsMouseCaptured)
        {
            _camera.Rotate(
                input.MouseDelta.X * _mouseSensitivity,
                input.MouseDelta.Y * _mouseSensitivity
            );
        }

        // Camera zoom from scroll
        if (input.ScrollDelta != 0)
            _camera.Zoom(input.ScrollDelta * 0.5f);

        // Player movement
        foreach (var (entity, player, transform) in _world.Query<PlayerComponent, TransformComponent>())
        {
            // Check combat state for movement modifiers
            CombatComponent? combat = _world.HasComponent<CombatComponent>(entity)
                ? _world.GetComponent<CombatComponent>(entity) : null;

            player.IsRunning = input.IsKeyDown(Key.ShiftLeft);
            float speed = player.IsRunning ? player.EffectiveRunSpeed : player.EffectiveMoveSpeed;

            // Slow down while blocking
            if (combat?.IsBlocking == true) speed *= 0.3f;
            // Can't move while attacking
            if (combat?.IsAttacking == true) speed *= 0.2f;

            var moveDir = Vector3.Zero;

            // Dodge overrides normal movement
            if (combat?.IsDodging == true)
            {
                moveDir = _camera.Forward; // Dodge forward
                speed = combat.DodgeDistance / combat.DodgeDuration;
            }
            else
            {
                if (input.IsKeyDown(Key.W)) moveDir += _camera.Forward;
                if (input.IsKeyDown(Key.S)) moveDir -= _camera.Forward;
                if (input.IsKeyDown(Key.A)) moveDir -= _camera.Right;
                if (input.IsKeyDown(Key.D)) moveDir += _camera.Right;
            }

            if (moveDir.LengthSquared() > 0.001f)
            {
                moveDir = Vector3.Normalize(moveDir);
                var newPos = transform.Position + moveDir * speed * dt;

                // Ground clamping
                float terrainHeight = _terrainGen.GetHeightAt(newPos.X, newPos.Z);
                newPos.Y = terrainHeight + 0.9f; // Player capsule half-height

                transform.Position = newPos;

                // Rotate player to face movement direction
                float targetYaw = MathF.Atan2(moveDir.X, moveDir.Z);
                transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, targetYaw);
            }
            else
            {
                // Even when standing still, clamp to terrain
                var pos = transform.Position;
                pos.Y = _terrainGen.GetHeightAt(pos.X, pos.Z) + 0.9f;
                transform.Position = pos;
            }

            // Update component back (class reference, so already updated)
            _camera.Target = transform.Position;

            // Toggle mouse capture with Escape
            if (input.IsKeyPressed(Key.Escape))
                input.IsMouseCaptured = !input.IsMouseCaptured;
        }
    }

    public void Render(GameTime time) { }
    public void Dispose() { }
}
