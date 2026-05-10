using Godot;

//т к стратегия не является частью дерева, то нужно вручную её инициализировать
namespace Scripts.Enemy.Strategies
{
    public class MeleeAttackStrategy : AttackStrategy
    {
        public MeleeAttackStrategy()
        {
            Range = 2.5f;
            Cooldown = 1.0f;
        }

        protected override void OnExecute(
            Node3D self,
            Node3D target)
        {

        }
    }
}