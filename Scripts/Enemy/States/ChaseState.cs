using Godot;
using Scripts.Enemy.Components;
using Scripts.Game;

namespace Scripts.Enemy.States
{

	public partial class ChaseState : State<IEntity>
	{
		private float _speed = 1.0f;
		public override void Enter()
		{
			//GD.Print("Enmey entered chase state");
			//GD.Print("");
			//_jumpReleased = false;
		}

		public override void PhysicsUpdate(double delta)
		{
			var _detector = Entity.GetComponent<TargetDetectorComponent>();
			var _selector = Entity.GetComponent<TargetSelectorComponent>();
			var _pathfinding = Entity.GetComponent<PathFindingComponent>();
			var _movement = Entity.GetComponent<MovementComponent>();
			var _attackComponent = Entity.GetComponent<EnemyAttackComponent>();

			//GD.Print($"Detector has targets: {_detector.HasTargets()}");
			if (_detector == null || !_detector.HasTargets())
				return;

			// 1. Select target from detected candidates
			//уйти от кастов! они, как минимум, занимают время
			Node3D target = _selector.SelectTarget(
				_detector.Targets,
				(Node3D)Entity);

			if (target == null)
				return;

			_attackComponent.UpdateStrategies(delta);

			// 2. Feed target into pathfinding system
			_pathfinding.Target = target.GlobalPosition;

			// 3. Check attack condition
			float distanceSq =
				((Node3D)Entity).GlobalPosition.DistanceSquaredTo(
					target.GlobalPosition);

			//GD.Print("Can attack: ", _attackComponent.CanAttack((Node3D)Entity, target));

			if (_attackComponent.CanAttack((Node3D)Entity, target))
			{
				TransitionTo("AttackState");
				return;
			}

			// 4. Get movement direction from path system
			Vector3 direction = _pathfinding.GetNextDirection();
			_movement.SetHorizontalVelocity(direction * _speed);
			// 5. Apply movement
			//_movement.Update((float)delta);

		}
	}
}
