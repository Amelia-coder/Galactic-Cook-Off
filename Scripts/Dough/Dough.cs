using Godot;
using System;

public partial class Dough : RigidBody3D, IThrowable
{
	//[Signal] public delegate void CanPickUpChangedEventHandler(bool canPickUp);
	[Export] public float Damage = 20f;
	[Export] public float StunDuration = 1.5f;

	public event Action<bool> PickupAvailabilityChanged;

	private Node _homeScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_homeScene = GetParent();
		// Подключаем физику удара
		BodyEntered += OnBodyEntered;

		// PickupZone — отдельная Area3D для обнаружения игроком
		var pickupZone = GetNode<Area3D>("PickupZone");
		pickupZone.BodyEntered += OnPickupBodyEntered;
		pickupZone.BodyExited += OnPickupBodyExited;

		GD.Print($"[Dough] PickupZone mask: {pickupZone.CollisionMask}, layer: {pickupZone.CollisionLayer}");
		GD.Print($"[Dough] Monitoring: {pickupZone.Monitoring}");

		GetTree().CreateTimer(115f).Timeout += QueueFree;
	}


	private void OnPickupBodyEntered(Node3D body)
	{

		if (body is not Player player) return;

		// Сообщаем игроку что рядом появился подбираемый объект
		PickupAvailabilityChanged?.Invoke(true);

		// Регистрируем себя у игрока напрямую
		player.RegisterNearbyThrowable(this);
	}

	private void OnPickupBodyExited(Node3D body)
	{
		if (body is not Player player) return;

		//PickupAvailabilityChanged?.Invoke(false);
		player.UnregisterNearbyThrowable(this);
	}

	public void OnBodyEntered(Node body)
	{
		// Урон только в полёте
		if (!Freeze && body.HasMethod("TakeDamage"))
			body.Call("TakeDamage", Damage);

		if (body is StaticBody3D)
			Freeze = true;
	}

	
	public bool CanBePickedUpBy(IEntity actor) => actor is Player;

	public void PickUp(IEntity actor)
	{
		if (actor is not Node3D actorNode) return;

		Freeze = true;
		LinearVelocity = Vector3.Zero;
		AngularVelocity = Vector3.Zero;

		// Отключаем зону — предмет уже в руках, незачем мониторить
		var pickupZone = GetNodeOrNull<Area3D>("PickupZone");
		if (pickupZone != null) pickupZone.Monitoring = false;

		_homeScene = GetParent();
		GetParent().RemoveChild(this);
		actorNode.AddChild(this);

		if (actor is Player carrier && carrier.ThrowPoint != null)
			Position = actorNode.ToLocal(carrier.ThrowPoint.GlobalPosition);
		else
			Position = Vector3.Up * 1.5f;
	}

	public void Throw(Vector3 impulse)
	{
		Vector3 worldPos = GlobalPosition;

		GetParent().RemoveChild(this);
		_homeScene.AddChild(this);
		GlobalPosition = worldPos;

		// Включаем зону обратно
		var pickupZone = GetNodeOrNull<Area3D>("PickupZone");
		if (pickupZone != null) pickupZone.Monitoring = true;

		Freeze = false;
		ApplyCentralImpulse(impulse);
	}

	public void Drop()
	{
		Vector3 worldPos = GlobalPosition;

		GetParent().RemoveChild(this);
		_homeScene.AddChild(this);
		GlobalPosition = worldPos;

		var pickupZone = GetNodeOrNull<Area3D>("PickupZone");
		if (pickupZone != null) pickupZone.Monitoring = true;

		Freeze = false;
	}
}
