using Godot;
using System.Collections.Generic;
using System.Linq;
using Scripts.Game;

namespace Scripts.Player.Abilities
{

	public partial class AbilitySystem : Node
	{
		private readonly List<Ability> _abilities = new();

		public void Initialize(IEnumerable<Ability> abilities)
		{
			_abilities.Clear();
			_abilities.AddRange(abilities);
		}


		public override void _PhysicsProcess(double delta)
		{
			bool blocked = IsAnyAbilityBlocking();

			foreach (var ability in _abilities)
			{
				if (blocked && !ability.BlocksOtherAbilities()) continue;
				ability.Update(delta);
			}
		}

		private bool IsAnyAbilityBlocking()
		=> _abilities.Any(a => a.IsActive() && a.BlocksOtherAbilities());

	}
}