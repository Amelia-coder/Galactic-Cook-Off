using Godot;
public interface IMovable
{
    Vector3 Velocity { get; set; }
    bool IsTouchingFloor { get; }
    bool CanJump { get; }
    bool TryJump();
    Vector3 GetMovementDirection(Vector2 input);
}