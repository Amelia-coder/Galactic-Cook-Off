using Godot;
using Scripts.Enemy.Bosses.EvilRamsy;
using Scripts.Enemy.Components;
using Scripts.Game;
using Scripts.Enemy.Strategies;

namespace Scripts.Enemy.States
{
	public partial class BossRageState : State<IEntity>
	{
		private EnemyAttackComponent _attack;
		private double _roarDuration = 1.5;
		private double _timer;

		public override void Initialize(IEntity entity)
		{
			base.Initialize(entity);
			_attack = entity.GetComponent<EnemyAttackComponent>();
		}

		public override void Enter()
		{
			_timer = _roarDuration;
			GD.Print("[Boss] ENTERING RAGE MODE!");

			// Swap to rage attacks
			_attack.RegisterStrategy(new RageAttackStrategy(
				range: 4f, damage: 30f, cooldown: 0.5f
			));
		}

		public override void PhysicsUpdate(double delta)
		{
			_timer -= delta;
			if (_timer <= 0)
			{
				EmitSignal(State<EvilRamsy>.SignalName.Finished, "BossRageChaseState");
			}
		}
	}
}
