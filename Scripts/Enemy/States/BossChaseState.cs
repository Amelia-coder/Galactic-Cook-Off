using Godot;
using Scripts.Enemy.Bosses.EvilRamsy;
using Scripts.Enemy.Components;
using Scripts.Game;
using Scripts.Enemy.Bosses.Components;
using Scripts.Game.GenericComponents;

namespace Scripts.Enemy.States
{
	public partial class BossChaseState : State<IEntity>
	{
		private TargetDetectorComponent _detector;
		private TargetSelectorComponent _selector;
		private PathFindingComponent _pathfinding;
		private GenericMovementComponent _movement;
		private GenericHealthComponent _health;
		private BossAttackComponent _attack;

		[Export] public float RageHealthPercent = 0.5f;
		private bool _isRaging = false;

		public override void Initialize(IEntity entity)
		{
			base.Initialize(entity);
			_detector = entity.GetComponent<TargetDetectorComponent>();
			_selector = entity.GetComponent<TargetSelectorComponent>();
			_pathfinding = entity.GetComponent<PathFindingComponent>();
			_movement = entity.GetComponent<GenericMovementComponent>();
			_health = entity.GetComponent<GenericHealthComponent>();
			_attack = entity.GetComponent<BossAttackComponent>();
		}

		public override void PhysicsUpdate(double delta)
		{
			GD.Print("[Boss] We are doing chase state!");
			if (!_detector.HasTargets())
				return;

			if (!_isRaging && _health.CurrentHealth <= _health.MaxHealth * RageHealthPercent)
			{
				TransitionTo("BossRageState");
				return;
			}

			var target = _selector.SelectTarget(_detector.Targets, (Node3D)Entity);
			if (target == null) return;

			if (_attack.GetBestAttack((Node3D)Entity, target) != null)
			{
				TransitionTo("BossAttackState");
				return;
			}

			_pathfinding.Target = target.GlobalPosition;
			var dir = _pathfinding.GetNextDirection();
			_movement.SetHorizontalVelocity(dir);
		}
  //      public override void PhysicsUpdate(double delta)
		//{
		//	GD.Print("[Boss] We are doing chase state!");
		//	if (!_detector.HasTargets())
		//		return;
			
		//	// Check rage transition
		//	if (!_isRaging && _health.CurrentHealth <= _health.MaxHealth * RageHealthPercent)
		//	{
		//		TransitionTo("BossRageState");
		//		return;
		//	}

		//	var target = _selector.SelectTarget(_detector.Targets, (Node3D)Entity);
		//	if (target == null) return;

		//	if (_attack.GetBestAttack((Node3D)Entity, target) != null)
		//	{
		//		GD.Print("we are entring atatck state");
		//		TransitionTo("BossAttackState");
		//		return;
		//	}

		//	// Chase
		//	_pathfinding.Target = target.GlobalPosition;
		//	var dir = _pathfinding.GetNextDirection();
		//	_movement.SetHorizontalVelocity(dir);
		//}
	}
}
