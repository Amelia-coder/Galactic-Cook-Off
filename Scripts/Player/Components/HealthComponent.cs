using Godot;
using System;

public partial class HealthComponent : Component
{
	[Signal] public delegate void HealthConsumedEventHandler(float consumedHealth);
	[Signal] public delegate void HealthChangedEventHandler(float consumedHealth, float maxHealth);

	[Export] public float MaxHealth = 100f;

	public float CurrentHealth { get; private set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
