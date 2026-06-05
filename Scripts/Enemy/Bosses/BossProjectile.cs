using Godot;
using Scripts.Game;
using System.Threading;

namespace Scripts.Enemy.Bosses
{
	public partial class BossProjectile : RigidBody3D, IThrowable
	{
		[Export] public float Damage = 50f;
		[Export] public float Lifetime = 5f;

		private Area3D _hurtbox;

		public override void _Ready()
		{
			_hurtbox = GetNode<Area3D>("Hurtbox");
			_hurtbox.AreaEntered += OnImpact;

			if (Multiplayer.IsServer())
				GetTree().CreateTimer(Lifetime).Timeout += () =>
				{
					if (IsInsideTree()) QueueFree();
				};
		}

		
		public void Throw(Vector3 impulse)
		{
			Rpc(MethodName.ThrowRpc, impulse);
		}
		


		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
		private void ThrowRpc(Vector3 impulse)
		{
			GD.Print("cum");
			Freeze = false;
			_hurtbox.CallDeferred(Area3D.MethodName.SetMonitoring, true);
			ApplyCentralImpulse(impulse);

			var sync = GetNodeOrNull<MultiplayerSynchronizer>("MultiplayerSynchronizer");
			if (sync != null) sync.SetProcess(true);
		}


		private void OnImpact(Area3D area)
		{
			if (!Multiplayer.IsServer()) return;

			Node entity = area;
			while (entity != null && entity is not IEntity)
				entity = entity.GetParent();

			if (entity is IEntity ie && entity.IsInGroup("Player"))
			{
				var health = ie.GetComponent<GenericHealthComponent>();
				health?.DealDamage(Damage);
			}

			QueueFree();
		}
	}
}
