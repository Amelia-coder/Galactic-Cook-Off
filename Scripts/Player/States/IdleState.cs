using Godot;
using System;
using System.Text.RegularExpressions;

public partial class IdleState : MovementState
{
	public override float StaminaRegenPerSecond => 10f;
	[Export] public float Speed { get; set; } = 0.0f;
	public override void Enter()
	{
		Entity.Velocity = new Vector3(0, 0, 0);
	}

	public override void PhysicsUpdate(double delta)
	{

		if (Input.IsActionJustPressed("jump") && Entity.CanJump)
		{

			GD.Print($"We jumped!", Entity.CanJump);
			TransitionTo("JumpState");
		}
		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		if (inputDir != Vector2.Zero)
		{
			TransitionTo("WalkState");
		}

		if (Input.IsActionJustPressed("sprint"))
		{
			GD.Print($"We are running!", Entity.CanJump);
			TransitionTo("RunState");
		}
		_staminaComponent.Regen(StaminaRegenPerSecond, (float)delta);
	}
}
