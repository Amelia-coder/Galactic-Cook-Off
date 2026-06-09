using Godot;
using Scripts.Game.GenericComponents;
using Scripts.Player.Components;

namespace Scripts.Player.States
{
	public partial class RunState : MovementState
	{
		[Export] public float SprintSpeed { get; set; } = 8.0f;


		public override void Enter()
		{
			//Entity.GetComponent<GenericAnimationComponent>()
			//.SetCurrent(EntityAnimation.Run);
			var _movement = Entity.GetComponent<PlayerMovementComponent>();
			var _input = Entity.GetComponent<InputComponent>();
			if (!_movement.TrySetSprintVelocity(_input.MoveDirection, SprintSpeed, (float)0.01))
			{
				GD.Print("will have to enter wal state");
				TransitionTo("WalkState");
				return;
			}
			//GD.Print("Entered RunState");
		}

		public override void PhysicsUpdate(double delta)
		{
			var _movement = Entity.GetComponent<PlayerMovementComponent>();
			var _input = Entity.GetComponent<InputComponent>();


			if (_input.JumpPressed && _movement.TryJump())
			{
				TransitionTo("JumpState");
				return;
			}
			
			if (!_movement.IsGrounded) //prevents running in air
				return;

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
				GD.Print("will have to enter wal state");
				TransitionTo("WalkState");
				return;
			}
		}

		public override bool CanEnter()
		{
			var _movement = Entity.GetComponent<PlayerMovementComponent>();
			// Can only enter if we have enough stamina for at least one frame
			return _movement.CanSprint(1f / 60f); // Assume 60 FPS check
		}
	}
}
