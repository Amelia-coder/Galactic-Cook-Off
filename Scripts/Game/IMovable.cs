using Godot;
public interface IMovable
{
    Vector3 Velocity { get; set; }
    bool IsTouchingFloor { get; }

    void Move(Vector3 direction, float speed);
}