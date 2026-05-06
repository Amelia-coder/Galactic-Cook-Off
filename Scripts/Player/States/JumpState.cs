using Godot;
using Scripts.Player.Components;
using Scripts.Game;

namespace Scripts.Player.States
{
	public partial class JumpState : MovementState
	{
		[Export] public float AirSpeed { get; set; } = 5.0f;
		[Export] public float AirControl { get; set; } = 0.6f; // How responsive movement is in air (0-1)
		[Export] public float JumpCutMultiplier { get; set; } = 0.5f; // How much to reduce upward velocity when releasing jump early

		private bool _jumpReleased = false;


		public override void Enter()
		{
			GD.Print("Entered Airborne State");
			_jumpReleased = false;
		}

		public override void PhysicsUpdate(double delta)
		{
			var _movement = Entity.GetComponent<MovementComponent>();
			var _stamina = Entity.GetComponent<StaminaComponent>();
			var _input = Entity.GetComponent<InputComponent>();
			_input.Update();

			// Variable jump height: if player releases jump early, cut upward velocity
			if (!_jumpReleased && !_input.JumpPressed && _movement.Velocity.Y > 0)
			{
				_movement.SetVerticalVelocity(_movement.Velocity.Y * JumpCutMultiplier);
				_jumpReleased = true;
			}

			// Air control: lerp toward desired direction instead of instant change
			if (_input.MoveDirection.LengthSquared() > 0.01f)
			{
				Vector3 currentHorizontal = new Vector3(_movement.Velocity.X, 0, _movement.Velocity.Z);
				Vector3 targetHorizontal = _input.MoveDirection * AirSpeed;
				Vector3 newHorizontal = currentHorizontal.Lerp(targetHorizontal, AirControl);

				_movement.SetHorizontalVelocity(newHorizontal);
			}

			// Apply physics
			_movement.Update((float)delta);

			// Transition when landing
			if (_movement.IsGrounded)
			{
				if (_input.MoveDirection.LengthSquared() > 0.01f)
				{
					if (_input.SprintPressed)
						TransitionTo("RunState");
					else
						TransitionTo("WalkState");
				}
				else
				{
					TransitionTo("IdleState");
				}
			}
		}
	}
}
