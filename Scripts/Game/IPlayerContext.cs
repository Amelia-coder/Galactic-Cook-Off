using Godot;

public interface IPlayerContext : IMovable
{
    StaminaComponent Stamina  { get; }
    HealthComponent Health { get; } //опасно! подумать о том, как ращершитиь игроку в него пиать, а сотльным - только читать
    IThrowable HeldItem { get; }
    void TryPickUp(Vector3 lookDirection, float minDot = 0.5f);
    void TryThrow(Vector3 impulse);
    void TryDrop();

    Vector3 ForwardDir { get; }
}