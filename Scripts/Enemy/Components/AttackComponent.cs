using Godot;
using Scripts.Game;
using System.Collections.Generic;

namespace Scripts.Enemy.Components
{
	public partial class AttackComponent : GenericAttackComponent
	{
		private readonly List<AttackStrategy> _strategies = new();

		public void RegisterStrategy(AttackStrategy strategy)
		{
			_strategies.Add(strategy);
		}

		public bool CanAttack(Node3D self, Node3D target)
		{
			foreach (var strategy in _strategies)
			{
				if (strategy.CanAttack(self, target))
					return true;
			}

			return false;
		}

		public AttackStrategy GetAvailableAttack(
			Node3D self,
			Node3D target)
		{
			foreach (var strategy in _strategies)
			{
				if (strategy.CanAttack(self, target))
					return strategy;
			}

			return null;
		}
	}
}
