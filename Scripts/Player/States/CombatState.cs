using Godot;
using System;

public partial class CombatState : State<IPlayerContext>
{
	public virtual float InstantStaminaConsumption => 0f;
	StaminaComponent _staminaComponent;
	HealthComponent _healthComponent;


	public override void Initialize(IPlayerContext entity)
	{
		base.Initialize(entity);
		_staminaComponent = entity.Stamina;
		_healthComponent = entity.Health; 
	}
}
