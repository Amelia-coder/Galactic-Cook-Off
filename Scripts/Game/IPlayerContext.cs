using Godot;

public interface IPlayerContext : IMovable
{
    StaminaComponent Stamina  { get; }
    HealthComponent Health { get; } 
    IThrowable HeldItem { get; }
    void TryPickUp(Vector3 lookDirection, float minDot = 0.5f);
    void TryThrow(Vector3 impulse);
    void TryDrop();

    Vector3 ForwardDir { get; }
}