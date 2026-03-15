// JumpState.cs
using Godot;

public partial class JumpState : MovementState
{
	[Export] public float JumpVelocity { get; set; } = 7.0f;
	[Export] public float AirControl { get; set; } = 0.3f;
	[Export] public float AirAcceleration { get; set; } = 15f;
	[Export] public float VariableJumpCut { get; set; } = 0.5f;
	public override float StaminaConsumptionPerSecond => 10f;

	public override void Enter()
	{
		Entity.Velocity = Vector3.Up * 5.0f;
		Entity.Stamina.TryConsume(StaminaConsumptionPerSecond);
	}

	public override void Exit()
	{

	}

	public override void PhysicsUpdate(double delta)
	{

		Vector2 inputDir = Input.GetVector("left", "right", "forward", "back");

		Vector3 moveDirection = Entity.GetMovementDirection(inputDir);

		Entity.Velocity = new Vector3(
			moveDirection.X * JumpVelocity,
			Entity.Velocity.Y,
			moveDirection.Z * JumpVelocity
		);

		// ground transition
		if (Entity.IsTouchingFloor)
		{
			if (inputDir.Length() > 0.1f)
				TransitionTo("WalkState");
			else
				TransitionTo("IdleState");
		}
	}
}
