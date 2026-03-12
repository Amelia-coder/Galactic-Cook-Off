using Godot;
using System;

public partial class StaminaComponent : Node
{

	[Signal] public delegate void StaminaConsumedEventHandler(float consumedStamina);
	[Signal] public delegate void StaminaChangedEventHandler(float consumedStamina, float maxStamina);

	[Export] public float MaxStamina = 100f;

	public float CurrentStamina { get; private set; }

	public override void _Ready()
	{
		CurrentStamina = MaxStamina;
	}
	public bool CanConsume(float cost) => CurrentStamina >= cost;

	public bool TryConsume(float amount)
	{
		if (CurrentStamina < amount)
			return false;

		CurrentStamina -= amount;

		EmitSignal(SignalName.StaminaConsumed, amount);
		EmitSignal(SignalName.StaminaChanged, CurrentStamina, MaxStamina);

		return true;
	}

	public void Regen(float regenPerSecond, float delta)
	{
		CurrentStamina = Mathf.Min(CurrentStamina + regenPerSecond * delta, MaxStamina);//thinkh about reden per se

		EmitSignal(SignalName.StaminaChanged, CurrentStamina, MaxStamina);
	}
}
