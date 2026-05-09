using Godot;
using Scripts.Player.Components;

namespace Scripts.Player.States
{
	public partial class WalkState : MovementState
	{
		[Export] public float WalkSpeed { get; set; } = 4.0f;

		public override void Enter()
		{
			GD.Print("Entered WalkState");
		}

		public override void PhysicsUpdate(double delta)
		{
			var _movement = Entity.GetComponent<MovementComponent>();
			var _stamina = Entity.GetComponent<StaminaComponent>();
			var _input = Entity.GetComponent<InputComponent>();


			// Try jump
			if (_input.JumpPressed && _movement.TryJump())
			{
				TransitionTo("JumpState");
				return;
			}

			// Check for sprint
			if (_input.SprintPressed && _input.MoveDirection.LengthSquared() > 0.01f)
			{
				TransitionTo("RunState");
				return;
			}

			// Check for idle
			if (_input.MoveDirection.LengthSquared() < 0.01f)
			{
				TransitionTo("IdleState");
				return;
			}

			// Walk movement
			_movement.SetHorizontalVelocity(_input.MoveDirection * WalkSpeed);

			// Regenerate stamina while walking
			_stamina.Regen(StaminaRegenPerSecond, (float)delta);
		}
	}
}
