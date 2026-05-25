using Godot;
using Scripts.Enemy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Enemy.Strategies
{
    public class RangedAttackStrategy : AttackStrategy
    {
        private readonly PackedScene _projectileScene;
        private readonly float _minRange;
        private readonly float _maxRange;
        private readonly float _damage;
        private readonly float _cooldown;
        private float _timer;

        public RangedAttackStrategy(PackedScene projectile, float minRange,
                                     float maxRange, float damage, float cooldown)
        {
            _projectileScene = projectile;
            _minRange = minRange;
            _maxRange = maxRange;
            _damage = damage;
            _cooldown = cooldown;
        }

        public override bool CanAttack(Node3D self, Node3D target)
        {
            if (_timer > 0) return false;
            float dist = self.GlobalPosition.DistanceTo(target.GlobalPosition);
            return dist >= _minRange && dist <= _maxRange;
        }

        protected override void OnExecute(Node3D self, Node3D target)
        {
            _timer = _cooldown;

            var proj = _projectileScene.Instantiate<Node3D>();
            proj.GlobalPosition = self.GlobalPosition + Vector3.Up * 1.5f;
            self.GetTree().Root.AddChild(proj, true);

            // Aim at target
            Vector3 dir = (target.GlobalPosition - self.GlobalPosition).Normalized();
            if (proj is RigidBody3D rb)
                rb.ApplyCentralImpulse(dir * 20f);

            GD.Print($"[Boss Ranged] Fired at {target.Name}");
        }

        public override void Update(double delta)
        {
            if (_timer > 0) _timer -= (float)delta;
        }
    }
}
