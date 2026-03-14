using Godot;

public interface IEntity
{
    bool IsTouchingFloor { get; }
    bool CanJump { get; }
    void PlayAnimation(string name);

    Vector3 GetMovementDirection(Vector2 input);
    void SetLookDirection(Vector3 direction);
}