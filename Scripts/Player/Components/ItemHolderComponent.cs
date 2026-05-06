using Godot;
using Scripts.Game;
/// <summary>
/// Manages the currently held throwable item
/// </summary>
/// 

namespace Scripts.Player.Components
{
	public partial class ItemHolderComponent : Component
	{
		public IThrowable HeldItem { get; private set; }

		public bool IsHoldingItem => HeldItem != null;

		public void SetHeldItem(IThrowable item)
		{
			HeldItem = item;
		}

		public void ClearHeldItem()
		{
			HeldItem = null;
		}
	}
}
