using Godot;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public partial class Player : CharacterBody3D, IEntity, IThrowable
{
	[Export] public float Speed = 5f;
	[Export] public float JumpVelocity = 5f;
	[Export] public float Gravity = 9.8f;
	[Export] public float MouseSensitivity = 0.003f;
	[Export] public float TiltMin = -70f; // градусы
	[Export] public float TiltMax = 20f;

	[Export] public float MinThrowForce = 10f;
	[Export] public float MaxThrowForce = 25f;
	[Export] public Node3D ThrowPoint;

	// Является ли этот игрок локальным (управляемым с этого компьютера)
	[Export] public bool IsLocalPlayer = true;

	private Dictionary<Type, Component> _components = new();
	public void RegisterComponent(Component component)
	{
		_components[component.GetType()] = component;
	}

	// --- IEntity ---
	public T GetComponent<T>() where T : Component
	{
		if (_components.TryGetValue(typeof(T), out Component component))
			return component as T;

		GD.PrintErr($"[Player] Component {typeof(T).Name} not found in dictionary!");
		return null;
	}


	// --- IThrowable ---
	public event Action<bool> PickupAvailabilityChanged;

	// Состояние "в руках у другого игрока"
	private bool _isHeld = false;
	// Импульс, который добавляется при броске
	private Vector3 _throwVelocity = Vector3.Zero;

	// --- Подбор предметов ---
	private IThrowable _heldObject;
	private readonly List<IThrowable> _itemsInRange = new();

	// --- Зарядка броска ---
	private float _chargeTime = 0f;
	private bool _isCharging = false;

	private ProgressBar _chargeBar;

	public HealthComponent Health;
	private MovementComponent _movementComponent;
	private InputComponent _inputComponent;
	private ThrowableDetectorComponent _detectionComponent;
	private CameraComponent _cameraComponent;
	private Camera3D _camera;
	private CameraControllerComponent _cameraControllerComponent;
	private ItemHolderComponent _itemHolderComponent;

	private MovementStateMachine _movementStateMachine;
	private AbilitySystem _abilitySystem;

	private StaminaComponent _staminaComponent;
	private BodyDetector _bodyDetector;

	private bool _wasOnFloor;
	public bool IsTouchingFloor => IsOnFloor();

	public bool CanJump => IsTouchingFloor;

	// =========================================================
	// Lifecycle
	// =========================================================


	public override void _Ready()
	{
		GD.Print($"[Player] layer: {CollisionLayer}, mask: {CollisionMask}");

		_movementStateMachine = GetNode<MovementStateMachine>("MovementStateMachine");


		_staminaComponent = GetNode<StaminaComponent>("ComponentRegistry/StaminaComponent");
		GD.Print("Stamina component is null: ", _staminaComponent == null);
		RegisterComponent(_staminaComponent);
		

		_movementComponent = GetNode<MovementComponent>("ComponentRegistry/MovementComponent");
		_movementComponent.Initialize(this, _staminaComponent);
		RegisterComponent(_movementComponent);
		GD.Print("Movement component is null: ", _movementComponent == null);


		_inputComponent = GetNode<InputComponent>("ComponentRegistry/InputComponent");
		_inputComponent.Initialize(this);
		RegisterComponent(_inputComponent);
		GD.Print("Input component is null: ", _inputComponent == null);


		_detectionComponent = GetNode<ThrowableDetectorComponent>("ComponentRegistry/ThrowableDetectorComponent");
		// Get the Area3D child node and pass it to component
		var bodyDetector = GetNode<Area3D>("BodyDetector");
		_detectionComponent.Initialize(bodyDetector);
		RegisterComponent(_detectionComponent);

		_itemHolderComponent = GetNode<ItemHolderComponent>("ComponentRegistry/ItemHolderComponent");
		RegisterComponent(_itemHolderComponent);

		_cameraComponent = GetNode<CameraComponent>("ComponentRegistry/CameraComponent");
		_camera = GetNode<Camera3D>("CameraPivot/SpringArm3D/Camera3D");
		_cameraComponent.Initialize(_camera);
		RegisterComponent(_cameraComponent);

		_cameraControllerComponent = GetNode<CameraControllerComponent>("ComponentRegistry/CameraControllerComponent");
		_cameraControllerComponent.Initialize(this, _camera, GetNode<Node3D>("CameraPivot"), GetNode<SpringArm3D>("CameraPivot/SpringArm3D"), true); 
		RegisterComponent(_cameraControllerComponent);

		GD.Print($"Inside player stamina is null:  ", _staminaComponent == null);
		List<Ability> abilities = new List<Ability>();
		PickupAbility pickupAbility = GetNode<PickupAbility>("AbilitySystem/PickupAbility");
		pickupAbility.Initialize(this);
		abilities.Add(pickupAbility);
		

		ThrowAbility throwAbility = GetNode<ThrowAbility>("AbilitySystem/ThrowAbility");
		throwAbility.Initialize(this);
		throwAbility.ChargeStarted += () => _chargeBar.Visible = true;
		throwAbility.ChargeUpdated += ratio => _chargeBar.Value = ratio * 100f;
		throwAbility.ChargeReleased += () => _chargeBar.Visible = false;
		throwAbility.ChargeCancelled += () => _chargeBar.Visible = false;

		abilities.Add(throwAbility);

		_abilitySystem = GetNode<AbilitySystem>("AbilitySystem");
		_abilitySystem.Initialize(abilities);
		
		_chargeBar = GetNode<ProgressBar>("CanvasLayer/ChargeBar");
		_chargeBar.Visible = false;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		GD.Print("[Player] _UnhandledInput fired"); // Add this
		_cameraControllerComponent.HandleInput(@event);
	}

	public override void _PhysicsProcess(double delta)
	{

		if (_isHeld) return;
		
		_movementStateMachine._PhysicsProcess(delta);
		_abilitySystem.PhysicsProcess(delta);
		_cameraControllerComponent.Update((float)delta);

	}

	public override void _Process(double delta)
	{
		if (!IsLocalPlayer || _isHeld) return;
	}



	// =========================================================
	// Подбор и бросок
	// =========================================================

	private void ShowPickupLabel(bool visible)
	{
		// TODO: показать/скрыть UI-подсказку "Нажми <клавиша для подбора> для подбора"
	}


	public void UnregisterNearbyThrowable(IThrowable throwable)
	{
		_itemsInRange.Remove(throwable);
		GD.Print($"[Player] убран предмет, всего: {_itemsInRange.Count}");
	}


	// =========================================================
	// IThrowable — этого игрока можно подобрать
	// =========================================================
	private void OnPickupAreaBodyEntered(Node3D body)
	{
		if (body is not Player otherPlayer || otherPlayer == this) return;
		GD.Print($"[PickupArea] вошёл игрок: {body.Name}");
		//RegisterNearbyThrowable(otherPlayer);
	}

	private void OnPickupAreaBodyExited(Node3D body)
	{
		if (body is not Player otherPlayer || otherPlayer == this) return;
		UnregisterNearbyThrowable(otherPlayer);
	}
	
	public bool CanBePickedUpBy(IEntity actor) => actor is Player p && p != this;

	public void Drop()
	{
		//DetachFromCarrier();
	}

	public void PlayAnimation(string name) { }
	public void Throw(Vector3 impulse) // TODO: rename, bacues ethis actually describes
	//jow player is THROWN, not how they themslves throw
	{

	}

	public void PickUp(IEntity actor)
	{
		throw new NotImplementedException();
	}


}
