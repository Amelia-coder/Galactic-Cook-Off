using Godot;
using Scripts.Game;
using Scripts.Player.Components;
using System.Collections.Generic;

namespace Scripts.Enemy.Bosses.Components
{
	public partial class BossTargetSelectorComponent : Component
	{
		public enum TargetPriority
		{
			Nearest,
			CarryingItem,
			LowestHealth,
		}

		[Export] public TargetPriority Priority { get; set; } = TargetPriority.CarryingItem;

		public Node3D SelectTarget(IEnumerable<Node3D> candidates, Node3D self)
		{
			return Priority switch
			{
				TargetPriority.CarryingItem => SelectByCarrying(candidates, self),
				TargetPriority.LowestHealth => SelectByLowestHealth(candidates, self),
				_ => SelectNearest(candidates, self),
			};
		}

		private Node3D SelectNearest(IEnumerable<Node3D> candidates, Node3D self)
		{
			Node3D best = null;
			float bestDist = float.MaxValue;

			foreach (var c in candidates)
			{
				if (!IsValid(c)) continue;
				float d = self.GlobalPosition.DistanceSquaredTo(c.GlobalPosition);
				if (d < bestDist)
				{
					bestDist = d;
					best = c;
				}
			}
			return best;
		}

		// Prioritize players carrying ingredients/dishes
		private Node3D SelectByCarrying(IEnumerable<Node3D> candidates, Node3D self)
		{
			Node3D carrier = null;
			float carrierDist = float.MaxValue;
			Node3D fallback = null;
			float fallbackDist = float.MaxValue;

			foreach (var c in candidates)
			{
				if (!IsValid(c)) continue;
				float d = self.GlobalPosition.DistanceSquaredTo(c.GlobalPosition);

				bool isCarrying = c is IEntity entity
					&& entity.GetComponent<PlayerInteractionComponent>()?.IsCarrying() == true;

				if (isCarrying && d < carrierDist)
				{
					carrierDist = d;
					carrier = c;
				}
				else if (d < fallbackDist)
				{
					fallbackDist = d;
					fallback = c;
				}
			}

			return carrier ?? fallback;
		}

		private Node3D SelectByLowestHealth(IEnumerable<Node3D> candidates, Node3D self)
		{
			Node3D best = null;
			float lowestHp = float.MaxValue;

			foreach (var c in candidates)
			{
				if (!IsValid(c) || c is not IEntity entity) continue;
				var health = entity.GetComponent<GenericHealthComponent>();
				if (health == null) continue;

				if (health.CurrentHealth < lowestHp)
				{
					lowestHp = health.CurrentHealth;
					best = c;
				}
			}
			return best;
		}

		private bool IsValid(Node3D node) =>
			node != null && GodotObject.IsInstanceValid(node);
	}
}
