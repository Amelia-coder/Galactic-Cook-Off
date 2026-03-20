using Godot;
public abstract partial class MovementState : State<IPlayerContext>
{
	public virtual float StaminaConsumptionPerSecond => 0f;
	public virtual float StaminaRegenPerSecond => 0f;

	public override void Initialize(IPlayerContext movableObjext)
	{
		base.Initialize(movableObjext);
	}
}
