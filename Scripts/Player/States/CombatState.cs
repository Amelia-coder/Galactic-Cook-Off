using Scripts.Player.Components;
using Scripts.Game;

namespace Scripts.Player.States
{
	public partial class CombatState : State<IEntity>
	{
		public virtual float InstantStaminaConsumption => 0f;
		StaminaComponent _staminaComponent;
		GenericHealthComponent _healthComponent;


		public override void Initialize(IEntity entity)
		{
			base.Initialize(entity);
			_staminaComponent = entity.GetComponent<StaminaComponent>();
			_healthComponent = entity.GetComponent<GenericHealthComponent>();
		}
	}
}
