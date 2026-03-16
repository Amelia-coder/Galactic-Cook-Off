using Godot;

public partial class CoyoteComponent : Node
{
	
	[Export] public float CoyoteTime = 1.5f;

	private Timer _timer;
	private bool _active;

	public bool IsActive => _active;

	public override void _Ready()
	{
		_timer = new Timer();
		_timer.OneShot = true;
		_timer.WaitTime = CoyoteTime;
		AddChild(_timer);

		var player = (IPlayerContext)GetParent<Player>();
		player.LeftGround += OnLeftGround;
		player.Landed += OnLanded;
	}

	private void OnLeftGround()
	{
		GD.Print("Left ground");
		_active = true;
		_timer.Start();
	}

	private void OnLanded()
	{
		_active = false;
		_timer.Stop();
	}

}

//using Godot;

//public partial class CoyoteComponent : Node
//{
//	[Export] public float CoyoteTime = 1.5f;

//	private Timer _timer;
//	private bool _active;

//	public bool IsActive => _active;

//	public override void _Ready()
//	{
//		_timer = new Timer();
//		_timer.OneShot = true;
//		_timer.WaitTime = CoyoteTime;
//		AddChild(_timer);

//		_timer.Timeout += OnTimeout;

//		var player = GetParent<PlayerController>();
//		player.LeftGround += OnLeftGround;
//		player.Landed += OnLanded;
//	}

//	private void OnLeftGround()
//	{
//		GD.Print("Left ground");
//		_active = true;
//		_timer.Start();
//	}

//	private void OnLanded()
//	{
//		_active = false;
//		_timer.Stop();
//	}

//	private void OnTimeout()
//	{
//		_active = false;
//	}
//}
