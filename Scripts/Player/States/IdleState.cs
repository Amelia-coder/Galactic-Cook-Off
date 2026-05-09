using Godot;
using Scripts.Player.Components;

namespace Scripts.Player.States
{
	public partial class IdleState : MovementState
	{
		public override float StaminaRegenPerSecond => 10f;
		[Export] public float Speed { get; set; } = 0.0f;
		public override void Enter()
		{
			var movementComponent = Entity.GetComponent<MovementComponent>();
			GD.Print($"is entity null?", Entity == null);
		}

		public override void PhysicsUpdate(double delta)
		{
			//better add sometying like initiliaze for sppeding up logic and escaping search on each tick
			var _movement = Entity.GetComponent<MovementComponent>();
			var _stamina = Entity.GetComponent<StaminaComponent>();
			var _input = Entity.GetComponent<InputComponent>();
			// 1. Read from InputComponent (not raw Input)
;

			// 2. Try actions based on input
			if (_input.JumpPressed && _movement.TryJump())
			{
				TransitionTo("JumpState");
				return;
			}

			// 3. Apply movement
			if (_input.MoveDirection.LengthSquared() > 0.01f)
			{
				if (_input.SprintPressed)
					TransitionTo("RunState");
				else
					TransitionTo("WalkState");
				return;
			}

			// 4. Idle behavior - just stand still and regen
			_movement.SetHorizontalVelocity(Vector3.Zero);
			_stamina.Regen(StaminaRegenPerSecond, (float)delta);
		}
	}
}
