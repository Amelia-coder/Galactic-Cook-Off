using Godot;
using Scripts.Enemy;
using Scripts.Enemy.Components;
using Scripts.Enemy.Strategies;

namespace Scripts.Enemy.Bosses.Components
{
    public partial class BossAttackComponent : EnemyAttackComponent
    {
        // Override to pick the BEST strategy, not just first available
        public AttackStrategy GetBestAttack(Node3D self, Node3D target)
        {
            float dist = self.GlobalPosition.DistanceTo(target.GlobalPosition);

            // Close range — prefer AoE if multiple players nearby
            if (dist < 5f && CountNearbyPlayers(self, 5f) >= 2)
            {
                var aoe = GetStrategyOfType<AoEAttackStrategy>();
                if (aoe != null && aoe.CanAttack(self, target))
                    return aoe;
            }

            // Far — prefer ranged
            if (dist > 8f)
            {
                var ranged = GetStrategyOfType<RangedAttackStrategy>();
                if (ranged != null && ranged.CanAttack(self, target))
                    return ranged;
            }

            // Fallback to any available
            return GetAvailableAttack(self, target);
        }

        private T GetStrategyOfType<T>() where T : AttackStrategy
        {
            foreach (var s in _strategies)
            {
                if (s is T typed) return typed;
            }
            return null;
        }

        private int CountNearbyPlayers(Node3D self, float radius)
        {
            int count = 0;
            foreach (var p in self.GetTree().GetNodesInGroup("Player"))
            {
                if (p is Node3D pn && self.GlobalPosition.DistanceTo(pn.GlobalPosition) < radius)
                    count++;
            }
            return count;
        }
    }
}
