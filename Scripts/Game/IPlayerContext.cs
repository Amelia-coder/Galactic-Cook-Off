using Godot;

public interface IPlayerContext : IMovable
{
    StaminaComponent Stamina { get; }
    HealthComponent Health { get; } //опасно! подумать о том, как ращершитиь игроку в него пиать, а сотльным - только читать
}