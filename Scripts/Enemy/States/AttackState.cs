using Scripts.Game;
using Scripts.Player.Components; //remove this and intorice genrilzed component is Scripts.Game namespace!

namespace Scripts.Enemy.States
{
	public partial class AttackState : State<IEntity>
	{
		public override void Enter()
		{
			//y play animation
		}


		public override void PhysicsUpdate(double delta)
		{
			var _movement = Entity.GetComponent<PlayerMovementComponent>();
			//var _stamina = Entity.GetComponent<StaminaComponent>();
			//var _input = Entity.GetComponent<InputComponent>();
			//_input.Update();


		}
	}
}
