using Godot;
using System.Collections.Generic;
using System.Linq;
using Scripts.Game;

namespace Scripts.Player.Abilities
{

	public partial class AbilitySystem : Node
	{
		private readonly List<Ability> _abilities = new();

		// =========================================================
		// Setup
		// =========================================================
		public void Initialize(IEnumerable<Ability> abilities)
		{
			_abilities.Clear();
			_abilities.AddRange(abilities);
		}

		// =========================================================
		// Tick — called by Player
		// =========================================================

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
		//// =========================================================
		//// Queries — used by Player for HUD, or abilities for each other
		//// =========================================================
		//public T Get<T>() where T : class, Ability
		//{
		//	foreach (var ability in _abilities)
		//		if (ability is T match) return match;
		//	return null;
		//}

		//public bool IsActive<T>() where T : class, IAbility
		//    => Get<T>()?.IsActive ?? false;

		//// =========================================================
		//// Helpers
		//// =========================================================
		//private bool IsAnyAbilityBlocking()
		//{
		//    foreach (var ability in _abilities)
		//        if (ability.IsActive && ability.BlocksOtherAbilities) return true;
		//    return false;
		//}
	}
}