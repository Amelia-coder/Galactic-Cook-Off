using Godot;
using Scripts.Game.GenericComponents;

namespace Scripts.Player.Components
{
	public partial class PlayerMovementComponent : GenericMovementComponent
	{
		private CharacterBody3D _body;
		private StaminaComponent _stamina;

		[Export] public float JumpStaminaCost { get; set; } = 10f; // think about moving stamina elsewhere
		[Export] public float SprintStaminaPerSecond { get; set; } = 15f;

		public void Initialize(CharacterBody3D body, StaminaComponent stamina)
		{
			_body = body;
			_stamina = stamina;
		}

		// One-time action: jump
		public bool TryJump()
		{
			if (!IsGrounded)
				return false;

			if (!_stamina.TryConsume(JumpStaminaCost))
				return false;

			Velocity = new Vector3(Velocity.X, JumpForce, Velocity.Z);
			return true;
		}

		// Continuous action: sprint (consumes stamina per frame)
		public bool TrySetSprintVelocity(Vector3 direction, float speed, float delta)
		{
			float staminaCost = SprintStaminaPerSecond * delta;

			if (!_stamina.TryConsume(staminaCost))
			{
                GD.Print("QWWWWWWWWWWWW");
                return false;
			}

			SetHorizontalVelocity(direction * speed);
			return true;
		}

		// Check if sprint is possible (for CanEnter checks)
		public bool CanSprint(float delta)
		{
			float staminaCost = SprintStaminaPerSecond * delta;
			return _stamina.CanConsume(staminaCost);
		}

		
		public override void Update(float delta)
		{
			UpdateGroundedState();
			ApplyGravity(delta);
			ApplyToBody();
		}

		private void UpdateGroundedState()
		{
			IsGrounded = _body.IsOnFloor();
		}

		private void ApplyGravity(float delta)
		{
			if (!IsGrounded)
			{
				Velocity = new Vector3(Velocity.X, Velocity.Y - Gravity * delta, Velocity.Z);
			}
			else if (Velocity.Y < 0)
			{
				Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
			}
		}

		private void ApplyToBody()
		{
			_body.Velocity = Velocity;
			_body.MoveAndSlide();
		}
	}
}
