using Godot;
using System;
using System.Collections.Generic;
using Scripts.Game;
using Scripts.Player.Components;
using Scripts.Player.Abilities;
using Scripts.Player.States;
public partial class Player : CharacterBody3D, IEntity, IThrowable
{
	// Является ли этот игрок локальным (управляемым с этого компьютера)
	[Export] public bool IsLocalPlayer = true;

	// --- IThrowable ---
	public event Action<bool> PickupAvailabilityChanged;

	// Состояние "в руках у другого игрока"
	private bool _isHeld = false;
	// Импульс, который добавляется при броске
	private Vector3 _throwVelocity = Vector3.Zero;

	// --- Подбор предметов ---
	private IThrowable _heldObject; // will be used for determination of where visually shall picked object be
	private readonly List<IThrowable> _itemsInRange = new();

	// --- Зарядка броска ---
	private ProgressBar _chargeBar;

	private Dictionary<Type, Component> _components = new();
	private GenericHealthComponent _healthComponent;
	private PlayerMovementComponent _movementComponent;
	private InputComponent _inputComponent;
	private ThrowableDetectorComponent _detectionComponent;
	private CameraComponent _cameraComponent;
	private CameraControllerComponent _cameraControllerComponent;
	private ItemHolderComponent _itemHolderComponent;
	private StaminaComponent _staminaComponent;
	
	private Camera3D _camera;
	private Area3D _bodyDetector;

	private MovementStateMachine _movementStateMachine;
	private AbilitySystem _abilitySystem;

	public bool IsTouchingFloor => IsOnFloor();

	public bool CanJump => IsTouchingFloor;

	// =========================================================
	// Lifecycle
	// =========================================================


	public override void _Ready()
	{
		GD.Print($"[Player] layer: {CollisionLayer}, mask: {CollisionMask}");

		InitAndRegisterComponents();

		_healthComponent = GetNode<GenericHealthComponent>("ComponentRegistry/HealthComponent");

		//_movementStateMachine = GetNode<MovementStateMachine>("MovementStateMachine");

		List<Ability> abilities = new List<Ability>();
		PickupAbility pickupAbility = GetNode<PickupAbility>("AbilitySystem/PickupAbility");
		pickupAbility.Initialize(this);
		abilities.Add(pickupAbility);


		ThrowAbility throwAbility = GetNode<ThrowAbility>("AbilitySystem/ThrowAbility");
		throwAbility.Initialize(this);
		throwAbility.ChargeStarted += () => _chargeBar.Visible = true;
		throwAbility.ChargeUpdated += ratio => _chargeBar.Value = ratio * 100f; //нужно соректировлоать, т к при дляительном заряде некорреткно ооюрадается шкала
		throwAbility.ChargeReleased += () => _chargeBar.Visible = false;
		throwAbility.ChargeCancelled += () => _chargeBar.Visible = false;

		abilities.Add(throwAbility);

		_abilitySystem = GetNode<AbilitySystem>("AbilitySystem");
		_abilitySystem.Initialize(abilities);

		_chargeBar = GetNode<ProgressBar>("CanvasLayer/ChargeBar");
		_chargeBar.Visible = false;
	}

	private void InitAndRegisterComponents()
	{
		_staminaComponent = GetNode<StaminaComponent>("ComponentRegistry/StaminaComponent");
		GD.Print("Stamina component is null: ", _staminaComponent == null);
		RegisterComponent(_staminaComponent);


		_movementComponent = GetNode<PlayerMovementComponent>("ComponentRegistry/MovementComponent");
		_movementComponent.Initialize(this, _staminaComponent);
		RegisterComponent(_movementComponent);
		GD.Print("Movement component is null: ", _movementComponent == null);


		_inputComponent = GetNode<InputComponent>("ComponentRegistry/InputComponent");
		_inputComponent.Initialize(this);
		RegisterComponent(_inputComponent);
		GD.Print("Input component is null: ", _inputComponent == null);


		_detectionComponent = GetNode<ThrowableDetectorComponent>("ComponentRegistry/ThrowableDetectorComponent");
		// Get the Area3D child node and pass it to component
		_bodyDetector = GetNode<Area3D>("BodyDetector");
		_detectionComponent.Initialize(_bodyDetector);
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
	}

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
		GD.PrintErr($"[Player] Call stack: {System.Environment.StackTrace}"); // Shows who called this

		return null;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		//GD.Print("[Player] _UnhandledInput fired"); // Add this
		_cameraControllerComponent.HandleInput(@event);
	}

	public override void _PhysicsProcess(double delta)
	{

		if (_isHeld) return;
		_inputComponent.Update();

		_cameraControllerComponent.Update((float)delta); // TODO: think about moving this to _PhysicsProcess in some system/component
		_movementComponent.Update((float)delta);
	}

	public override void _Process(double delta)
	{
		if (!IsLocalPlayer || _isHeld) return;
	}



	// =========================================================
	// Подбор и бросок
	// =========================================================

	private void ShowPickupLabel(bool visible) //должны быть сигналом в рамкх UI
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

	public void PlayAnimation(string name) { } //уйдет в AnmationComponent
	public void Throw(Vector3 impulse) // TODO: rename, bacues ethis actually describes
	//jow player is THROWN, not how they themslves throw
	{

	}

	public void PickUp(IEntity actor)
	{
		throw new NotImplementedException();
	}


}
