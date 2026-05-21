using Godot;
using Scripts.Game;

namespace Scripts.Game
{
	public partial class GenericHealthComponent : Component
	{
		[Signal] public delegate void DiedEventHandler();
		[Signal] public delegate void HealthConsumedEventHandler(float consumedHealth);
		[Signal] public delegate void HealthChangedEventHandler(float consumedHealth, float maxHealth);

		[Export] public float MaxHealth = 100f;

		public float CurrentHealth { get; private set; }
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			CurrentHealth = MaxHealth;
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
		private void TakeDamageRpc(float damage)
		{
			// Only server processes damage
			if (!Multiplayer.IsServer()) return;

			CurrentHealth -= damage;
			GD.Print($"[Health] {GetParent().GetParent().Name} took {damage}, now {CurrentHealth}");

			if (CurrentHealth <= 0)
			{
				CurrentHealth = 0;
				EmitSignal(SignalName.Died);
			}
		}

		public void DealDamage(float damage)
		{
			Rpc(MethodName.TakeDamageRpc, damage);
		}

		
	}
}
