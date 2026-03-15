using Godot;

/// <summary>
/// Управляет камерой одного игрока.
/// Активируется только для локального игрока — IsLocalPlayer.
/// </summary>
public partial class PlayerCameraController : Node
{
    [Export] public float MouseSensitivity = 0.003f;
    [Export] public float TiltMin = -70f;
    [Export] public float TiltMax = 20f;

    [Export] public float ZoomMin = 2f;
    [Export] public float ZoomMax = 12f;
    [Export] public float ZoomStep = 1f;
    [Export] public float ZoomSpeed = 10f;

    private Player _player;
    private Node3D _cameraPivot;
    private SpringArm3D _springArm;
    private Camera3D _camera;

    private float _targetZoom;

    // Зарезервировано под шейк
    private Vector3 _shakeOffset = Vector3.Zero;

    // =========================================================
    // Lifecycle
    // =========================================================

    public override void _Ready()
    {
        _player = GetParent<Player>();

        _cameraPivot = _player.GetNode<Node3D>("CameraPivot");
        _springArm = _player.GetNode<SpringArm3D>("CameraPivot/SpringArm3D");
        _camera = _player.GetNode<Camera3D>("CameraPivot/SpringArm3D/Camera3D");

        if (!_player.IsLocalPlayer)
        {
            // Для нелокальных игроков контроллер вообще не работает
            SetProcess(false);
            SetProcessUnhandledInput(false);
            _camera.Current = false;
            return;
        }

        _camera.MakeCurrent();
        _targetZoom = _springArm.SpringLength;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _Process(double delta)
    {
        UpdateZoom(delta);
        ApplyShake();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        HandleCursorCapture(@event);
        HandleZoomInput(@event);
        HandleMouseLook(@event);
    }

    // =========================================================
    // Вращение и зум
    // =========================================================

    private void HandleCursorCapture(InputEvent @event)
    {
        // Escape — отпустить курсор (например для меню паузы)
        if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            return;
        }

        // Клик по окну — захватить обратно
        if (@event is InputEventMouseButton click && click.Pressed
            && Input.MouseMode == Input.MouseModeEnum.Visible)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }

    private void HandleZoomInput(InputEvent @event)
    {
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

        // Горизонталь — вращаем самого игрока (WASD остаётся относительным)
        _player.RotateY(-mouseMotion.Relative.X * MouseSensitivity);

        // Вертикаль — только пивот камеры
        _cameraPivot.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);
        Vector3 rot = _cameraPivot.RotationDegrees;
        rot.X = Mathf.Clamp(rot.X, TiltMin, TiltMax);
        _cameraPivot.RotationDegrees = rot;
    }

    private void UpdateZoom(double delta)
    {
        _springArm.SpringLength = Mathf.Lerp(
            _springArm.SpringLength,
            _targetZoom,
            ZoomSpeed * (float)delta
        );
    }

    // =========================================================
    // Шейк — зарезервировано
    // =========================================================

    private void ApplyShake()
    {
        // TODO: когда появится CameraShakeComponent — брать offset отсюда
        // _shakeOffset = _shakeComponent.GetCurrentOffset();

        // Применяем offset к пивоту (Y не трогаем — он задаётся позицией игрока)
        _cameraPivot.Position = new Vector3(
            _shakeOffset.X,
            _cameraPivot.Position.Y,
            _shakeOffset.Z
        );
    }

    /// <summary>
    /// Публичный вход для внешних систем — например при получении урона.
    /// </summary>
    public void TriggerShake(float intensity, float duration)
    {
        // TODO: передать в CameraShakeComponent когда будет реализован
        GD.Print($"[Camera] TriggerShake: intensity={intensity}, duration={duration}");
    }
}
