using Godot;
public abstract partial class MovementState : State<IPlayerContext>
{
	public virtual float StaminaConsumptionPerSecond => 0f;
	public virtual float StaminaRegenPerSecond => 0f;

	protected StaminaComponent _staminaComponent;

	public override void Initialize(IPlayerContext movableObjext)
	{
		base.Initialize(movableObjext);
		_staminaComponent = movableObjext.Stamina;
	}
}
