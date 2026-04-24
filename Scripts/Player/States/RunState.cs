using Godot;

public partial class RunState : MovementState
{
	[Export] public float SprintSpeed { get; set; } = 8.0f;
 

	public override void Enter()
	{
		GD.Print("Entered RunState");
	}

	public override void PhysicsUpdate(double delta)
	{
		var _movement = Entity.GetComponent<MovementComponent>();
		var _stamina = Entity.GetComponent<StaminaComponent>();
		var  _input = Entity.GetComponent<InputComponent>();
		
		_input.Update();

		// Try jump
		if (_input.JumpPressed && _movement.TryJump())
		{
			TransitionTo("JumpState");
			return;
		}

		// Check if still moving
		if (_input.MoveDirection.LengthSquared() < 0.01f)
		{
			TransitionTo("IdleState");
			return;
		}

		// Check if sprint button released - transition to walk
		if (!_input.SprintPressed)
		{
			TransitionTo("WalkState");
			return;
		}

		// Try to sprint - if stamina runs out, fall back to walk
		if (!_movement.TrySetSprintVelocity(_input.MoveDirection, SprintSpeed, (float)delta))
		{
			TransitionTo("WalkState");
			return;
		}

		// Apply physics
		_movement.Update((float)delta);
	}

	public override bool CanEnter()
	{
		var _movement = Entity.GetComponent<MovementComponent>();
		// Can only enter if we have enough stamina for at least one frame
		return _movement.CanSprint(1f / 60f); // Assume 60 FPS check
	}
}
