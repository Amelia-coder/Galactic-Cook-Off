using Godot;
public abstract partial class MovementState : State
{
	public virtual float StaminaConsumptionPerSecond => 0f;
	public virtual float StaminaRegenPerSecond => 0f;

	protected StaminaComponent _staminaComponent;

	public void Initialize(IEntity entity, StaminaComponent stamina)
	{
		base.Initialize(entity);
		_staminaComponent = stamina;
	}
}
