using Godot;
using Scripts.Player.Components;
using Scripts.Game;

namespace Scripts.Player.Abilities
{
	public partial class PickupAbility : Ability
	{
		private InputComponent _input;
		private PickableDetectorComponent _detector;
		private CameraComponent _camera;
		private ItemHolderComponent _itemHolder;
		private CharacterBody3D _body;

		[Export] public float MinDotProduct { get; set; } = 0.5f;

		public override bool IsActive() => true;

		public void Initialize(IEntity entity)
		{
			_input = entity.GetComponent<InputComponent>();
			_detector = entity.GetComponent<PickableDetectorComponent>();
			_camera = entity.GetComponent<CameraComponent>();
			_itemHolder = entity.GetComponent<ItemHolderComponent>();
			_body = entity as CharacterBody3D;
		}

		public override void Update(double delta)
		{
			//GD.Print($"Pickup pressed={_input.PickupPressed} Server={Multiplayer.IsServer()}");
			if (!_input.PickupPressed) return;

			if (_itemHolder.IsHoldingItem)
			{
				TryDrop();
				//GD.Print("Dropped");
			}
			else
			{
				TryPickUp();
				//GD.Print("PICKED");
			}
			//GD.Print($"Pickup pressed={_input.PickupPressed} Server={Multiplayer.IsServer()}");
		}

		private void TryPickUp()
		{
			Vector3 lookDir = _camera.GetForwardDirection();

			IPickable target = _detector.GetBestInDirection(
				_body.GlobalPosition,
				lookDir,
				MinDotProduct
			);

			if (target == null || !target.CanBePickedUpBy(_body as IEntity))
				return;

			_itemHolder.SetHeldItem(target);
			target.PickUp(_body as IEntity);
		}

		private void TryDrop()
		{
			if (!_itemHolder.IsHoldingItem) return;

			IPickable item = _itemHolder.HeldItem;
			_itemHolder.ClearHeldItem();
			item.Drop();
		}
	}
}
