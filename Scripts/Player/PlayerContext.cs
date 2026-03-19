using Godot;
using System;

public partial class PlayerContext : Node, IPlayerContext
{
	public StaminaComponent Stamina => _staminaComponent;

	public HealthComponent Health => throw new NotImplementedException();

	
	Vector3 IMovable.Velocity
	{
		get => _body.Velocity;
		set => _body.Velocity = value;
	}

	public bool IsTouchingFloor => _body.IsOnFloor();

	public bool CanJump => true;

	private CharacterBody3D _body;
	private Camera3D _camera;
	private StaminaComponent _staminaComponent;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Initialize(CharacterBody3D body, Camera3D camera, StaminaComponent staminaComponent)
	{
		_body = body;
		_camera = camera;
		GD.Print("And inside initiliazer, stamina is null: ", staminaComponent == null);
		_staminaComponent = staminaComponent;
	}

	public Vector3 GetMovementDirection(Vector2 input)
	{
		if (_camera == null || input.Length() < 0.1f) return Vector3.Zero;

		Basis camBasis = _camera.GlobalTransform.Basis;
		Vector3 camForward = new Vector3(-camBasis.Z.X, 0, -camBasis.Z.Z).Normalized();
		Vector3 camRight = new Vector3(camBasis.X.X, 0, camBasis.X.Z).Normalized();

		return (camRight * input.X + camForward * -input.Y).Normalized();
	}

}
