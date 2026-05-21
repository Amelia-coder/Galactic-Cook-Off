using Godot;
using System;
using Scripts.Game;

namespace Scripts.Game.RecipeSystem.Ingredients
{
	public partial class Cheese : RigidBody3D, IIngredient, IThrowable
	{
		public IngredientData getIngredientIdentData => _data;
		private IngredientData _data;

		private bool _inFlight = false;
		private Node3D _carrier = null;

		public event Action<bool> PickupAvailabilityChanged;

		public override void _Ready()
		{
			var pickupZone = GetNode<Area3D>("PickupZone");
			pickupZone.BodyEntered += OnPickupZoneBodyEntered;
			pickupZone.BodyExited += OnPickupZoneBodyExited;
			_data = IngredientRegistry.Get("cheese");

			if (!Multiplayer.IsServer())
				Freeze = true;
		}

		public override void _PhysicsProcess(double delta)
		{
			// Follow the carrier instead of reparenting
			if (_carrier != null)
				GlobalPosition = _carrier.GlobalPosition + Vector3.Up * 1.5f;
		}

		public bool CanBePickedUpBy(IEntity actor) =>
			((Node3D)actor).IsInGroup("Player");

		public void PickUp(IEntity actor)
		{
            if (actor is not Node3D actorNode) return;
            Rpc(MethodName.PickUpRpc, actorNode.GetPath());
        }

		public void Throw(Vector3 impulse)
		{
            Rpc(MethodName.ThrowRpc, impulse);
        }

        public void Consume()
        {
            Rpc(MethodName.ConsumeRpc);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void ConsumeRpc()
        {
            if (!Multiplayer.IsServer()) return;
            QueueFree();
        }


        public void Drop()
		{
            Rpc(MethodName.DropRpc);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void PickUpRpc(string actorPath)
        {
            var actorNode = GetNodeOrNull<Node3D>(actorPath);
            if (actorNode == null) return;
            _carrier = actorNode;
            _inFlight = false;
            Freeze = true;
            LinearVelocity = Vector3.Zero;
            AngularVelocity = Vector3.Zero;
            SetPickupZoneActive(false);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void ThrowRpc(Vector3 impulse)
        {
            _carrier = null;
            _inFlight = true;
            Freeze = false;
            ApplyCentralImpulse(impulse);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void DropRpc()
        {
            _carrier = null;
            _inFlight = false;
            Freeze = false;
        }



        private void OnPickupZoneBodyEntered(Node3D body)
		{
			GD.Print("Someone entred!");
			if (body is IEntity actor && CanBePickedUpBy(actor))
				PickupAvailabilityChanged?.Invoke(true);
		}

		private void OnPickupZoneBodyExited(Node3D body)
		{
			if (body is IEntity actor && CanBePickedUpBy(actor))
				PickupAvailabilityChanged?.Invoke(false);
		}

		private void SetPickupZoneActive(bool active)
		{
			var zone = GetNodeOrNull<Area3D>("PickupZone");
			if (zone != null) zone.Monitoring = active;
		}

	}

}
