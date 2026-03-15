using Godot;
using System;
using System.Buffers;
using System.Collections.Generic;

public partial class Player : CharacterBody3D, IEntity, IPlayerContext, IThrowable
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

	// IMovable
	Vector3 IMovable.Velocity
	{
		get => Velocity;
		set => Velocity = value;
	}

	// IPlayerContext
	public StaminaComponent Stamina;
	public HealthComponent Health => throw new NotImplementedException();

	StaminaComponent IPlayerContext.Stamina { get => _staminaComponent; }
	HealthComponent IPlayerContext.Health { get => Health; }


	void OnPickableStateChanged(IThrowable item, bool canPick) { }

	private MovementStateMachine _movementStateMachine;

	private StaminaComponent _staminaComponent;
	//private StaminaComponent _staminaComponent;

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

		_movementStateMachine = GetNode<MovementStateMachine>("MovementStateMachine");

		_staminaComponent = GetNode<StaminaComponent>("StaminaComponent");

		_chargeBar = GetNode<ProgressBar>("CanvasLayer/ChargeBar");
		_chargeBar.Visible = false;


		var pickupArea = GetNode<Area3D>("PickupArea");
		pickupArea.BodyEntered += OnPickupAreaBodyEntered;
		pickupArea.BodyExited += OnPickupAreaBodyExited;

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

		HandlePickupInput();
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

	private void HandlePickupInput()
	{
		if (!Input.IsActionJustPressed("pickup")) return;
		GD.Print($"[Pickup] нажат pickup, heldObject={_heldObject}, inRange={_itemsInRange.Count}");
		
		if (_heldObject != null)
		{
			// Уже что-то держим — кладём
			_heldObject.Drop();
			_heldObject = null;
			return;
		}

		IThrowable target = GetItemPlayerIsLookingAt();
		if (target == null || !target.CanBePickedUpBy(this)) return;

		_heldObject = target;
		_heldObject.PickUp(this);
	}

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

	private IThrowable GetItemPlayerIsLookingAt()
	{
		if (_itemsInRange.Count == 0) return null;

		Vector3 forward = -GlobalTransform.Basis.Z;
		IThrowable best = null;
		float bestDot = 0.5f; // минимальный порог — конус ~60°

		foreach (var item in _itemsInRange)
		{
			if (item is Player p && p == this) continue;
			if (item is not Node3D itemNode) continue;
			Vector3 dir = (itemNode.GlobalPosition - GlobalPosition).Normalized();
			float dot = forward.Dot(dir);
			if (dot > bestDot)
			{
				bestDot = dot;
				best = item;
			}
		}

		return best;
	}

	private void ThrowDough()
	{
		if (DoughScene == null) return;

		var dough = DoughScene.Instantiate<RigidBody3D>();
		GetTree().Root.AddChild(dough);
		dough.GlobalPosition = ThrowPoint.GlobalPosition;

		float force = Mathf.Lerp(MinThrowForce, MaxThrowForce, _chargeTime / 1.5f);
		Vector3 direction = -Transform.Basis.Z;
		dough.ApplyImpulse(direction * force);
	}

	// =========================================================
	// Область обнаружения предметов (Area3D callbacks)
	// =========================================================

	private void OnThrowableEntered(Node3D body)
	{
		GD.Print($"Тип: {body.GetType().FullName}, скрипт: {body.GetScript()}");
		GD.Print($"[PickupArea] вошёл: {body.Name}, IThrowable: {body is IThrowable}");
		//if (body is IThrowable throwable)
		//{
		//	_itemsInRange.Add(throwable);
		//	throwable.PickupAvailabilityChanged += OnPickupAvailabilityChanged;
		//	GD.Print($"[PickupArea] добавлен в список: {body.Name}");
		//}

		IThrowable throwable = body as IThrowable;
		if (throwable == null && body is Node node)
			throwable = node as IThrowable;

		if (throwable != null && body != this)
		{
			_itemsInRange.Add(throwable);
			throwable.PickupAvailabilityChanged += OnPickupAvailabilityChanged;
			GD.Print($"[PickupArea] добавлен: {body.Name}");
		}
	}

	private void OnThrowableExited(Node3D body)
	{
		if (body is IThrowable throwable)
		{
			_itemsInRange.Remove(throwable);
			throwable.PickupAvailabilityChanged -= OnPickupAvailabilityChanged;
		}
	}

	private void OnPickupAvailabilityChanged(bool canPickUp)
	{
		ShowPickupLabel(canPickUp);
	}

	private void ShowPickupLabel(bool visible)
	{
		// TODO: показать/скрыть UI-подсказку "Нажми F для подбора"
	}

	public void RegisterNearbyThrowable(IThrowable throwable)
	{
		if (_itemsInRange.Contains(throwable)) return;
		_itemsInRange.Add(throwable);
		GD.Print($"[Player] добавлен предмет, всего: {_itemsInRange.Count}");
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
		RegisterNearbyThrowable(otherPlayer);
	}

	private void OnPickupAreaBodyExited(Node3D body)
	{
		if (body is not Player otherPlayer || otherPlayer == this) return;
		UnregisterNearbyThrowable(otherPlayer);
	}
	
	public bool CanBePickedUpBy(IEntity actor) => actor is Player p && p != this;


	public void PickUp(IEntity actor)
	{
		if (actor is not Node3D actorNode) return;

		_isHeld = true;

		// Убираем из текущей иерархии сцены и вешаем на носителя
		GetParent().RemoveChild(this);
		actorNode.AddChild(this);

		// Позиционируем относительно ThrowPoint носителя, если есть
		if (actor is Player carrier && carrier.ThrowPoint != null)
			Position = actorNode.ToLocal(carrier.ThrowPoint.GlobalPosition);
		else
			Position = Vector3.Up * 1.5f;
	}

	public void Drop()
	{
		DetachFromCarrier();
	}

	private void DetachFromCarrier()
	{
		if (!_isHeld) return;

		Vector3 worldPos = GlobalPosition;
		Node carrier = GetParent();

		carrier.RemoveChild(this);

		// Дед = сцена арены. Если деда нет — корень дерева
		Node homeScene = carrier.GetParent() ?? GetTree().Root;
		homeScene.AddChild(this);
		GlobalPosition = worldPos;

		_isHeld = false;
	}



	public void PlayAnimation(string name) { }

	public Vector3 GetMovementDirection(Vector2 input)
	{
		if (_camera == null || input.Length() < 0.1f) return Vector3.Zero;

		Basis camBasis = _camera.GlobalTransform.Basis;
		Vector3 camForward = new Vector3(-camBasis.Z.X, 0, -camBasis.Z.Z).Normalized();
		Vector3 camRight = new Vector3(camBasis.X.X, 0, camBasis.X.Z).Normalized();

		return (camRight * input.X + camForward * -input.Y).Normalized();
	}



	public void Throw(Vector3 impulse)
	{
		DetachFromCarrier();
		_throwVelocity = impulse;
	}

}
