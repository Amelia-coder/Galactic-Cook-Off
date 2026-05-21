using Godot;
using Scripts.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Scripts.Game.RecipeSystem.Ingredients;
using static System.Formats.Asn1.AsnWriter;

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
		if (_carrier != null)
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
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void ThrowRpc(Vector3 impulse)
	{
		_carrier = null;
		_inFlight = true;
		Freeze = false;
		_hurtbox.CallDeferred(Area3D.MethodName.SetMonitoring, true);
		ApplyCentralImpulse(impulse);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void DropRpc()
	{
		_carrier = null;
		_inFlight = false;
		Freeze = false;
	}




	// =========================================================
	// Impact
	// =========================================================
	private void OnImpact(Area3D area)
	{
		_hurtbox.CallDeferred(Area3D.MethodName.SetMonitoring, false);

		if (!_inFlight)
			return;

		Node entity = area;
		while (entity != null && entity is not IEntity)
			entity = entity.GetParent();

		if (entity is not IEntity ie)
			return;

		var health = ie.GetComponent<GenericHealthComponent>();
		if (health == null)
			return;

		if (entity.IsInGroup("Enemy"))
		{
			health.DealDamage(Damage);
			GD.Print("Hit enemy");
		}
		else if (entity.IsInGroup("Player"))
		{
			GD.Print("Hit player");
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
