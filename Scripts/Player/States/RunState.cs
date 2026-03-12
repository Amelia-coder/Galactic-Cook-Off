using Godot;
using System;

public partial class RunState : MovementState
{
	public override float StaminaConsumptionPerSecond => 10f;


	[Export] public float Speed { get; set; } = 40.0f;

	public override void Enter()
	{
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "back");
		Vector3 moveDirection = Entity.GetMovementDirection(inputDir);

		Entity.Velocity = new Vector3(
			moveDirection.X * Speed,
			Entity.Velocity.Y,
			moveDirection.Z * Speed
		);
	}



	public override void PhysicsUpdate(double delta)
	{


		if (Input.IsActionJustPressed("jump") && Entity.CanJump)
		{
			GD.Print($"We jumped!", Entity.CanJump);
			TransitionTo("JumpState");
		}

		Vector2 inputDir = Input.GetVector("left", "right", "forward", "back");

		
		if (inputDir == Vector2.Zero)
		{
			TransitionTo("IdleState");
			return;
		}

		if (!CanEnter())
		{
			TransitionTo("WalkState");
		}

		_staminaComponent.TryConsume(StaminaConsumptionPerSecond);
		Vector3 moveDirection = Entity.GetMovementDirection(inputDir);

		Entity.Velocity = new Vector3(
			moveDirection.X * Speed,
			Entity.Velocity.Y,
			moveDirection.Z * Speed
		);
		//// Rotate player to face direction
		//if (Entity.AsNode() is Player player)
		//{
		//    player.LookDirection(direction);
		//}

		//Entity.PlayAnimation("walk");
	}
	public override bool CanEnter()
	{
		GD.Print($"We know have stima:", _staminaComponent.CanConsume(StaminaConsumptionPerSecond));
		return _staminaComponent.CanConsume(StaminaConsumptionPerSecond);
	}
}
