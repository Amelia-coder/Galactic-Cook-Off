using Godot;
using Scripts.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Enemy.Strategies
{
    public class AoEAttackStrategy : AttackStrategy
    {
        private readonly float _radius;
        private readonly float _damage;
        private readonly float _cooldown;
        private float _timer;

        public AoEAttackStrategy(float radius, float damage, float cooldown)
        {
            _radius = radius;
            _damage = damage;
            _cooldown = cooldown;
        }

        public override bool CanAttack(Node3D self, Node3D target)
        {
            if (_timer > 0) return false;
            return self.GlobalPosition.DistanceTo(target.GlobalPosition) < _radius;
        }

        protected override void OnExecute(Node3D self, Node3D target)
        {
            GD.Print("[Boss] We are doing AOE!");
            _timer = _cooldown;

            // Hit ALL players in radius
            var space = self.GetWorld3D().DirectSpaceState;
            var players = self.GetTree().GetNodesInGroup("Player");

            foreach (var player in players)
            {
                if (player is not Node3D playerNode) continue;
                if (self.GlobalPosition.DistanceTo(playerNode.GlobalPosition) > _radius)
                    continue;

                if (player is IEntity entity)
                {
                    var health = entity.GetComponent<GenericHealthComponent>();
                    health?.DealDamage(_damage);
                    GD.Print($"[Boss AoE] Hit {playerNode.Name} for {_damage}");
                }
            }
        }

        public override void Update(double delta)
        {
            if (_timer > 0) _timer -= (float)delta;
        }
    }
}
