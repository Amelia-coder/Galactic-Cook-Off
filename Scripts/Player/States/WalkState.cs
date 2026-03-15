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

		if (Input.IsActionPressed("sprint") && Input.IsActionPressed("forward") && CanEnter())
		{
			TransitionTo("RunState");
		}

		Vector2 inputDir = Input.GetVector("left", "right", "forward", "back");
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
		Entity.Stamina.Regen(StaminaRegenPerSecond, (float)delta);
		
	}
}
