using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Player
{
	public partial class PlayerInput : MultiplayerSynchronizer
	{
		// These exports are synced to the server via the synchronizer
		[Export] public Vector3 MoveDirection = Vector3.Zero;
		[Export] public bool JumpPressed = false;
		[Export] public bool SprintPressed = false;
		[Export] public bool PickupPressed = false;
		[Export] public bool ThrowHeld = false;
		[Export] public bool ThrowReleased = false;
		[Export] public bool InteractPressed = false;
	}
}
