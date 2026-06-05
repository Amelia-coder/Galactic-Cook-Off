using Godot;
using Scripts.Enemy.Components;
using Scripts.Game;
using Scripts.Enemy.Bosses.Components;

namespace Scripts.Enemy.States
{
	public partial class BossAttackState : State<IEntity>
	{
		private BossAttackComponent _attack;
		private TargetDetectorComponent _detector;
		private TargetSelectorComponent _selector;
		private double _attackDuration = 0.8;
		private double _timer;

		public override void Initialize(IEntity entity)
		{
			base.Initialize(entity);
			_attack = entity.GetComponent<BossAttackComponent>();
			_detector = entity.GetComponent<TargetDetectorComponent>();
			_selector = entity.GetComponent<TargetSelectorComponent>();
		}

		public override void Enter()
		{
			_timer = _attackDuration;
			var target = _selector.SelectTarget(_detector.Targets, (Node3D)Entity);
			if (target == null) return;

			var strategy = _attack.GetBestAttack((Node3D)Entity, target);
			GD.Print("we selected attack:", strategy);
			strategy?.Execute((Node3D)Entity, target);
		}

		public override void PhysicsUpdate(double delta)
		{
			//_timer -= delta;
			//if (_timer <= 0)
			//{
			//    TransitionTo("BossChaseState");
			//}

			if (!_detector.HasTargets())
			{
				TransitionTo("BossChaseState");
				return;
			}

			Node3D target = _selector.SelectTarget(
				_detector.Targets, (Node3D)Entity);

			if (target == null)
			{
				TransitionTo("BossChaseState");
				return;
			}

			AttackStrategy strategy = _attack.GetBestAttack((Node3D)Entity, target);

			if (strategy == null)
			{
				TransitionTo("BossChaseState");
				return;
			}

			strategy.Execute((Node3D)Entity, target);
		}
	}
}
