using Godot;

namespace Scripts.Game.GenericComponents
{
	public partial class GenericMovementComponent : Component
	{
		private CharacterBody3D _body;

		[Export] public float Gravity { get; set; } = 7.8f;
		[Export] public float JumpForce { get; set; } = 5f;

		public Vector3 Velocity { get; protected set; }
		public bool IsGrounded { get; protected set; }

		public void Initialize(CharacterBody3D body)
		{
			_body = body;
		}


		public override void _PhysicsProcess(double delta)
		{
			HandlePhysics(delta);
		}

		protected virtual void HandlePhysics(double delta)
		{
			UpdateGroundedState();
			ApplyGravity(delta);
			ApplyToBody();
		}

		public void Jump()
		{
			if (!IsGrounded)
				return;

			SetVerticalVelocity(JumpForce);
		}

		public void SetHorizontalVelocity(Vector3 horizontal)
		{
			Velocity = new Vector3(
				horizontal.X,
				Velocity.Y,
				horizontal.Z
			);
		}

		public void SetVerticalVelocity(float vertical)
		{
			Velocity = new Vector3(
				Velocity.X,
				vertical,
				Velocity.Z
			);
		}

		public void AddVelocity(Vector3 velocity)
		{
			Velocity += velocity;
		}

		public void StopHorizontalMovement()
		{
			Velocity = new Vector3(
				0,
				Velocity.Y,
				0
			);
		}

		private void UpdateGroundedState()
		{
			IsGrounded = _body.IsOnFloor();
		}

		private void ApplyGravity(double delta)
		{
			if (!IsGrounded)
			{
				Velocity = new Vector3(
					Velocity.X,
					(float)(Velocity.Y - Gravity * delta),
					Velocity.Z
				);
			}
			else if (Velocity.Y < 0)
			{
				Velocity = new Vector3(
					Velocity.X,
					0,
					Velocity.Z
				);
			}
		}

		private void ApplyToBody()
		{
			_body.Velocity = Velocity;
			_body.MoveAndSlide();
		}
	}
}
