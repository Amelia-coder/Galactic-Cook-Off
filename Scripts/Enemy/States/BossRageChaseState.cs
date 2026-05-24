using Godot;
using Scripts.Enemy.Bosses.EvilRamsy;
using Scripts.Enemy.Components;
using Scripts.Game;
using Scripts.Game.GenericComponents;

namespace Scripts.Enemy.States
{
    public partial class BossRageChaseState : State<IEntity>
    {
        private TargetDetectorComponent _detector;
        private TargetSelectorComponent _selector;
        private PathFindingComponent _pathfinding;
        private GenericMovementComponent _movement;
        private EnemyAttackComponent _attack;

        [Export] public float RageSpeedMultiplier = 1.5f;

        public override void Initialize(IEntity entity)
        {
            base.Initialize(entity);
            _detector = entity.GetComponent<TargetDetectorComponent>();
            _selector = entity.GetComponent<TargetSelectorComponent>();
            _pathfinding = entity.GetComponent<PathFindingComponent>();
            _movement = entity.GetComponent<GenericMovementComponent>();
            _attack = entity.GetComponent<EnemyAttackComponent>();
        }

        public override void PhysicsUpdate(double delta)
        {
            if (!_detector.HasTargets()) return;

            var target = _selector.SelectTarget(_detector.Targets, (Node3D)Entity);
            if (target == null) return;

            if (_attack.CanAttack((Node3D)Entity, target))
            {
                EmitSignal(State<EvilRamsy>.SignalName.Finished, "BossRageAttackState");
                return;
            }

            _pathfinding.Target = target.GlobalPosition;
            var dir = _pathfinding.GetNextDirection();
            _movement.SetHorizontalVelocity(dir * RageSpeedMultiplier);
        }
    }
}
