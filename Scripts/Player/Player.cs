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

	[Export] public PackedScene DoughScene; ///re-consider
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

	public float ChargeRatio => _chargeTime / MaxChargeTime;


	private ProgressBar _chargeBar;


	[Export] public float MaxChargeTime = 1.5f;

	private PlayerContext _playerContext; 

	// IEntity
	public StaminaComponent Stamina;
	public HealthComponent Health => throw new NotImplementedException();

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
		//foreach (var child in _abilitySystem.GetChildren())
		//{
		//	if (child is Ability ability)
		//	{
		//		abilities.Add(ability);
		//	}
		//}
		_abilitySystem = GetNode<AbilitySystem>("AbilitySystem");
		_abilitySystem.Initialize(abilities);
		_chargeBar = GetNode<ProgressBar>("CanvasLayer/ChargeBar");
		_chargeBar.Visible = false;


		//var pickupArea = GetNode<Area3D>("PickupArea");
		//pickupArea.BodyEntered += OnPickupAreaBodyEntered;
		//pickupArea.BodyExited += OnPickupAreaBodyExited;

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

		//HandlePickupInput();
		HandleThrowInput(delta);

		// Обновляем шкалу — только один раз
		if (_chargeBar != null)
		{
			_chargeBar.Visible = _isCharging && _heldObject != null;
			_chargeBar.Value = ChargeRatio;
		}
	}



	// =========================================================
	// Подбор и бросок
	// =========================================================


	private void HandleThrowInput(double delta)
	{
		//if (_heldObject == null) return;

		//if (Input.IsActionPressed("throw"))
		//{
		//	_isCharging = true;
		//	_chargeTime = Mathf.Min(_chargeTime + (float)delta, 1.5f);
		//}

		//if (Input.IsActionJustReleased("throw") && _isCharging)
		//{
		//	float force = Mathf.Lerp(MinThrowForce, MaxThrowForce, _chargeTime / 1.5f);
		//	_heldObject.Throw(-Transform.Basis.Z * force);
		//	_heldObject = null;
		//	_isCharging = false;
		//	_chargeTime = 0f;
		//}

		// Начало зарядки — зажали ЛКМ, держим предмет
		if (Input.IsActionPressed("throw") && _heldObject != null)
		{
			_isCharging = true;
			_chargeTime = Mathf.Min(_chargeTime + (float)delta, MaxChargeTime);
		}

		// Бросок — отпустили ЛКМ во время зарядки
		if (Input.IsActionJustReleased("throw") && _isCharging && _heldObject != null)
		{
			float force = Mathf.Lerp(MinThrowForce, MaxThrowForce, ChargeRatio);
			Vector3 direction = -Transform.Basis.Z;

			_heldObject.Throw(direction * force);
			_heldObject = null;
			_isCharging = false;
			_chargeTime = 0f;
		}
	}
	private void ShowPickupLabel(bool visible)
	{
		// TODO: показать/скрыть UI-подсказку "Нажми F для подбора"
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
	public void Throw(Vector3 impulse)
	{
		//DetachFromCarrier();
		//_throwVelocity = impulse;
	}

	public void PickUp(IEntity actor)
	{
		throw new NotImplementedException();
	}
}
