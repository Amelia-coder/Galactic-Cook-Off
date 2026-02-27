using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export] public float Speed = 5f;
	[Export] public float JumpVelocity = 5f;
	[Export] public float Gravity = 9.8f;
	
	public override void _PhysicsProcess(double delta)
	{
		var velocity = Velocity;

		if (!IsOnFloor())
			velocity.Y -= Gravity * (float)delta;

		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVelocity;

		var dir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		
		Vector3 moveDir = (Transform.Basis * new Vector3(dir.X, 0, dir.Y)).Normalized();
		
		velocity.X = dir.X * Speed;
		velocity.Z = dir.Y * Speed;

		Velocity = velocity;
		MoveAndSlide();
	}
	

}
