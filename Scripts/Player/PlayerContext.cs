using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerContext : Node, IPlayerContext
{
	// =========================================================
	// IPlayerContext — public surface
	// =========================================================
	public StaminaComponent Stamina => _staminaComponent;
	public HealthComponent Health => _healthComponent;
	public IThrowable HeldItem => _heldItem;

	public bool IsTouchingFloor => _body.IsOnFloor();
	public bool CanJump => IsTouchingFloor;

	Vector3 IMovable.Velocity
	{
		get => _body.Velocity;
		set => _body.Velocity = value;
	}

	// Raised for HUD: "Press F to pick up" prompt
	public event Action<IThrowable, bool> NearbyItemPromptChanged;

	// =========================================================
	// Private state
	// =========================================================
	private CharacterBody3D _body;
	private Camera3D _camera;
	private StaminaComponent _staminaComponent;
	private HealthComponent _healthComponent;
	private BodyDetector _bodyDetector;

	private IThrowable _heldItem;

	// =========================================================
	// Lifecycle
	// =========================================================
	public override void _Ready() { }

	public override void _Process(double delta) { }

	public void Initialize(
		CharacterBody3D body,
		Camera3D camera,
		StaminaComponent stamina,
		BodyDetector bodyDetector,
		HealthComponent health = null)   // optional until HealthComponent is wired
	{
		_body = body;
		_camera = camera;
		_staminaComponent = stamina;
		_healthComponent = health;
		_bodyDetector = bodyDetector;
		GD.Print($"Detector is null: ", _bodyDetector == null);

		//_bodyDetector.ThrowableEntered += OnThrowableEntered;
		//_bodyDetector.ThrowableExited += OnThrowableExited;
	}

	// =========================================================
	// Actions — called by abilities, never by Player directly
	// =========================================================

	/// <summary>
	/// Picks up the best item in the cone defined by lookDirection.
	/// Selects the item with the highest dot product above minDot (default ~60° cone).
	/// </summary>
	public void TryPickUp(Vector3 lookDirection, float minDot = 0.5f)
	{
		if (_heldItem != null) return;

		IThrowable target = GetBestInDirection(lookDirection, minDot);
		if (target == null || !target.CanBePickedUpBy(GetEntity())) return;

		_heldItem = target;
		_heldItem.PickUp(GetEntity());
	}

	public void TryThrow(Vector3 impulse)
	{
		if (_heldItem == null) return;

		var thrown = _heldItem;
		_heldItem = null;       // clear before Throw so re-entry is safe
		thrown.Throw(impulse);
	}

	public void TryDrop()
	{
		if (_heldItem == null) return;

		var dropped = _heldItem;
		_heldItem = null;
		dropped.Drop();
	}

	// =========================================================
	// Movement helper — used by MovementStateMachine
	// =========================================================
	public Vector3 GetMovementDirection(Vector2 input)
	{
		if (_camera == null || input.Length() < 0.1f) return Vector3.Zero;

		Basis camBasis = _camera.GlobalTransform.Basis;
		Vector3 camForward = new Vector3(-camBasis.Z.X, 0, -camBasis.Z.Z).Normalized();
		Vector3 camRight = new Vector3(camBasis.X.X, 0, camBasis.X.Z).Normalized();

		return (camRight * input.X + camForward * -input.Y).Normalized();
	}
	Vector3 ForwardDir => -_body.GlobalTransform.Basis.Z;

	Vector3 IPlayerContext.ForwardDir => ForwardDir;

	// =========================================================
	// BodyDetector callbacks
	// =========================================================
	private void OnThrowableEntered(IThrowable throwable)
	{
		//// Subscribe for UI prompt events from this item
		//throwable.PickupAvailabilityChanged += available =>
		//	OnItemPromptChanged(throwable, available);
	}

	//private void OnThrowableExited(IThrowable throwable)
	//{
	//	// Item left range — clear its prompt if it was showing
	//	NearbyItemPromptChanged?.Invoke(throwable, false);

	//	// If we're holding it (picked it up), don't unsubscribe — it will be
	//	// re-added to BodyDetector when thrown/dropped back into range naturally.
	//	throwable.PickupAvailabilityChanged -= available =>
	//		OnItemPromptChanged(throwable, available);
	//}

	//private void OnItemPromptChanged(IThrowable throwable, bool available)
	//{
	//	// Don't show prompt for item we're already holding
	//	if (throwable == _heldItem) return;
	//	NearbyItemPromptChanged?.Invoke(throwable, available);
	//}

	// =========================================================
	// Selection logic — best item in look direction
	// =========================================================
	private IThrowable GetBestInDirection(Vector3 lookDirection, float minDot)
	{
		return _bodyDetector.GetClosest(_body.Position);
		//IThrowable best = null;
		//float bestDot = minDot;

		//foreach (IThrowable throwable in _bodyDetector.GetNearby())
		//{
		//	// Skip self (Player also implements IThrowable)
		//	if (throwable is Node3D node && node == _body) continue;
		//	if (throwable == _heldItem) continue;
		//	if (throwable is not Node3D itemNode) continue;

		//	Vector3 toItem = (itemNode.GlobalPosition - _body.GlobalPosition).Normalized();
		//	float dot = lookDirection.Dot(toItem);

		//	if (dot > bestDot)
		//	{
		//		bestDot = dot;
		//		best = throwable;
		//	}
		//}

		//return best;
	}

	// =========================================================
	// Helpers
	// =========================================================

	// Resolves the IEntity this context represents.
	// _body is CharacterBody3D which implements IEntity in our setup.
	private IEntity GetEntity() => _body as IEntity;
}
