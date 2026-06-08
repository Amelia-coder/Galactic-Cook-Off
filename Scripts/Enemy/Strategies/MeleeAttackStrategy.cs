using Godot;
using Scripts.Game;

//т к стратегия не является частью дерева, то нужно вручную её инициализировать
// стоит добавить урон от аткаи как поле
namespace Scripts.Enemy.Strategies
{
    public class MeleeAttackStrategy : AttackStrategy
    {
        private float damage = 0.10f;
        public MeleeAttackStrategy(float Range, float Cooldown)
        {
            this.Range = Range;
            this.Cooldown = Cooldown;
        }

        protected override void OnExecute(Node3D self, Node3D target)
        {
            GD.Print("[Boss] We are doing Mellee attack!");
            IEntity entity = (IEntity)target;
            var targetHealthComponent = entity.GetComponent<GenericHealthComponent>();
            if (targetHealthComponent != null)
            {
                targetHealthComponent.DealDamage(damage);
            }
        }
    }
}