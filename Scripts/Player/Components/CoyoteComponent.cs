using Godot;

public partial class CoyoteComponent : Node
{
	
	[Export] public float CoyoteTime = 0.5f;

    private Timer _timer;
    private bool _active;

    public bool IsActive => _active;

    private bool _jumpedThisFrame;

    public void NotifyJumped() => _jumpedThisFrame = true;

    public override void _Ready()
	{
        _timer = new Timer();
        _timer.OneShot = true;
        _timer.WaitTime = CoyoteTime;
        _timer.Timeout += OnTimerExpired;
        AddChild(_timer);

        var player = GetParent<Player>();
        player.LeftGround += OnLeftGround;
        player.Landed += OnLanded;
    }

    public override void _PhysicsProcess(double delta)
    {
        _jumpedThisFrame = false; // сброс каждый кадр
    }

    //   private void OnLeftGround()
    //{
    //       if (_jumpedThisFrame)
    //       {
    //           _jumpedThisFrame = false;
    //           return; // не открываем coyote — ушли с земли прыжком
    //       }
    //       GD.Print("Left ground");
    //       _active = true;
    //       _timer.Start();
    //   }

    private void OnLeftGround()
    {
        GD.Print($"[Coyote] OnLeftGround. _jumpedThisFrame={_jumpedThisFrame}");
        if (_jumpedThisFrame)
        {
            _jumpedThisFrame = false;
            GD.Print("[Coyote] Игнорируем — ушли с земли прыжком");
            return;
        }
        GD.Print("[Coyote] АКТИВИРОВАН");
        _active = true;
        _timer.Start();
    }

    private void OnLanded()
    {
        _active = false;
        _timer.Stop();
    }

    //private void OnTimerExpired()
    //{
    //    _active = false;
    //}

    ///// <summary>
    ///// Вызывается при использовании прыжка — «тратит» coyote time.
    ///// </summary>
    //public void Consume()
    //{
    //    _active = false;
    //    _timer.Stop();
    //}


    private void OnTimerExpired()
    {
        GD.Print("[Coyote] Таймер истёк — деактивирован");
        _active = false;
    }

    public void Consume()
    {
        GD.Print("[Coyote] Consume() вызван");
        _active = false;
        _timer.Stop();
    }
}