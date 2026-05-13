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

	private List<IHitEffect> _effects = new(); //так мы можем дать любой набор эффеетов чему угдно, что долно их приент при попадании
	//todo: придумать разгранчиени емуде игроком и врагом, т к к ним приподании даоны примянть разыне эфеек. Скорее всего, у эффетков будет слой применисоти - что=-то на подобии позици в enum
	// =========================================================
	// Exports
	// =========================================================
	[Export] public float Damage = 100f; //for omneshting enemises
	[Export] public float StunDuration = 1.5f;
	[Export] public float DisappearTimeout = 115f;

	private Area3D _hurtbox;
	// =========================================================
	// IThrowable
	// =========================================================
	// Raised for UI only ("Press F to pick up") — NOT for tracking.
	// BodyDetector on the player owns the nearby-items list.
	public event Action<bool> PickupAvailabilityChanged;
	public bool CanBePickedUpBy(IEntity actor) => ((Node3D)actor).IsInGroup("Player");

	// =========================================================
	// Private state
	// =========================================================
	private Node _homeScene;   // scene to return to after being dropped/thrown
	private bool _inFlight = false;

	public IngredientData getIngredientIdentData => _ingredientIdentData;


	private IngredientData _ingredientIdentData;

	// =========================================================
	// Lifecycle
	// =========================================================
	public override void _Ready()
	{
		_homeScene = GetParent();
		_hurtbox = GetNodeOrNull<Area3D>("Hurtbox");
		_hurtbox.SetMonitoring(false);

		_ingredientIdentData = IngredientRegistry.Get("dough");
		///to fix - expecvtion on emey collision E 0:00:06:399   NativeCalls.cs:140 @ void Godot.NativeCalls.godot_icall_1_14(nint, nint, Godot.NativeInterop.godot_bool): Function blocked during in/out signal. Use set_deferred("monitoring", true/false).
		//< C++ Error > Condition "locked" is true.
		//< C++ Source > scene / 3d / physics / area_3d.cpp:379 @ set_monitoring()
		//              Area3D.cs:679 @ void Godot.Area3D.SetMonitoring(bool)
		//              Area3D.cs:52 @ void Godot.Area3D.set_Monitoring(bool)
		//              Dough.cs:90 @ void Dough.OnImpact(Godot.Area3D)
		//              Dough_ScriptMethods.generated.cs:84 @ bool Dough.InvokeGodotClassMethod(Godot.NativeInterop.godot_string_name &, Godot.NativeInterop.NativeVariantPtrArgs, Godot.NativeInterop.godot_variant &)
		//              CSharpInstanceBridge.cs:24 @ Godot.NativeInterop.godot_bool Godot.Bridge.CSharpInstanceBridge.Call(nint, Godot.NativeInterop.godot_string_name *, Godot.NativeInterop.godot_variant * *, int, Godot.NativeInterop.godot_variant_call_error *, Godot.NativeInterop.godot_variant *)


		// PickupZone fires PickupAvailabilityChanged for the HUD prompt only.
		// Detection/tracking is handled by BodyDetector on the player side.
		var pickupZone = GetNode<Area3D>("PickupZone");
		pickupZone.BodyEntered += OnPickupZoneBodyEntered;
		pickupZone.BodyExited += OnPickupZoneBodyExited;

		GetTree().CreateTimer(DisappearTimeout).Timeout += QueueFree;
	}

	// =========================================================
	// IThrowable — state transitions
	// =========================================================
	public void PickUp(IEntity actor)
	{
		if (actor is not Node3D actorNode) return;

		_inFlight = false;
		Freeze = true;
		LinearVelocity = Vector3.Zero;
		AngularVelocity = Vector3.Zero;

		SetPickupZoneActive(false);

		Reparent(actorNode);

		Position = Vector3.Up * 1.5f;
	}

	public void Throw(Vector3 impulse)
	{
		ReturnToScene();
		_inFlight = true;
		Freeze = false;
		_hurtbox.SetMonitoring(true);
		ApplyCentralImpulse(impulse);
	}

	public void Drop()
	{
		ReturnToScene();
		_inFlight = false;
		Freeze = false;
	}

	// =========================================================
	// Impact — called by BodyEntered (Hurtbox - Aread3d -signal)
	// =========================================================
	private void OnImpact(Area3D area)
	{
		_hurtbox.SetMonitoring(false);
		GD.Print("ON IMPACT FIRED: " + area?.Name);
		GD.Print("Area hit triggered...");
		GD.Print($"Area is null: {area == null}");

		if (!_inFlight)
			return;

		// Start from the Area3D that was hit
		Node entity = area;

		// Walk up to find IEntity
		while (entity != null && entity is not IEntity)
		{
			GD.Print($"Node: {entity.Name}, Type: {entity.GetType().Name}");
			entity = entity.GetParent();
		}

		if (entity is not IEntity ie)
			return;

		var health = ie.GetComponent<GenericHealthComponent>();

		if (health == null)
			return;

		// Temporary grouping logic (can be replaced later with components/factions)
		if (entity.IsInGroup("Player"))
		{
			GD.Print("Hit player");
		}
		else if (entity.IsInGroup("Enemy"))
		{
			health.TryTakeDamage(Damage);
			GD.Print("Hit enemy");
		}
	}

	// =========================================================
	// PickupZone
	// =========================================================
	private void OnPickupZoneBodyEntered(Node3D body)
	{
		GD.Print("Someone entred!");
		if (body is IEntity actor && CanBePickedUpBy(actor))
			PickupAvailabilityChanged?.Invoke(true);
	}

	private void OnPickupZoneBodyExited(Node3D body)
	{
		if (body is IEntity actor && CanBePickedUpBy(actor))
			PickupAvailabilityChanged?.Invoke(false);
	}

	// =========================================================
	// Helpers
	// =========================================================

	// Moves Dough back to the arena scene, preserving world position.
	private void ReturnToScene()
	{
		Vector3 worldPos = GlobalPosition;
		GetParent().RemoveChild(this);
		GD.Print(GetParent());
		_homeScene.AddChild(this);
		GlobalPosition = worldPos;
		SetPickupZoneActive(true); // возмжно, в полете это стоит отключить. Но не факт
	}

	private void SetPickupZoneActive(bool active)
	{
		var zone = GetNodeOrNull<Area3D>("PickupZone");
		if (zone != null) zone.Monitoring = active;
	}
}
