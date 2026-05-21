using Godot;
using Scripts.Game;
using Scripts.Game.RecipeSystem.Ingredients;


namespace Scripts.Player.Components
{
	public partial class PlayerInteractionComponent : Component
	{
		private IEntity _entity;
		private ItemHolderComponent _holder;

		private IInteractable _currentInteractable;

		public void Initialize(IEntity entity, ItemHolderComponent itemHolderComponent)
		{
			_entity = entity;
			_holder = itemHolderComponent;
		}

		public void SetCurrentInteractable(IInteractable interactable)
		{
			_currentInteractable = interactable;
		}

		public void ClearCurrentInteractable(IInteractable interactable)
		{
			if (_currentInteractable == interactable)
				_currentInteractable = null;
		}

		public void Update(InputComponent input)
		{
			if (!input.InteractPressed)
				return;

			TryInteract();
		}

		private void TryInteract()
		{
			if (_currentInteractable == null)
				return;

			if (_currentInteractable is IItemReceiver receiver)
			{
				if (!_holder.IsHoldingItem)
				{
					_currentInteractable.Interact(_entity);
					return;
				}

				var item = _holder.HeldItem;
				if (item is not IIngredient ingredient)
					return;

				bool accepted = receiver.TryInsert(ingredient, _entity);
				if (accepted)
				{
					_holder.ClearHeldItem();
					item.Consume();  // just destroy — no need to drop first
				}
				return;
			}
		}

	}
}
