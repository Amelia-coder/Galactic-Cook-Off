using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.World
{
	public partial class ItemDispenser : Node3D
	{
		[Export] public float Interval = 5f;
		[Export] public float MinAngle = 10f;
		[Export] public float MaxAngle = 30f;
		[Export] public float LaunchForce = 8f;
		public Node ItemContainer;

		[Export] public Godot.Collections.Array<PackedScene> ItemScenes { get; set; } = new();

		private float _timer;
		private RandomNumberGenerator _rng = new();

		public override void _Ready()
		{
			ItemContainer = GetTree().Root.GetNode<Node>("AppRoot/Level/Arena/Items");
			_rng.Randomize();
		}

		public void RegisterItem(PackedScene scene)
		{
			ItemScenes.Add(scene);
		}

		public override void _PhysicsProcess(double delta)
		{
			if (!Multiplayer.IsServer()) return;
			if (ItemScenes.Count == 0) return;

			_timer -= (float)delta;
			if (_timer > 0) return;

			_timer = Interval;
			int index = _rng.RandiRange(0, ItemScenes.Count - 1);
			Vector3 impulse = GenerateRandomUpwardImpulse();

			Rpc(MethodName.DispenseRpc, index, impulse);
		}

		private Vector3 GenerateRandomUpwardImpulse()
		{
			// Random angle from vertical
			float angle = _rng.RandfRange(MinAngle, MaxAngle);
			float angleRad = Mathf.DegToRad(angle);

			// Random horizontal direction
			float rotation = _rng.RandfRange(0f, Mathf.Tau);

			float horizontal = Mathf.Sin(angleRad) * LaunchForce;
			float vertical = Mathf.Cos(angleRad) * LaunchForce;

			return new Vector3(
				horizontal * Mathf.Cos(rotation),
				vertical,
				horizontal * Mathf.Sin(rotation)
			);
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true,
			 TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
		private void DispenseRpc(int itemIndex, Vector3 impulse)
		{
			if (!Multiplayer.IsServer()) return;

			var scene = ItemScenes[itemIndex];
			var item = scene.Instantiate<RigidBody3D>();

			
			var container = ItemContainer ?? GetTree().Root;
			container.AddChild(item, true);
			
			item.GlobalPosition = GlobalPosition + Vector3.Up * 0.5f;

			item.Freeze = false;
			item.ApplyCentralImpulse(impulse);

			GD.Print($"[Dispenser] Launched {item.Name} with impulse {impulse}");
		}
	}
}
