using Godot;
using Scripts.Game;

namespace Scripts.Player.Components
{
	public partial class MovementComponent : Component
	{
		private CharacterBody3D _body;
		private StaminaComponent _stamina;

		[Export] public float Gravity { get; set; } = 7.8f;
		[Export] public float JumpForce { get; set; } = 5f;
		[Export] public float JumpStaminaCost { get; set; } = 10f;
		[Export] public float SprintStaminaPerSecond { get; set; } = 15f;

		public Vector3 Velocity { get; private set; }
		public bool IsGrounded { get; private set; }

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
				return false;

			SetHorizontalVelocity(direction * speed);
			return true;
		}

		// Check if sprint is possible (for CanEnter checks)
		public bool CanSprint(float delta)
		{
			float staminaCost = SprintStaminaPerSecond * delta;
			return _stamina.CanConsume(staminaCost);
		}

		// Simple velocity setter (for walk, idle, etc.)
		public void SetHorizontalVelocity(Vector3 horizontal)
		{
			Velocity = new Vector3(horizontal.X, Velocity.Y, horizontal.Z);
		}

		public void SetVerticalVelocity(float vertical)
		{
			Velocity = new Vector3(Velocity.X, vertical, Velocity.Z);
		}


		public void Update(float delta)
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
