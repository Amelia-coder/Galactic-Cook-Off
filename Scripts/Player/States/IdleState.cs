using Godot;
using System;
using System.Text.RegularExpressions;

public partial class IdleState : MovementState
{
	public override float StaminaRegenPerSecond => 10f;
	[Export] public float Speed { get; set; } = 0.0f;
	public override void Enter()
	{
		//Чтобы не сбрасывать гравитацию
		Entity.Velocity = new Vector3(0, Entity.Velocity.Y, 0);
	}

	public override void PhysicsUpdate(double delta)
	{

		if (Input.IsActionJustPressed("jump") && Entity.TryJump())
		{

			GD.Print($"We jumped!", Entity.CanJump);
			TransitionTo("JumpState");
		}
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "back");
		if (inputDir != Vector2.Zero)
		{
			TransitionTo("WalkState");
		}

		if (Input.IsActionJustPressed("sprint"))
		{
			GD.Print($"We are running!", Entity.CanJump);
			TransitionTo("RunState");
		}
		Entity.Stamina.Regen(StaminaRegenPerSecond, (float)delta);

	}
}
