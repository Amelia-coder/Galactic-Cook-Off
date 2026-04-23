using Godot;

public partial class PickupAbility : Ability
{
    private InputComponent _input;
    private ThrowableDetectorComponent _detector;
    private CameraComponent _camera;
    private ItemHolderComponent _itemHolder;
    private CharacterBody3D _body;

    [Export] public float MinDotProduct { get; set; } = 0.5f;

    public override bool IsActive() => true;

    public void Initialize(IEntity entity)
    {
        _input = entity.GetComponent<InputComponent>();
        _detector = entity.GetComponent<ThrowableDetectorComponent>();
        _camera = entity.GetComponent<CameraComponent>();
        _itemHolder = entity.GetComponent<ItemHolderComponent>();
        _body = entity as CharacterBody3D;
    }

    public override void Update(double delta)
    {
        _input.Update();

        if (!_input.PickupPressed) return;

        if (_itemHolder.IsHoldingItem)
            TryDrop();
        else
            TryPickUp();
    }

    private void TryPickUp()
    {
        Vector3 lookDir = _camera.GetForwardDirection();

        IThrowable target = _detector.GetBestInDirection(
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

        IThrowable item = _itemHolder.HeldItem;
        _itemHolder.ClearHeldItem();
        item.Drop();
    }
}