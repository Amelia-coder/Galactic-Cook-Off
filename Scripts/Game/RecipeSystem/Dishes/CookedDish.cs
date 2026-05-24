using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Game.RecipeSystem.Dishes
{
    public partial class CookedDish : RigidBody3D, IThrowable
    {
        [Export] public float Damage = 50f;

        private Area3D _hurtbox;
        private Node3D _carrier = null;
        private bool _inFlight = false;

        public event Action<bool> PickupAvailabilityChanged;

        public override void _Ready()
        {
            _hurtbox = GetNode<Area3D>("Hurtbox");
            _hurtbox.SetMonitoring(false);

            var pickupZone = GetNode<Area3D>("PickupZone");
            pickupZone.BodyEntered += OnPickupZoneBodyEntered;
            pickupZone.BodyExited += OnPickupZoneBodyExited;

            if (!Multiplayer.IsServer())
                Freeze = true;
        }

        public override void _Process(double delta)
        {
            if (_carrier == null) return;
            if (!GodotObject.IsInstanceValid(_carrier))
            {
                _carrier = null;
                Freeze = false;
                return;
            }
            GlobalPosition = _carrier.GlobalPosition + Vector3.Up * 1.5f;
        }

        // =========================================================
        // IThrowable
        // =========================================================
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

        public void Drop()
        {
            Rpc(MethodName.DropRpc);
        }

        public void Consume()
        {
            Rpc(MethodName.ConsumeRpc);
        }

        // =========================================================
        // RPCs
        // =========================================================
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
            _hurtbox.CallDeferred(Area3D.MethodName.SetMonitoring, true);
            ApplyCentralImpulse(impulse);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void DropRpc()
        {
            _carrier = null;
            _inFlight = false;
            Freeze = false;
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void ConsumeRpc()
        {
            _carrier = null;
            Visible = false;
            SetProcess(false);
            SetPhysicsProcess(false);
            if (!Multiplayer.IsServer()) return;
            QueueFree();
        }

        // =========================================================
        // Impact — damages BOTH regular enemies and bosses
        // =========================================================
        private void OnImpact(Area3D area)
        {
            if (!Multiplayer.IsServer()) return;
            _hurtbox.CallDeferred(Area3D.MethodName.SetMonitoring, false);
            if (!_inFlight) return;

            Node entity = area;
            while (entity != null && entity is not IEntity)
                entity = entity.GetParent();

            if (entity is not IEntity ie) return;

            var health = ie.GetComponent<GenericHealthComponent>();
            if (health == null) return;

            if (entity.IsInGroup("Enemy") || entity.IsInGroup("Boss"))
            {
                health.DealDamage(Damage);
                GD.Print($"[CookedDish] Hit {entity.Name} for {Damage}");
            }
        }

        // =========================================================
        // PickupZone
        // =========================================================
        private void OnPickupZoneBodyEntered(Node3D body)
        {
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
            if (zone != null)
                zone.CallDeferred(Area3D.MethodName.SetMonitoring, active);
        }
    }
}
