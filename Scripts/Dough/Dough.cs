using Godot;
using System;
using System.Collections.Generic;

using Scripts.Game;
using Scripts.Game.RecipeSystem.Ingredients;

public partial class Dough : RigidBody3D, IThrowable, IIngredient
{
	private List<IHitEffect> _effects = new();

	[Export] public float Damage = 100f;
	[Export] public float StunDuration = 1.5f;
	[Export] public float DisappearTimeout = 115f;

	private Area3D _hurtbox;
	private Node3D _carrier = null;
	private bool _inFlight = false;

	public event Action<bool> PickupAvailabilityChanged;

	public IngredientData getIngredientIdentData => _ingredientIdentData;
	private IngredientData _ingredientIdentData;

	// =========================================================
	// Lifecycle
	// =========================================================
	public override void _Ready()
	{
		_hurtbox = GetNodeOrNull<Area3D>("Hurtbox");
		_hurtbox.SetMonitoring(false);

		_ingredientIdentData = IngredientRegistry.Get("dough");

		var pickupZone = GetNode<Area3D>("PickupZone");
		pickupZone.BodyEntered += OnPickupZoneBodyEntered;
		pickupZone.BodyExited += OnPickupZoneBodyExited;

		if (!Multiplayer.IsServer())
			Freeze = true;

		// Only server manages lifetime
		if (Multiplayer.IsServer())
			GetTree().CreateTimer(DisappearTimeout).Timeout += () =>
			{
				if (IsInsideTree()) QueueFree();
			};
	}

	public override void _PhysicsProcess(double delta)
	{
		//if (_carrier != null)
		//	GlobalPosition = _carrier.GlobalPosition + Vector3.Up * 1.5f;
	}

	public override void _Process(double delta)
	{
		if (_carrier == null) return;

		if (!GodotObject.IsInstanceValid(_carrier))
		{
			_carrier = null;
			Freeze = false;
			return;
		}

		GlobalPosition = _carrier.GlobalPosition + Vector3.Up * 1.5f;
	}

	// =========================================================
	// IThrowable
	// =========================================================
	public bool CanBePickedUpBy(IEntity actor) =>
		((Node3D)actor).IsInGroup("Player");

	public void PickUp(IEntity actor)
	{
		if (actor is not Node3D actorNode) return;
		Rpc(MethodName.PickUpRpc, actorNode.GetPath());
	}

	public void Throw(Vector3 impulse)
	{
		Rpc(MethodName.ThrowRpc, impulse);
	}

	public void Drop()
	{
		Rpc(MethodName.DropRpc);
	}


	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void PickUpRpc(string actorPath)
	{
		var actorNode = GetNodeOrNull<Node3D>(actorPath);
		if (actorNode == null) return;
		_carrier = actorNode;
		_inFlight = false;
		Freeze = true;
		LinearVelocity = Vector3.Zero;
		AngularVelocity = Vector3.Zero;
		SetPickupZoneActive(false);

		// All peers compute position locally from carrier — no sync needed
		var sync = GetNodeOrNull<MultiplayerSynchronizer>("MultiplayerSynchronizer");
		if (sync != null) sync.SetProcess(false);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void ThrowRpc(Vector3 impulse)
	{
		_carrier = null;
		_inFlight = true;
		Freeze = false;
		_hurtbox.CallDeferred(Area3D.MethodName.SetMonitoring, true);
		ApplyCentralImpulse(impulse);

		var sync = GetNodeOrNull<MultiplayerSynchronizer>("MultiplayerSynchronizer");
		if (sync != null) sync.SetProcess(true);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void DropRpc()
	{
		_carrier = null;
		_inFlight = false;
		Freeze = false;

		var sync = GetNodeOrNull<MultiplayerSynchronizer>("MultiplayerSynchronizer");
		if (sync != null) sync.SetProcess(true);
	}


	public void Consume()
	{
		Rpc(MethodName.ConsumeRpc);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void ConsumeRpc()
	{
		_carrier = null;
		Visible = false;
		SetProcess(false);
		SetPhysicsProcess(false);

		if (!Multiplayer.IsServer()) return;
		QueueFree();
	}

	// =========================================================
	// Impact
	// =========================================================
	private void OnImpact(Area3D area)
	{
		if (!Multiplayer.IsServer()) return;

		_hurtbox.CallDeferred(Area3D.MethodName.SetMonitoring, false);

		if (!_inFlight)
			return;

		GD.Print($"[Dough] OnImpact fired, area={area.Name}, parent={area.GetParent()?.Name}");

		Node entity = area;
		while (entity != null && entity is not IEntity)
			entity = entity.GetParent();

		if (entity is not IEntity ie)
		{
			GD.Print("[Dough] No IEntity found in parents");
			return;
		}

		var health = ie.GetComponent<GenericHealthComponent>();
		if (health == null)
		{
			GD.Print("[Dough] No health component found");
			return;
		}

		if (entity.IsInGroup("Enemy"))
		{
			health.DealDamage(Damage);
			GD.Print("Hit enemy");
		}
	}

	// =========================================================
	// PickupZone
	// =========================================================
	private void OnPickupZoneBodyEntered(Node3D body)
	{
		if (body is IEntity actor && CanBePickedUpBy(actor))
			PickupAvailabilityChanged?.Invoke(true);
	}

	private void OnPickupZoneBodyExited(Node3D body)
	{
		if (body is IEntity actor && CanBePickedUpBy(actor))
			PickupAvailabilityChanged?.Invoke(false);
	}

	private void SetPickupZoneActive(bool active)
	{
		var zone = GetNodeOrNull<Area3D>("PickupZone");
		if (zone != null)
			zone.CallDeferred(Area3D.MethodName.SetMonitoring, active);
	}
}
