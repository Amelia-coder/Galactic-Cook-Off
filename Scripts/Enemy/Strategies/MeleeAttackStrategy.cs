using Godot;
using Scripts.Game;

//т к стратегия не является частью дерева, то нужно вручную её инициализировать
// стоит добавить урон от аткаи как поле
namespace Scripts.Enemy.Strategies
{
    public class MeleeAttackStrategy : AttackStrategy
    {
        private float damage = 90.0f;
        public MeleeAttackStrategy()
        {
            Range = 2.5f;
            Cooldown = 1.0f;
        }

        protected override void OnExecute(Node3D self, Node3D target)
        {
            IEntity entity = (IEntity)target;
            var targetHealthComponent = entity.GetComponent<GenericHealthComponent>();
            if (targetHealthComponent != null)
            {
                targetHealthComponent.DealDamage(damage);
            }
        }
    }
}