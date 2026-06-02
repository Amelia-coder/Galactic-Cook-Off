using Godot;
using Scripts.Game;

namespace Scripts.Player.Components
{
	public partial class InputComponent : Component
	{
		private Node3D _transform;

		public Vector3 MoveDirection { get; private set; }
		public bool JumpPressed { get; set; }
		public bool SprintPressed { get; private set; }
		public bool PickupPressed { get; set; }
		public bool ThrowHeld { get; private set; }
		public bool ThrowReleased { get; set; }
		public bool InteractPressed { get; set; }

		public void Initialize(Node3D transform)
		{
			_transform = transform;
		}

		/// <summary>
		/// Server calls this to write network-received input into the component.
		/// States then read from the public properties above.
		/// </summary>
		public void SetFromNetwork(Vector3 direction, bool jump, bool sprint,
			bool pickup, bool throwHeld, bool throwReleased, bool interact)
		{
			MoveDirection = direction;
			JumpPressed = jump;
			SprintPressed = sprint;
			PickupPressed = pickup;
			ThrowHeld = throwHeld;
			ThrowReleased = throwReleased;
			InteractPressed = interact;
		}

		/// <summary>
		/// Local player calls this to read keyboard input.
		/// </summary>
		public void Update()
		{
			Vector2 inputDir = Input.GetVector("left", "right", "forward", "back");

			
			if (inputDir.LengthSquared() > 0.01f)
			{
				Vector3 forward = -_transform.Transform.Basis.Z;
				Vector3 right = _transform.Transform.Basis.X;
				forward.Y = 0;
				right.Y = 0;
				forward = forward.Normalized();
				right = right.Normalized();
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
			InteractPressed = Input.IsActionJustPressed("add_piece_to_kitchen");
		}

		public void Reset()
		{
			JumpPressed = false;
			PickupPressed = false;
			ThrowReleased = false;
		}
	}
}
