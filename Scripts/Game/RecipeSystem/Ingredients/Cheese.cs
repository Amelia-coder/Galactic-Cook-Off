using Godot;
using System;
using Scripts.Game;

namespace Scripts.Game.RecipeSystem.Ingredients
{
	public partial class Cheese : RigidBody3D, IIngredient, IThrowable
	{
		public IngredientData getIngredientIdentData => _data;

		private IngredientData _data;

		
		// =========================================================
		// Private state
		// =========================================================
		private Node _homeScene;   // scene to return to after being dropped/thrown
		private bool _inFlight = false;

		public event Action<bool> PickupAvailabilityChanged;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			var pickupZone = GetNode<Area3D>("PickupZone");
			pickupZone.BodyEntered += OnPickupZoneBodyEntered;
			pickupZone.BodyExited += OnPickupZoneBodyExited;
			_data = IngredientRegistry.Get("cheese");
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}

		public bool CanBePickedUpBy(IEntity actor) => ((Node3D)actor).IsInGroup("Player"); //in fufutire player is not only one who can pick up stuff - companions or other enirs in the same with player group caould
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
			ApplyCentralImpulse(impulse);
		}

		public void Drop()
		{
			ReturnToScene();
			_inFlight = false;
			Freeze = false;
		}

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

		private void SetPickupZoneActive(bool active)
		{
			var zone = GetNodeOrNull<Area3D>("PickupZone");
			if (zone != null) zone.Monitoring = active;
		}

		private void ReturnToScene()
		{
			Vector3 worldPos = GlobalPosition;
			GetParent().RemoveChild(this);
			_homeScene.AddChild(this);
			GlobalPosition = worldPos;
			SetPickupZoneActive(true); // возмжно, в полете это стоит отключить. Но не факт
		}

	}

}
