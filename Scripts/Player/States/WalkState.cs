using Godot;

public partial class WalkState : MovementState
{
	public override float StaminaRegenPerSecond => 1f;
	[Export] public float Speed { get; set; } = 5.0f;

	public override void Enter()
	{
	}


	public override void PhysicsUpdate(double delta)
	{
		if (Input.IsActionJustPressed("jump") && Entity.CanJump)
		{
			GD.Print($"We jumped!", Entity.CanJump);
			TransitionTo("JumpState");
		}

		if (Input.IsActionPressed("sprint") && Input.IsActionPressed("move_forward") && CanEnter())
		{
			TransitionTo("RunState");
		}

		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		if (inputDir == Vector2.Zero)
		{
			TransitionTo("IdleState");
			return;
		}

		Vector3 moveDirection = Entity.GetMovementDirection(inputDir);

		Entity.Velocity = new Vector3(
			moveDirection.X * Speed,
			Entity.Velocity.Y,
			moveDirection.Z * Speed
		);
		_staminaComponent.Regen(StaminaRegenPerSecond, (float)delta);
		
	}
}
