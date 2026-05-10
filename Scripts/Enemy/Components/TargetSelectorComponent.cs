using Godot;
using Scripts.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Enemy.Components
{
    public partial class TargetSelectorComponent : Component
    {
        public Node3D SelectTarget(IEnumerable<Node3D> candidates, Node3D self)
        {
            Node3D best = null;

            float bestDistanceSq = float.MaxValue;

            foreach (var candidate in candidates)
            {
                if (candidate == null ||
                    !GodotObject.IsInstanceValid(candidate))
                {
                    continue;
                }

                float distanceSq =
                    self.GlobalPosition.DistanceSquaredTo(
                        candidate.GlobalPosition);

                if (distanceSq < bestDistanceSq)
                {
                    bestDistanceSq = distanceSq;
                    best = candidate;
                }
            }

            return best;
        }
    }
}
