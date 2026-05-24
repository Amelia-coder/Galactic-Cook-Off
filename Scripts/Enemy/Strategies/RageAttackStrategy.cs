using Godot;
using Scripts.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Enemy.Strategies
{
    public class RageAttackStrategy : AttackStrategy
    {
        private float _damage;

        public RageAttackStrategy(float range = 4f, float damage = 30f, float cooldown = 0.5f)
        {
            Range = range;
            Cooldown = cooldown;
            _damage = damage;
        }

        protected override void OnExecute(Node3D self, Node3D target)
        {
            if (target is IEntity entity)
            {
                var health = entity.GetComponent<GenericHealthComponent>();
                health?.DealDamage(_damage);
            }
            GD.Print($"[Boss] Rage attack! {_damage} damage");
        }
    }
}
