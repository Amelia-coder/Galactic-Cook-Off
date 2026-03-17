using Godot;
using System;

public partial class Player : CharacterBody3D, IEntity, IPlayerContext
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
	private float _chargeTime = 0f;
	private bool _isCharging = false;
	private CoyoteComponent _coyoteTimer;

	private JumpCounterComponent _jumpCounter;

	Vector3 IMovable.Velocity
	{
		get => Velocity;
		set => Velocity = value;
	}

	public void ConsumeCoyote() => _coyoteTimer.Consume();


	private MovementStateMachine _movementStateMachine;

	private StaminaComponent _staminaComponent;

	private bool _wasOnFloor;
	public bool IsTouchingFloor => IsOnFloor();

	public bool CanJump => _jumpCounter.CanJump;

	private bool CanDoubleJump => !IsTouchingFloor && !_coyoteTimer.IsActive && _jumpCounter.CanJump;

	public bool TryJump()
	{
		if (!_jumpCounter.TryConsume()) return false;
		return true;
	}




	public StaminaComponent Stamina;

	public event Action LeftGround;
	public event Action Landed;
	private bool _wasGrounded;

	public HealthComponent Health => throw new NotImplementedException();

	StaminaComponent IPlayerContext.Stamina { get => _staminaComponent; }
	HealthComponent IPlayerContext.Health { get => Health; }


	public override void _Ready()
	{
		_cameraPivot = GetNode<Node3D>("CameraPivot");
		_camera = GetNode<Camera3D>("CameraPivot/SpringArm3D/Camera3D");
		
		_movementStateMachine = GetNode<MovementStateMachine>("MovementStateMachine");
		
		_staminaComponent = GetNode<StaminaComponent>("StaminaComponent");

		_jumpCounter = GetNode<JumpCounterComponent>("JumpCounterComponent");
		_jumpCounter.Refill();

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

		_coyoteTimer = GetNode<CoyoteComponent>("CoyoteComponent");

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
		_movementStateMachine._PhysicsProcess(delta);

		Velocity += Vector3.Down * Gravity * (float)delta;

		GD.Print($"[Physics] Velocity.Y={Velocity.Y:F2} OnFloor={IsOnFloor()}");

		MoveAndSlide();

		UpdateGroundState(IsTouchingFloor);
	}

	public override void _Process(double delta)
	{
		if (!IsLocalPlayer) return;

		if (Input.IsActionPressed("throw"))
		{
			_isCharging = true;
			_chargeTime += (float)delta;
			_chargeTime = Mathf.Min(_chargeTime, 1.5f);
		}

		if (Input.IsActionJustReleased("throw") && _isCharging)
		{
			ThrowDough();
			_isCharging = false;
			_chargeTime = 0f;
		}
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

	public void PlayAnimation(string name)
	{
		
	}

	public Vector3 GetMovementDirection(Vector2 input)
	{
		if(_camera == null || input.Length() < 0.1f)
			return Vector3.Zero;

		Basis camBasis = _camera.GlobalTransform.Basis;
		Vector3 camForward = -camBasis.Z; // Godot camera forward is -Z
		Vector3 camRight = camBasis.X;

		camForward.Y = 0;
		camRight.Y = 0;

		camForward = camForward.Normalized();
		camRight = camRight.Normalized();


		Vector3 direction = (camRight * input.X) + (camForward * -input.Y);

		return direction.Normalized();
	}

	void UpdateGroundState(bool grounded)
	{
		if (_wasGrounded && !grounded)
			LeftGround?.Invoke();

		if (!_wasGrounded && grounded)
			Landed?.Invoke();

		// Сбрасываем накопленную гравитацию когда стоим на земле
		if (grounded && Velocity.Y < 0)
			Velocity = new Vector3(Velocity.X, 0, Velocity.Z);

		_wasGrounded = grounded;
	}

}
