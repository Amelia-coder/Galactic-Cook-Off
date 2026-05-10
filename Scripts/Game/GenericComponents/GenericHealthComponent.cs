using Godot;
using Scripts.Game;

namespace Scripts.Game
{
	public partial class GenericHealthComponent : Component
	{
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

		public bool TryTakeDamage(float damage)
		{
			CurrentHealth -= damage;
			GD.Print($"Now current health is: {CurrentHealth} ");
			return CurrentHealth > 0;
		}
	}
}
