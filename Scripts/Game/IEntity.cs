// IEntity.cs
using Godot;

public interface IEntity
{
    Vector3 Velocity { get; set; }
    bool IsTouchingFloor { get; }
    bool CanJump { get; }
    void PlayAnimation(string name);

    Vector3 GetMovementDirection(Vector2 input);
    void SetLookDirection(Vector3 direction);

    Node AsNode();
}