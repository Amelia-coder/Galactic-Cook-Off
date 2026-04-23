using Godot;
public abstract partial class MovementState : State<IEntity>
{
	public virtual float StaminaConsumptionPerSecond => 0f; //нужно ли оно здесь? например, прыжок потребляет вынсливость только при входе в состояние, а вот над зарядом силы броска надо подумать
	public virtual float StaminaRegenPerSecond => 0f;


	public override void Initialize(IEntity movableObject)
	{
		base.Initialize(movableObject);
	}
}
