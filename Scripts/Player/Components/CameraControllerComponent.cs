using Godot;

/// <summary>
/// Handles camera control input (mouse look, zoom, shake)
/// Only active for local player
/// </summary>
public partial class CameraControllerComponent : Component
{
	private CharacterBody3D _player;
	private Node3D _cameraPivot;
	private SpringArm3D _springArm;
	private Camera3D _camera;

	[Export] public float MouseSensitivity { get; set; } = 0.003f;
	[Export] public float TiltMin { get; set; } = -70f;
	[Export] public float TiltMax { get; set; } = 20f;
	[Export] public float ZoomMin { get; set; } = 2f;
	[Export] public float ZoomMax { get; set; } = 12f;
	[Export] public float ZoomStep { get; set; } = 1f;
	[Export] public float ZoomSpeed { get; set; } = 10f;

	private float _targetZoom;
	private Vector3 _shakeOffset = Vector3.Zero;
	private bool _isActive = false;

	public void Initialize(CharacterBody3D player, Camera3D camera, Node3D cameraPivot, SpringArm3D springArm, bool isLocalPlayer)
	{
		_player = player;
		_camera = camera;
		_cameraPivot = cameraPivot;
		_springArm = springArm;
		_isActive = isLocalPlayer;

		if (_isActive)
		{
			_camera.MakeCurrent();
			_targetZoom = _springArm.SpringLength;
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
		else
		{
			_camera.Current = false;
		}
	}

	public void Update(float delta)
	{
		if (!_isActive) return;

		UpdateZoom(delta);
		ApplyShake();
	}

	public void HandleInput(InputEvent @event)
	{
		if (!_isActive) return;

		HandleCursorCapture(@event);
		HandleZoomInput(@event);
		HandleMouseLook(@event);
	}

	// =========================================================
	// Input Handling
	// =========================================================

	private void HandleCursorCapture(InputEvent @event)
	{
		if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
			return;
		}

		if (@event is InputEventMouseButton click && click.Pressed
			&& Input.MouseMode == Input.MouseModeEnum.Visible)
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
	}

	private void HandleZoomInput(InputEvent @event)
	{
		GD.Print("We will try to zoom!");
		if (@event is not InputEventMouseButton scroll || !scroll.Pressed) return;

		if (scroll.ButtonIndex == MouseButton.WheelUp)
			_targetZoom = Mathf.Clamp(_targetZoom - ZoomStep, ZoomMin, ZoomMax);
		else if (scroll.ButtonIndex == MouseButton.WheelDown)
			_targetZoom = Mathf.Clamp(_targetZoom + ZoomStep, ZoomMin, ZoomMax);
	}

	private void HandleMouseLook(InputEvent @event)
	{
		if (@event is not InputEventMouseMotion mouseMotion) return;
		if (Input.MouseMode != Input.MouseModeEnum.Captured) return;

		// Horizontal - rotate player
		_player.RotateY(-mouseMotion.Relative.X * MouseSensitivity);

		// Vertical - rotate camera pivot
		_cameraPivot.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);
		Vector3 rot = _cameraPivot.RotationDegrees;
		rot.X = Mathf.Clamp(rot.X, TiltMin, TiltMax);
		_cameraPivot.RotationDegrees = rot;
	}

	private void UpdateZoom(float delta)
	{
		_springArm.SpringLength = Mathf.Lerp(
			_springArm.SpringLength,
			_targetZoom,
			ZoomSpeed * delta
		);
	}

	// =========================================================
	// Camera Shake (reserved)
	// =========================================================

	private void ApplyShake()
	{
		// TODO: implement shake system
		_cameraPivot.Position = new Vector3(
			_shakeOffset.X,
			_cameraPivot.Position.Y,
			_shakeOffset.Z
		);
	}

	public void TriggerShake(float intensity, float duration)
	{
		GD.Print($"[Camera] TriggerShake: intensity={intensity}, duration={duration}");
		// TODO: implement shake
	}
}
