using Godot;
using System;

public partial class CombatState : State
{
	public virtual float InstantStaminaConsumption => 0f;
	StaminaComponent _staminaComponent;

	public void Initialize(IEntity entity, StaminaComponent stamina)
	{
		base.Initialize(entity);
		_staminaComponent = stamina;
	}
}
