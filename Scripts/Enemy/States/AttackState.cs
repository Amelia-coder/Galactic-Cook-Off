using Godot;

public partial class AttackState : State<IEnemyEntity>
{
	public override void Enter()
	{
		//y play animation
	}


	public override void PhysicsUpdate(double delta)
	{
		var _movement = Entity.GetComponent<MovementComponent>();
		var _stamina = Entity.GetComponent<StaminaComponent>();
		var _input = Entity.GetComponent<InputComponent>();
		_input.Update();

		
	}
}
