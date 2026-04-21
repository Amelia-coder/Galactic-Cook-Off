using Godot;
using System;
using System.Buffers;
using System.Collections.Generic;
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

	private Node3D _cameraPivot;
	private Camera3D _camera;



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

	private PlayerContext _playerContext; 

	// IEntity
	public StaminaComponent Stamina => _staminaComponent;
	public HealthComponent Health;

	private MovementStateMachine _movementStateMachine;
	private AbilitySystem _abilitySystem;

	private StaminaComponent _staminaComponent;
	private BodyDetector _bodyDetector;

	private bool _wasOnFloor;
	public bool IsTouchingFloor => IsOnFloor();

	public bool CanJump => IsTouchingFloor; //||CoyoteTimer.IsActive()

	// =========================================================
	// Lifecycle
	// =========================================================


	public override void _Ready()
	{
		GD.Print($"[Player] layer: {CollisionLayer}, mask: {CollisionMask}");

		_cameraPivot = GetNode<Node3D>("CameraPivot");
		_camera = GetNode<Camera3D>("CameraPivot/SpringArm3D/Camera3D");

		_movementStateMachine = GetNode<MovementStateMachine>("PlayerContext/MovementStateMachine");


		_staminaComponent = GetNode<StaminaComponent>("StaminaComponent");

		GD.Print($"Inside player stamina is null:  ", _staminaComponent == null);

		_playerContext = GetNode<PlayerContext>("PlayerContext");
		_bodyDetector = GetNode<BodyDetector>("BodyDetector");
		_playerContext.Initialize(this, _camera, _staminaComponent, _bodyDetector);


		List<Ability> abilities = new List<Ability>();
		
		PickupAbility pickupAbility = GetNode<PickupAbility>("AbilitySystem/PickupAbility");
		pickupAbility.Initialize(_playerContext);
		abilities.Add(pickupAbility);
		
		ThrowAbility throwAbility = GetNode<ThrowAbility>("AbilitySystem/ThrowAbility");
		throwAbility.Initialize(_playerContext);
		throwAbility.ChargeStarted += () => _chargeBar.Visible = true;
		throwAbility.ChargeUpdated += ratio => _chargeBar.Value = ratio * 100f;
		throwAbility.ChargeReleased += () => _chargeBar.Visible = false;
		throwAbility.ChargeCancelled += () => _chargeBar.Visible = false;

		abilities.Add(throwAbility);

		_abilitySystem = GetNode<AbilitySystem>("AbilitySystem");
		_abilitySystem.Initialize(abilities);
		
		_chargeBar = GetNode<ProgressBar>("CanvasLayer/ChargeBar");
		_chargeBar.Visible = false;

		if (IsLocalPlayer)
		{
			_camera.MakeCurrent();
			//Input.MouseMode = Input.MouseModeEnum.Captured;
		}
		else
		{
			// У других игроков камера не активна
			_camera.Current = false;
		}
	}

	//если при беге камера как-то старнно ведет себ
	//то это можно исправит, настроив коллизии для SpingArm
	public override void _UnhandledInput(InputEvent @event)
	{
		if (!IsLocalPlayer) return; // только локальный игрок управляет мышью

		if (@event is InputEventMouseMotion mouseMotion)
		{
			// Вращаем самого игрока горизонтально — движение WASD станет относительным
			RotateY(-mouseMotion.Relative.X * MouseSensitivity);

			// Вертикальный наклон — только пивот камеры
			_cameraPivot.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);

			// Clamp вертикального угла
			Vector3 pivotRot = _cameraPivot.RotationDegrees;
			pivotRot.X = Mathf.Clamp(pivotRot.X, TiltMin, TiltMax);
			_cameraPivot.RotationDegrees = pivotRot;
		}
	}

	public override void _PhysicsProcess(double delta)
	{

		if (_isHeld) return;
		
		_movementStateMachine._PhysicsProcess(delta);
		_abilitySystem.PhysicsProcess(delta);

		Velocity += Vector3.Down * Gravity * (float)delta;


		// Применяем импульс броска (затухает за кадр)
		if (_throwVelocity != Vector3.Zero)
		{
			Velocity += _throwVelocity;
			_throwVelocity = Vector3.Zero;
		}

		MoveAndSlide();
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
