using Godot;
using Scripts.Enemy.Bosses.EvilRamsy;
using Scripts.Enemy.Components;
using Scripts.Game;

namespace Scripts.Enemy.States
{
    public partial class BossAttackState : State<IEntity>
    {
        private EnemyAttackComponent _attack;
        private TargetDetectorComponent _detector;
        private TargetSelectorComponent _selector;
        private double _attackDuration = 0.8;
        private double _timer;

        public override void Initialize(IEntity entity)
        {
            base.Initialize(entity);
            _attack = entity.GetComponent<EnemyAttackComponent>();
            _detector = entity.GetComponent<TargetDetectorComponent>();
            _selector = entity.GetComponent<TargetSelectorComponent>();
        }

        public override void Enter()
        {
            _timer = _attackDuration;
            var target = _selector.SelectTarget(_detector.Targets, (Node3D)Entity);
            if (target == null) return;

            var strategy = _attack.GetAvailableAttack((Node3D)Entity, target);
            strategy?.Execute((Node3D)Entity, target);
        }

        public override void PhysicsUpdate(double delta)
        {
            _timer -= delta;
            if (_timer <= 0)
            {
                EmitSignal(State<EvilRamsy>.SignalName.Finished, "BossChaseState");
            }
        }
    }
}
