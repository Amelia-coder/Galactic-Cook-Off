using Godot;
using Scripts.Game;

namespace Scripts.Player.Components
{
	public partial class InputComponent : Component
	{
		private Node3D _transform; // The player's transform

		public Vector3 MoveDirection { get; private set; }
		public bool JumpPressed { get; private set; }
		public bool SprintPressed { get; private set; }
		public bool PickupPressed { get; private set; }
		public bool ThrowHeld { get; private set; }
		public bool ThrowReleased { get; private set; }
        public bool InteractPressed { get; private set; }
		///

        public void Initialize(Node3D transform)
		{
			_transform = transform;
		}

		public void Update()
		{
			Vector2 inputDir = Input.GetVector("left", "right", "forward", "back");

			// Transform input to player-local space
			if (inputDir.LengthSquared() > 0.01f)
			{
				// Get player's forward and right vectors
				Vector3 forward = -_transform.Transform.Basis.Z; // Player's forward
				Vector3 right = _transform.Transform.Basis.X;    // Player's right

				// Project to horizontal plane
				forward.Y = 0;
				right.Y = 0;
				forward = forward.Normalized();
				right = right.Normalized();

				// Combine based on input
				MoveDirection = (right * inputDir.X + forward * -inputDir.Y).Normalized();
			}
			else
			{
				MoveDirection = Vector3.Zero;
			}

			JumpPressed = Input.IsActionJustPressed("jump");
			SprintPressed = Input.IsActionPressed("sprint");
			PickupPressed = Input.IsActionJustPressed("pickup");
			ThrowHeld = Input.IsActionPressed("throw");
			ThrowReleased = Input.IsActionJustReleased("throw");
			InteractPressed = Input.IsActionJustPressed("add_piece_to_kitchen");//maybe make intecation - cook andf putting to station  - same; but netter do it as separat actrions
		}

		public void Reset()
		{
			JumpPressed = false;
			PickupPressed = false;
			ThrowReleased = false;
		}
	}
}
