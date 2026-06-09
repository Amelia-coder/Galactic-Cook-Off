using Godot;
using Scripts.Game;

namespace Scripts.Player.Components
{
	public partial class InputComponent : Component
	{
		private Node3D _transform;
		private int _playerId;

		public Vector3 MoveDirection { get; private set; }
		public bool JumpPressed { get; set; }
		public bool SprintPressed { get; private set; }
		public bool PickupPressed { get; set; }
		public bool ThrowHeld { get; private set; }
		public bool ThrowReleased { get; set; }
		public bool InteractPressed { get; set; }

		// Buffers so one-shot actions survive until the next send
		private bool _jumpBuffer;
		private bool _pickupBuffer;
		private bool _throwReleasedBuffer;
		private bool _interactBuffer;

		private bool IsLocalPlayer => Multiplayer.GetUniqueId() == _playerId;

		public void Initialize(Node3D transform, int playerId)
		{
			_transform = transform;
			_playerId = playerId;
		}

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

		public void Reset()
		{
			JumpPressed = false;
			PickupPressed = false;
			ThrowReleased = false;
			InteractPressed = false;
		}

		public override void _PhysicsProcess(double delta)
		{
			// Only the owning client reads the keyboard
			if (!IsLocalPlayer)
				return;

			// --- continuous input (fine to overwrite every frame) ---
			Vector2 inputDir = Input.GetVector("left", "right", "forward", "back");
			if (inputDir.LengthSquared() > 0.01f)
			{
				Vector3 forward = -_transform.Transform.Basis.Z;
				Vector3 right = _transform.Transform.Basis.X;
				forward.Y = 0; right.Y = 0;
				forward = forward.Normalized();
				right = right.Normalized();
				MoveDirection = (right * inputDir.X + forward * -inputDir.Y).Normalized();
			}
			else
			{
				MoveDirection = Vector3.Zero;
			}

			SprintPressed = Input.IsActionPressed("sprint");
			ThrowHeld = Input.IsActionPressed("throw");

			// --- one-shot input: latch into buffers so a single frame isn't missed ---
			if (Input.IsActionJustPressed("jump")) _jumpBuffer = true;
			if (Input.IsActionJustPressed("pickup")) _pickupBuffer = true;
			if (Input.IsActionJustReleased("throw")) _throwReleasedBuffer = true;
			if (Input.IsActionJustPressed("add_piece_to_kitchen")) _interactBuffer = true;

			// Send everything to the server
			RpcId(1, MethodName.ServerReceiveInput,
				MoveDirection,
				_jumpBuffer, SprintPressed,
				_pickupBuffer, ThrowHeld, _throwReleasedBuffer, _interactBuffer);

			// Clear one-shot buffers after sending
			_jumpBuffer = false;
			_pickupBuffer = false;
			_throwReleasedBuffer = false;
			_interactBuffer = false;
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true,
			 TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
		private void ServerReceiveInput(Vector3 direction, bool jump, bool sprint,
			bool pickup, bool throwHeld, bool throwReleased, bool interact)
		{
			if (!Multiplayer.IsServer())
				return;

			// Validate that the sender actually owns this player
			int sender = Multiplayer.GetRemoteSenderId();
			if (sender != _playerId)
				return;

			SetFromNetwork(direction, jump, sprint, pickup, throwHeld, throwReleased, interact);
		}
	}
	//public partial class InputComponent : Component
	//{
	//	private Node3D _transform;

	//	public Vector3 MoveDirection { get; private set; }
	//	public bool JumpPressed { get; set; }
	//	public bool SprintPressed { get; private set; }
	//	public bool PickupPressed { get; set; }
	//	public bool ThrowHeld { get; private set; }
	//	public bool ThrowReleased { get; set; }
	//	public bool InteractPressed { get; set; }

	//	public void Initialize(Node3D transform)
	//	{
	//		_transform = transform;
	//	}

	//	/// <summary>
	//	/// Server calls this to write network-received input into the component.
	//	/// States then read from the public properties above.
	//	/// </summary>
	//	public void SetFromNetwork(Vector3 direction, bool jump, bool sprint,
	//		bool pickup, bool throwHeld, bool throwReleased, bool interact)
	//	{
	//		MoveDirection = direction;
	//		JumpPressed = jump;
	//		SprintPressed = sprint;
	//		PickupPressed = pickup;
	//		ThrowHeld = throwHeld;
	//		ThrowReleased = throwReleased;
	//		InteractPressed = interact;
	//	}



	//	public void Reset()
	//	{
	//		JumpPressed = false;
	//		PickupPressed = false;
	//		ThrowReleased = false;
	//	}

	//	/// <summary>
	//	/// Local player calls this to read keyboard input.
	//	/// </summary>

	//	public override void _PhysicsProcess(double delta)
	//	{
	//		Vector2 inputDir = Input.GetVector("left", "right", "forward", "back");


	//		if (inputDir.LengthSquared() > 0.01f)
	//		{
	//			Vector3 forward = -_transform.Transform.Basis.Z;
	//			Vector3 right = _transform.Transform.Basis.X;
	//			forward.Y = 0;
	//			right.Y = 0;
	//			forward = forward.Normalized();
	//			right = right.Normalized();
	//			MoveDirection = (right * inputDir.X + forward * -inputDir.Y).Normalized();
	//		}
	//		else
	//		{
	//			MoveDirection = Vector3.Zero;
	//		}

	//		JumpPressed = Input.IsActionJustPressed("jump");
	//		SprintPressed = Input.IsActionPressed("sprint");
	//		PickupPressed = Input.IsActionJustPressed("pickup");
	//		ThrowHeld = Input.IsActionPressed("throw");
	//		ThrowReleased = Input.IsActionJustReleased("throw");
	//		InteractPressed = Input.IsActionJustPressed("add_piece_to_kitchen");
	//	}
	//}
}
