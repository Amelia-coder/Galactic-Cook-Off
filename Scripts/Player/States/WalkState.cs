using Godot;
using Scripts.Game.GenericComponents;
using Scripts.Player.Components;

namespace Scripts.Player.States
{
	public partial class WalkState : MovementState
	{
		[Export] public float WalkSpeed { get; set; } = 4.0f;

		public override void Enter()
		{
			//Entity.GetComponent<GenericAnimationComponent>()
			//.SetCurrent(EntityAnimation.Walk);
			var _movement = Entity.GetComponent<PlayerMovementComponent>();
			var _input = Entity.GetComponent<InputComponent>();
			_movement.SetHorizontalVelocity(_input.MoveDirection * WalkSpeed);

			//GD.Print("Entered WalkState");
		}

		public override void PhysicsUpdate(double delta)
		{
			var _movement = Entity.GetComponent<PlayerMovementComponent>();
			var _stamina = Entity.GetComponent<StaminaComponent>();
			var _input = Entity.GetComponent<InputComponent>();


			// Try jump
			if (_input.JumpPressed && _movement.TryJump())
			{
				TransitionTo("JumpState");
				return;
			}

			// Check for sprint
			if (_stamina.CanConsume(0.1f) && _input.SprintPressed && _input.MoveDirection.LengthSquared() > 0.01f)
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
